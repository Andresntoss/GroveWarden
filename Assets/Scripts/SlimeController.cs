using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public float _moveSpeedSlime = 3.5f;
    public float damageAmount = 10f;
    [Header("Configurações de Knockback")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;
    
    [Header("Configurações de Movimento Aleatório")]
    public float wanderSpeed = 1.5f;
    public float wanderTimeMax = 3.0f;
    public float idleTimeMax = 2.0f;
    private float currentActionTime;
    private Vector2 wanderDirection;
    
    private enum SlimeState { Idle, Wander, Chase, KnockedBack }
    private SlimeState currentSlimeState;
    
    private Vector2 _slimeDirection;
    private Rigidbody2D _slimeRB2D;
    public DetectionController _detectionArea;
    private SpriteRenderer _spriteRenderer;
    private Animator _slimeAnimator;
    private bool isKnockedBack = false;

    void Start()
    {
        _slimeRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _slimeAnimator = GetComponent<Animator>();
        
        currentSlimeState = SlimeState.Idle;
        currentActionTime = Random.Range(1f, idleTimeMax);
    }

    private void FixedUpdate() 
    {
        if (isKnockedBack)
        {
            return;
        }

        if (_detectionArea.detectedObjs.Count > 0)
        {
            currentSlimeState = SlimeState.Chase;
            ChasePlayer();
        } 
        else 
        {
            currentActionTime -= Time.fixedDeltaTime;

            if (currentActionTime <= 0)
            {
                if (currentSlimeState == SlimeState.Idle)
                {
                    currentSlimeState = SlimeState.Wander;
                    wanderDirection = GetRandomDirection();
                    currentActionTime = Random.Range(1f, wanderTimeMax);
                }
                else
                {
                    currentSlimeState = SlimeState.Idle;
                    currentActionTime = Random.Range(1f, idleTimeMax);
                }
            }

            if (currentSlimeState == SlimeState.Wander)
            {
                Wander();
            }
            else
            {
                Idle();
            }
            _slimeAnimator.SetInteger("Movimento", (currentSlimeState == SlimeState.Wander) ? 1 : 0);
        }
    }
    
    private void ChasePlayer()
    {
        _slimeDirection = (_detectionArea.detectedObjs[0].transform.position - transform.position).normalized;
        _slimeRB2D.MovePosition(_slimeRB2D.position + _slimeDirection * _moveSpeedSlime * Time.fixedDeltaTime);
        _slimeAnimator.SetInteger("Movimento", 1);
        Flip();
    }
    
    private void Wander()
    {
        _slimeDirection = wanderDirection;
        _slimeRB2D.MovePosition(_slimeRB2D.position + _slimeDirection * wanderSpeed * Time.fixedDeltaTime);
        Flip();
    }
    
    private void Idle()
    {
        _slimeRB2D.linearVelocity = Vector2.zero; // Garante que o slime esteja parado
    }

    private Vector2 GetRandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        return new Vector2(x, y).normalized;
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
        isKnockedBack = true;
        _slimeRB2D.linearVelocity = Vector2.zero;
        _slimeRB2D.AddForce(direction * force, ForceMode2D.Impulse);
        StartCoroutine(ResetKnockback());
    }

    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        _slimeRB2D.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }
    
    private void Flip()
    {
        if (_slimeDirection.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_slimeDirection.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
    }
}