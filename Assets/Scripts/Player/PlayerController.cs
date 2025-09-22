using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance; // O singleton do Player
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

    // ----- VARIÁVEIS PARA AS CAMADAS -----
    private int playerLayer;
    private int invincibleLayer;
    
    // ----- VARIÁVEIS DE KNOCKBACK -----
    [Header("Configurações de Knockback")]
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    // ----- VARIÁVEL PARA A ÁREA DE ATAQUE (Alterada para pública) -----
    [Header("Configurações de Ataque")]
    public GameObject attackArea; // Linha 29: Referência ao objeto AttackArea

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject); // <--- Não Destruir o player onde trocar de cena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _PlayerAnimator = GetComponent<Animator>();
        _playerInitialSpeed = _playerSpeed;
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

        if (!_isAttacking && !_isRolling && !isKnockedBack)
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
        if (!_isAttacking && !_isRolling && !isKnockedBack)
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
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0)) && !_isAttacking && !_isRolling && !isKnockedBack)
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
        _playerRigidbody2D.bodyType = isAttacking ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
    }

    void OnRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextRollTime && !_isAttacking && !isKnockedBack)
        {
            _isRolling = true;
            _PlayerAnimator.SetTrigger("Rolamento");
            _nextRollTime = Time.time + _rollCooldown;
            StartCoroutine(Roll());
        }
    }

    private IEnumerator Roll()
    {
        gameObject.layer = invincibleLayer;
        _playerRigidbody2D.linearVelocity = _playerDirection * _rollSpeed;
        yield return new WaitForSeconds(_rollDuration);
        _isRolling = false;
        _playerRigidbody2D.linearVelocity = Vector2.zero;
        gameObject.layer = playerLayer;
        _playerRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
    }

    // --- Funções para Animação Events ---
    public void EnableAttackArea()
    {
        Debug.Log("Ativando área de ataque!");
        if (attackArea != null)
        {
            attackArea.SetActive(true);
        }
    }

    public void DisableAttackArea()
    {
        Debug.Log("Desativando área de ataque!");
        if (attackArea != null)
        {
            attackArea.SetActive(false);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isKnockedBack) return;
        isKnockedBack = true;
        _playerRigidbody2D.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        StartCoroutine(ResetKnockback());
    }

    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        _playerRigidbody2D.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }
}