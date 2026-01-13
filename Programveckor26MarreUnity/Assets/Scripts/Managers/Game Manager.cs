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
    [SerializeField] private Image sliderImgage;

    [Header("Cinemachine")]
    [SerializeField] CinemachineCamera cinemachineCamera;

    [Header("Round Manager")]
    [SerializeField] private RoundManager roundManager;

    [Header("Spawn Patterns")]
    [SerializeField] private SpawnPatternsData spawnPatterns;

    [Header("Sound")]
    [SerializeField] private AudioSource goodDreamMusic;
    [SerializeField] private AudioSource badDreamMusic;

    private List<GameObject> doors = new List<GameObject>();

    private void Start()
    {
        manager = GetComponent<CharacterManager>();

        // Spawn player
        Player player = manager.SpawnPlayer(Vector3.zero, speed: 4f, health: 100f, damage: 10f, size: 0.5f);

        cinemachineCamera.Follow = player.transform;

        // Collect all doors
        for (int i = 0; i < transform.childCount; i++)
        {
            doors.Add(transform.GetChild(i).gameObject);
        }
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
                sliderImgage.color = Color.red;
                dreamCount++;
                SpawnEnemies();
                manager.SetDreamState(isGoodDream);

                //Play Evil Music
                goodDreamMusic.Stop();
                badDreamMusic.Play();
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
                dreamCount++;
                SpawnEnemies();
                manager.SetDreamState(isGoodDream);

                //Play Good Music
                badDreamMusic.Stop();
                goodDreamMusic.Play();
            }
        }
    }

    void StartTimer()
    {
        startTime = (int)Time.time;
        countDownText.text = (countDownTime - startTime).ToString();
        if (startTime >= countDownTime)
        {
            //Start Music after start timer
            goodDreamMusic.Play();

            countDownText.text = "";
            isGameActive = true;
            SpawnEnemies();
        }
    }

    void SpawnEnemies()
    {
        Round round = roundManager.rounds[dreamCount];

        foreach (EnemySpawnData enemyData in round.enemies)
        {
            Debug.Log($"Enemy Type: {enemyData.enemyType}, Count: {enemyData.spawnCount}");

            // Get the predetermined door indices for this spawn count
            int[] doorIndices = spawnPatterns.GetDoorIndices(enemyData.spawnCount);

            //Spawn Specific Enemy at predetermined doors
            for (int i = 0; i < enemyData.spawnCount && i < doorIndices.Length; i++)
            {
                int doorIndex = doorIndices[i];

                // Safety check
                if (doorIndex < 0 || doorIndex >= doors.Count)
                {
                    Debug.LogError($"Door index {doorIndex} is out of range! You have {doors.Count} doors.");
                    continue;
                }

                Vector3 doorPos = doors[doorIndex].transform.position;
                doorPos.z = 1;

                if (enemyData.enemyType == EnemyType.BasicEnemy)
                {
                    manager.SpawnDefaultEnemy(doorPos);
                }
            }
        }
    }
}