using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private CharacterManager manager;

    bool isGameActive = false;
    bool isGoodDream = true;
    int dreamCount = 0;

    [Header("Timers")]
    [SerializeField] private int goodDreamTime;
    [SerializeField] private int badDreamTime;
    [SerializeField] private int countDownTime;

    private float currentTime = 0;
    private int startTime;

    [Header("Ui")]
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private RectTransform sliderTransform;
    [SerializeField] private Image sliderImage;

    [Header("Cinemachine")]
    [SerializeField] CinemachineCamera cinemachineCamera;

    [Header("Round Manager")]
    [SerializeField] private RoundManager roundManager;

    [Header("Children")]
    public Transform doorsParent;

    private List<GameObject> doors = new List<GameObject>();

    private void Start()
    {
        manager = GetComponent<CharacterManager>();

        // Spawn player
        Player player = manager.SpawnPlayer(Vector3.zero, speed: 4f, health: 100f, damage: 10f, size: 0.5f);

        cinemachineCamera.Follow = player.transform;

        //Find all doors
        for (int i = 0; i < doorsParent.childCount; i++)
        {
            doors.Add(doorsParent.GetChild(i).gameObject);
        }
        
        sliderImage.color = Color.white;
    }

    void Update()
    {
        //Start timer
        if (!isGameActive) { StartTimer(); return; }

        //Dream switching
        currentTime += Time.deltaTime;
        if (isGoodDream)
        {
            sliderTransform.localScale = new Vector2(currentTime / goodDreamTime, sliderTransform.localScale.y);
            if (currentTime >= goodDreamTime)
            {
                isGoodDream = false;
                currentTime = 0;
                sliderImage.color = Color.red;
                dreamCount++;
                SpawnEnemies();
                manager.SetDreamState(isGoodDream);
            }
        }
        else
        {
            sliderTransform.localScale = new Vector2(currentTime / badDreamTime, sliderTransform.localScale.y);
            if (currentTime >= badDreamTime)
            {
                isGoodDream = true;
                currentTime = 0;
                sliderImage.color = Color.white;
                dreamCount++;
                SpawnEnemies();
                manager.SetDreamState(isGoodDream);
            }
        }

    }
    
    void StartTimer()
    {
        startTime = (int)Time.time;
        countDownText.text = (countDownTime - startTime).ToString();
        if (startTime >= countDownTime)
        {
            countDownText.text = "";
            isGameActive = true;
            SpawnEnemies();
        }
    }

    void SpawnEnemies()
    {
        Round round = roundManager.rounds[dreamCount];
        int[] usedDoors = new int[doors.Count];

        foreach (EnemySpawnData enemyData in round.enemies)
        {
            Debug.Log($"Enemy Type: {enemyData.enemyType}, Count: {enemyData.spawnCount}");

            for (int i = 0; i < enemyData.spawnCount; i++)
            {
                int randomDoorNumber = Random.Range(0, doors.Count - 1);
                usedDoors[i] = randomDoorNumber;
                Vector3 randomDoorPos = doors[randomDoorNumber].transform.position;

                if (enemyData.enemyType == EnemyType.BasicEnemy)
                {
                    manager.SpawnDefaultEnemy(randomDoorPos);
                }
            }
        }
    }
}