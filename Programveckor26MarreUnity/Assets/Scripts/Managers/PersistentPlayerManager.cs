using System.Collections.Generic;
using TMPro;
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

    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI bossCoinsText;
    [SerializeField] private TextMeshProUGUI chaosLevelText;

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

        UpdateDisplay();
    }
    /// <summary>
    /// Lägg till en uppgradering i spelarens ägda uppgraderingar
    /// </summary>
    /// <param name="upgrade"></param>
    public void AddUpgrade(Upgrade upgrade)
    {
        if (!acquiredUpgrades.Contains(upgrade))
        {
            acquiredUpgrades.Add(upgrade);
            chaosLevel += upgrade.chaosLevel;
        }
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        coinsText.text = coins.ToString();
        bossCoinsText.text = bossCoins.ToString();
        chaosLevelText.text = chaosLevel.ToString();
    }
}

