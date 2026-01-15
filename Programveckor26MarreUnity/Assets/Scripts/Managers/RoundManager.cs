using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyType enemyType;
    public int spawnCount;
}

[System.Serializable]
public class Round
{
    public string roundName;
    public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
}

public enum EnemyType
{
    BasicEnemy
    // Add more enemy types here
}

public class RoundManager : MonoBehaviour
{
    [Header("Dream Settings")]
    [Tooltip("If true, the game starts with a good dream. If false, starts with a bad dream.")]
    public bool startWithGoodDream = true;

    [Header("Rounds")]
    public List<Round> rounds = new List<Round>();
}