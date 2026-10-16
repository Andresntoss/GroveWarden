using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class MushroomController : MonoBehaviour
{
    public float _moveSpeed = 1.5f;
    public float damageAmount = 20f;
    public float knockbackForce = 15f;
    public float knockbackDuration = .2f;
    [Header("Configurações da IA")]
    public float detectionRadius = 5f;
    public float wanderTimeMax = 3f;
    public float idleTimeMax = 2f;
    [Header("Ataque e Stun")]
    public float attackRadius = 1.5f;
    [Tooltip("Intervalo mínimo após terminar um ataque.")]
    public float attackDelay = 1f;
    public float stunDuration = 3f;
    public Collider2D attackCollider;
    [Tooltip("Limite de segurança caso o evento final da animação não chegue.")]
    public float attackTimeout = 2.5f;

    enum State { Idle, Wander, Chase, Attack, Stun, Dead }
    State state;
    Rigidbody2D body;
    Animator animator;
    SpriteRenderer sprite;
    Health health;
    Transform player;
    Health playerHealth;
    MushroomAttackArea attackArea;
    Vector2 wanderDirection;
    Vector3 attackScale;
    float timer, nextAttack, nextPlayerSearch, knockbackUntil;
    bool windowOpened;
    public bool IsAttacking => isActiveAndEnabled && state == State.Attack && (health == null || !health.IsDead);
    public bool IsAttackWindowOpen => IsAttacking && windowOpened && attackCollider != null && attackCollider.enabled;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        if (attackCollider != null)
        {
            if (attackCollider.gameObject == gameObject)
            {
                Debug.LogError("Mushroom: a área de ataque precisa estar em um objeto filho.", this);
                enabled = false; return;
            }
            attackScale = attackCollider.transform.localScale;
            attackArea = attackCollider.GetComponent<MushroomAttackArea>();
            if (attackArea == null) attackArea = attackCollider.gameObject.AddComponent<MushroomAttackArea>();
            attackArea.owner = this;
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
        }
        else Debug.LogWarning("Mushroom: atribua Attack Collider no Inspector.", this);
        ChangeState(State.Idle);
    }
    void FindPlayer()
    {
        if (player != null || Time.time < nextPlayerSearch) return;
        nextPlayerSearch = Time.time + 1f;
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found == null) return;
        player = found.transform;
        playerHealth = found.GetComponent<Health>();
    }
    bool HasTarget => player != null && player.gameObject.activeInHierarchy && (playerHealth == null || !playerHealth.IsDead);
    void Update()
    {
        if (health != null && health.IsDead) { StopForDeath(); return; }
        FindPlayer();
        timer -= Time.deltaTime;
        if (state == State.Stun)
        {
            if (timer <= 0) ChangeState(State.Idle);
            return;
        }
        if (state == State.Attack)
        {
            if (!HasTarget || timer <= 0) AttackAnimationFinished();
            return;
        }
        if (Time.time < knockbackUntil) return;
        if (HasTarget)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRadius)
            {
                Face(player.position.x - transform.position.x);
                if (Time.time >= nextAttack) ChangeState(State.Attack);
                else if (state != State.Idle) ChangeState(State.Idle);
                return;
            }
            if (distance <= detectionRadius)
            {
                if (state != State.Chase) ChangeState(State.Chase);
                return;
            }
        }
        if (state == State.Chase) ChangeState(State.Idle);
        else if (timer <= 0) ChangeState(state == State.Idle ? State.Wander : State.Idle);
    }
    void FixedUpdate()
    {
        if (state == State.Dead || Time.time < knockbackUntil) return;
        Vector2 velocity = Vector2.zero;
        if (state == State.Wander) velocity = wanderDirection * _moveSpeed;
        if (state == State.Chase && HasTarget) velocity = ((Vector2)(player.position - transform.position)).normalized * _moveSpeed;
        body.linearVelocity = velocity;
        if (Mathf.Abs(velocity.x) > .05f) Face(velocity.x);
    }
    void Face(float direction)
    {
        if (Mathf.Abs(direction) < .01f) return;
        // Original Mushroom sprites face left when flipX is false.
        sprite.flipX = direction > 0;
        if (attackCollider != null)
        {
            Vector3 scale = attackScale;
            scale.x = Mathf.Abs(scale.x) * (sprite.flipX ? -1f : 1f);
            attackCollider.transform.localScale = scale;
        }
    }
    void ChangeState(State value)
    {
        DeactivateAttackCollider();
        state = value;
        body.linearVelocity = Vector2.zero;
        if (value == State.Idle) { timer = Random.Range(.5f, Mathf.Max(.5f,idleTimeMax)); animator.SetInteger("Estado",0); }
        if (value == State.Wander) { timer = Random.Range(.5f, Mathf.Max(.5f,wanderTimeMax)); wanderDirection = Random.insideUnitCircle.normalized; animator.SetInteger("Estado",1); }
        if (value == State.Chase) animator.SetInteger("Estado",1);
        if (value == State.Attack)
        {
            if (HasTarget) Face(player.position.x - transform.position.x);
            timer = Mathf.Max(1.25f,attackTimeout);
            windowOpened = false;
            animator.ResetTrigger("AttackFinished");
            animator.SetInteger("Estado",2);
        }
        if (value == State.Stun) { timer = Mathf.Max(.1f,stunDuration); animator.SetInteger("Estado",3); }
    }
    public void ActivateAttackCollider()
    {
        if (!IsAttacking || windowOpened || attackCollider == null) return;
        windowOpened = true;
        attackArea.BeginSwing();
        attackCollider.enabled = true;
    }
    public void DeactivateAttackCollider() { if (attackCollider != null) attackCollider.enabled = false; }
    public void AttackAnimationFinished()
    {
        if (!IsAttacking) return;
        nextAttack = Time.time + Mathf.Max(.1f,attackDelay);
        animator.SetTrigger("AttackFinished");
        ChangeState(State.Idle);
    }
    // Called once by the player's damage area, only after accepted damage.
    public void ReceiveHit(Vector2 attackerPosition, float force)
    {
        if (!isActiveAndEnabled || (health != null && health.IsDead)) return;
        float dx = attackerPosition.x - transform.position.x;
        bool behind = sprite.flipX ? dx < -.05f : dx > .05f;
        if (behind) { ChangeState(State.Stun); return; }
        // Preserve facing and current animation; use a short physical recoil.
        if (state != State.Attack && state != State.Stun)
        {
            knockbackUntil = Time.time + Mathf.Max(0,knockbackDuration);
            body.linearVelocity = Vector2.zero;
            body.AddForce(((Vector2)transform.position-attackerPosition).normalized * force, ForceMode2D.Impulse);
        }
    }
    public void StopForDeath()
    {
        state = State.Dead;
        DeactivateAttackCollider();
        if (body != null) body.linearVelocity = Vector2.zero;
        enabled = false;
    }
    void OnDisable()
    {
        DeactivateAttackCollider();
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}
