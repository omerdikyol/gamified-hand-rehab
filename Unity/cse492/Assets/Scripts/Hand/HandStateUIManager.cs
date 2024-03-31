using UnityEngine;
using UnityEngine.UI; // Make sure to include this for UI elements
using TMPro;


public class HandStateUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject attributesUI;

    // UI elements
    [Header("UI Elements")]
    public TextMeshProUGUI stateNameText;
    public TMP_InputField stateNameInputField;
    public Button submitAttributesButton;

    [Header("Toggles")]
    public Toggle quaternionToggle;
    public Toggle fingerToggle;
    public Toggle qxToggle;
    public Toggle qyToggle;
    public Toggle qzToggle;
    public Toggle qwToggle;
    private void Start()
    {
        // Initially hide the UI
        SetInputUIVisibility(false);
    }

    public void ShowAttributesInputUI(System.Action<bool, bool, bool, bool, bool, bool, string> callback)
    {
        // Show the UI
        SetInputUIVisibility(true);

        // Get values from toggles
        bool quaternion = quaternionToggle.isOn;
        bool finger = fingerToggle.isOn;
        bool qx = qxToggle.isOn;
        bool qy = qyToggle.isOn;
        bool qz = qzToggle.isOn;
        bool qw = qwToggle.isOn;

        // Ensure the input field is empty and focused
        stateNameInputField.text = "";
        stateNameInputField.Select();
        stateNameInputField.ActivateInputField();

        // Listen for the submit button click
        submitAttributesButton.onClick.RemoveAllListeners(); // Remove existing listeners to avoid duplication
        submitAttributesButton.onClick.AddListener(() => SubmitAttributes(callback));
    }

    private void SubmitAttributes(System.Action<bool, bool, bool, bool, bool, bool, string> callback)
    {
        // Call the callback with the input field's text
        if(callback != null)
        {
            callback(quaternionToggle.isOn, fingerToggle.isOn, qxToggle.isOn, qyToggle.isOn, qzToggle.isOn, qwToggle.isOn, stateNameInputField.text);
        }

        // Hide the UI again
        SetInputUIVisibility(false);
    }
    public void SetInputUIVisibility(bool visible)
    {
        // Get all children of the input UI
        foreach (Transform child in attributesUI.transform)
        {
            child.gameObject.SetActive(visible);
        }

        // Set visibility of the background image
        attributesUI.SetActive(visible);
    }

    public void SetCurrentStateName(string stateName)
    {
        stateNameText.text = stateName;
    }
}
