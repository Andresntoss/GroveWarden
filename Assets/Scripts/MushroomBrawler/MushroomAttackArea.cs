using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MushroomAttackArea : MonoBehaviour
{
    public float damageAmount = 20f;
    public float knockbackForce = 15f;
    [HideInInspector] public MushroomController owner;
    Collider2D hitbox;
    bool hasHit;
    void Awake() { hitbox = GetComponent<Collider2D>(); if (owner == null) owner = GetComponentInParent<MushroomController>(); }
    public void BeginSwing() { hasHit = false; }
    void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    void OnTriggerStay2D(Collider2D other) { TryHit(other); }
    void TryHit(Collider2D other)
    {
        // Detection triggers are not the target body.
        if (other.isTrigger) return;
        if (hasHit || hitbox == null || !hitbox.enabled || owner == null || !owner.IsAttackWindowOpen) return;
        Health target = other.GetComponentInParent<Health>();
        if (target == null || !target.CompareTag("Player")) return;
        int invincible = LayerMask.NameToLayer("Invencivel");
        if (invincible >= 0 && target.gameObject.layer == invincible) return;
        if (!target.TryTakeDamage(owner.damageAmount)) return;
        hasHit = true;
        if (target.IsDead) return;
        PlayerController pc = target.GetComponent<PlayerController>();
        if (pc != null) pc.ApplyKnockback((target.transform.position-owner.transform.position).normalized, owner.knockbackForce);
    }
}
