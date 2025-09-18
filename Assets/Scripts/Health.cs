using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Configurações de Invulnerabilidade")]
    public float invulnerabilityDuration = 1.5f; // Tempo de invulnerabilidade
    public float flashInterval = 0.1f; // Frequência do piscar
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Este método agora só causa dano se o jogador NÃO estiver invulnerável
    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " sofreu " + damageAmount + " de dano. Vida restante: " + currentHealth);
        
        StartCoroutine(BecomeInvulnerable());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // O que acontece quando a vida chega a zero
    private void Die()
    {
        Debug.Log(gameObject.name + " foi derrotado!");
        Destroy(gameObject);
    }

    // Coroutine para gerenciar o tempo de invulnerabilidade
    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        float invulnerabilityEndTime = Time.time + invulnerabilityDuration;

        while (Time.time < invulnerabilityEndTime)
        {
            // O personagem pisca para dar feedback visual
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }
}