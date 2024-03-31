using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamesListScene : MonoBehaviour
{
    private bool isHandCalibrated = false;

    [SerializeField] private Button[] gameButtons;

    // Start is called before the first frame update
    void Start()
    {
        // Disable all game buttons
        foreach (Button button in gameButtons)
        {
            button.interactable = false;
        }

        if (!isHandCalibrated)
        {
            Debug.Log("Hand not calibrated");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isHandCalibrated)
        {
            // If Hand object is found, set startButton onClick event to call Hand's StartCalibration method
            if (GameObject.FindWithTag("Hand").GetComponent<GloveController>().GetIsCalibrated()) {
                Debug.Log("Hand calibrated");
                isHandCalibrated = true;
                
                // Enable all game buttons
                foreach (Button button in gameButtons)
                {
                    button.interactable = true;
                }
            }
        }
    }
}
