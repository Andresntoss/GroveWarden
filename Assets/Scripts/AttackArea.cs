using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public float damageAmount = 20f;

    // Este método é chamado quando a área de ataque entra em colisão com outro objeto
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Linha 9: Adicionamos o Debug.Log para nos ajudar a encontrar a origem do dano
        Debug.Log("Trigger ativado por: " + other.gameObject.name);

        // Se o objeto colidido tiver a tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            // Tenta encontrar o componente Health
            Health enemyHealth = other.GetComponent<Health>();
            
            if (enemyHealth != null)
            {
                // Se encontrar, causa dano
                enemyHealth.TakeDamage(damageAmount);
            }
        }
    }
}