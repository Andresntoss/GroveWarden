using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetleController : MonoBehaviour
{
    public float _moveSpeed = 2.0f;
    public float damageAmount = 15f;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;

    [Header("Configurações da IA")]
    public float detectionRadius = 6f;
    public float wanderTimeMax = 3.0f;
    
    [Header("Ataque de Investida")]
    public float chargeRadius = 1.0f;
    public float chargeForce = 20f;
    public float chargeDuration = 0.5f;
    public float chargeCooldown = 2.0f;
    
    private enum BeetleState { Wander, Chase, Charge }
    private BeetleState currentState;

    private Rigidbody2D _rb2d;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    public DetectionController _detectionArea;

    private Transform _playerTransform;
    private float currentActionTime;
    private Vector2 _wanderDirection;
    private bool _isCharging = false;
    private float _nextChargeTime = 0f;

    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        ChangeState(BeetleState.Wander);
    }

    void Update()
    {
        if (_playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            
            if (currentState != BeetleState.Charge)
            {
                if (distanceToPlayer <= chargeRadius && Time.time > _nextChargeTime)
                {
                    ChangeState(BeetleState.Charge);
                }
                else if (distanceToPlayer < detectionRadius)
                {
                    ChangeState(BeetleState.Chase);
                }
                else
                {
                    ChangeState(BeetleState.Wander);
                }
            }
        }
        else
        {
            ChangeState(BeetleState.Wander);
        }

        switch (currentState)
        {
            case BeetleState.Wander:
                HandleWanderState();
                break;
            case BeetleState.Chase:
                HandleChaseState();
                break;
            case BeetleState.Charge:
                // A lógica de charge é feita na Coroutine
                break;
        }
    }
    
    private void FixedUpdate()
    {
        Vector2 desiredVelocity = Vector2.zero;
        
        if (currentState == BeetleState.Wander)
        {
            desiredVelocity = _wanderDirection * _moveSpeed;
        }
        else if (currentState == BeetleState.Chase)
        {
            if (_playerTransform != null)
            {
                desiredVelocity = (_playerTransform.position - transform.position).normalized * _moveSpeed;
            }
        }
        
        if (!_isCharging)
        {
            _rb2d.linearVelocity = desiredVelocity;
        }

        if (Mathf.Abs(_rb2d.linearVelocity.x) > 0.05f)
        {
            _spriteRenderer.flipX = _rb2d.linearVelocity.x < 0;
        }
    }

    private void ChangeState(BeetleState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        StopAllCoroutines();

        switch (currentState)
        {
            case BeetleState.Wander:
                _animator.SetInteger("Estado", 0);
                _wanderDirection = GetRandomDirection();
                currentActionTime = Random.Range(1f, wanderTimeMax);
                _isCharging = false;
                break;
            case BeetleState.Chase:
                _animator.SetInteger("Estado", 0);
                _isCharging = false;
                break;
            case BeetleState.Charge:
                _animator.SetInteger("Estado", 1);
                _isCharging = true;
                _rb2d.linearVelocity = Vector2.zero;
                StartCoroutine(PerformChargeAttack());
                break;
        }
    }

    private void HandleWanderState()
    {
        currentActionTime -= Time.deltaTime;
        if (currentActionTime <= 0)
        {
            _wanderDirection = GetRandomDirection();
            currentActionTime = Random.Range(1f, wanderTimeMax);
        }
    }

    private void HandleChaseState()
    {
        //Essa função é chamada diretamente no update
    }
    
    private void HandleChargeState()
    {
        // ...Essa função é chamada diretamente no update
    }

    private IEnumerator PerformChargeAttack()
    {
        _isCharging = true;
        _rb2d.linearVelocity = Vector2.zero;
        
        Vector2 chargeDirection = (_playerTransform.position - transform.position).normalized;
        _rb2d.AddForce(chargeDirection * chargeForce, ForceMode2D.Impulse);
        
        yield return new WaitForSeconds(chargeDuration);

        _isCharging = false;
        _rb2d.linearVelocity = Vector2.zero;
        
        _nextChargeTime = Time.time + chargeCooldown;
        
        ChangeState(BeetleState.Wander);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && _isCharging)
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
    
    private Vector2 GetRandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}