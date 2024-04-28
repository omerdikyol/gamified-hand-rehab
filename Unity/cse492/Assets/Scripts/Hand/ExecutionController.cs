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
        // Register the OnSceneLoaded method to be called when a scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Get the player controller component based on the current scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update the current scene index
        currentSceneIndex = scene.buildIndex;

        // Depending on the new scene, find and set up the necessary components
        switch (currentSceneIndex)
        {
            case 3:
                birdJumpPlayer = FindObjectOfType<Player>();
                break;
            case 4:
                playerController = FindObjectOfType<PlayerController>();
                break;
            case 6:
                playerControllerSpaceShooter = FindObjectOfType<PlayerControllerSpaceShooter>();
                break;
            default:
                break;
        }
    }

    // Execute scene action based on the matched hand state name
    public void ExecuteSceneAction(string stateName)
    {
        switch (currentSceneIndex)
        {
            case 3:
                ExecuteBirdJumpControl(stateName);
                break;
            case 4:
                ExecuteCoinRunnerControl(stateName);
                break;
            case 6:
                ExecuteSpaceShooterControl(stateName);
                break;
            default:
                break;
        }
    }

    //** Bird Jump **//

    private Player birdJumpPlayer;

    // Execute bird jump control based on the matched hand state name
    private void ExecuteBirdJumpControl(string stateName)
    {
        if (birdJumpPlayer != null)
        {
            if (stateName == "OpenHand")
            {
                birdJumpPlayer.Jump();
            }
        }
    }

    //** Coin Runner **//

    private PlayerController playerController;

    private void ExecuteCoinRunnerControl(string stateName)
    {
        if (playerController != null)
        {
            if (stateName == "OpenHand" && playerController.IsGrounded())
            {
                playerController.Jump();
            }
            else if (stateName == "4FingersExceptThumb")
            {
                playerController.Roll();
            }
            else if (stateName == "IndexFinger")
            {
                playerController.MoveLane(-1);
            }
            else if (stateName == "IndexAndMiddleFinger")
            {
                playerController.MoveLane(1);
            }
        }
    }

    //** Space Shooter **//

    private PlayerControllerSpaceShooter playerControllerSpaceShooter;

    private void ExecuteSpaceShooterControl(string stateName)
    {
        if (playerControllerSpaceShooter != null)
        {
            if (stateName == "Fist")
            {
                playerControllerSpaceShooter.Shoot();
            }
            else if (stateName == "IndexFinger")
            {
                playerControllerSpaceShooter.MoveUp();
            }
            else if (stateName == "IndexAndMiddleFinger")
            {
                playerControllerSpaceShooter.MoveDown();
            }
        }
    }
}
