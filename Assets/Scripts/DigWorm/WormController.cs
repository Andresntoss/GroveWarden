using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormController : MonoBehaviour
{
    public float damageAmount = 10f;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;

    [Header("Configurações do Verme")]
    public float jumpForce = 5f;
    public float jumpDuration = 1f;
    public float idleTimeMax = 3f;
    public float submergedTime = 2f;
    public float emergeTime = 0.5f;

    private Rigidbody2D _wormRB2D;
    public DetectionController _detectionArea;
    private SpriteRenderer _spriteRenderer;
    private Animator _wormAnimator;
    private Collider2D _wormCollider;

    private enum WormState { Idle, Attack, Submerged, Emerge }
    private WormState currentState;
    private float currentActionTime;
    
    private Vector2 targetPosition;

    void Start()
    {
        _wormRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _wormAnimator = GetComponent<Animator>();
        _wormCollider = GetComponent<Collider2D>();
        
        ChangeState(WormState.Idle);
    }

    void Update()
    {
        // Se o player for detectado, mude para o estado de Ataque
        if (_detectionArea.detectedObjs.Count > 0 && currentState != WormState.Attack)
        {
            ChangeState(WormState.Attack);
        }

        switch (currentState)
        {
            case WormState.Idle:
                HandleIdleState();
                break;
            case WormState.Attack:
                HandleAttackState();
                break;
            case WormState.Submerged:
                HandleSubmergedState();
                break;
            case WormState.Emerge:
                HandleEmergeState();
                break;
        }
    }
    
    // Removido o FixedUpdate() para evitar conflitos de lógica
    private void FixedUpdate() {}

    private void ChangeState(WormState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case WormState.Idle:
                _wormAnimator.SetInteger("Estado", 0);
                _spriteRenderer.enabled = true;
                _wormCollider.enabled = true;
                _wormRB2D.linearVelocity = Vector2.zero;
                currentActionTime = idleTimeMax;
                break;
            case WormState.Attack:
    _wormAnimator.SetInteger("Estado", 1);
    _wormRB2D.linearVelocity = Vector2.zero;
    _wormRB2D.bodyType = RigidbodyType2D.Kinematic; // <--- Alterado de isKinematic = true
    StartCoroutine(JumpTowardsPlayer());
    break;
            case WormState.Submerged:
                _wormAnimator.SetInteger("Estado", 2);
                _spriteRenderer.enabled = false;
                _wormCollider.enabled = false;
                _wormRB2D.linearVelocity = Vector2.zero;
                currentActionTime = submergedTime;
                break;
            case WormState.Emerge:
    _wormAnimator.SetInteger("Estado", 3);
    _spriteRenderer.enabled = true;
    _wormCollider.enabled = true;
    _wormRB2D.linearVelocity = Vector2.zero;
    _wormRB2D.bodyType = RigidbodyType2D.Dynamic; // <--- Alterado de isKinematic = false
    currentActionTime = emergeTime;
    break;
        }
    }

    private void HandleIdleState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(WormState.Submerged);
        }
    }

    private void HandleAttackState()
    {
        // A lógica do ataque é feita na Coroutine, então não precisamos de nada aqui.
    }

    private void HandleSubmergedState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(WormState.Emerge);
        }
    }

    private void HandleEmergeState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(WormState.Idle);
        }
    }
    
    private IEnumerator JumpTowardsPlayer()
    {
        Vector2 playerPosition = _detectionArea.detectedObjs[0].transform.position;
        Vector2 jumpDirection = (playerPosition - (Vector2)transform.position).normalized;

        _wormRB2D.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        
        yield return new WaitForSeconds(jumpDuration);
        
        ChangeState(WormState.Submerged);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
            
            if (playerController != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerController.ApplyKnockback(knockbackDirection, playerController.knockbackForce);
            }
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        // ...
    }
}