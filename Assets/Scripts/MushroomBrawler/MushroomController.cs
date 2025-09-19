using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomController : MonoBehaviour
{
    // Variáveis que podem ser reutilizadas do Slime
    public float _moveSpeed = 1.5f;
    public float damageAmount = 20f;
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.2f;

    [Header("Configurações da IA")]
    public float detectionRadius = 5f;
    public float wanderTimeMax = 3.0f;
    public float idleTimeMax = 2.0f;
    
    [Header("Ataque e Stun")]
    public float attackRadius = 1.5f;
    public float attackDelay = 1.0f; // Este delay será menos relevante, mas pode ser um "wind-up" inicial
    public float stunDuration = 3.0f;
    public Collider2D attackCollider; // Certifique-se de que este é o seu Trigger Collider para o ataque!

    private enum MushroomState { Idle, Wander, Chase, Attack, Stun }
    private MushroomState currentState;

    private Rigidbody2D _rb2d;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider; // Collider principal do cogumelo

    private Transform _playerTransform;
    private float currentActionTime;
    private Vector2 _wanderDirection;
    private bool _hasAttackedThisCycle = false; // Garante que o ataque só cause dano uma vez por ativação de collider

    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        ChangeState(MushroomState.Idle);
        
        if (attackCollider != null)
        {
            attackCollider.enabled = false; // O attackCollider começa desativado
        }
    }

    void Update()
    {
        if (_playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            
            // Lógica de estado principal
            if (distanceToPlayer <= attackRadius)
            {
                if (currentState != MushroomState.Attack && currentState != MushroomState.Stun)
                {
                    ChangeState(MushroomState.Attack);
                }
            }
            else if (distanceToPlayer <= detectionRadius)
            {
                if (currentState != MushroomState.Chase && currentState != MushroomState.Attack && currentState != MushroomState.Stun)
                {
                    ChangeState(MushroomState.Chase);
                }
            }
            else
            {
                if (currentState != MushroomState.Wander && currentState != MushroomState.Idle && currentState != MushroomState.Attack && currentState != MushroomState.Stun)
                {
                    ChangeState(MushroomState.Wander);
                }
            }
        }
        else // Se o jogador não for encontrado
        {
            if (currentState != MushroomState.Wander && currentState != MushroomState.Idle && currentState != MushroomState.Stun)
            {
                ChangeState(MushroomState.Wander);
            }
        }

        // Lógica de tratamento de estados (chamada a cada frame)
        switch (currentState)
        {
            case MushroomState.Idle:
                HandleIdleState();
                break;
            case MushroomState.Wander:
                HandleWanderState();
                break;
            case MushroomState.Chase:
                HandleChaseState();
                break;
            case MushroomState.Attack:
                // O ataque é controlado principalmente pelos Animation Events agora
                // HandleAttackState(); // Pode ser vazio ou para lógicas adicionais durante o ataque
                break;
            case MushroomState.Stun:
                HandleStunState();
                break;
        }
    }
    
    private void FixedUpdate()
    {
        Vector2 desiredVelocity = Vector2.zero;
        
        if (currentState == MushroomState.Wander)
        {
            desiredVelocity = _wanderDirection * _moveSpeed;
        }
        else if (currentState == MushroomState.Chase && _playerTransform != null)
        {
            // Ajusta a direção baseada na posição do jogador
            desiredVelocity = (_playerTransform.position - transform.position).normalized * _moveSpeed;
            
            // Inverte o sprite para virar na direção do jogador durante a perseguição
            if (Mathf.Abs(desiredVelocity.x) > 0.05f)
            {
                _spriteRenderer.flipX = desiredVelocity.x > 0;
            }
        }
        // No estado de ataque, o cogumelo não se move, então desiredVelocity permanece zero
        // No estado de stun, também não se move

        _rb2d.linearVelocity = desiredVelocity;
        
        // Inverte o sprite para virar na direção do movimento durante a caminhada
        if (currentState == MushroomState.Wander && Mathf.Abs(desiredVelocity.x) > 0.05f)
        {
            _spriteRenderer.flipX = desiredVelocity.x > 0;
        }
    }

    // --- Lógica de Estados ---
    private void ChangeState(MushroomState newState)
    {
        currentState = newState;
        StopAllCoroutines(); // Parar todas as corotinas ao mudar de estado

        // Desativa o collider de ataque ao mudar para um novo estado, a menos que seja o estado de ataque
        if (attackCollider != null && currentState != MushroomState.Attack)
        {
            attackCollider.enabled = false;
        }
        _hasAttackedThisCycle = false; // Sempre reseta esta flag ao mudar de estado

        switch (currentState)
        {
            case MushroomState.Idle:
                _animator.SetInteger("Estado", 0);
                currentActionTime = Random.Range(1f, idleTimeMax);
                _rb2d.linearVelocity = Vector2.zero;
                break;
            case MushroomState.Wander:
                _animator.SetInteger("Estado", 1);
                _wanderDirection = GetRandomDirection();
                currentActionTime = Random.Range(1f, wanderTimeMax);
                break;
            case MushroomState.Chase:
                _animator.SetInteger("Estado", 1);
                break;
            case MushroomState.Attack:
                _animator.SetInteger("Estado", 2);
                _rb2d.linearVelocity = Vector2.zero; // Para movimento durante o ataque
                // A ativação/desativação do collider de ataque e o dano serão controlados pelos Animation Events
                
                // Virar para o jogador antes de atacar
                if (_playerTransform != null)
                {
                    _spriteRenderer.flipX = (_playerTransform.position.x - transform.position.x) > 0;
                }
                break;
            case MushroomState.Stun:
                _animator.SetInteger("Estado", 3);
                currentActionTime = stunDuration;
                _rb2d.linearVelocity = Vector2.zero;
                break;
        }
    }

    private void HandleIdleState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(MushroomState.Wander);
        }
    }

    private void HandleWanderState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(MushroomState.Idle);
        }
    }

    private void HandleChaseState()
    {
        float distanceToAttack = Vector2.Distance(transform.position, _playerTransform.position);
        if (distanceToAttack < attackRadius)
        {
            ChangeState(MushroomState.Attack);
        }
    }
    
    private void HandleStunState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(MushroomState.Idle);
        }
    }

    // --- Funções Chamadas pelos Animation Events ---
    public void ActivateAttackCollider()
    {
        if (currentState == MushroomState.Attack && attackCollider != null)
        {
            attackCollider.enabled = true;
            _hasAttackedThisCycle = false; // Permite que o ataque cause dano novamente
        }
    }

    public void DeactivateAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    public void AttackAnimationFinished()
    {
        // Esta função é chamada no final da animação de ataque
        if (currentState == MushroomState.Attack)
        {
            // Depois do ataque, verifica o estado do jogador para decidir o próximo estado
            if (_playerTransform != null)
            {
                if (Vector2.Distance(transform.position, _playerTransform.position) <= detectionRadius)
                {
                    ChangeState(MushroomState.Chase);
                }
                else
                {
                    ChangeState(MushroomState.Wander);
                }
            }
            else
            {
                ChangeState(MushroomState.Wander);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se este collider for o attackCollider do cogumelo
        if (other.CompareTag("Player") && attackCollider != null && other == _playerTransform.GetComponent<Collider2D>())
        {
            // Certifique-se de que é o attackCollider que está ativo e não o collider principal do cogumelo
            // Isso pode ser um pouco complicado se o attackCollider for um filho.
            // A maneira mais robusta é usar o evento "OnTriggerEnter2D" *no script do attackCollider*
            // ou ter certeza que este método só é chamado pelo collider de ataque.

            // Para este setup, presumimos que este `OnTriggerEnter2D` está no GameObject pai (o Mushroom),
            // e que `attackCollider` é um `Trigger` anexado ao Mushroom ou a um filho.
            // Para ter certeza que a detecção de colisão é apenas para o attackCollider:
            // O `other` é o jogador. Se o `attackCollider` é que está causando o trigger,
            // então não precisamos verificar `currentState == MushroomState.Attack` aqui,
            // pois o `attackCollider` só é ativado nesse estado pelos eventos de animação.

            // A flag `_hasAttackedThisCycle` é crucial para garantir um único hit por ataque.
            if (attackCollider.enabled && !_hasAttackedThisCycle) // Verifica se o collider de ataque está ativo e ainda não atacou
            {
                Health playerHealth = other.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                    _hasAttackedThisCycle = true; // Marca como hit para evitar múltiplos hits

                    Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                        playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
        else if (other.CompareTag("PlayerAttackArea")) // Este é um ataque do jogador contra o cogumelo
        {
            Vector2 attackDirection = other.transform.position - transform.position;

            // Lógica para verificar se o ataque do jogador foi pelas costas do cogumelo
            bool hitFromBehind = (_spriteRenderer.flipX && attackDirection.x > 0) || (!_spriteRenderer.flipX && attackDirection.x < 0);

            if (hitFromBehind)
            {
                ChangeState(MushroomState.Stun);
            }
            // Não retorna aqui para permitir que outros triggers (se houver) sejam processados
        }
    }

    private Vector2 GetRandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}