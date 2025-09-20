using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    public float CurrentHealth { get { return currentHealth; } }

    [Header("Configurações de Invulnerabilidade")]
    public float invulnerabilityDuration = 1.5f;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public Color invulnerableColor = Color.white;
    private Color originalColor;
    private Collider2D _collider2D;
    private GameManager _gameManager;
    public int coinDropAmount;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;

        _gameManager = FindAnyObjectByType<GameManager>();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvulnerable) return;

        currentHealth -= damageAmount;

        if (gameObject.CompareTag("Enemy"))
        {
            if (animator != null)
            {
                animator.SetTrigger("Dano");
            }
        }

        Debug.Log(gameObject.name + " sofreu " + damageAmount + " de dano. Vida restante: " + currentHealth);

        StartCoroutine(BecomeInvulnerable());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " foi derrotado!");

        if (animator != null)
        {
            animator.SetTrigger("Morte");
        }

        _collider2D.enabled = false;

        if (gameObject.CompareTag("Player"))
        {
            GetComponent<PlayerController>().enabled = false;
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            SlimeController slimeController = GetComponent<SlimeController>();
            if (slimeController != null)
            {
                slimeController.enabled = false;
            }
        }

        Debug.Log(gameObject.name + " foi derrotado!");
    
        // Adiciona moedas antes de o objeto ser destruído
        CurrencyManager.AddCoins(coinDropAmount);
    }

    // --- NOVO: Função para ser chamada pelo Animation Event de morte do Player ---
    public void ShowGameOverScreen()
    {
        if (_gameManager != null)
        {
            _gameManager.ShowGameOverPanel();
        }
    }

    // --- Chamado pelo Animation Event do Slime ---
    public void DestroyAfterAnimation()
    {
        Destroy(gameObject);
    }

    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        spriteRenderer.color = invulnerableColor;
        yield return new WaitForSeconds(invulnerabilityDuration);
        spriteRenderer.color = originalColor;
        isInvulnerable = false;
    }

    public static void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}