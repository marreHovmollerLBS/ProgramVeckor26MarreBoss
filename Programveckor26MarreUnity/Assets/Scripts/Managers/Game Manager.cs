using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private CharacterManager manager;
    private ExitScript exitScript;

    bool isGameActive = false;
    bool isGoodDream = true;
    int dreamCount = 0;

    [Header("Timers")]
    [SerializeField] private int countDownTime;

    [HideInInspector] public float currentTime = 0;
    private int startTime;
    private float currentRoundDuration = 0; // Store the current round's duration

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

    [Header("Tileset")]
    public GameObject GrassWater;
    public GameObject Lava;

    private void Start()
    {
        manager = GetComponent<CharacterManager>();
        exitScript = GetComponent<ExitScript>();

        // Set initial dream state from RoundManager
        isGoodDream = roundManager.startWithGoodDream;
        manager.SetDreamState(isGoodDream);

        // Activate correct tileset at start
        GrassWater.SetActive(isGoodDream);
        Lava.SetActive(!isGoodDream);

        // Set initial UI based on starting dream state
        if (!isGoodDream)
        {
            sliderImgage.color = Color.red;
        }

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
            if (exitScript != null)
            {
                exitScript.IsGoodDream = true;
            }

            sliderTransform.localScale = new Vector2(currentTime / currentRoundDuration, sliderTransform.localScale.y);
            if (currentTime >= currentRoundDuration)
            {
                if (exitScript != null)
                {
                    exitScript.OnSwitchToBadDream();
                }

                isGoodDream = false;
                currentTime = 0;
                sliderImgage.color = Color.red;
                dreamCount++;

                // Update the duration for the new round
                UpdateRoundDuration();

                SpawnEnemies();
                manager.SetDreamState(isGoodDream);

                // Switch tileset
                GrassWater.SetActive(false);
                Lava.SetActive(true);

                //Play Evil Music
                goodDreamMusic.Stop();
                badDreamMusic.Play();
            }
        }
        else
        {
            if (exitScript != null)
            {
                exitScript.IsGoodDream = false;
            }

            sliderTransform.localScale = new Vector2(currentTime / currentRoundDuration, sliderTransform.localScale.y);
            if (currentTime >= currentRoundDuration)
            {
                isGoodDream = true;
                currentTime = 0;
                sliderImgage.color = Color.white;
                dreamCount++;

                // Update the duration for the new round
                UpdateRoundDuration();

                SpawnEnemies();
                manager.SetDreamState(isGoodDream);

                // Switch tileset
                GrassWater.SetActive(true);
                Lava.SetActive(false);

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
            //Start Music after start timer based on initial dream state
            if (isGoodDream)
            {
                goodDreamMusic.Play();
            }
            else
            {
                badDreamMusic.Play();
            }

            countDownText.text = "";
            isGameActive = true;

            // Notify exit script that game is active
            if (exitScript != null)
            {
                exitScript.IsGameActive = true;
            }

            // Set the initial round duration
            UpdateRoundDuration();

            SpawnEnemies();
        }
    }

    void UpdateRoundDuration()
    {
        // Check if we have a valid round
        if (dreamCount >= 0 && dreamCount < roundManager.rounds.Count)
        {
            currentRoundDuration = roundManager.rounds[dreamCount].duration;
        }
        else
        {
            Debug.LogWarning($"Dream count {dreamCount} is out of range! Using default duration of 30 seconds.");
            currentRoundDuration = 30f; // Fallback duration
        }
    }

    void SpawnEnemies()
    {
        // Check if we have a valid round
        if (dreamCount < 0 || dreamCount >= roundManager.rounds.Count)
        {
            Debug.LogWarning($"Dream count {dreamCount} is out of range! No enemies to spawn.");
            return;
        }

        Round round = roundManager.rounds[dreamCount];

        foreach (EnemySpawnData enemyData in round.enemies)
        {
            Debug.Log($"Enemy Type: {enemyData.enemyType}, Count: {enemyData.spawnCount}");

            int[] doorIndices = spawnPatterns.GetDoorIndices(enemyData.spawnCount);

            for (int i = 0; i < enemyData.spawnCount && i < doorIndices.Length; i++)
            {
                int doorIndex = doorIndices[i];

                if (doorIndex < 0 || doorIndex >= doors.Count)
                {
                    Debug.LogError($"Door index {doorIndex} is out of range! You have {doors.Count} doors.");
                    continue;
                }

                Vector3 doorPos = doors[doorIndex].transform.position;
                doorPos.z = 1;

                // Spawn appropriate enemy type
                switch (enemyData.enemyType)
                {
                    case EnemyType.BasicEnemy:
                        manager.SpawnDefaultEnemy(doorPos);
                        break;

                    case EnemyType.HealEnemy:
                        manager.SpawnHealEnemy(doorPos);
                        break;

                    case EnemyType.RangedEnemy:
                        manager.SpawnRangedEnemy(doorPos);
                        break;

                    case EnemyType.TankEnemy:
                        manager.SpawnTankEnemy(doorPos);
                        break;

                    case EnemyType.EvilFather:
                        manager.SpawnEvilFather(doorPos);
                        break;

                    case EnemyType.TheMare:
                        manager.SpawnTheMare(doorPos);
                        break;

                    case EnemyType.TheDevil:
                        manager.SpawnTheDevil(doorPos);
                        break;

                    default:
                        Debug.LogWarning($"Unknown enemy type: {enemyData.enemyType}");
                        break;
                }
            }
        }
    }
}