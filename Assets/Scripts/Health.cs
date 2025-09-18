using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Configurações de Invulnerabilidade")]
    public float invulnerabilityDuration = 1.5f;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public Color invulnerableColor = Color.white;
    private Color originalColor;
    private Collider2D _collider2D;

    private GameManager _gameManager; // <--- Adicionamos uma referência ao GameManager

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;

        _gameManager = FindAnyObjectByType<GameManager>(); // <--- Encontra o GameManager
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
        
        if (gameObject.CompareTag("Player"))
        {
            GetComponent<PlayerController>().enabled = false;
            _collider2D.enabled = false;
            
            // Chama o GameManager para mostrar a tela
            if (_gameManager != null)
            {
                _gameManager.ShowGameOverPanel();
            }
        }
        
        if (gameObject.CompareTag("Enemy"))
        {
            _collider2D.enabled = false;
            spriteRenderer.enabled = false;
            if (animator != null)
            {
                animator.SetTrigger("Morte");
            }
            SlimeController slimeController = GetComponent<SlimeController>();
            if (slimeController != null)
            {
                slimeController.enabled = false;
            }
            Destroy(gameObject, 1.0f);
        }
    }
    
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
    
    // Removido o método RestartGame, que agora está no GameManager
}