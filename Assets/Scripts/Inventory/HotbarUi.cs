using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public List<InventorySlotUI> slots = new List<InventorySlotUI>(); 
    public Image selectionIndicator; 

    void Start()
    {
        if (InventoryManager.instance != null)
        {
            // Escuta por mudanças no inventário e na seleção
            InventoryManager.instance.OnInventoryChanged += UpdateInventoryDisplay;
            InventoryManager.instance.OnSlotSelected += HighlightSlot;
            
            UpdateInventoryDisplay(); 
            
            if (slots.Count > 0)
            {
                HighlightSlot(InventoryManager.instance.selectedSlotIndex);
            }
        }
    }

    public void UpdateInventoryDisplay()
{
    if (InventoryManager.instance == null) return;

    Item[] slotsData = InventoryManager.instance.InventorySlots; 
    
    // NOVO: O loop não vai além do número de slots que você conectou (slots.Count)
    for (int i = 0; i < slots.Count; i++) // Itera apenas pelo número de slots conectados
    {
        // Certifique-se de que o slot exista no array de dados antes de usá-lo
        if (i < InventoryManager.MAX_SLOTS && slotsData[i] != null) 
        {
            slots[i].SetItem(slotsData[i]);
        }
        else
        {
            slots[i].ClearSlot();
        }
    }
}

    private void HighlightSlot(int index)
    {
        if (index >= 0 && index < slots.Count && selectionIndicator != null)
        {
            selectionIndicator.transform.localPosition = slots[index].transform.localPosition; 
        }
    }
}