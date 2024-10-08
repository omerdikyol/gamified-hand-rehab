using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public float quaternionThreshold = 0.02f; // Threshold for avoiding the unnecessary rotation of the hand model

    public bool isCalibrated = false; // Track if the system has been calibrated
    private float[] fingerMinValuesOfUser = new float[5]; // Minimum calibration values for fingers
    private float[] fingerMaxValuesOfUser = new float[5]; // Maximum calibration values for fingers
    private float[] fingerMinValuesHealthy = { 637, 632, 650, 711, 721 }; // Minimum calibration values for fingers of a healthy hand (Approximately)
    private float[] fingerMaxValuesHealthy = { 445, 496, 470, 522, 522 }; // Maximum calibration values for fingers of a healthy hand (Approximately)
    private float[] tempCalibrationValues = new float[5]; // Temporary storage for calibration values
    private Vector3 initialPosition;  // Store the initial position of the model
    private Quaternion initialRotation; // Store the initial rotation of the model
    private float qw, qx, qy, qz; // Quaternion values for the model rotation
    private float prevQw, prevQx, prevQy, prevQz; // Previous quaternion values
    private float[] prevFingerValues = new float[5]; // Previous finger values
    private float[] fingerNormalizedValuesForModel = new float[5]; // Normalized finger values for hand model
    private float[] fingerNormalizedValuesForAngleCollector = new float[5]; // Normalized finger values for angle collector
    private float[] fingerAngles = new float[5]; // Store the angles of the fingers
    public bool isAnglesReversed = false; // Track if the finger angles are reversed

    // GUI elements for displaying finger angles
    [Header("GUI Elements")]
    public TextMeshProUGUI[] fingerAngleTexts;
    public SettingsManager settingsManager;

    [Header("Angle Collector")]
    public FingerAngleCollector angleCollector;
    
    // Multithreading approach for reading serial data
    [Header("Serial Port Settings")]
    public SerialPortManager serialPortManager;
    private Thread serialThread;
    private string serialData;
    private readonly object lockObject = new object();
    private readonly object portLock = new object();

    [Header("Localization")]
    public LocalizationManager localizationManager;

    void Start()
    {
        serialPortManager = FindObjectOfType<SerialPortManager>();
        if (serialPortManager == null)
        {
            Debug.LogError("SerialPortManager not found. Please add SerialPortManager to the scene.");
            return;
        }
        settingsManager = FindObjectOfType<SettingsManager>();
    }

    void Update()
    {
        if (!InputController.isAwaitingInput && serialPortManager.isCalibrated)
        {
            string data;
            while (serialPortManager.DataQueue.TryDequeue(out data))
            {
                ParseData(data, fingerMinValuesOfUser, fingerMaxValuesOfUser, fingerNormalizedValuesForModel, true); // Parse the finger data for changing the hand model
                ParseData(data, fingerMinValuesHealthy, fingerMaxValuesHealthy, fingerNormalizedValuesForAngleCollector, false); // Parse the finger data for logging the user's correct finger angles
                if (angleCollector != null && isCalibrated)
                {
                    // Log the data for the angle collector but normalize the finger angles based on the ROM and calibration values of a healthy hand first
                    float[] fingerAnglesOfModel = CalculateFingerAngles(fingerNormalizedValuesForModel);
                    float[] fingerAnglesOfHand = CalculateFingerAngles(fingerNormalizedValuesForAngleCollector);

                    // Reverse the angles before saving to csv file if needed
                    if (isAnglesReversed)
                    {
                        // Reverse the angles based on the maximum ROM values
                        float[] maxROM = new float[] { 60, 90, 90, 90, 90 };
                        for (int i = 0; i < 5; i++)
                        {
                            fingerAnglesOfHand[i] = maxROM[i] - fingerAnglesOfHand[i];
                        }
                    }

                    angleCollector.LogData(fingerAnglesOfModel, fingerAnglesOfHand);
                    
                    // Update the finger angles on the GUI
                    UpdateFingerAnglesGUI(fingerAnglesOfModel);
                }
            }
        }
    }

    private Quaternion initialQuaternion;
    private bool initialQuaternionSet = false;

    void ParseData(string data, float[] fingerMinValues, float[] fingerMaxValues, float[] outputValues, bool applyToModel = true)
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

            // Apply the corrected quaternion to the transform if the difference exceeds the threshold based on the settings of hand rotation
            if (settingsManager == null || settingsManager.isRotationEnabled)
            {
                if (Math.Abs(newQw - prevQw) > quaternionThreshold 
                    || Math.Abs(newQx - prevQx) > quaternionThreshold 
                    || Math.Abs(newQy - prevQy) > quaternionThreshold 
                    || Math.Abs(newQz - prevQz) > quaternionThreshold)
                {
                    transform.rotation = correctedQuaternion;
                }
            }

            // Update previous quaternion values (if needed)
            prevQw = newQw;
            prevQx = newQx;
            prevQy = newQy;
            prevQz = newQz;
            
            // Parse finger potentiometer values
            for (int i = 0; i < 5; i++)
            {
                float rawValue = float.Parse(values[4 + i]);
                float normalizedValue = 0f;
                if (!isAnglesReversed)
                {
                    normalizedValue = 100 - Mathf.Clamp((rawValue - fingerMinValues[i]) / (fingerMaxValues[i] - fingerMinValues[i]) * 100, 0, 100);
                }
                else
                {
                    normalizedValue = Mathf.Clamp((rawValue - fingerMinValues[i]) / (fingerMaxValues[i] - fingerMinValues[i]) * 100, 0, 100);
                }
                
                outputValues[i] = normalizedValue;
                // Debug.Log("Finger " + i + " value: " + normalizedValue);
                if (applyToModel)
                {
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
        CountdownTimer countdownTimer = GameObject.Find("CountdownTimer").GetComponent<CountdownTimer>();
        RawImage openHandImage = GameObject.Find("OpenHandImage").GetComponent<RawImage>();
        RawImage fistImage = GameObject.Find("FistImage").GetComponent<RawImage>();

        // Get Text From Calibration Scene which name is "StatusText"
        TextMeshProUGUI statusText = GameObject.Find("StatusText").GetComponent<TextMeshProUGUI>();

        // Start the calibration process
        statusText.text = localizationManager.GetLocalizedString("starting_calibration_waiting_for_sensor");
        // statusText.text = "Starting calibration. Waiting for sensor initialization...";
        Debug.Log("Starting calibration. Waiting for sensor initialization...");
        countdownTimer.SetTime(15f);
        yield return new WaitForSeconds(15f); // Wait for the initial calibration of MPU6050 (can be modified at arduino code maybe)

        // Finger calibration is done in two parts: first, the user makes a fist, then opens their hand
        statusText.text = localizationManager.GetLocalizedString("calibration_start_part1");
        // statusText.text = "Calibration start (Part 1): Make a fist.";
        Debug.Log("Calibration start (Part 1): Make a fist.");
        
        countdownTimer.SetTime(10f);
        fistImage.enabled = true;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReadFingerCalibrationValues(true)); // true for min values
        yield return new WaitForSeconds(3f);
        fistImage.enabled = false;

        countdownTimer.SetTime(10f);
        openHandImage.enabled = true;
        statusText.text = localizationManager.GetLocalizedString("calibration_start_part2");
        // statusText.text = "Calibration start (Part 2): Open your hand.";
        Debug.Log("Calibration start (Part 2): Open your hand.");
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReadFingerCalibrationValues(false)); // false for max values
        yield return new WaitForSeconds(3f);
        openHandImage.enabled = false;

        // Optionally, log the min and max values for each finger
        for (int i = 0; i < 5; i++)
        {
            if (fingerMinValuesOfUser[i] > fingerMaxValuesOfUser[i])
            {
                // Swap the values if the min value is greater than the max value
                float temp = fingerMinValuesOfUser[i];
                fingerMinValuesOfUser[i] = fingerMaxValuesOfUser[i];
                fingerMaxValuesOfUser[i] = temp;
                isAnglesReversed = true;
            }

            Debug.Log($"Finger {i} min: {fingerMinValuesOfUser[i]}, max: {fingerMaxValuesOfUser[i]}");
        }

        // Complete the calibration processs
        isCalibrated = true;
        serialPortManager.isCalibrated = true;
        statusText.text = localizationManager.GetLocalizedString("calibration_completed");
        // statusText.text = "Calibration completed.";
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
                    fingerMinValuesOfUser[i] = averageValue;
                }
                else
                {
                    fingerMaxValuesOfUser[i] = averageValue;
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
        fingerMinValuesOfUser = new float[5];
        fingerMaxValuesOfUser = new float[5];
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

    public float[] CalculateFingerAngles(float[] fingerNormalizedValues)
    {
        int[] fingerMaxROM = new int[] {60, 90, 90, 90, 90}; // Maximum range of motion for each finger
        float[] result = new float[5];

        for (int i = 0; i < 5; i++)
        {
            // Calculate the angle based on the normalized value and the maximum ROM
            result[i] = fingerNormalizedValues[i] / 100f * fingerMaxROM[i];
        }

        return result;
    }

    void UpdateFingerAnglesGUI(float[] fingerAngles)
    {
        for (int i = 0; i < fingerAngles.Length; i++)
        {
            fingerAngleTexts[i].text = $"{fingerAngles[i]:F3}°";
        }

        // Log the angles for debugging purposes
        // Debug.Log($"Angles - Thumb: {fingerAngles[0]:F2}, Index: {fingerAngles[1]:F2}, Middle: {fingerAngles[2]:F2}, Ring: {fingerAngles[3]:F2}, Pinky: {fingerAngles[4]:F2}");
    }

    public void ToggleAnglesReversed()
    {
        isAnglesReversed = !isAnglesReversed;
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
        return fingerNormalizedValuesForModel;
    }

    public float[] GetFingerMinValues()
    {
        return fingerMinValuesOfUser;
    }

    public float[] GetFingerMaxValues()
    {
        return fingerMaxValuesOfUser;
    }

    public float[] GetFingerMaxValuesHealthy()
    {
        return fingerMaxValuesHealthy;
    }

    public float[] GetFingerMinValuesHealthy()
    {
        return fingerMinValuesHealthy;
    }
}
