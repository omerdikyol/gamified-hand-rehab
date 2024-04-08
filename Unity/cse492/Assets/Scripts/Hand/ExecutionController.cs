using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExecutionController : MonoBehaviour
{
    public int currentSceneIndex;

    // Start is called before the first frame update
    void Start()
    {
        // Get the current scene index
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        switch (currentSceneIndex)
        {
            case 0:
                carController = FindObjectOfType<CarController>();
                break;
            case 3:
                birdJumpPlayer = FindObjectOfType<Player>();
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Execute scene action based on the matched hand state name
    public void ExecuteSceneAction(string stateName)
    {
        switch (currentSceneIndex)
        {
            case 0:
                ExecuteCarControl(stateName);
                break;
            case 3:
                ExecuteBirdJumpControl(stateName);
                break;
            default:
                break;
        }
    }

    //** Car Scene **//
    // Flags to track control states - could also be part of a dedicated control state object
    private CarController carController;
    private bool shouldAccelerate = false;
    private bool shouldBrake = false;
    private bool shouldSteerLeft = false;
    private bool shouldSteerRight = false;

    // Execute car control based on the matched hand state name
    private void ExecuteCarControl(string stateName)
    {
        // Reset control flags
        shouldAccelerate = false;
        shouldBrake = false;
        shouldSteerLeft = false;
        shouldSteerRight = false;

        // Set flags based on recognized hand states
        // Note: This assumes that CheckHandState and related logic can set multiple flags per frame as needed
        if (stateName == "OpenHandFingersOnly") shouldAccelerate = true;
        if (stateName == "FistFingersOnly") shouldBrake = true;
        if (stateName == "AlignLeft") shouldSteerLeft = true;
        if (stateName == "AlignRight") shouldSteerRight = true;

        
        if (carController != null)
        {
            carController.ProcessControlStates(shouldAccelerate, shouldBrake, shouldSteerLeft, shouldSteerRight);
        }
    }

    //** Bird Jump **//

    private Player birdJumpPlayer;

    // Execute bird jump control based on the matched hand state name
    private void ExecuteBirdJumpControl(string stateName)
    {
        if (birdJumpPlayer != null)
        {
            if (stateName == "OpenHandFingersOnly")
            {
                birdJumpPlayer.Jump();
            }
        }
    }
}
