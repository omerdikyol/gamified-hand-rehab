using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManagerSpaceShooter : MonoBehaviour
{
    public DeathMenu deathMenu;
    public static int score;
    private bool isDead = false;
    public TextMeshProUGUI text;

    void Awake()
    {
        score = 0;
    }

    void Update()
    {
        text.text = "Score: " + score;

        if(isDead == true)
        {
            return;
        }
    }

    public void Death()
    {
        isDead = true; 
        deathMenu.ToggleEndMenu(score);
    }
}
