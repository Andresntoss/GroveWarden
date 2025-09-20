using UnityEngine;
using TMPro;

public class SignInteraction : MonoBehaviour
{
    [Header("UI de Interação")]
    public GameObject interactionPrompt; // O aviso visual (ex: ícone do 'E')
    public string signMessage; // O texto da mensagem

    // --- Variáveis para a conexão automática ---
    private GameObject messagePanel;
    private TextMeshProUGUI messageText;

    private bool _isPlayerInTrigger = false;

    void Start()
    {
        // Encontra o Canvas principal do jogo
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();

        if (mainCanvas != null)
        {
            // Procura o painel e o texto como filhos do Canvas
            Transform panelTransform = mainCanvas.transform.Find("PanelPlaca");
            if (panelTransform != null)
            {
                messagePanel = panelTransform.gameObject;
                messageText = panelTransform.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (_isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (messagePanel != null)
            {
                messagePanel.SetActive(!messagePanel.activeSelf);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            if (messageText != null)
            {
                messageText.text = signMessage;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
        }
    }
}