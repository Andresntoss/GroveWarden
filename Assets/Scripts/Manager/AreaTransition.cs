using UnityEngine;

public class AreaTransition : MonoBehaviour
{
    public AreaManager targetArea;
    public Vector2 spawnPointInTargetArea;
    
    [Header("Configurações de Interação")]
    public bool needsKeypress;
    public GameObject interactionPrompt;

    private bool _isPlayerInTrigger = false;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (needsKeypress && _isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            // Pega o jogador e o teletransporta com fade
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && FadeManager.instance != null)
            {
                StartCoroutine(FadeManager.instance.FadeAndTeleport(player, spawnPointInTargetArea));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
            if (!needsKeypress)
            {
                // Teleporta imediatamente se não precisar de tecla
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && FadeManager.instance != null)
                {
                    StartCoroutine(FadeManager.instance.FadeAndTeleport(player, spawnPointInTargetArea));
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}