using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    float currentHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }
    [Header("Invulnerabilidade")]
    public float invulnerabilityDuration = 1.5f;
    public Color invulnerableColor = Color.white;
    bool isInvulnerable;
    SpriteRenderer spriteRenderer;
    Color originalColor;
    Animator animator;
    Rigidbody2D body;
    Collider2D[] colliders;
    bool[] initialColliderStates;
    Coroutine invulnerability;
    public int coinDropAmount;
    [Header("Drop de Itens")]
    public ItemData itemToDrop;
    public int dropAmount = 1;
    [Header("Remoção de inimigos")]
    [Tooltip("Segurança caso o evento da animação de morte não seja executado.")]
    public float deathCleanupDelay = 5f;

    void Awake()
    {
        currentHealth = Mathf.Max(1,maxHealth);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);
        initialColliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++) initialColliderStates[i] = colliders[i].enabled;
    }
    // Compatibility for existing enemy scripts and UI.
    public void TakeDamage(float amount) { TryTakeDamage(amount); }
    public bool TryTakeDamage(float amount)
    {
        if (IsDead || isInvulnerable || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return false;
        if (CompareTag("Player"))
        {
            int layer = LayerMask.NameToLayer("Invencivel");
            if (layer >= 0 && gameObject.layer == layer) return false;
        }
        currentHealth = Mathf.Max(0,currentHealth-amount);
        if (currentHealth == 0) Die();
        else invulnerability = StartCoroutine(BecomeInvulnerable());
        return true;
    }
    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        if (invulnerability != null) StopCoroutine(invulnerability);
        isInvulnerable = false;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (body != null) { body.linearVelocity = Vector2.zero; body.angularVelocity = 0; }
        foreach (var collider in colliders) if (collider != null) collider.enabled = false;
        if (CompareTag("Enemy"))
        {
            // Disable only behaviour controllers; leave Health and Animator running.
            var mushroom = GetComponent<MushroomController>();
            if (mushroom != null) mushroom.StopForDeath();
            StopEnemy(GetComponent<SlimeController>());
            StopEnemy(GetComponent<BeetleController>());
            StopEnemy(GetComponent<WormController>());
            if (body != null) body.simulated = false;
            CurrencyManager.AddCoins(Mathf.Max(0,coinDropAmount));
            if (itemToDrop != null && dropAmount > 0 &&
                (InventoryManager.instance == null || !InventoryManager.instance.TryAddItem(itemToDrop,dropAmount)))
                InventoryPickup.Spawn(itemToDrop,dropAmount,transform.position);
            StartCoroutine(CleanupEnemy());
        }
        SetDeathAnimation();
        if (CompareTag("Player"))
        {
            var pc = GetComponent<PlayerController>();
            if (pc != null) { pc.DisableAttackArea(); pc.enabled = false; }
            FaintManager.instance?.TriggerFaint();
        }
    }
    static void StopEnemy(MonoBehaviour controller)
    {
        if (controller == null) return;
        controller.StopAllCoroutines();
        controller.enabled = false;
    }
    void SetDeathAnimation()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == "Estado" && parameter.type == AnimatorControllerParameterType.Int) animator.SetInteger("Estado",-1);
            if (parameter.type == AnimatorControllerParameterType.Trigger) animator.ResetTrigger(parameter.nameHash);
        }
        foreach (var parameter in animator.parameters)
            if (parameter.name == "Morte" && parameter.type == AnimatorControllerParameterType.Trigger) animator.SetTrigger(parameter.nameHash);
    }
    IEnumerator CleanupEnemy()
    {
        yield return new WaitForSeconds(Mathf.Max(.1f,deathCleanupDelay));
        if (IsDead && CompareTag("Enemy")) Destroy(gameObject);
    }
    public void DestroyAfterAnimation()
    {
        if (IsDead && CompareTag("Enemy")) Destroy(gameObject);
    }
    public void RestoreFullHealth()
    {
        // Player revival; enemy respawns should create a new prefab instance.
        if (CompareTag("Enemy") && IsDead) return;
        StopAllCoroutines();
        currentHealth = Mathf.Max(1,maxHealth);
        IsDead = false;
        isInvulnerable = false;
        invulnerability = null;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = initialColliderStates[i];
        var pc = GetComponent<PlayerController>();
        if (pc != null) { pc.DisableAttackArea(); pc.enabled = true; }
        if (animator != null) { animator.Rebind(); animator.Update(0f); }
    }
    IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        if (spriteRenderer != null) spriteRenderer.color = invulnerableColor;
        yield return new WaitForSeconds(Mathf.Max(0,invulnerabilityDuration));
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        isInvulnerable = false;
        invulnerability = null;
    }
}
