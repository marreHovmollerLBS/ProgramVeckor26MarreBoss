using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's health bar UI
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Character player;

    private void Start()
    {
        // If player not assigned, try to find it
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
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
        if (player != null && healthBarFill != null)
        {
            // Calculate health percentage (0 to 1)
            float healthPercent = player.CurrentHealth / player.MaxHealth;

            // Update the fill amount
            healthBarFill.fillAmount = healthPercent;
        }
    }
}