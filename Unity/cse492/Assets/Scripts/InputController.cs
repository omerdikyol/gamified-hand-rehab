using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    // Reference to the GloveController script
    public GloveController gloveController;

    // List to store predefined hand states
    private List<HandState> handStates = new List<HandState>();
    public float quaternionThreshold = 0.08f; // Threshold for comparing quaternion values
    public float fingerThreshold = 30f; // Threshold for comparing finger values

    private float[] fingerMinValues; // Array to store the minimum finger values given by the GloveController
    private float[] fingerMaxValues; // Array to store the maximum finger values given by the GloveController
    public float minMaxThreshold = 60f; // Threshold for checking if a finger value is near the min or max value

    // Start is called before the first frame update
    void Start()
    {
        // Add predefined hand states and their associated values to the list (example values)
        // AddHandState(new HandState(new float[] {0.99f, 0.01f, -0.01f, -0.17f}, new float[] { 604, 402, 359, 441, 520 }));
        // AddHandState(new HandState(new float[] {0.99f, 0.00f, -0.01f, -0.17f}, new float[] { 607, 400, 360, 442, 532 }));
        // AddHandState(new HandState(new float[] {0.99f, 0.00f, -0.01f, -0.15f}, new float[] { 610, 399, 360, 442, 545 }));
        // AddHandState(new HandState(new float[] {0.99f, 0.00f, -0.01f, -0.14f}, new float[] { 612, 399, 360, 442, 558 }));
    }

    // Update is called once per frame
    void Update()
    {

        // Get the finger min and max values from the GloveController for checking fingers that are closest to the min and max values
        if (gloveController.GetIsCalibrated())
        {
            fingerMinValues = gloveController.GetFingerMinValues();
            fingerMaxValues = gloveController.GetFingerMaxValues();
        }

        // Check the current hand state
        CheckHandState();

        // Check for a A key press to collect and add a hand state
        // Different keys for different types of data capture
        if (Input.GetKeyDown(KeyCode.A)) // Full hand state
        {
            StartCoroutine(CollectAndAddHandState(true, true));
        }
        if (Input.GetKeyDown(KeyCode.S)) // Only finger values
        {
            StartCoroutine(CollectAndAddHandState(false, true));
        }
        if (Input.GetKeyDown(KeyCode.D)) // Only hand rotation
        {
            StartCoroutine(CollectAndAddHandState(true, false));
        }
    }

    // Function to add a hand state to the list 
    private void AddHandState(HandState handState)
    {
        handStates.Add(handState);
    }

    // Function to check the current hand state against the predefined states
    private void CheckHandState()
    {
        if (gloveController != null && gloveController.GetIsCalibrated())
        {
            // Get the current quaternion values and finger values from the GloveController
            float[] handStateValues = gloveController.GetQuaternionValues();

            float[] fingerValues = gloveController.GetFingerValues(); 

            // Create a HandState object with the current hand values
            HandState currentHandState = new HandState(handStateValues, fingerValues, true, true);

            // Check if the current hand state matches any of the predefined states
            foreach (HandState state in handStates)
            {
                if (IsMatchingState(currentHandState, state))
                {
                    // Perform action for the matched hand state
                    Debug.Log("Current hand state matches a predefined state.");
                    break;
                }
            }
        }
    }

    // Function to compare two hand states
    private bool IsMatchingState(HandState currentState, HandState predefinedState)
    {
        // Compare quaternion values if the predefined state includes quaternion values
        if (predefinedState.includesQuaternion)
        {
            for (int i = 0; i < currentState.quaternionValues.Length; i++)
            {
                if (!FastApproximately(currentState.quaternionValues[i], predefinedState.quaternionValues[i], quaternionThreshold))
                {
                    return false; // Quaternion values do not match within the threshold
                }
            }
        }

        // Compare finger values if the predefined state includes finger values
        if (predefinedState.includesFingers)
        {
            for (int i = 0; i < currentState.fingerValues.Length; i++)
            {
                // The comparison logic for finger values is adjusted to account for values beyond max/min thresholds
                bool isCurrentNearMax = FastApproximately(currentState.fingerValues[i], fingerMaxValues[i], fingerThreshold);
                bool isCurrentNearMin = FastApproximately(currentState.fingerValues[i], fingerMinValues[i], fingerThreshold);
                bool isPredefinedNearMax = FastApproximately(predefinedState.fingerValues[i], fingerMaxValues[i], fingerThreshold);
                bool isPredefinedNearMin = FastApproximately(predefinedState.fingerValues[i], fingerMinValues[i], fingerThreshold);

                // Check if current and predefined states are near max or min and consider them matching in those cases
                if ((isCurrentNearMax && isPredefinedNearMax) || (isCurrentNearMin && isPredefinedNearMin))
                {
                    // If both are near max or both are near min, consider this a match for the current finger
                    continue;
                }
                else if (!FastApproximately(currentState.fingerValues[i], predefinedState.fingerValues[i], fingerThreshold))
                {
                    // If they are not near the extremes, they must be approximately equal within the threshold
                    return false; // Finger values do not match within the threshold
                }
            }
        }

        return true; // All compared values match within their respective thresholds
    }

    // Function to collect and add a hand state to the list
    private IEnumerator CollectAndAddHandState(bool includeQuaternion, bool includeFingers)
    {
        // Immediate feedback to ensure the coroutine has started
        Debug.Log("Coroutine started. Collecting hand state for 5 seconds. Include quaternion: " + includeQuaternion + ", Include fingers: " + includeFingers);

        if (gloveController != null && gloveController.GetIsCalibrated())
        {
            List<float[]> quaternionValuesList = new List<float[]>();
            List<float[]> fingerValuesList = new List<float[]>();

            float startTime = Time.time;
            float endTime = startTime + 5f; // Collect data for 5 seconds

            while (Time.time < endTime)
            {
                // Efficient Data Collection
                if (includeQuaternion)
                {
                    quaternionValuesList.Add(gloveController.GetQuaternionValues());
                }
                if (includeFingers)
                {
                    fingerValuesList.Add(gloveController.GetFingerValues());
                }

                // Spread Computation by yielding for the next frame
                yield return null;
            }

            // Compute the mean of the collected values
            float[] meanQuaternion = includeQuaternion ? ComputeMean(quaternionValuesList) : new float[0];
            float[] meanFingerValues = includeFingers ? ComputeMean(fingerValuesList) : new float[0];

            // Add the new hand state
            HandState newHandState = new HandState(meanQuaternion, meanFingerValues, includeQuaternion, includeFingers);
            handStates.Add(newHandState);
            
            Debug.Log("Coroutine ended. Hand state added.");
        }
        else
        {
            Debug.Log("GloveController is not calibrated or available.");
        }
    }

    // Function to compute the mean of a list of float arrays
    private float[] ComputeMean(List<float[]> valuesList)
    {
        if (valuesList.Count == 0)
            return null;

        float[] meanValues = new float[valuesList[0].Length];
        foreach (var values in valuesList)
        {
            for (int i = 0; i < values.Length; i++)
            {
                meanValues[i] += values[i];
            }
        }

        for (int i = 0; i < meanValues.Length; i++)
        {
            meanValues[i] /= valuesList.Count;
        }

        return meanValues;
    }

    public static bool FastApproximately(float a, float b, float threshold)
    {
        return Mathf.Abs(a - b) <= threshold;
    }
}
