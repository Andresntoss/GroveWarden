using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public float damageAmount = 20f;
    public float knockbackForce = 10f;
    readonly HashSet<Health> hitTargets = new HashSet<Health>();
    Health ownerHealth;
    void Awake() { ownerHealth = GetComponentInParent<Health>(); }
    void OnEnable() { hitTargets.Clear(); }
    void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    void OnTriggerStay2D(Collider2D other) { TryHit(other); }
    void TryHit(Collider2D other)
    {
        // Detection triggers are not the target body.
        if (other.isTrigger) return;
        if (ownerHealth != null && ownerHealth.IsDead) return;
        Health target = other.GetComponentInParent<Health>();
        if (target == null || !target.CompareTag("Enemy") || hitTargets.Contains(target)) return;
        if (!target.TryTakeDamage(damageAmount)) return;
        hitTargets.Add(target);
        if (target.IsDead) return;
        Transform attacker = ownerHealth != null ? ownerHealth.transform : transform.root;
        var mushroom = target.GetComponent<MushroomController>();
        if (mushroom != null) mushroom.ReceiveHit(attacker.position, knockbackForce);
        else
        {
            var slime = target.GetComponent<SlimeController>();
            if (slime != null) slime.ApplyKnockback((target.transform.position-attacker.position).normalized,knockbackForce);
        }
    }
}
