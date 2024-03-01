using System;
using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class GloveController : MonoBehaviour
{
    public string portName = "COM8";
    public int baudRate = 9600;
    private SerialPort serialPort;

    // Joint transforms for each finger
    public Transform[] thumbJoints;
    public Transform[] indexJoints;
    public Transform[] middleJoints;
    public Transform[] ringJoints;
    public Transform[] pinkyJoints;

    private float[] minValues = new float[5]; // Minimum calibration values for fingers
    private float[] maxValues = new float[5]; // Maximum calibration values for fingers
    private float[] tempCalibrationValues = new float[5]; // Temporary storage for calibration values
    private bool isCalibrated = false; // Track if the system has been calibrated

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            Debug.Log("Serial port opened");
            StartCalibration();
        }
        catch (Exception e)
        {
            Debug.LogError("Could not open serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (isCalibrated && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string dataString = serialPort.ReadLine();
                ParseData(dataString);
            }
            catch (TimeoutException)
            {
                Debug.LogWarning("Timeout exception");
            }
        }
    }

    void ParseData(string data)
    {
        string[] values = data.Split(',');
        if (values.Length >= 11) // 6 for IMU data and 5 for finger data
        {
            // Parse the IMU data
            float ax = float.Parse(values[0]);
            float ay = float.Parse(values[1]);
            float az = float.Parse(values[2]);
            float gx = float.Parse(values[3]);
            float gy = float.Parse(values[4]);
            float gz = float.Parse(values[5]);

            // Use the parsed data
            Debug.Log($"Accelerometer: {ax}, {ay}, {az} | Gyroscope: {gx}, {gy}, {gz}");

            // Parse finger potentiometer values
            for (int i = 0; i < 5; i++)
            {
                // float normalizedValue = MapValueToRange(float.Parse(values[6 + i]), 0, 1023, 90, 0); // Without calibration
                float normalizedValue = MapValueToRange(float.Parse(values[6 + i]), minValues[i], maxValues[i], 85, 5); // With calibration
                Debug.Log("Finger " + i + " value: " + normalizedValue);
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
        else
        {
            Debug.LogError("Data string does not contain enough values. Received: " + values.Length);
        }
    }

    // Clamping approach for finger rotation (Not effective as i thought)
    void RotateFinger(Transform[] fingerJoints, float angle)
    {
        float[] jointRotations = CalculateJointRotations(angle, fingerJoints.Length);

        for (int i = 0; i < fingerJoints.Length; i++)
        {
            // Clamp the joint rotation to avoid unrealistic movement
            float clampedRotation = Mathf.Clamp(jointRotations[i], 0, 180);
            
            // Assuming the fingers rotate around their local Z-axis
            fingerJoints[i].localEulerAngles = new Vector3(fingerJoints[i].localEulerAngles.x, fingerJoints[i].localEulerAngles.y, -clampedRotation);
        }
    }

    // // Direct approach for finger rotation
    // void RotateFinger(Transform[] fingerJoints, float angle)
    // {
    //     // Adjust these based on your model's rotation limits and joint configurations
    //     float[] jointRotations = CalculateJointRotations(angle, fingerJoints.Length);

    //     for (int i = 0; i < fingerJoints.Length; i++)
    //     {
    //         // Assuming the fingers rotate around their local X-axis
    //         fingerJoints[i].localEulerAngles = new Vector3(0f, 0f, -jointRotations[i]);
    //     }
    // }

    private IEnumerator StartCalibrationRoutine()
    {
        Debug.Log("Calibration start (Part 1): Make a fist.");
        
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReadCalibrationValues(true)); // true for min values
        yield return new WaitForSeconds(5f);

        Debug.Log("Calibration start (Part 2): Open your hand.");
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ReadCalibrationValues(false)); // false for max values

        isCalibrated = true;
        Debug.Log("Calibration completed.");

        // Optionally, log the min and max values for each finger
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"Finger {i} min: {minValues[i]}, max: {maxValues[i]}");
        }
    }

    private IEnumerator ReadCalibrationValues(bool isReadingMinValues)
    {
        float[] sumValues = new float[5];
        int readingsCount = 0;

        float startTime = Time.time;
        while (Time.time - startTime < 5f) // Collect data for 5 seconds
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                string dataString = serialPort.ReadLine();
                string[] values = dataString.Split(',');
                for (int i = 0; i < 5; i++)
                {
                    sumValues[i] += float.Parse(values[6 + i]); // Adjust index based on your data format
                }
                readingsCount++;
            }
            yield return null; // Wait for the next frame
        }

        // Calculate the mean for each finger
        for (int i = 0; i < 5; i++)
        {
            tempCalibrationValues[i] = sumValues[i] / readingsCount;
        }

        // Assign the averaged values to the appropriate calibration array
        if (isReadingMinValues)
        {
            minValues = (float[])tempCalibrationValues.Clone();
        }
        else
        {
            maxValues = (float[])tempCalibrationValues.Clone();
        }

        Debug.Log("Calibration values read.");
    }

    public void StartCalibration()
    {
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
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}