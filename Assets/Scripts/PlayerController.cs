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

    void Start()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _PlayerAnimator = GetComponent<Animator>();

        _playerInitialSpeed = _playerSpeed;
    }

    void Update()
    {
        _playerDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        OnAttack();

        if (!_isAttacking)
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
        if (!_isAttacking)
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
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0)) && !_isAttacking)
        {
            _isAttacking = true;
            _PlayerAnimator.SetTrigger("Ataque");
            
            SetAttackState(true); // <--- Adicione esta linha
            
            StartCoroutine(ResetAttackAfterDelay());
        }
    }

    private IEnumerator ResetAttackAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        SetAttackState(false); // <--- Adicione esta linha
    }

    // <--- Adicione este novo método
    private void SetAttackState(bool isAttacking)
    {
        _isAttacking = isAttacking;
        _playerSpeed = isAttacking ? 0 : _playerInitialSpeed;
        _playerRigidbody2D.isKinematic = isAttacking;
    }
}