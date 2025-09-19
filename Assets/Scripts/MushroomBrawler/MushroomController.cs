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
    public float attackDelay = 1.0f;
    public float stunDuration = 3.0f;
    public Collider2D attackCollider;

    private enum MushroomState { Idle, Wander, Chase, Attack, Stun }
    private MushroomState currentState;

    private Rigidbody2D _rb2d;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    private Transform _playerTransform;
    private float currentActionTime;
    private Vector2 _wanderDirection;
    private bool _isKnockedBack = false;

    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        ChangeState(MushroomState.Idle);
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
        else
        {
            if (currentState != MushroomState.Wander && currentState != MushroomState.Idle && currentState != MushroomState.Stun)
            {
                ChangeState(MushroomState.Wander);
            }
        }

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
                HandleAttackState();
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
            desiredVelocity = (_playerTransform.position - transform.position).normalized * _moveSpeed;
        }

        _rb2d.linearVelocity = desiredVelocity;
        
        if (Mathf.Abs(desiredVelocity.x) > 0.05f)
        {
            _spriteRenderer.flipX = desiredVelocity.x < 0;
        }
    }

    // --- Lógica de Estados ---
    private void ChangeState(MushroomState newState)
    {
        currentState = newState;
        // Stop any coroutines that might be running from a previous state
        StopAllCoroutines();

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
                StartCoroutine(PerformHeadbuttAttack());
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
    
    private void HandleAttackState()
    {
        // ...
    }

    private void HandleStunState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            ChangeState(MushroomState.Idle);
        }
    }

    private IEnumerator PerformHeadbuttAttack()
    {
        _rb2d.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(attackDelay); 
        
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
        
        yield return new WaitForSeconds(0.2f);
        
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttackArea"))
        {
            Vector2 attackDirection = other.transform.position - transform.position;
            
            bool hitFromBehind = (_spriteRenderer.flipX && attackDirection.x > 0) || (!_spriteRenderer.flipX && attackDirection.x < 0);
            
            if (hitFromBehind)
            {
                ChangeState(MushroomState.Stun);
            }
        }
    }

    private Vector2 GetRandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}