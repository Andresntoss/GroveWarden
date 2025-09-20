using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider healthSlider;
    public Health playerHealth;

    void Update()
    {
        // Garante que o slider não seja nulo e que o jogador tenha o script Health
        if (healthSlider != null && playerHealth != null)
        {
            // O valor máximo do slider é a vida máxima do jogador
            healthSlider.maxValue = playerHealth.maxHealth;
            // O valor atual do slider é a vida atual do jogador
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }
}