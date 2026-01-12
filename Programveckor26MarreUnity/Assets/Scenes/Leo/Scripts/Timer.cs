using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Slider timerSlider;
    public Timer timerText;
    public float gameTime;

    private bool stopTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stopTimer = false;
        timerSlider.maxValue = gameTime;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
