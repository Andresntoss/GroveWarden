using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Configurações de Invulnerabilidade")]
    public float invulnerabilityDuration = 1.5f;
    public Color invulnerableColor = Color.white;

    private bool isDead = false;
    private bool isInvulnerable = false;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    // --- Propriedade para compatibilidade com HealthBarUI ---
    public float CurrentHealth
    {
        get { return currentHealth; }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (spriteRenderer != null)
            StartCoroutine(BecomeInvulnerable());

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        if (spriteRenderer != null)
            spriteRenderer.color = invulnerableColor;

        yield return new WaitForSeconds(invulnerabilityDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
            animator.SetTrigger("Morte"); // Configure o trigger "Morte" no Animator

        if (gameObject.CompareTag("Player"))
        {
            // Player desmaia
            if (FaintManager.instance != null)
                FaintManager.instance.AtivarDesmaio();

            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            Debug.Log("[Health] Player desmaiou!");
        }
        else
        {
            // Inimigo morre normalmente
            var enemyController = GetComponent<SlimeController>();
            if (enemyController != null)
                enemyController.enabled = false;

            // Opcional: Destruir inimigo após animação
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// Restaura a vida máxima e permite nova morte.
    /// </summary>
    public void RestaurarVidaMaxima()
    {
        currentHealth = maxHealth;
        isDead = false;

        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = true;
    }

    /// <summary>
    /// Retorna a vida atual (opcional se preferir usar CurrentHealth).
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
