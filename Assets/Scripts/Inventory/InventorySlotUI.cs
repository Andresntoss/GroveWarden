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
    // No script InventorySlotUI.cs
public void SetItem(Item item)
{
    // --- 1. VERIFICAÇÃO DE COMPONENTES VISUAIS (DEBUG FATAL) ---
    if (itemIcon == null || quantityText == null)
    {
        Debug.LogError($"[ERRO FATAL] SLOT {gameObject.name}: ItemIcon ou QuantityText estão NULOS no Prefab. Reveja a conexão.");
        return; 
    }
    
    // --- 2. VERIFICAÇÃO DE DADOS (Checa se o item está corrompido ou é nulo) ---
    if (item == null || item.itemData == null)
    {
        ClearSlot(); // Se o item estiver corrompido, limpa o slot para evitar crash
        return;
    }
    // -------------------------------------------------------------------------
    
    _item = item;
    
    // As linhas de exibição (sempre seguras aqui)
    itemIcon.sprite = item.itemData.itemIcon;
    quantityText.text = item.quantity.ToString();
    
    // CRUCIAL: Ativa apenas o ícone, o slot em si está sempre ativo
    itemIcon.gameObject.SetActive(true);
}
    
        public void ClearSlot() // Limpa o slot
{
    _item = null;
    
    
    if (itemIcon != null) // Garante que os componentes não sejam nulos antes de tentar usá-los
    {
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false); // Desativa o visual
    }

    if (quantityText != null)
    {
        quantityText.text = "";
    }
    
    // O GameObject raiz do slot (com o Box Collider) permanece ativo para o DND
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
        // A seleção deve sempre ocorrer, para que o jogador saiba qual slot está ativo, 
    // mesmo que vazio.
            InventoryManager.instance.SelectSlot(fixedSlotIndex);
        
    }
}