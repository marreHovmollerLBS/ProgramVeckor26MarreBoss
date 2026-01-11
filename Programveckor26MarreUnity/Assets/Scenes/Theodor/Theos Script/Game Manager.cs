using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    bool isGameActive = false;
    bool isGoodDream = true;

    [SerializeField] private int goodDreamTime;
    [SerializeField] private int badDreamTime;
    [SerializeField] private int countDownTime;

    private float currentTime = 0;
    private int startTime;

    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private RectTransform sliderTransform;
    [SerializeField] private Image sliderImgage;

    private GameObject[] doors = new GameObject[6];

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            doors[i] = transform.GetChild(i).gameObject;
        }
        sliderImgage.color = Color.white;
    }

    void Update()
    {
        if(!isGameActive) { StartTimer(); return; }
        currentTime += Time.deltaTime;
        if (isGoodDream)
        {
            sliderTransform.localScale = new Vector2(currentTime / goodDreamTime, sliderTransform.localScale.y);
            if(currentTime >= goodDreamTime)
            {
                isGoodDream = false;
                currentTime = 0;
                sliderImgage.color = Color.red;
            }
        }
        else
        {
            sliderTransform.localScale = new Vector2(currentTime / badDreamTime, sliderTransform.localScale.y);
            if (currentTime >= badDreamTime)
            {
                isGoodDream = true;
                currentTime = 0;
                sliderImgage.color = Color.white;
            }
        }
    }

    void StartTimer()
    {
        startTime = (int)Time.time;
        countDownText.text = (countDownTime - startTime).ToString();
        if(startTime >= countDownTime)
        {
            countDownText.text = "";
            isGameActive = true;
        }
    }
}
