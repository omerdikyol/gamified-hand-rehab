using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private Vector3 direction;
    private int desiredLane = 1;
    public float laneDistance = 4;
    public float jumpForce;
    public float gravity = -20;
    private Touch touch;
    private Vector2 initPos;
    private Vector2 endPos;
    private int collisionCount = 3;
    public GameObject characterMesh;
    public GameManagerCoinRunner gameManager;

    private const float MobileSwipeThreshold = 300;
    private const float MobileSwipeRightThreshold = 100;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        direction.y += gravity * Time.deltaTime;

        HandlePCInput();

        if (IsMobileInput())
        {
            HandleMobileInput();
        }

    }

    private void HandlePCInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && IsGrounded())
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Roll();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveLane(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLane(-1);
        }
    }

    private void HandleMobileInput()
    {
        touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            initPos = touch.position;
        }

        if (touch.phase == TouchPhase.Ended)
        {
            endPos = touch.position;

            float swipeDistanceY = endPos.y - initPos.y;
            float swipeDistanceX = endPos.x - initPos.x;

            if (swipeDistanceY > MobileSwipeThreshold && IsGrounded())
            {
                Jump();
            }
            else if (swipeDistanceY < -MobileSwipeThreshold)
            {
                Roll();
            }
            else if (swipeDistanceX > MobileSwipeRightThreshold)
            {
                MoveLane(1);
            }
            else if (swipeDistanceX < -MobileSwipeRightThreshold)
            {
                MoveLane(-1);
            }
        }
    }

    private void FixedUpdate()
    {
        controller.Move(direction * Time.deltaTime);
        PlayerMove();
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

    private bool IsGrounded()
    {
        return controller.isGrounded;
    }

    private void Jump()
    {
        direction.y = jumpForce;
        animator.SetTrigger("Jump");
    }

    private void Roll()
    {
        if (!IsGrounded())
        {
            direction.y = -jumpForce;
        }
        animator.SetTrigger("Roll");
        controller.height = 0f;
        controller.center = new Vector3(controller.center.x, 0.34f, controller.center.z);
    }

    private void MoveLane(int direction)
    {
        desiredLane += direction;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);

        if (direction == 1)
        {
            animator.SetTrigger("Right");
        }
        else
        {
            animator.SetTrigger("Left");
        }
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

    private bool IsMobileInput()
    {
        return Input.touchCount > 0;
    }
}