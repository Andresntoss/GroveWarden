using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    // NOVO: Qual slot (índice da lista) está atualmente selecionado (0 a N)
    public int selectedSlotIndex = 0;

    [Header("Configurações do Inventário")]
    public int maxUniqueSlots = 30; // Limite de 30 tipos de itens diferentes
    public List<Item> inventoryItems = new List<Item>();

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ItemData itemData, int quantity)
    {
        // Verifica se o item já existe no inventário
        foreach (var item in inventoryItems)
        {
            if (item.itemData == itemData)
            {
                item.quantity += quantity;
                Debug.Log($"Adicionou {quantity} {itemData.itemName}. Quantidade total: {item.quantity}");
                return;
            }

            // Se o item não existe, checa se há espaço para um novo slot
            if (inventoryItems.Count >= maxUniqueSlots)
            {
                Debug.LogWarning("Inventário cheio! Não há espaço para novos tipos de item.");
                return; // Bloqueia a adição de um novo slot
            }
        }

        // Se o item não existe, adiciona-o à lista
        Item newItem = new Item { itemData = itemData, quantity = quantity };
        inventoryItems.Add(newItem);
        Debug.Log($"Novo item adicionado: {itemData.itemName} com quantidade {quantity}");

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData itemData, int quantity)
    {
        foreach (var item in inventoryItems)
        {
            if (item.itemData == itemData)
            {
                item.quantity -= quantity;
                if (item.quantity <= 0)
                {
                    inventoryItems.Remove(item);
                }
                Debug.Log($"Removeu {quantity} {itemData.itemName}. Quantidade restante: {item.quantity}");
                return;
            }
        }
        OnInventoryChanged?.Invoke();

        Debug.Log($"Não foi possível remover {itemData.itemName}. Item não encontrado.");
    }
    public void DisplayInventory()
    {
        Debug.Log("--- Inventário ---");
        if (inventoryItems.Count == 0)
        {
            Debug.Log("O inventário está vazio.");
            return;
        }

        foreach (var item in inventoryItems)
        {
            // Esta linha irá imprimir o nome do item e a quantidade
            Debug.Log($"{item.itemData.itemName}: {item.quantity}");
        }
        Debug.Log("------------------");
    }
    public bool HasItem(ItemData itemData, int requiredQuantity)
    {
        foreach (var item in inventoryItems)
        {
            // Verifica se o ItemData é o correto E se a quantidade é suficiente
            if (item.itemData == itemData && item.quantity >= requiredQuantity)
            {
                return true;
            }
        }
        return false;
    }

    // NOVO: Método para trocar o slot ativo
    public void SelectSlot(int index)
    {
        // Garante que o índice está dentro dos limites da lista
        if (index >= 0 && index < inventoryItems.Count)
        {
            selectedSlotIndex = index;
            Debug.Log($"Slot {index + 1} selecionado: {inventoryItems[index].itemData.itemName}");
            // [AQUI você chamaria um evento para notificar a UI de Hotbar]
        }
        else if (inventoryItems.Count > 0 && index >= 0 && index < maxUniqueSlots)
        {
            // Se o índice estiver vazio, mas dentro da capacidade máxima, apenas registre o índice
            selectedSlotIndex = index;
        }
    }

    // NOVO: Método para obter o item selecionado
    public Item GetSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < inventoryItems.Count)
        {
            return inventoryItems[selectedSlotIndex];
        }
        return null; // Retorna null se o slot estiver vazio
    }
}