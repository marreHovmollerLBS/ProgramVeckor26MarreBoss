using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating health bar that appears above enemies
/// </summary>
public class FloatingHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    private Canvas canvas;
    private Image backgroundImage;
    private Image fillImage;
    private RectTransform rectTransform;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.8f, 0);
    [SerializeField] private Vector2 barSize = new Vector2(1f, 0.15f);
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color fillColor = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float lowHealthThreshold = 0.3f;

    [Header("Visibility")]
    [SerializeField] private float displayDuration = 5f; // How long bar stays visible after hit
    [SerializeField] private float fadeOutDuration = 0.5f;
    private float hideTimer = 0f;
    private bool isVisible = false;
    private CanvasGroup canvasGroup;

    private Character targetCharacter;
    private Camera mainCamera;

    private void Awake()
    {
        CreateHealthBar();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Create the health bar UI elements
    /// </summary>
    private void CreateHealthBar()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.zero;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f; // Higher value for better quality

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Start invisible

        rectTransform = canvasObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = barSize; // Use size directly in world space

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale = Vector3.one;

        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill container (for padding)
        GameObject fillContainerObj = new GameObject("FillContainer");
        fillContainerObj.transform.SetParent(canvasObj.transform);
        fillContainerObj.transform.localPosition = Vector3.zero;
        fillContainerObj.transform.localScale = Vector3.one;

        RectTransform fillContainerRect = fillContainerObj.AddComponent<RectTransform>();
        fillContainerRect.anchorMin = Vector2.zero;
        fillContainerRect.anchorMax = Vector2.one;
        fillContainerRect.sizeDelta = new Vector2(-0.02f, -0.02f); // Small padding
        fillContainerRect.anchoredPosition = Vector2.zero;

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillContainerObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = Vector3.one;

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f); // Anchor to left side only
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(barSize.x, 0f); // Start at full width
    }

    /// <summary>
    /// Initialize the health bar with a target character
    /// </summary>
    public void Initialize(Character character, Vector2 size, Vector3 posOffset)
    {
        targetCharacter = character;
        barSize = size;
        offset = posOffset;

        rectTransform.sizeDelta = barSize; // Use size directly in world space

        // Start invisible
        canvasGroup.alpha = 0f;
        isVisible = false;
    }

    /// <summary>
    /// Show the health bar (called when enemy is hit)
    /// </summary>
    public void Show()
    {
        isVisible = true;
        hideTimer = displayDuration;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Update the health bar fill amount
    /// </summary>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (fillImage == null || targetCharacter == null) return;

        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        // Scale the width of the fill image
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        if (fillRect != null)
        {
            fillRect.sizeDelta = new Vector2(barSize.x * healthPercent, 0f);
        }

        // Change color based on health
        if (healthPercent <= lowHealthThreshold)
        {
            fillImage.color = Color.Lerp(lowHealthColor, fillColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            fillImage.color = fillColor;
        }
    }

    private void Update()
    {
        if (targetCharacter == null)
        {
            Destroy(gameObject);
            return;
        }

        // Update position to follow character
        transform.position = targetCharacter.transform.position + offset;

        // Make health bar face camera
        if (mainCamera != null)
        {
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - mainCamera.transform.position);
        }

        // Handle auto-hide
        if (isVisible)
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0f)
            {
                // Fade out
                if (hideTimer <= -fadeOutDuration)
                {
                    isVisible = false;
                    canvasGroup.alpha = 0f;
                }
                else
                {
                    canvasGroup.alpha = 1f + (hideTimer / fadeOutDuration);
                }
            }
        }

        // Update health display
        UpdateHealth(targetCharacter.CurrentHealth, targetCharacter.MaxHealth);
    }

    /// <summary>
    /// Set bar colors
    /// </summary>
    public void SetColors(Color bgColor, Color fillCol, Color lowHealthCol)
    {
        backgroundColor = bgColor;
        fillColor = fillCol;
        lowHealthColor = lowHealthCol;

        if (backgroundImage != null) backgroundImage.color = backgroundColor;
        if (fillImage != null) fillImage.color = fillColor;
    }

    /// <summary>
    /// Set display duration
    /// </summary>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
    }
}

/// <summary>
/// Static player health bar that stays on screen
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    private Canvas canvas;
    private Image backgroundImage;
    private Image fillImage;
    private Text healthText;

    [Header("Settings")]
    [SerializeField] private Vector2 barSize = new Vector2(300f, 40f);
    [SerializeField] private Vector2 position = new Vector2(20f, -20f); // Offset from anchor
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color fillColor = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool showHealthText = true;

    private Player targetPlayer;
    private RectTransform canvasRect;

    private void Awake()
    {
        CreateHealthBar();
    }

    /// <summary>
    /// Create the static health bar UI
    /// </summary>
    private void CreateHealthBar()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("PlayerHealthBarCanvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObj.AddComponent<GraphicRaycaster>();

        canvasRect = canvasObj.GetComponent<RectTransform>();

        // Create container for health bar
        GameObject containerObj = new GameObject("HealthBarContainer");
        containerObj.transform.SetParent(canvasObj.transform);

        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0f, 1f); // Top-left
        containerRect.anchorMax = new Vector2(0f, 1f);
        containerRect.pivot = new Vector2(0f, 1f);
        containerRect.sizeDelta = barSize;
        containerRect.anchoredPosition = position;

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(containerObj.transform);

        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill container (for padding)
        GameObject fillContainerObj = new GameObject("FillContainer");
        fillContainerObj.transform.SetParent(containerObj.transform);

        RectTransform fillContainerRect = fillContainerObj.AddComponent<RectTransform>();
        fillContainerRect.anchorMin = Vector2.zero;
        fillContainerRect.anchorMax = Vector2.one;
        fillContainerRect.sizeDelta = new Vector2(-4f, -4f); // Padding
        fillContainerRect.anchoredPosition = Vector2.zero;

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillContainerObj.transform);

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f); // Anchor to left side only
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(barSize.x - 4f, 0f); // Start at full width minus padding

        // Create health text
        if (showHealthText)
        {
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(containerObj.transform);

            healthText = textObj.AddComponent<Text>();
            healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            healthText.fontSize = 24;
            healthText.alignment = TextAnchor.MiddleCenter;
            healthText.color = Color.white;
            healthText.text = "100 / 100";

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Add shadow for better readability
            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }
    }

    /// <summary>
    /// Initialize with target player
    /// </summary>
    public void Initialize(Player player)
    {
        targetPlayer = player;
        UpdateHealth();
    }

    /// <summary>
    /// Update the health bar display
    /// </summary>
    private void UpdateHealth()
    {
        if (targetPlayer == null || fillImage == null) return;

        float currentHealth = targetPlayer.CurrentHealth;
        float maxHealth = targetPlayer.MaxHealth;
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        // Scale the width of the fill image
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        if (fillRect != null)
        {
            fillRect.sizeDelta = new Vector2((barSize.x - 4f) * healthPercent, 0f);
        }

        // Change color based on health
        if (healthPercent <= lowHealthThreshold)
        {
            fillImage.color = Color.Lerp(lowHealthColor, fillColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            fillImage.color = fillColor;
        }

        // Update text
        if (showHealthText && healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void Update()
    {
        UpdateHealth();
    }

    /// <summary>
    /// Set bar position on screen
    /// </summary>
    public void SetPosition(Vector2 pos)
    {
        position = pos;
        if (canvasRect != null)
        {
            RectTransform containerRect = canvasRect.GetChild(0).GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchoredPosition = position;
            }
        }
    }

    /// <summary>
    /// Set bar size
    /// </summary>
    public void SetSize(Vector2 size)
    {
        barSize = size;
        if (canvasRect != null)
        {
            RectTransform containerRect = canvasRect.GetChild(0).GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.sizeDelta = barSize;
            }
        }
    }

    /// <summary>
    /// Set bar colors
    /// </summary>
    public void SetColors(Color bgColor, Color fillCol, Color lowHealthCol)
    {
        backgroundColor = bgColor;
        fillColor = fillCol;
        lowHealthColor = lowHealthCol;

        if (backgroundImage != null) backgroundImage.color = backgroundColor;
        UpdateHealth(); // Update fill color
    }

    /// <summary>
    /// Toggle health text display
    /// </summary>
    public void SetShowHealthText(bool show)
    {
        showHealthText = show;
        if (healthText != null)
        {
            healthText.gameObject.SetActive(show);
        }
    }
}