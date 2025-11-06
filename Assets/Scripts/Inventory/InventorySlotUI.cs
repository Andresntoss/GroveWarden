using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Configuração de Slot")]
    public int fixedSlotIndex; // O ID de 0 a 8

    [Header("Componentes do Item (Button)")]
    public Button itemButton; 
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    
    private Item _item;
    
    // --- LIGAÇÃO INICIAL ---
    void Start()
    {
        // Garante que o botão chame a função de seleção ao ser clicado
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(SelectSlot);
        }
    }
    
    // --- SELEÇÃO DE SLOT ---
    public void SelectSlot()
    {
        // Esta função será chamada pelo onClick do Button.
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.SelectSlot(fixedSlotIndex);
        }
    }

    // --- Configuração de Dados ---
    public void SetItem(Item item)
    {
        _item = item;
        
        // ... (Verificações de Segurança) ...

        if (item == null || item.itemData == null)
        {
            ClearSlot();
            return;
        }
        
        itemIcon.sprite = item.itemData.itemIcon;
        quantityText.text = item.quantity.ToString();
        
        // O Centralize precisa estar LIGADO, pois o item agora tem que dar snap no slot
        Centralize centralizeScript = itemButton.GetComponent<Centralize>();
        if (centralizeScript != null)
        {
            centralizeScript.enabled = true;
        }
        
        itemButton.gameObject.SetActive(true); 
    }
    
    public void ClearSlot()
    {
        _item = null;
        
        if (itemButton != null)
        {
            // O botão deve ser desativado para que não possa ser arrastado
            itemButton.gameObject.SetActive(false);
        }
        // ... (Restante da limpeza visual) ...
    }
    
    public Item GetItem()
    {
        return _item;
    }
}