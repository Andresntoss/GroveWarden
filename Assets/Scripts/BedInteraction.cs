using UnityEngine;

public class BedInteraction : MonoBehaviour
{
    // A referência ao script TimeManager
    private TimeManager _timeManager;
    
    // O objeto de aviso visual (o sprite ou texto) que você conectou no Inspector
    public GameObject avisoInteracao;

    // Flag para verificar se o jogador está na área de interação
    private bool _isPlayerInTrigger = false;

    void Start()
    {
        // Encontra o TimeManager na cena e o armazena
        _timeManager = FindAnyObjectByType<TimeManager>();

        // Começa com o aviso de interação desativado
        if (avisoInteracao != null)
        {
            avisoInteracao.SetActive(false);
        }
    }
    
    // O Input.GetKeyDown é verificado aqui, a cada frame
    void Update()
    {
        // Se o jogador estiver na área E pressionar a tecla 'E'
        if (_isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            // Chamamos a função para avançar o dia
            _timeManager.PassarParaProximoDia();
        }
    }

    // Chamado quando um objeto entra na área do gatilho
    void OnTriggerEnter2D(Collider2D other)
    {
        // Se o objeto que entrou for o jogador
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = true; // Ativa a flag

            if (avisoInteracao != null)
            {
                avisoInteracao.SetActive(true);
            }
        }
    }

    // Chamado quando um objeto sai da área do gatilho
    void OnTriggerExit2D(Collider2D other)
    {
        // Se o objeto que saiu for o jogador
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = false; // Desativa a flag

            if (avisoInteracao != null)
            {
                avisoInteracao.SetActive(false);
            }
        }
    }
}