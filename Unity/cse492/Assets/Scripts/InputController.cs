using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    // Reference to the GloveController script
    public GloveController gloveController;

    // List to store predefined hand states
    public List<HandState> handStates = new List<HandState>();
    public float quaternionThreshold = 0.04f; // Threshold for comparing quaternion values
    public float fingerThreshold = 10; // Threshold for comparing finger values

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
            bool isQuaternionMatching = true;
            for (int i = 0; i < currentState.quaternionValues.Length; i++)
            {
                if (!FastApproximately(currentState.quaternionValues[i], predefinedState.quaternionValues[i], quaternionThreshold))
                {
                    isQuaternionMatching = false;
                    break;
                }
            }
            if (!isQuaternionMatching)
            {
                return false;
            }
        }
        
        // Compare finger values if the predefined state includes finger values
        if (predefinedState.includesFingers)
        {
            bool isFingerMatching = true;
            for (int i = 0; i < currentState.fingerValues.Length; i++)
            {
                if (!FastApproximately(currentState.fingerValues[i], predefinedState.fingerValues[i], fingerThreshold))
                {
                    isFingerMatching = false;
                    break;
                }
            }
            if (!isFingerMatching)
            {
                return false;
            }
        }

        // Return true if both quaternion values and finger values match
        return true;
    }

    // The adjusted coroutine to collect data based on flags for what to include
    private IEnumerator CollectAndAddHandState(bool includeQuaternion, bool includeFingers)
    {
        if (gloveController != null && gloveController.GetIsCalibrated())
        {
            List<float[]> quaternionValuesList = new List<float[]>();
            List<float[]> fingerValuesList = new List<float[]>();

            Debug.Log("Collecting hand state for 5 seconds. Include quaternion: " + includeQuaternion + ", Include fingers: " + includeFingers);

            float startTime = Time.time;
            while (Time.time - startTime < 5f) // Collect data for 5 seconds
            {
                if (includeQuaternion) quaternionValuesList.Add(gloveController.GetQuaternionValues());
                if (includeFingers) fingerValuesList.Add(gloveController.GetFingerValues());
                yield return null; // Wait for the next frame
            }

            // Compute the mean of the collected values
            float[] meanQuaternion = ComputeMean(quaternionValuesList);
            float[] meanFingerValues = ComputeMean(fingerValuesList);

            // Add the new hand state
            HandState newHandState = new HandState(meanQuaternion, meanFingerValues, includeQuaternion, includeFingers);
            handStates.Add(newHandState);
            Debug.Log("Added new hand state with averaged values.");
        }
    }

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

    // Function to compare two float values with a threshold
    public static bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }
}
