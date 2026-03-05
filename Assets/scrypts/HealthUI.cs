using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public skeleton player;
    public Slider healthSlider;
    public Slider staminaSlider;
    public Text healthText; // опционально
    public Text staminaText; // опционально

    void Update()
    {
        if (player == null) return;

        if (healthSlider != null)
        {
            healthSlider.maxValue = player.maxHealth;
            healthSlider.value = player.currentHealth;
        }
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = player.maxStamina;
            staminaSlider.value = player.currentStamina;
        }
        if (healthText != null)
            healthText.text = $"{player.currentHealth}/{player.maxHealth}";
        if (staminaText != null)
            staminaText.text = $"{player.currentStamina:F0}/{player.maxStamina}";
    }
}