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

    public float timeBetweenInputs = 0.5f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        direction.y += gravity * Time.deltaTime;
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

    // private void FixedUpdate()
    // {
    //     controller.Move(direction * Time.deltaTime);
    //     PlayerMove();
    // }

    private float lastInputTime;

    private void FixedUpdate()
    {
        controller.Move(direction * Time.deltaTime);

        if (Time.time - lastInputTime >= timeBetweenInputs)
        {
            PlayerMove();
            lastInputTime = Time.time;
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
        direction.y = jumpForce;
        animator.SetTrigger("Jump");
    }

    public void Roll()
    {
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