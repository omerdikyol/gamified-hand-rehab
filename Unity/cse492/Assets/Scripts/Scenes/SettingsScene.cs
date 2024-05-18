using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingsScene : MonoBehaviour
{
    public SettingsManager settingsManager;
    public GloveController gloveController;

    public Toggle rotationToggle;
    public Toggle reverseToggle;
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (settingsManager == null)
        {
            try
            {
                settingsManager = GameObject.Find("SettingsManager").GetComponent<SettingsManager>();
                rotationToggle.isOn = settingsManager.isRotationEnabled;
                rotationToggle.onValueChanged.AddListener((value) => settingsManager.ToggleRotation());
            }
            catch (System.Exception)
            {
            }
        }
        if (gloveController == null)
        {
            try
            {
                gloveController = GameObject.Find("HandRight").GetComponent<GloveController>();
                reverseToggle.isOn = gloveController.isAnglesReversed;
                reverseToggle.onValueChanged.AddListener((value) => gloveController.ToggleAnglesReversed());
            }
            catch (System.Exception)
            {
            }
        }
    }

    public void SetLanguage(string localeCode)
    {
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }
    }
}
