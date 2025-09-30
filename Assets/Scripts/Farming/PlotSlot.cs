using UnityEngine;
using System.Collections;
using TMPro;

public class PlotSlot : MonoBehaviour
{
    // Conectado via CropData.asset
    public CropData cropData;

    [Header("Slot State")]
    [SerializeField] private int daysSincePlanted;
    [SerializeField] private bool isPlanted = false;
    [SerializeField] private bool isWatered = false;
    [SerializeField] private bool isHarvestable = false;

    // Componentes para UI
    public GameObject interactionPrompt;
    public GameObject waterPrompt;

    // Componentes de Jogo
    private SpriteRenderer _spriteRenderer;
    private bool _isPlayerInside = false;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = null;

        // Inicializa prompts como invisíveis
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (waterPrompt != null) waterPrompt.SetActive(false);
    }

    void Update()
    {
        if (_isPlayerInside)
        {
            // Lógica para mostrar o prompt de regar
            if (isPlanted && !isWatered && !isHarvestable)
            {
                if (waterPrompt != null) waterPrompt.SetActive(true);
            }
            else
            {
                if (waterPrompt != null) waterPrompt.SetActive(false);
            }
        }

        // --- INTERAÇÃO COM 'E' (PLANTIO/REGA) ---
        if (Input.GetKeyDown(KeyCode.E) && _isPlayerInside)
        {
            if (!isPlanted)
        {
            // 1. Tenta obter o item selecionado na Hotbar
            Item selectedItem = InventoryManager.instance?.GetSelectedItem();

            // 2. Se houver um item e for uma semente...
            if (selectedItem != null && selectedItem.itemData.itemType == ItemType.Seed) // Supondo que você tem um Enum ItemType
            {
                // ... tenta plantar o CropData relacionado à semente
                CropData cropToPlant = selectedItem.itemData.relatedCropData;
                
                if (cropToPlant != null)
                {
                    TryPlanting(cropToPlant); 
                }
            }
            else
            {
                Debug.Log("Você não está segurando uma semente na Hotbar.");
            }
        }
            else if (isPlanted && !isWatered)
            {
                Water();
            }
        }

        // --- INTERAÇÃO COM ATAQUE (COLHEITA) ---
        if (Input.GetMouseButtonDown(0) && _isPlayerInside && isHarvestable)
        {
            Harvest();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            if (waterPrompt != null) waterPrompt.SetActive(false); // Esconde o balde ao sair
        }
    }

    public void Plant(CropData data)
    {
        this.cropData = data;
        isPlanted = true;
        daysSincePlanted = 0;
        isWatered = false;

        // Exibe o sprite da semente (primeiro sprite do array)
        if (cropData.growthSprites.Length > 0)
        {
            _spriteRenderer.sprite = cropData.growthSprites[0];
        }
    }

    public void Water()
    {
        isWatered = true;
        // Adicione um efeito visual de rega aqui, se desejar
    }

    public void Harvest()
    {
        if (InventoryManager.instance != null)
        {
            // Adiciona o item ao inventário
            InventoryManager.instance.AddItem(cropData.yieldItem, cropData.yieldAmount);
            Debug.Log("Colheita concluída! Adicionado ao inventário.");
        }

        // Reseta o slot para estado vazio
        isPlanted = false;
        isHarvestable = false;
        _spriteRenderer.sprite = null;
    }

    public void AdvanceDay()
    {
        Debug.Log($"Slot {gameObject.name}: Recebeu a ordem de avanço. isWatered: {isWatered}"); // <--- ADICIONE ISTO
    
    // A planta só cresce se estiver plantada E regada E não estiver para colheita.
        if (isPlanted && isWatered && !isHarvestable)
        {
            daysSincePlanted++; // AUMENTA O CONTADOR DE CRESCIMENTO!
            UpdateGrowthVisual();
        }
    
    // NOVO: Reseta a flag de rega para o novo dia,
    // independentemente de a planta ter crescido ou não.
    if (isPlanted)
    {
        isWatered = false;
        // Opcional: Se a planta não foi regada por X dias, ela morre aqui.
    }
}

    private void UpdateGrowthVisual()
    {
        // Verifica se chegou à maturidade
        if (daysSincePlanted >= cropData.daysToGrow)
        {
            isHarvestable = true;
            _spriteRenderer.sprite = cropData.growthSprites[cropData.growthSprites.Length - 1]; // Último sprite
            // O prompt de colheita agora será o botão de ataque, então o prompt de E pode sumir
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
        // Se ainda estiver crescendo
        else if (cropData.growthSprites.Length > daysSincePlanted)
        {
            _spriteRenderer.sprite = cropData.growthSprites[daysSincePlanted];
        }
    }
    public void TryPlanting(CropData data)
{
    // Verifica se o Inventário tem a semente antes de plantar
    if (InventoryManager.instance != null && InventoryManager.instance.HasItem(data.seedItem, 1))
    {
        InventoryManager.instance.RemoveItem(data.seedItem, 1);
        Plant(data); 
    }
    else
    {
        Debug.Log("Você não tem a semente para plantar!");
    }
}

// O campo cropData público agora pode ser removido ou usado internamente:
// [SerializeField] private CropData currentCropData; 
// public CropData cropData; // REMOVA ESTA LINHA OU MUDE PARA private/SerializeField
}