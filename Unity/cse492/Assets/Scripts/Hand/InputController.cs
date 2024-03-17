using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    // Reference to the GloveController script
    public GloveController gloveController;

    // Reference to the HandStateUIManager script
    public HandStateUIManager handStateUIManager;

    // Reference to the ExecutionController script
    public ExecutionController executionController;

    // Variable to keep track of whether the system is awaiting input
    public static bool isAwaitingInput = false;

    // List to store predefined hand states
    private List<HandState> handStates = new List<HandState>();
    public float quaternionThreshold = 0.05f; // Threshold for comparing quaternion values
    public float fingerThreshold = 15f; // Threshold for comparing finger values
    public float minMaxThreshold = 10f; // Threshold for checking if a finger value is near the min or max value

    private const int IncludeQx = 1; // 0001 in binary
    private const int IncludeQy = 2; // 0010 in binary
    private const int IncludeQz = 4; // 0100 in binary
    private const int IncludeQw = 8; // 1000 in binary

    private HandStateCollection handStateCollection;
    private string filePath;

    private float checkInterval = 0.1f; // Check every 0.1 seconds
    private float nextCheckTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Add predefined hand states and their associated values to the list (example values)
        // int includedComponents = IncludeQx | IncludeQz; // Assuming constants are defined as before
        // AddHandState(new HandState(new float[] {0, 0.01f, 0, -0.17f}, new float[] { 604, 402, 359, 441, 520 }, true, true, includedComponents));

        handStateCollection = new HandStateCollection();
        
        filePath = Path.Combine(Application.persistentDataPath, "handStates.json");
        // If file does not exist, create a new one
        if (!File.Exists(filePath))
        {
            File.CreateText(filePath);
        }
        else 
        {
            LoadHandStates();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAwaitingInput)
        {
            // Check the current hand state
            if (Time.time >= nextCheckTime) 
            {
                CheckHandState();
                nextCheckTime = Time.time + checkInterval;
            }

            // Collect and add a hand state to the list
            if (Input.GetKeyDown(KeyCode.Alpha1)) //
            {
                StartCoroutine(CollectAndAddHandState()); 
            }
    

            if (Input.GetKeyDown(KeyCode.L)) // Load hand states from file and print them
            {
                LoadHandStates();
                foreach (HandState state in handStateCollection.handStates)
                {
                    Debug.Log("Quaternion: ");
                    for (int i = 0; i < state.quaternionValues.Length; i++)
                    {
                        Debug.Log(state.quaternionValues[i]);
                    }
                    Debug.Log("Fingers: ");
                    for (int i = 0; i < state.fingerValues.Length; i++)
                    {
                        Debug.Log(state.fingerValues[i]);
                    }
                    Debug.Log("Includes Quaternion: " + state.includesQuaternion);
                    Debug.Log("Includes Fingers: " + state.includesFingers);
                    Debug.Log("Included Quaternion Components: " + state.includedQuaternionComponents);
                }
            }
        }
    }

    // Function to add a hand state to the list 
    private void AddHandState(HandState handState)
    {
        handStateCollection.handStates.Add(handState);
        SaveHandStates(); // Save every time a new state is added
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
            HandState currentHandState = new HandState("", handStateValues, fingerValues, true, true);

            // Check if the current hand state matches any of the predefined states
            foreach (HandState state in handStateCollection.handStates)
            {
                if (IsMatchingState(currentHandState, state))
                {
                    // Perform action for the matched hand state
                    // Debug.Log("Match with State named: " + state.name);
                    handStateUIManager.SetCurrentStateName("Current State: " + state.name);

                    // **New Step: Execute car control based on the matched hand state name**
                    executionController.ExecuteSceneAction(state.name);
                    break;
                }
            }
        }
    }

    // Function to compare two hand states
    private bool IsMatchingState(HandState currentState, HandState predefinedState)
    {
        // Compare quaternion values if included
        if (predefinedState.includesQuaternion)
        {
            for (int i = 0; i < currentState.quaternionValues.Length; i++)
            {
                // Check if this component should be considered
                if ((predefinedState.includedQuaternionComponents & (1 << i)) != 0)
                {
                    if (!FastApproximately(currentState.quaternionValues[i], predefinedState.quaternionValues[i], quaternionThreshold))
                    {
                        return false; // This quaternion component does not match
                    }
                }
            }
        }

        // Compare finger values if the predefined state includes finger values
        if (predefinedState.includesFingers)
        {
            for (int i = 0; i < currentState.fingerValues.Length; i++)
            {
                // The comparison logic for finger values is adjusted to account for values beyond max/min thresholds
                bool isCurrentNearMax = FastApproximately(currentState.fingerValues[i], 100, fingerThreshold);
                bool isCurrentNearMin = FastApproximately(currentState.fingerValues[i], 0, fingerThreshold);
                bool isPredefinedNearMax = FastApproximately(predefinedState.fingerValues[i], 100, fingerThreshold);
                bool isPredefinedNearMin = FastApproximately(predefinedState.fingerValues[i], 0, fingerThreshold);

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

    // Coroutine to collect and add a hand state to the list
    private IEnumerator CollectAndAddHandState()
    {
        Debug.Log("Getting attributes from the user for new state.");

        bool isAttributesEntered = false;

        bool includeQuaternion = false;
        bool includeFingers = false;
        int quaternionComponents = 0;
        string stateName = "";

        // Pause other game logic
        isAwaitingInput = true;

        handStateUIManager.ShowAttributesInputUI((incQ, incF, incQx, incQy, incQz, incQw, stName) => 
        {
            includeQuaternion = incQ;
            includeFingers = incF;
            quaternionComponents = (incQx ? IncludeQx : 0) | (incQy ? IncludeQy : 0) | (incQz ? IncludeQz : 0) | (incQw ? IncludeQw : 0);
            stateName = stName;
            isAttributesEntered = true;
            isAwaitingInput = false; // Resume other game logic
        });

        // Wait for the user to enter the attributes and submit them
        yield return new WaitUntil(() => isAttributesEntered);

        Debug.Log("Coroutine started. Collecting hand state for 5 seconds.");

        if (gloveController != null && gloveController.GetIsCalibrated())
        {
            List<float[]> quaternionValuesList = new List<float[]>();
            List<float[]> fingerValuesList = new List<float[]>();

            float startTime = Time.time;
            float endTime = startTime + 5f; // Collect data for 5 seconds

            while (Time.time < endTime)
            {
                if (includeQuaternion)
                {
                    float[] currentQuaternionValues = gloveController.GetQuaternionValues();
                    float[] selectedQuaternionValues = new float[4]; // Always size 4 for quaternion values

                    // Selecting specific quaternion components based on the quaternionComponents parameter
                    for (int i = 0; i < 4; i++)
                    {
                        if ((quaternionComponents & (1 << i)) != 0)
                        {
                            selectedQuaternionValues[i] = currentQuaternionValues[i];
                        }
                        else
                        {
                            selectedQuaternionValues[i] = 0; // Zero out non-selected components
                        }
                    }

                    quaternionValuesList.Add(selectedQuaternionValues);
                }
                if (includeFingers)
                {
                    fingerValuesList.Add(gloveController.GetFingerValues());
                }

                yield return null; // Wait for the next frame
            }

            // Compute mean quaternion and finger values from the collected data
            float[] meanQuaternion = includeQuaternion ? ComputeMean(quaternionValuesList) : new float[4]; // Initialize with size 4 for quaternion
            float[] meanFingerValues = includeFingers ? ComputeMean(fingerValuesList) : new float[0]; // Initialize empty for fingers if not included

            // Create a new HandState object and add it to the list
            HandState newHandState = new HandState(stateName, meanQuaternion, meanFingerValues, includeQuaternion, includeFingers, quaternionComponents);
            AddHandState(newHandState);

            Debug.Log($"Coroutine ended. Hand state named '{stateName}' added.");
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

    private void SaveHandStates()
    {
        string json = JsonUtility.ToJson(handStateCollection, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Hand states saved to " + filePath);
    }

    private void LoadHandStates()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            handStateCollection = JsonUtility.FromJson<HandStateCollection>(json);
            if (handStateCollection == null) // Check if deserialization failed
            {
                handStateCollection = new HandStateCollection(); // Initialize to prevent null reference
            }
            // Debug.Log("Hand states loaded from " + filePath);
            // Debug.Log("Number of hand states: " + handStateCollection.handStates.Count);
            // // Print the loaded hand states
            // foreach (HandState state in handStateCollection.handStates)
            // {
            //     Debug.Log("Name: " + state.name);
            //     Debug.Log("Quaternion: ");
            //     for (int i = 0; i < state.quaternionValues.Length; i++)
            //     {
            //         Debug.Log(state.quaternionValues[i]);
            //     }
            //     Debug.Log("Fingers: ");
            //     for (int i = 0; i < state.fingerValues.Length; i++)
            //     {
            //         Debug.Log(state.fingerValues[i]);
            //     }
            //     Debug.Log("Includes Quaternion: " + state.includesQuaternion);
            //     Debug.Log("Includes Fingers: " + state.includesFingers);
            //     Debug.Log("Included Quaternion Components: " + state.includedQuaternionComponents);
            // }
        }
        else
        {
            handStateCollection = new HandStateCollection(); // Initialize to prevent null reference
            Debug.Log("No saved hand states found.");
        }
    }
}
