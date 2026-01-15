using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private Upgrade upgrade;

    public void Init(Upgrade upgrade)
    {
        this.upgrade = upgrade;
        iconImage.sprite = upgrade.icon;
        titleText.text = upgrade.title;
        descriptionText.text = upgrade.description;
    }

    public Upgrade GetUpgrade()
    {
        return upgrade;
    }
}
