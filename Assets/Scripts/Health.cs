using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float CurrentHealth => currentHealth;

    [Header("Invulnerabilidade")]
    public float invulnerabilityDuration = 1.5f;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color invulnerableColor = Color.white;

    private Collider2D _collider2D;
    private Animator animator;
    private bool isDead = false;
    public int coinDropAmount = 0; // Quantidade de moedas que este inimigo solta

    [Header("Drop de Itens")]
    public ItemData itemToDrop; // O ScriptableObject do item que o inimigo vai dropar
    public int dropAmount = 1; // Quantidade de itens a serem dropados

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damageAmount;
        StartCoroutine(BecomeInvulnerable());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (_collider2D != null)
            _collider2D.enabled = false;

        if (animator != null)
            animator.SetTrigger("Morte"); // Trigger deve existir no Animator

        if (gameObject.CompareTag("Player"))
        {
            // Pausa o controle do jogador
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            // Ativa desmaio
            FaintManager.instance?.TriggerFaint();
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            // Drop de moedas do inimigo
            CurrencyManager.AddCoins(coinDropAmount);
            // Desativa controlador do inimigo se existir
            SlimeController ec = GetComponent<SlimeController>();
            if (ec != null) ec.enabled = false;
        }
        Debug.Log(gameObject.name + " foi derrotado!");

        // --- NOVO: Lógica para o Drop de Itens ---
        if (itemToDrop != null && InventoryManager.instance != null)
        {
            InventoryManager.instance.AddItem(itemToDrop, dropAmount);
        }

        if (animator != null)
        {
            animator.SetTrigger("Morte");
        }
        
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (_collider2D != null)
            _collider2D.enabled = true;

        if (animator != null)
            animator.Play("Base"); // Reseta para animação base

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
            pc.enabled = true;
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

    // --- Chamado pelo AnimationEvent 'DestroyAfterAnimation' ---
    public void DestroyAfterAnimation()
    {
        Destroy(gameObject);
    }
}
