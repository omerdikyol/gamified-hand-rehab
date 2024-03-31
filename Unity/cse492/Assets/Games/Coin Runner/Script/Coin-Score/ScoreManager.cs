using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int coinCount = 0;
    private int score = 0;
    private float scoreSpeed = 0;
    private float scoreCounter;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinText;
    public static ScoreManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Start()
    {
        IncreaseCoin(0);
        scoreText.text = score.ToString("D6");
    }
    private void Update()
    {
        scoreCounter += Time.deltaTime * scoreSpeed;
        score = (int)scoreCounter;
        scoreText.text = score.ToString("D6");
        HighScoreManager.SaveHighScore(score);
    }
    public void IncreaseCoin(int value)
    {
        coinCount += value;
        coinText.text = coinCount.ToString();
    }
    public void ChangeScoreSpeed(float newSpeed)
    {
        scoreSpeed = newSpeed;
    }
    public void ChangeScorePlus(float score)
    {
        scoreCounter += score;
    }
    public void ScoreColorChange()
    {
        scoreText.color = Color.green;
    }
}
