using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDifficultyManager : MonoBehaviour
{
    [SerializeField] private float maxRunSpeed = 27f;
    [SerializeField] private float startSpeed = 10f;
    [SerializeField] private float speedAdd = 1.5f;
    [SerializeField] private float difficultyIncreaseTime = 10f;
    [SerializeField] private float nextDifficultyTimeAdd = 7f;
    private float timeCounter;
    private bool limitReached = false;
    public float speed;
    ChunkSpawnerManager chunkSpawnerManager;
    private void Start()
    {
        speed = startSpeed;
        chunkSpawnerManager = GetComponent<ChunkSpawnerManager>();
        chunkSpawnerManager.ChangeRunSpeed(speed);
        ScoreManager.Instance.ChangeScoreSpeed(speed);
    }

    private void Update()
    {
        if (limitReached) return;
        timeCounter += Time.deltaTime;
        if (timeCounter > difficultyIncreaseTime)
        {
            timeCounter = 0;
            difficultyIncreaseTime += nextDifficultyTimeAdd;
            speed += speedAdd;
            float newSpeed = Mathf.Min(maxRunSpeed, speed);
            if (newSpeed == maxRunSpeed)
                limitReached = true;
            chunkSpawnerManager.ChangeRunSpeed(newSpeed);
            ScoreManager.Instance.ChangeScoreSpeed(speed);
        }
    }
}
