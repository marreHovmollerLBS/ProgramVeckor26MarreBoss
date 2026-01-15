using System.Collections.Generic;
using UnityEngine;

public class UpgradeDisplay : MonoBehaviour
{
    [SerializeField] private List<Upgrade> availableUpgrades;
    [SerializeField] private UpgradeView upgradeCardPrefab;
    [SerializeField] private float spacing = 30f; //Avståndet mellan korten
    [SerializeField] private Transform canvasTransform;

    void Start()
    {
        DisplayRandomUpgrades();
    }

    void DisplayRandomUpgrades()
    {
        // Slupmar fram tre uppgraderingar
        List<Upgrade> selectedUpgrades = new List<Upgrade>();
        List<Upgrade> tempList = new List<Upgrade>(availableUpgrades);

        Debug.Log($"Available upgrades: {availableUpgrades.Count}");

        for (int i = 0; i < 3 && tempList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            selectedUpgrades.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        Debug.Log($"Selected {selectedUpgrades.Count} upgrades");

        for (int i = 0; i < selectedUpgrades.Count; i++)
        {
            UpgradeView card = Instantiate(upgradeCardPrefab, canvasTransform); // SKapa kortet som barn till canvas
            card.Init(selectedUpgrades[i]);

            float xPos = (i - 1) * spacing; // placerar korten på -1, 0, 1
            card.transform.localPosition = new Vector3(xPos, 0, 0);
        }
    }
}
