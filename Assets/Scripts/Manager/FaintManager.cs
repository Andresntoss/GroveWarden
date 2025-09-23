using System.Collections;
using UnityEngine;

public class FaintManager : MonoBehaviour
{
    public static FaintManager instance;

    [Header("Referências")]
    public Transform camaPosition;

    [Header("Mensagem de Desmaio")]
    [TextArea]
    public string faintMessage = "Você desmaiou! Pressione E para continuar.";

    private PlayerController playerController;
    private Health playerHealth;
    private Animator playerAnimator;

    private bool isFainting = false;

    private void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerHealth = player.GetComponent<Health>();
            playerAnimator = player.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("[FaintManager] Jogador não encontrado!");
        }
    }

    // Chamada quando o jogador morre
    public void TriggerFaint()
    {
        if (!isFainting)
        {
            isFainting = true;
            StartCoroutine(FaintSequence());
        }
    }

    private IEnumerator FaintSequence()
    {
        // Pausa o jogador
        if (playerController != null)
            playerController.enabled = false;

        // Pausa o tempo do jogo e mostra mensagem
        Time.timeScale = 0f;
        UIManager.instance.ShowGameOverPanel();

        // Altera o texto do painel para a mensagem definida
        TMPro.TextMeshProUGUI textUI = UIManager.instance.gameOverPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textUI != null)
            textUI.text = faintMessage;

        // Espera o jogador apertar E
        yield return StartCoroutine(WaitForPlayerInput());

        // Esconde painel e retoma tempo
        UIManager.instance.HideGameOverPanel();
        Time.timeScale = 1f;

        // Fade e teleporte para a cama
        if (FadeManager.instance != null)
            yield return StartCoroutine(FadeManager.instance.FadeAndTeleport(playerController.gameObject, camaPosition.position));

        // Recupera vida completa
        playerHealth?.RestoreFullHealth();

        // Avança o dia
        FindAnyObjectByType<TimeManager>()?.PassarParaProximoDia();

        // Reseta animação para Idle
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        // Reativa jogador
        if (playerController != null)
            playerController.enabled = true;

        isFainting = false;
    }

    private IEnumerator WaitForPlayerInput()
    {
        while (!Input.GetKeyDown(KeyCode.E))
            yield return null;
    }
}
