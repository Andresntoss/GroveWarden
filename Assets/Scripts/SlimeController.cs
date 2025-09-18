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
    
    private Vector2 _slimeDirection;
    private Rigidbody2D _slimeRB2D;
    public DetectionController _detectionArea;
    private SpriteRenderer _spriteRenderer;
    private bool isKnockedBack = false;
    private Animator _slimeAnimator; // <--- Adicionamos o Animator

    void Start()
    {
        _slimeRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _slimeAnimator = GetComponent<Animator>(); // <--- Obtemos o Animator no Start
    }

    private void FixedUpdate() 
    {
        if (_detectionArea.detectedObjs.Count > 0 && !isKnockedBack)
        {
            _slimeDirection = (_detectionArea.detectedObjs[0].transform.position - transform.position).normalized;
            _slimeRB2D.MovePosition(_slimeRB2D.position + _slimeDirection * _moveSpeedSlime * Time.fixedDeltaTime);
            
            // Lógica para a animação
            _slimeAnimator.SetInteger("Movimento", 1); // <--- Ativa a animação de walk
            
            if (_slimeDirection.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else if (_slimeDirection.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
        } 
        else
        {
            _slimeAnimator.SetInteger("Movimento", 0); // <--- Ativa a animação de idle
        }
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
        _slimeRB2D.AddForce(direction * force, ForceMode2D.Impulse);
        StartCoroutine(ResetKnockback());
    }

    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        _slimeRB2D.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }
}