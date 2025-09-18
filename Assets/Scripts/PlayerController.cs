using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _PlayerAnimator;
    public float _playerSpeed;
    private float _playerInitialSpeed;
    public float _playerRunSpeed;
    private Vector2 _playerDirection;

    private bool _isAttacking = false;
    private Coroutine _attackCoroutine;

    // ----- VARIÁVEIS DO ROLAMENTO -----
    [Header("Configurações de Rolamento")]
    public float _rollSpeed = 8f;
    public float _rollDuration = 0.5f;
    public float _rollCooldown = 1.5f;

    private bool _isRolling = false;
    private float _nextRollTime = 0f;

    // ----- NOVAS VARIÁVEIS PARA AS CAMADAS -----
    private int playerLayer;
    private int invincibleLayer;

    void Start()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _PlayerAnimator = GetComponent<Animator>();

        _playerInitialSpeed = _playerSpeed;

        // Armazena as camadas para uso futuro
        playerLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("Invencivel");
    }

    void Update()
    {
        _playerDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        OnAttack();
        OnRoll();

        if (!_isAttacking && !_isRolling)
        {
            if (_playerDirection.sqrMagnitude > 0)
            {
                _PlayerAnimator.SetInteger("Movimento", 1);
            }
            else
            {
                _PlayerAnimator.SetInteger("Movimento", 0);
            }

            Flip();
            PlayerRun();
        }
    }

    void FixedUpdate()
    {
        if (!_isAttacking && !_isRolling)
        {
            _playerRigidbody2D.MovePosition(_playerRigidbody2D.position + _playerDirection * _playerSpeed * Time.fixedDeltaTime);
        }
    }

    void Flip()
    {
        if (_playerDirection.x > 0)
        {
            transform.eulerAngles = new Vector2(0f, 0f);
        }
        else if (_playerDirection.x < 0)
        {
            transform.eulerAngles = new Vector2(0f, 180f);
        }
    }

    void PlayerRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _playerSpeed = _playerRunSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _playerSpeed = _playerInitialSpeed;
        }
    }

    void OnAttack()
    {
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0)) && !_isAttacking && !_isRolling)
        {
            _isAttacking = true;
            _PlayerAnimator.SetTrigger("Ataque");
            
            SetAttackState(true);
            
            StartCoroutine(ResetAttackAfterDelay());
        }
    }

    private IEnumerator ResetAttackAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        SetAttackState(false);
    }

    private void SetAttackState(bool isAttacking)
    {
        _isAttacking = isAttacking;
        _playerSpeed = isAttacking ? 0 : _playerInitialSpeed;
    }

    void OnRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextRollTime && !_isAttacking)
        {
            _isRolling = true;
            _PlayerAnimator.SetTrigger("Rolamento");
            
            _nextRollTime = Time.time + _rollCooldown;

            StartCoroutine(Roll());
        }
    }

    private IEnumerator Roll()
    {
        gameObject.layer = invincibleLayer; // <--- Muda a camada para invencível
        _playerRigidbody2D.linearVelocity = _playerDirection * _rollSpeed;

        yield return new WaitForSeconds(_rollDuration);

        _isRolling = false;
        _playerRigidbody2D.linearVelocity = Vector2.zero;
        gameObject.layer = playerLayer; // <--- Retorna para a camada normal
    }
}