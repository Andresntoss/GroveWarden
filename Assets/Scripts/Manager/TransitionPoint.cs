using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
    [Header("Configurações de Transição")]
    public string sceneToLoad; // O nome da cena a ser carregada
    public bool needsKeypress; // Se a transição precisa da tecla 'E'
    public GameObject interactionPrompt; // O aviso visual (ícone do 'E')

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
            if (_transitionManager != null)
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
                // Se não precisar de tecla, carrega a cena imediatamente
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