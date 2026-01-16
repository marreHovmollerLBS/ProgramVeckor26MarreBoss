using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's health bar UI
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private RectTransform healthBarFill;
    [SerializeField] private Character playerClass;

    private void Start()
    {
        // If player not assigned, try to find it
        if (playerClass == null)
        {
            playerClass = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
        }

        // Initialize health bar to full
        UpdateHealthBar();
    }

    private void Update()
    {
        // Update health bar every frame
        UpdateHealthBar();
    }

    /// <summary>
    /// Updates the health bar fill amount based on player's current health
    /// </summary>
    private void UpdateHealthBar()
    {
        if (playerClass != null && healthBarFill != null)
        {
            // Calculate health percentage (0 to 1)
            float healthPercent = playerClass.CurrentHealth / playerClass.MaxHealth;

            // Update the fill amount
            healthBarFill.localScale = new Vector3(healthPercent,1,1);
        }
    }
}