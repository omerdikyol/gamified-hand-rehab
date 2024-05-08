using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalibrationScene : MonoBehaviour
{
    public Button startButton;
    private bool isHandFound = false;
    private bool isPopUpAppeared = false;
    public GameObject calibrationFinishedPopUp;
    public TMP_Text calibrationFinishedPopUpText;
    private GloveController gloveController;

    int[] fingerMaxDegrees = new int[] {60, 90, 90, 90, 90}; // Maximum range of motion for each finger


    // Start is called before the first frame update
    void Start()
    {
        if (!isHandFound)
        {
            Debug.Log("Hand not found");
            startButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isHandFound)
        {
            // If Hand object is found, set startButton onClick event to call Hand's StartCalibration method
            if (GameObject.FindWithTag("Hand")) {
                Debug.Log("Hand found");
                startButton.onClick.AddListener(GameObject.FindWithTag("Hand").GetComponent<GloveController>().StartCalibration); // Add StartCalibration method as onClick event
                startButton.onClick.AddListener(GameObject.FindWithTag("FingerAngleCollector").GetComponent<FingerAngleCollector>().ToggleLogging); // Add ToggleLogging method as onClick event
                isHandFound = true;
                startButton.interactable = true;
                gloveController = GameObject.FindWithTag("Hand").GetComponent<GloveController>();
            }
        }

        if (gloveController != null && gloveController.isCalibrated && !isPopUpAppeared)
        {
            float[] fingerMinValuesOfUser = gloveController.GetFingerMinValues();
            float[] fingerMaxValuesOfUser = gloveController.GetFingerMaxValues();
            float[] fingerMinValuesHealty = gloveController.GetFingerMinValuesHealthy();
            float[] fingerMaxValuesHealty = gloveController.GetFingerMaxValuesHealthy();

            float[] userFingerAngles = CalculateFingerAngles(fingerMinValuesOfUser, fingerMaxValuesOfUser, fingerMinValuesHealty, fingerMaxValuesHealty);
            calibrationFinishedPopUp.SetActive(true);
            calibrationFinishedPopUpText.text = "Calibration finished! Your Range of Motion for each Finger are:\n\n" +
                $"Thumb: {userFingerAngles[0]:F2}°\n" +
                $"Index: {userFingerAngles[1]:F2}°\n" +
                $"Middle: {userFingerAngles[2]:F2}°\n" +
                $"Ring: {userFingerAngles[3]:F2}°\n" +
                $"Pinky: {userFingerAngles[4]:F2}°";
            
            isPopUpAppeared = true;
        }
    }

    public void CloseCalibrationFinishedPopUp()
    {
        calibrationFinishedPopUp.SetActive(false);
    }

    
    // Calculate the finger angles based on the user's and healthy values
    float[] CalculateFingerAngles(float[] userMin, float[] userMax, float[] healthyMin, float[] healthyMax)
    {
        float[] fingerAngles = new float[5];
        for (int i = 0; i < 5; i++)
        {
            float healthyDifference = healthyMax[i] - healthyMin[i];
            float userDifference = userMax[i] - userMin[i];

            fingerAngles[i] = (userDifference / healthyDifference) * fingerMaxDegrees[i];

            if (fingerAngles[i] > fingerMaxDegrees[i])
            {
                fingerAngles[i] = fingerMaxDegrees[i];
            }
            if (fingerAngles[i] < 0)
            {
                fingerAngles[i] = 0;
            }
        }
        return fingerAngles;
    }

    public void SetIsPopUpAppeared(bool value)
    {
        isPopUpAppeared = value;
    }
}
