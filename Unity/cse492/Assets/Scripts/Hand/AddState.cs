using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Make sure to include this for UI elements
using TMPro;

public class AddState : MonoBehaviour
{
    [Header("References")]
    // Reference to the GloveController script
    public GloveController gloveController;

    // Reference to the HandStateUIManager script
    public HandStateUIManager handStateUIManager;
    public InputController inputController;
    public GameObject attributesUI; 

    [Header("UI Elements")]
    public TMP_InputField stateNameInputField;
    public Button submitAttributesButton;

    [Header("Toggles")]
    public Toggle quaternionToggle, fingerToggle, qxToggle, qyToggle, qzToggle, qwToggle;

    // Variables to store the values of the toggles
    public bool quaternion, finger;
    public int quaternionComponents;
    public string stateName;
    private string filePath;

    // Start is called before the first frame update
    void Start()
    {
        if (inputController == null)
        {
            inputController = FindObjectOfType<InputController>();
        }

        if (gloveController == null)
        {
            gloveController = FindObjectOfType<GloveController>();
        }

        if (handStateUIManager == null)
        {
            handStateUIManager = FindObjectOfType<HandStateUIManager>();
        }

        quaternion = quaternionToggle.isOn;
        finger = fingerToggle.isOn;
        quaternionComponents = (qxToggle.isOn ? 1 : 0) | (qyToggle.isOn ? 2 : 0) | (qzToggle.isOn ? 4 : 0) | (qwToggle.isOn ? 8 : 0);
        stateNameInputField.text = "";
        stateName = stateNameInputField.text;

        // If object is active
        if (attributesUI.activeSelf)
        {
            // If the user presses the enter key
            quaternionToggle.onValueChanged.AddListener(delegate { ToggleChanged(quaternionToggle); });
            fingerToggle.onValueChanged.AddListener(delegate { ToggleChanged(fingerToggle); });
            qxToggle.onValueChanged.AddListener(delegate { ToggleChanged(qxToggle); });
            qyToggle.onValueChanged.AddListener(delegate { ToggleChanged(qyToggle); });
            qzToggle.onValueChanged.AddListener(delegate { ToggleChanged(qzToggle); });
            qwToggle.onValueChanged.AddListener(delegate { ToggleChanged(qwToggle); });

            stateNameInputField.onValueChanged.AddListener(delegate { stateName = stateNameInputField.text; Debug.Log("State Name: " + stateName);});
        }

        // handStateCollection = new HandStateCollection();
        
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
        
    }

    void ToggleChanged(Toggle toggle)
    {
        if (toggle == quaternionToggle)
        {
        quaternion = toggle.isOn;
        }
        else if (toggle == fingerToggle)
        {
        finger = toggle.isOn;
        }
        else if (toggle == qxToggle)
        {
        quaternionComponents = toggle.isOn ? quaternionComponents | 1 : quaternionComponents & ~1;
        }
        else if (toggle == qyToggle)
        {
        quaternionComponents = toggle.isOn ? quaternionComponents | 2 : quaternionComponents & ~2;
        }
        else if (toggle == qzToggle)
        {
        quaternionComponents = toggle.isOn ? quaternionComponents | 4 : quaternionComponents & ~4;
        }
        else if (toggle == qwToggle)
        {
        quaternionComponents = toggle.isOn ? quaternionComponents | 8 : quaternionComponents & ~8;
        }

        Debug.Log("Quaternion: " + quaternion);
        Debug.Log("Finger: " + finger);
        Debug.Log("Quaternion Components: " + quaternionComponents);
    }

    private IEnumerator CollectAndAddHandState(bool includeQuaternion, bool includeFingers, int quaternionComponents, string stateName)
    {
        yield return new WaitUntil(() => gloveController.GetIsCalibrated());  // Ensure glove is calibrated

        // Debug the values
        // Debug.Log("InputController.cs: CollectAndAddHandState() called!");
        // Debug.Log("Quaternion: " + includeQuaternion);
        // Debug.Log("Finger: " + includeFingers);
        // Debug.Log("Quaternion Components: " + quaternionComponents);
        // Debug.Log("State Name: " + stateName);

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

    public void HandleAttributesSubmitted()
    {
        
        StartCoroutine(CollectAndAddHandState(quaternion, finger, quaternionComponents, stateName));
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
        string json = JsonUtility.ToJson(InputController.handStateCollection, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Hand states saved to " + filePath);
    }

    private void LoadHandStates()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            InputController.handStateCollection = JsonUtility.FromJson<HandStateCollection>(json);
            if (InputController.handStateCollection == null) // Check if deserialization failed
            {
                InputController.handStateCollection = new HandStateCollection(); // Initialize to prevent null reference
            }
        }
        else
        {
            InputController.handStateCollection = new HandStateCollection(); // Initialize to prevent null reference
            Debug.Log("No saved hand states found.");
        }
    }

    private void AddHandState(HandState handState)
    {
        InputController.handStateCollection.handStates.Add(handState);
        SaveHandStates(); // Save every time a new state is added
    }
}
