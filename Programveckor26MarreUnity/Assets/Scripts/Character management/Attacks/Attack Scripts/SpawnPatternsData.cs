using System;
using UnityEngine;

[Serializable]
public class SpawnPattern
{
    [Tooltip("Number of enemies this pattern is for")]
    public int enemyCount;

    [Tooltip("Door indices to spawn at (0-8 for 9 doors)")]
    public int[] doorIndices;
}

[CreateAssetMenu(fileName = "New Spawn Patterns", menuName = "Game/Spawn Patterns")]
public class SpawnPatternsData : ScriptableObject
{
    [Tooltip("Define spawn patterns for different enemy counts")]
    public SpawnPattern[] patterns;

    // Get the door indices for a specific enemy count
    public int[] GetDoorIndices(int enemyCount)
    {
        foreach (SpawnPattern pattern in patterns)
        {
            if (pattern.enemyCount == enemyCount)
            {
                return pattern.doorIndices;
            }
        }

        Debug.LogWarning($"No spawn pattern found for {enemyCount} enemies! Using default.");
        return new int[] { 4 }; // Default fallback
    }
}