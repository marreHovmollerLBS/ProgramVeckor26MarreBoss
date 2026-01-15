using System.Collections.Generic;
using UnityEngine;

public class PersistentPlayerManager : MonoBehaviour
{
    public static PersistentPlayerManager Instance;

    /// <summary>
    /// Uppgraderingar spelaren fått mellan världar, behålls mellan scenbyten
    /// </summary>
    public List<Upgrade> acquiredUpgrades = new();
    /// <summary>
    /// Ju högre nivå desto svårare blir det
    /// </summary>
    public int chaosLevel;
    public int coins;
    public int bossCoins;

    private void Awake()
    {
        // Se till att det bara finns en instans
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    /// <summary>
    /// Lägg till en uppgradering i spelarens ägda uppgraderingar
    /// </summary>
    /// <param name="upgrade"></param>
    public void AddUpgrade(Upgrade upgrade)
    {
        if (!acquiredUpgrades.Contains(upgrade))
            acquiredUpgrades.Add(upgrade);
    }
}

