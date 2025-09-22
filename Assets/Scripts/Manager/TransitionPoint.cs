using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
    [Header("Configurações de Transição")]
    public string sceneToLoad;
    public bool needsKeypress;
    public GameObject interactionPrompt;

    private bool _isPlayerInTrigger = false;
    private TransitionManager _transitionManager;

    void Start()
    {
        _transitionManager = FindAnyObjectByType<TransitionManager>();
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (needsKeypress && _isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
{
    if (sceneToLoad == "voltar") // ou qualquer string que você defina para o retorno
    {
        _transitionManager.LoadPreviousScene();
    }
    else
    {
        _transitionManager.LoadScene(sceneToLoad);
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
                if (_transitionManager != null)
                {
                    _transitionManager.LoadScene(sceneToLoad);
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