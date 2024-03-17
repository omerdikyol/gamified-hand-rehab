using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    public bool isBreaking;

    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private void FixedUpdate() {
        if (!InputController.isAwaitingInput)
        {
            // GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();

            // Reset rotation
            if (Input.GetKey(KeyCode.R)) {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void GetInput() {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);

        // Reset rotation
        if (Input.GetKey(KeyCode.R)) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

     // Public methods to be called from InputController based on hand gestures
    public void ProcessControlStates(bool shouldAccelerate, bool shouldBrake, bool shouldSteerLeft, bool shouldSteerRight)
    {
        if (shouldAccelerate)
        {
            ReleaseBrake();
            Accelerate();
        }
        else if (shouldBrake)
        {
            Brake();
        }
        else
        {
            // Neutral throttle control, if necessary
            ReleaseBrake();
        }

        if (shouldSteerLeft)
        {
            Steer(-0.1f);
        }
        else if (shouldSteerRight)
        {
            Steer(0.1f);
        }
        else
        {
            SteerStraight();
        }
    }
    public void Accelerate() {
        verticalInput = 1;
    }

    public void Decelerate() {
        verticalInput = -1;
    }

    public void Brake() {
        isBreaking = true;
    }

    public void ReleaseBrake() {
        isBreaking = false;
    }

    public void Steer(float value) {
        horizontalInput = value;
    }

    public void SteerStraight() {
        horizontalInput = 0;
    }

    private void HandleMotor() {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering() {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}