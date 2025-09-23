using UnityEngine;
using System.Collections;

public class FaintManager : MonoBehaviour
{
    public static FaintManager instance;

    [Header("Referências")]
    public GameObject gameOverPanel; // Painel "Você desmaiou"
    public GameObject player;        // Referência ao player
    public Transform camaTransform;  // Local onde o player aparecerá após desmaiar

    private bool esperandoInput = false;

    private void Awake()
    {
        // Singleton simples
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Desativa o painel no início
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Chame este método quando a vida do jogador chegar a zero.
    /// </summary>
    public void AtivarDesmaio()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            esperandoInput = true;
        }

        // Pausa o jogo
        Time.timeScale = 0f;

        // Desativa PlayerController para evitar movimento
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
        }
    }

    private void Update()
    {
        // Aguarda input do jogador
        if (esperandoInput && Input.GetKeyDown(KeyCode.E))
        {
            esperandoInput = false;

            // Retoma o tempo antes do fade
            Time.timeScale = 1f;

            // Inicia o fade e move o jogador para a cama
            StartCoroutine(FadeAndMoveToBed());
        }
    }

    private IEnumerator FadeAndMoveToBed()
    {
        // Verifica se o FadeManager existe
        if (FadeManager.instance != null)
        {
            // Faz fade usando o FadeManager
            yield return StartCoroutine(FadeManager.instance.FadeAndTeleport(player, camaTransform.position));
        }
        else
        {
            // Se não houver FadeManager, move o player diretamente
            if (player != null && camaTransform != null)
                player.transform.position = camaTransform.position;
        }

        // Restaura vida do jogador
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.RestaurarVidaMaxima();
        }

        // Passa para o próximo dia
        if (TimeManager.instance != null)
            TimeManager.instance.PassarParaProximoDia();

        // Fecha o painel e reativa PlayerController
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = true;
        }
    }
}
