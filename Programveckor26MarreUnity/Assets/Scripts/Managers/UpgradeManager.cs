using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    /// <summary>
    /// Uppgraderingar spelaren fått mellan världar, behålls mellan scenbyten
    /// </summary>
    public List<Upgrade> acquiredUpgrades = new();

    private void Awake()
    {
        // Se till att det bara finns en instans av UpgradeManager
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

