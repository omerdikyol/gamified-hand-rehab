using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public float TimeLeft;
    public bool TimerOn = false;
    private TextMeshProUGUI TimerText;

    void Start()
    {
        TimerText = GetComponent<TextMeshProUGUI>();    
    }

    void Update()
    {
        if (TimerOn)
        {
            TimeLeft -= Time.deltaTime;
            TimerText.text = TimeLeft.ToString("F2");
            if (TimeLeft < 0)
            {
                TimerOn = false;
                TimerText.text = "0.00";
            }
        }
    }

    
    public void StartTimer()
    {
        TimerOn = true;
    }

    public void StopTimer()
    {
        TimerOn = false;
    }

    public void SetTime(float time)
    {
        TimeLeft = time;
        StartTimer();
    }
}
