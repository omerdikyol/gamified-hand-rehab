using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionController : MonoBehaviour
{
    public int currentSceneIndex;

    // Start is called before the first frame update
    void Start()
    {
        if (currentSceneIndex == 0)
        {
            carController = FindObjectOfType<CarController>();
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
            default:
                break;
        }
    }

    //** Car Scene **//
    // Flags to track control states - could also be part of a dedicated control state object
    public CarController carController;
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
    //** Car Scene **//
}
