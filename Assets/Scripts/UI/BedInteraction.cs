using UnityEngine;
using System.Collections;

public class BedInteraction : MonoBehaviour
{
    private FadeManager _fadeManager; // Referência para o FadeManager

    [Header("UI de Interação")]
    public GameObject interactionPrompt; // O aviso visual (ícone do 'E')

    private bool _isPlayerInTrigger = false;
    private bool _canSleep = true; // Controla se o jogador pode dormir

    void Start()
    {
        _fadeManager = FindAnyObjectByType<FadeManager>();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Se a tecla for pressionada e o jogador estiver na área
        if (Input.GetKeyDown(KeyCode.E) && _isPlayerInTrigger && _canSleep)
        {
            if (_fadeManager != null)
            {
                // Bloqueia novas tentativas até o fade terminar
                _canSleep = false;
                StartCoroutine(SleepRoutine());
            }
            else
            {
                Debug.LogWarning("[BedInteraction] FadeManager não encontrado na cena.");
            }
        }
    }

    private IEnumerator SleepRoutine()
    {
        // Espera a animação de fade + avanço de dia terminar
        yield return StartCoroutine(_fadeManager.FadeAndPassDay());

        // Libera novamente para permitir dormir no próximo dia
        _canSleep = true;
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
