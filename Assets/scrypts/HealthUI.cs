using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public skeleton player;
    public Slider healthSlider;

    void Update()
    {
    if (player == null || healthSlider == null) return;
    healthSlider.maxValue = player.maxHealth;
    healthSlider.value = player.currentHealth;
    Debug.Log($"HP: {player.currentHealth}/{player.maxHealth}, Slider value: {healthSlider.value}");
    }
}