using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalibrationScene : MonoBehaviour
{
    public Button startButton;
    private bool isHandFound = false;


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
            }
        }
    }
}
