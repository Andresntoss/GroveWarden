using UnityEngine;

public class BuildSign : MonoBehaviour
{
    [Header("Configurações do Módulo")]
    public GameObject modulePrefab; // O PlotModulePrefab para esta placa (arrastado do Inspector)
    public int moduleCost;
    public string moduleName = "Canteiro de Plantio";

    [Header("UI de Interação")]
    public GameObject interactionPrompt; // Ícone 'E'

    private bool _isPlayerInTrigger = false;
    private FarmManager _farmManager;

    void Start()
    {
        _farmManager = FarmManager.instance;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (_isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (_farmManager != null)
            {
                // Tenta construir o módulo
                bool success = _farmManager.TryBuildModule(transform.position, modulePrefab, moduleCost);
                
                if (success)
                {
                    // Se a construção for bem-sucedida, a placa deve desaparecer
                    Destroy(gameObject);
                }
                // Adicione uma lógica de feedback visual (UI text) aqui se quiser.
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