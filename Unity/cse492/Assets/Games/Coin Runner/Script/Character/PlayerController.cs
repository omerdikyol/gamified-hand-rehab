using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private Vector3 direction;
    public int desiredLane = 1;
    public float laneDistance = 4;
    public float jumpForce;
    public float gravity = -20;
    private Touch touch;
    private Vector2 initPos;
    private Vector2 endPos;
    private int collisionCount = 3;
    public GameObject characterMesh;
    public GameManagerCoinRunner gameManager;

    public float timeBetweenInputs = 2.0f; // Cooldown time in seconds between movements
    public float lastInputTime = -2.0f; // Initialize with -2.0 so that player can move immediately at start
    public bool canMove = true;
    public bool isChangingLanes = false;
    private Coroutine currentLaneChangeCoroutine = null; // To manage lane change coroutine


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        direction.y += gravity * Time.deltaTime;
        if (Time.time - lastInputTime > timeBetweenInputs)
        {
            canMove = true;
            // PlayerMove();
        }

    }

    private void FixedUpdate()
    {
        if (!isChangingLanes)
        {
            Vector3 moveVector = new Vector3(0, direction.y, direction.z);
            controller.Move(moveVector * Time.deltaTime);
        }
    }

    private void PlayerMove()
    {
        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;

        if (desiredLane == 0)
            targetPosition += Vector3.left * laneDistance;

        else if (desiredLane == 2)
            targetPosition += Vector3.right * laneDistance;

        transform.position = targetPosition;
    }

    public bool IsGrounded()
    {
        return controller.isGrounded;
    }

    public void Jump()
    {
        if (!canMove || !IsGrounded()) {return;}
        canMove = false;
        lastInputTime = Time.time;

        direction.y = jumpForce;
        animator.SetTrigger("Jump");
    }

    public void Roll()
    {
        if (!canMove) return;
        canMove = false;
        lastInputTime = Time.time;

        if (!IsGrounded())
        {
            direction.y = -jumpForce;
        }
        animator.SetTrigger("Roll");
        controller.height = 0f;
        controller.center = new Vector3(controller.center.x, 0.34f, controller.center.z);
    }

    public void MoveLane(int direction)
    {
        // if (!canMove || isChangingLanes) return;
        if (!canMove) return;
        int newLane = desiredLane + direction;
        if (newLane < 0 || newLane > 2) return; // Prevent invalid lane changes

        if (currentLaneChangeCoroutine != null)
        {
            StopCoroutine(currentLaneChangeCoroutine); // Stop the current coroutine if it's running
        }
        
        canMove = false;
        isChangingLanes = true;
        lastInputTime = Time.time;

        desiredLane += direction;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);

        animator.SetTrigger(direction == 1 ? "Right" : "Left");

        currentLaneChangeCoroutine = StartCoroutine(MoveLaneCoroutine(direction));
    }

    private IEnumerator MoveLaneCoroutine(int direction)
    {
        if (canMove)
        {
            canMove = false;
            lastInputTime = Time.time;
        }

        Vector3 targetPosition = controller.transform.position + direction * laneDistance * Vector3.right;

        while (controller.transform.position.x != targetPosition.x)
        {
            Vector3 newPosition = Vector3.MoveTowards(controller.transform.position, targetPosition, 10 * Time.deltaTime);
            controller.Move(newPosition - controller.transform.position);
            yield return null;
        }
        isChangingLanes = false;
        currentLaneChangeCoroutine = null; // Reset coroutine tracker
    }

    public void OnRollAnimationFinished()
    {
        controller.center = new Vector3(controller.center.x, 0.84f, controller.center.z);
        controller.height = 1.83f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (collisionCount > 0)
            {
                StartCoroutine(BlinkAndInvincible(2.0f));
                collisionCount--;
                Debug.Log("Remaining collision count: " + collisionCount);
            }
            else
            {
                gameManager.EndGame();
                Debug.Log("Game over!");
            }
        }
    }

    private IEnumerator BlinkAndInvincible(float duration)
    {
        float elapsedTime = 0f;
        bool isVisible = true;

        while (elapsedTime < duration)
        {
            isVisible = !isVisible;
            characterMesh.SetActive(isVisible);
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        characterMesh.SetActive(true);
    }
}