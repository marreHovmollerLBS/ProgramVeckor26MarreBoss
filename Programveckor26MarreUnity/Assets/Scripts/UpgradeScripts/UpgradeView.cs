using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler

{
    private Vector3 originalScale;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Image coinIcon;
    [SerializeField] private TextMeshProUGUI costText;

    [SerializeField] private Sprite normalUpgradeIcon;
    [SerializeField] private Sprite bossUpgradeIcon;

    private Upgrade upgrade;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Init(Upgrade upgrade)
    {
        this.upgrade = upgrade;
        iconImage.sprite = upgrade.icon;
        titleText.text = upgrade.title;
        descriptionText.text = upgrade.description;
        costText.text = upgrade.cost.ToString();
        SetIcon();
    }
    private void SetIcon()
    {
        if (upgrade.isBossUpgrade)
        {
            coinIcon.sprite = bossUpgradeIcon;
        }
        else
        {
            coinIcon.sprite = normalUpgradeIcon;
        }
    }
    public Upgrade GetUpgrade()
    {
        return upgrade;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Kortet växer lite för att visa att spelaren hoverar
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Ta bort hover-effekt
        transform.localScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PersistentPlayerManager.Instance.AddUpgrade(upgrade);
        Debug.Log($"Selected {upgrade.title}");
        Destroy(gameObject);
    }
}
