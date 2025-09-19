using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public float damageAmount = 20f;
    public float knockbackForce = 10f; // <--- Força do knockback do jogador

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            SlimeController slimeController = other.GetComponent<SlimeController>(); // <--- Obtém o SlimeController
            
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
            }
            
            if (slimeController != null)
            {
                // Calcula a direção do knockback (do jogador para o slime)
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                // Aplica o knockback
                slimeController.ApplyKnockback(knockbackDirection, knockbackForce);
            }
        }
    }
}