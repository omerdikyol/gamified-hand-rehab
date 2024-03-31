using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveObstacles : MonoBehaviour
{
    public Vector3 targetPosition;
    GameDifficultyManager gameDifficultyManager;
    private float speed = 0f;

    private void Start()
    {
        gameDifficultyManager = transform.root.GetComponent<GameDifficultyManager>();
        ChangeRunSpeed(gameDifficultyManager.speed);
    }
    private void Update()
    {
        ChangeRunSpeed(gameDifficultyManager.speed);
    }

    private void OnEnable()
    {
        transform.localPosition = targetPosition;
    }
    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (transform.parent.transform.position.z < 60)
        {
            rb.MovePosition(transform.position + Vector3.back * speed * Time.fixedDeltaTime);
        }
    }

    private void ChangeRunSpeed(float newSpeed)
    {
        speed = newSpeed + 10;
    }
}
