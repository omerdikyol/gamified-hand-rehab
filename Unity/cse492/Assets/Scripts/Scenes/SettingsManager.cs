using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    private GameObject handModel;

    public bool isRotationEnabled = true;

    public Slider volumeSlider;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Makes sure the object is not destroyed on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy the object if it is not the first instance
        }
    }

    void Start()
    {
        // Set the hand model to the object with the tag "Hand"
        handModel = GameObject.FindWithTag("Hand");

        if (volumeSlider == null)
        {
            Debug.LogError("Volume slider is not assigned in the inspector");
            return;
        }

        // Set the slider's value to the current volume level when the game starts
        volumeSlider.value = AudioListener.volume;

        // Add a listener to the slider to call the OnVolumeChange method whenever the value changes
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Method to update the volume
    public void OnVolumeChange(float volume)
    {
        AudioListener.volume = volume;  // Set the global volume to the value of the slider
    }

    void OnDestroy()
    {
        // Remove listener to avoid memory leaks
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChange);
    }

    public void ToggleRotation()
    {
        isRotationEnabled = !isRotationEnabled;
        if (handModel != null && !isRotationEnabled)
        {
            handModel.transform.rotation = Quaternion.Euler(isRotationEnabled ? 0f : 70f, 18f, 0f);
        }
    }
}
