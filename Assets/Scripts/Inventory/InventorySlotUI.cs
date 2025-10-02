using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // <--- NOVO: Essencial para o DND

// ADICIONAMOS AS INTERFACES DE DRAG & DROP
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IDropHandler
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    
    [Header("Configuração de Slot")]
    public int fixedSlotIndex; // O ID de 0 a 35 (definido no Inspector)

    private Item _item;
    private Transform _originalParent;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    
    // Configura o slot para exibir o item
    public void SetItem(Item item)
    {
        _item = item;
        itemIcon.sprite = item.itemData.itemIcon;
        quantityText.text = item.quantity.ToString();
        gameObject.SetActive(true);
    }
    
    // Limpa o slot
    public void ClearSlot()
    {
        _item = null;
        itemIcon.sprite = null;
        quantityText.text = "";
        gameObject.SetActive(false);
    }
    
    // --- LÓGICA DE DRAG AND DROP (INÍCIO) ---
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null) return; // Não pode arrastar slot vazio

        _originalParent = transform.parent;
        transform.SetParent(transform.root); // Coloca o slot no topo da tela (visualmente)
        _canvasGroup.blocksRaycasts = false; // CRUCIAL: Permite que o mouse 'veja' o slot alvo
        itemIcon.raycastTarget = false; // Desliga o raycast no ícone
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_item == null) return;
        transform.position = eventData.position; // Segue o mouse
    }

    public void OnDrop(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        
        InventorySlotUI sourceSlot = eventData.pointerDrag.GetComponent<InventorySlotUI>();

        if (sourceSlot != null && InventoryManager.instance != null)
        {
            // Índices de 0 a 35 são usados para o swap
            int indexA = sourceSlot.fixedSlotIndex; 
            int indexB = this.fixedSlotIndex;

            // 1. Chama o método de troca de dados no InventoryManager
            InventoryManager.instance.SwapItems(indexA, indexB);
        }

        // 2. Garante que o item arrastado volte para o painel correto
        sourceSlot.transform.SetParent(_originalParent);
        sourceSlot.transform.localPosition = Vector3.zero;
        sourceSlot.itemIcon.raycastTarget = true;
    }
    
    // --- LÓGICA DE CLIQUE (SELEÇÃO) ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item != null)
        {
            InventoryManager.instance.SelectSlot(fixedSlotIndex);
        }
    }
}