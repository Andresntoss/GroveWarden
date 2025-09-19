using UnityEngine;

public class MushroomAttackArea : MonoBehaviour
{
    public float damageAmount = 20f;
    public float knockbackForce = 15f;
    
    private bool _hasAttacked = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_hasAttacked)
        {
            // Aplica o dano e o knockback ao jogador
            Health playerHealth = other.GetComponent<Health>();
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
            
            if (playerController != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerController.ApplyKnockback(knockbackDirection, knockbackForce);
            }
            
            // Impede que o ataque dê dano mais de uma vez
            _hasAttacked = true;
        }
    }
    
    // Método para resetar o ataque quando ele for desativado
    private void OnDisable()
    {
        _hasAttacked = false;
    }
}