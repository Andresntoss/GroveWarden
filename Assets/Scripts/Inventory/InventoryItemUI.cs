using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

// Implementa as interfaces de ARRASTO
[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    // Dados para o visual
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    // Dados para a troca (O Slot de Origem)
    public InventorySlotUI originSlot; 
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    // Configura o item fantasma antes de arrastar
    public void Setup(Item item, InventorySlotUI origin)
    {
        // Configura o visual
        itemIcon.sprite = item.itemData.itemIcon;
        quantityText.text = item.quantity.ToString();

        this.originSlot = origin;
        
        // Coloca o item no topo da hierarquia de UI
        transform.SetParent(origin.transform.root); 
        transform.SetAsLastSibling(); 
    }
    
    // --- MÉTODOS DE DRAG ---
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // O slot de origem já fez a checagem. Agora desliga o bloqueio do item arrastado.
        _canvasGroup.blocksRaycasts = false;
        itemIcon.raycastTarget = false; 
        
        // Esconde o sprite original
        originSlot.itemIcon.gameObject.SetActive(false); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Faz o objeto arrastável seguir o mouse
        transform.position = Input.mousePosition; 
    }
}