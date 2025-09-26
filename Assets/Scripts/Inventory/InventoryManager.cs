using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public List<Item> inventoryItems = new List<Item>();

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
        }

        // Se o item não existe, adiciona-o à lista
        Item newItem = new Item { itemData = itemData, quantity = quantity };
        inventoryItems.Add(newItem);
        Debug.Log($"Novo item adicionado: {itemData.itemName} com quantidade {quantity}");
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
}