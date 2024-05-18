using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }
    private GameObject handModel;
    public bool isRotationEnabled = true;
    public Slider volumeSlider;
    public Button enButton;
    public Button trButton;

    void Start()
    {
        // Set the hand model to the object with the tag "Hand"
        handModel = GameObject.FindWithTag("Hand");
    }

    // Update is called once per frame
    void Update()
    {
        if (volumeSlider == null)
        {
            try
            {
                volumeSlider = GameObject.Find("VolumeSlider").GetComponent<Slider>();
                volumeSlider.onValueChanged.AddListener(OnVolumeChange);
            }
            catch (System.Exception)
            {
            }
        }
        else
        {
            volumeSlider.value = AudioListener.volume;
        }
    }

    // Method to update the volume
    public void OnVolumeChange(float volume)
    {
        AudioListener.volume = volume;  // Set the global volume to the value of the slider
    }

    public void ToggleRotation()
    {
        isRotationEnabled = !isRotationEnabled;
        if (handModel != null && !isRotationEnabled)
        {
            handModel.transform.rotation = Quaternion.Euler(isRotationEnabled ? 0f : 70f, 182f, 0f);
        }
    }
}
