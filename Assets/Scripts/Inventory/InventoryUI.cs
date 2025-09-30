using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; // O GameObject do painel que será ativado/desativado
    public InventorySlotUI slotUIPrefab; // O Prefab visual do seu slot
    public Transform slotsParent; // O Container (Layout Group) que segura os slots

    // Lista para manter a referência dos slots já criados na tela
    private List<InventorySlotUI> activeUISlots = new List<InventorySlotUI>();

    void Start()
    {
        // Garante que o inventário comece desativado
        inventoryPanel.SetActive(false); 
        
        if (InventoryManager.instance != null)
        {
            // O UI se inscreve no evento de mudança de inventário
            InventoryManager.instance.OnInventoryChanged += UpdateInventoryDisplay;
        }
        
        // Chamada inicial para preencher a UI com os slots vazios
        UpdateInventoryDisplay();
    }

    // Chamado sempre que um item é adicionado ou removido
    public void UpdateInventoryDisplay()
    {
        // Se o InventoryManager for null, não faz nada
        if (InventoryManager.instance == null) return;

        List<Item> items = InventoryManager.instance.inventoryItems;
        
        // Garante que a UI tenha slots visuais suficientes para os itens
        for (int i = 0; i < items.Count; i++)
        {
            if (i >= activeUISlots.Count)
            {
                // Cria um novo slot UI se não houver slots suficientes
                InventorySlotUI newSlot = Instantiate(slotUIPrefab, slotsParent);
                activeUISlots.Add(newSlot);
            }
            
            // Atribui os dados do item ao slot visual
            activeUISlots[i].SetItem(items[i]);
            activeUISlots[i].gameObject.SetActive(true);
        }
        
        // Esconde os slots de UI extras (se o jogador tiver menos itens que o máximo de slots)
        for (int i = items.Count; i < activeUISlots.Count; i++)
        {
            activeUISlots[i].ClearSlot();
            activeUISlots[i].gameObject.SetActive(false);
        }
    }
}