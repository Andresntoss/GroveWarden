using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider healthSlider;
    public Health playerHealth;

    void Update()
    {
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }
}
