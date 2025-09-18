using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    
    public float _moveSpeedSlime = 3.5f;
    public float damageAmount = 10f; // <-- NOVA VARIÁVEL: a quantidade de dano que o slime causa
    private Vector2 _slimeDirection;
    private Rigidbody2D _slimeRB2D;
    public DetectionController _detectionArea;
    private SpriteRenderer _spriteRenderer;
    void Start()
    {
        _slimeRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        // Removemos esta linha para que o slime não siga o input do teclado
        // _slimeDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate() 
    {
        if (_detectionArea.detectedObjs.Count > 0)
        {
            _slimeDirection = (_detectionArea.detectedObjs[0].transform.position - transform.position).normalized;
            _slimeRB2D.MovePosition(_slimeRB2D.position + _slimeDirection * _moveSpeedSlime * Time.fixedDeltaTime);

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

    // <-- NOVO MÉTODO: O slime causa dano ao colidir com o jogador
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Encontra o componente de vida do jogador e causa dano
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}