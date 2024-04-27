using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using TMPro;

public class GloveController : MonoBehaviour
{
    // Joint transforms for each finger
    [Header("Finger Joints")]
    public Transform[] thumbJoints;
    public Transform[] indexJoints;
    public Transform[] middleJoints;
    public Transform[] ringJoints;
    public Transform[] pinkyJoints;

    [Header("Threshold Values")]
    public float quaternionThreshold = 0.03f; // Threshold for avoiding the unnecessary rotation of the hand model

    public bool isCalibrated = false; // Track if the system has been calibrated
    private float[] fingerMinValues = new float[5]; // Minimum calibration values for fingers
    private float[] fingerMaxValues = new float[5]; // Maximum calibration values for fingers
    private float[] tempCalibrationValues = new float[5]; // Temporary storage for calibration values
    private Vector3 initialPosition;  // Store the initial position of the model
    private Quaternion initialRotation; // Store the initial rotation of the model
    private float qw, qx, qy, qz; // Quaternion values for the model rotation
    private float prevQw, prevQx, prevQy, prevQz; // Previous quaternion values
    private float[] prevFingerValues = new float[5]; // Previous finger values
    private float[] fingerNormalizedValues = new float[5]; // Normalized finger values
    private float[] fingerAngles = new float[5]; // Store the angles of the fingers

    // GUI elements for displaying finger angles
    [Header("GUI Elements")]
    public TextMeshProUGUI[] fingerAngleTexts;

    // Multithreading approach for reading serial data
    [Header("Serial Port Settings")]
    private Thread serialThread;
    private bool isRunning = true;
    private string serialData;
    private readonly object lockObject = new object();
    private readonly object portLock = new object();
    public SerialPortManager serialPortManager;

    void Start()
    {
        serialPortManager = FindObjectOfType<SerialPortManager>();
        if (serialPortManager == null)
        {
            Debug.LogError("SerialPortManager not found. Please add SerialPortManager to the scene.");
            return;
        }
    }

    void Update()
    {
        if (!InputController.isAwaitingInput && serialPortManager.isCalibrated)
        {
            string data;
            while (serialPortManager.DataQueue.TryDequeue(out data))
            {
                ParseData(data);
            }

            UpdateFingerAnglesGUI(CalculateFingerAngles());
        }
    }

    private Quaternion initialQuaternion;
    private bool initialQuaternionSet = false;

    void ParseData(string data)
    {
        string[] values = data.Split(',');

        if (values.Length >= 9) // 4 for IMU data and 5 for finger data
        {
            // Parse quaternion values
            float newQw = float.Parse(values[0]);
            float newQx = float.Parse(values[1]);
            float newQy = float.Parse(values[2]);
            float newQz = float.Parse(values[3]);

            Quaternion incomingQuaternion = new Quaternion(newQx, newQz, newQy, -newQw);

            // Set the initial quaternion if not set
            if (!initialQuaternionSet)
            {
                initialQuaternion = incomingQuaternion;
                initialQuaternionSet = true;
            }

            // Adjust the quaternion by the initial quaternion
            Quaternion correctedQuaternion = Quaternion.Inverse(initialQuaternion) * incomingQuaternion;

            // Apply the corrected quaternion to the transform
            if (QuaternionDifferenceExceedsThreshold(correctedQuaternion, transform.rotation))
            {
                transform.rotation = correctedQuaternion;
            }

            transform.rotation = correctedQuaternion;

            // Update previous quaternion values (if needed)
            prevQw = newQw;
            prevQx = newQx;
            prevQy = newQy;
            prevQz = newQz;
            
            // Parse finger potentiometer values
            for (int i = 0; i < 5; i++)
            {
                float rawValue = float.Parse(values[4 + i]);
                // float normalizedValue = MapValueToRange(float.Parse(values[4 + i]), fingerMinValues[i], fingerMaxValues[i], 85, 5); // With calibration
                float normalizedValue = 100 - Mathf.Clamp((rawValue - fingerMinValues[i]) / (fingerMaxValues[i] - fingerMinValues[i]) * 100, 0, 100);
                fingerNormalizedValues[i] = normalizedValue;
                // Debug.Log("Finger " + i + " value: " + normalizedValue);
                switch(i)
                {
                    case 0:
                        RotateFinger(thumbJoints, normalizedValue);
                        break;
                    case 1:
                        RotateFinger(indexJoints, normalizedValue);
                        break;
                    case 2:
                        RotateFinger(middleJoints, normalizedValue);
                        break;
                    case 3:
                        RotateFinger(ringJoints, normalizedValue);
                        break;
                    case 4:
                        RotateFinger(pinkyJoints, normalizedValue);
                        break;
                }
            }

            // Print the quaternion and finger values for debugging
            // Debug.Log($"Quaternion: {qw}, {qx}, {qy}, {qz} | Fingers: {fingerNormalizedValues[0]}, {fingerNormalizedValues[1]}, {fingerNormalizedValues[2]}, {fingerNormalizedValues[3]}, {fingerNormalizedValues[4]}");
        }
        else
        {
            Debug.LogError("Data string does not contain enough values. Received: " + values.Length);
        }
    }

    bool QuaternionDifferenceExceedsThreshold(Quaternion q1, Quaternion q2)
    {
        return Mathf.Abs(Quaternion.Dot(q1, q2)) < quaternionThreshold;
    }

    // Clamping approach for finger rotation (Not effective as i thought)
    void RotateFinger(Transform[] fingerJoints, float angle)
    {
        float[] jointRotations = CalculateJointRotations(angle, fingerJoints.Length);

        for (int i = 0; i < fingerJoints.Length; i++)
        {
            // Clamp the joint rotation to avoid unrealistic movement
            float clampedRotation = Mathf.Clamp(jointRotations[i], -10, 190); // previously 0, 180
            
            // Assuming the fingers rotate around their local Z-axis
            fingerJoints[i].localEulerAngles = new Vector3(fingerJoints[i].localEulerAngles.x, fingerJoints[i].localEulerAngles.y, -clampedRotation);
        }
    }

    private IEnumerator StartCalibrationRoutine()
    {
        // Get Text From Calibration Scene which name is "StatusText"
        TextMeshProUGUI statusText = GameObject.Find("StatusText").GetComponent<TextMeshProUGUI>();

        statusText.text = "Starting calibration...";
        Debug.Log("Starting calibration...");
        yield return new WaitForSeconds(15f); // Wait for the initial calibration of MPU6050 (can be modified at arduino code maybe)

        // Finger calibration is done in two parts: first, the user makes a fist, then opens their hand
        statusText.text = "Calibration start (Part 1): Make a fist.";
        Debug.Log("Calibration start (Part 1): Make a fist.");
        
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReadFingerCalibrationValues(true)); // true for min values
        yield return new WaitForSeconds(5f);

        statusText.text = "Calibration start (Part 2): Open your hand.";
        Debug.Log("Calibration start (Part 2): Open your hand.");
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReadFingerCalibrationValues(false)); // false for max values

        // Optionally, log the min and max values for each finger
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"Finger {i} min: {fingerMinValues[i]}, max: {fingerMaxValues[i]}");
        }

        isCalibrated = true;
        serialPortManager.isCalibrated = true;
        statusText.text = "Calibration completed.";
        Debug.Log("Calibration completed.");
    }

    private IEnumerator ReadFingerCalibrationValues(bool isReadingMinValues)
    {
        float[] sumValues = new float[5];
        int readingsCount = 0;
        float startTime = Time.time;

        // Loop for 5 seconds, regardless of data availability
        while (Time.time - startTime < 5f)
        {
            // Attempt to process all available data in the queue each frame
            while (serialPortManager.DataQueue.TryDequeue(out string data))
            {
                string[] values = data.Split(',');

                if (values.Length >= 9) // Assuming data format with at least 9 values
                {
                    for (int i = 0; i < 5; i++) // Process only the finger values
                    {
                        if (float.TryParse(values[4 + i], out float parsedValue))
                        {
                            sumValues[i] += parsedValue;
                        }
                        else
                        {
                            Debug.LogError("Error parsing data: " + values[4 + i]);
                        }
                    }
                    readingsCount++;
                }
            }

            yield return null; // Yield to the next frame, allowing other updates to process
        }

        if (readingsCount > 0) // Ensure we have at least one reading to avoid division by zero
        {
            for (int i = 0; i < 5; i++)
            {
                float averageValue = sumValues[i] / readingsCount; // Calculate average or accumulate values
                if (isReadingMinValues)
                {
                    fingerMinValues[i] = averageValue;
                }
                else
                {
                    fingerMaxValues[i] = averageValue;
                }
            }
        }
        else
        {
            Debug.LogError("No valid readings were processed.");
        }
    }


    public void StartCalibration()
    {
        // Clear the calibration values
        fingerMinValues = new float[5];
        fingerMaxValues = new float[5];
        tempCalibrationValues = new float[5];
        isCalibrated = false;
        serialPortManager.isCalibrated = false;

        // Start the calibration routine
        StartCoroutine(StartCalibrationRoutine());
    }

    float[] CalculateJointRotations(float angle, int jointCount)
    {
        // This function should be calibrated based on the actual movement range of your glove and 3D model
        // For example, if the base joint should move less than the tip, the angles should be distributed accordingly
        // For 3 joints, the angles could be distributed like this: 0.9, 0.8, 1
        // For 4 joints: 0.7, 0.6, 0.5, 0.4

        switch (jointCount)
        {
            case 3:
                return new float[] { angle * 0.9f, angle * 0.8f, angle };
            case 4:
                return new float[] { angle * 0.7f, angle * 0.6f, angle * 0.5f, angle * 0.4f };
            default:
                return new float[] { angle, angle, angle };
        }
    }

    float MapValueToRange(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return Mathf.Clamp((value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin, 0f, 100f);
    }

    public float[] CalculateFingerAngles()
    {
        int[] fingerMaxROM = new int[] {60, 90, 90, 90, 90}; // Maximum range of motion for each finger

        for (int i = 0; i < 5; i++)
        {
            // Calculate the angle based on the normalized value
            // fingerNormalizedValues[i] should be a percentage (0-100)
            fingerAngles[i] = fingerNormalizedValues[i] / 100f * fingerMaxROM[i];
        }

        return fingerAngles;
    }

    void UpdateFingerAnglesGUI(float[] fingerAngles)
    {
        for (int i = 0; i < fingerAngles.Length; i++)
        {
            fingerAngleTexts[i].text = $"{fingerAngles[i]:F2}°";
        }

        // Log the angles for debugging purposes
        Debug.Log($"Angles - Thumb: {fingerAngles[0]:F2}, Index: {fingerAngles[1]:F2}, Middle: {fingerAngles[2]:F2}, Ring: {fingerAngles[3]:F2}, Pinky: {fingerAngles[4]:F2}");
    }

    public bool GetIsCalibrated()
    {
        return isCalibrated;
    }

    public float[] GetQuaternionValues()
    {
        float[] quaternionValues = new float[4];
        quaternionValues[0] = qw;
        quaternionValues[1] = qx;
        quaternionValues[2] = qy;
        quaternionValues[3] = qz;

        return quaternionValues;
    }

    public float[] GetFingerValues()
    {
        return fingerNormalizedValues;
    }

    public float[] GetFingerMinValues()
    {
        return fingerMinValues;
    }

    public float[] GetFingerMaxValues()
    {
        return fingerMaxValues;
    }
}
