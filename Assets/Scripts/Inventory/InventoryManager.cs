using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public int selectedSlotIndex = 0;

    [Header("Configurações do Inventário")]
    public const int MAX_SLOTS = 36; // Tamanho Fixo da Mochila e Hotbar
    
    // Array privado para evitar conflitos de serialização, mas visível para debug
    [SerializeField] private Item[] _inventorySlots = new Item[MAX_SLOTS]; 

    // Propriedade pública para que scripts externos (HotbarUI) possam LER o array
    public Item[] InventorySlots => _inventorySlots; 
    
    public event Action OnInventoryChanged;
    public event Action<int> OnSlotSelected;

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
        
        // Garante que o array seja do tamanho correto se for a primeira vez que está sendo criado
        //if (_inventorySlots == null || _inventorySlots.Length != MAX_SLOTS)
        //{
            _inventorySlots = new Item[MAX_SLOTS];
        //}
    }

    // --- LÓGICA DE ADIÇÃO E REMOÇÃO ---
    public void AddItem(ItemData itemData, int quantity)
    {
        // 1. Tenta empilhar em slot existente
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (_inventorySlots[i] != null && _inventorySlots[i].itemData == itemData)
            {
                _inventorySlots[i].quantity += quantity;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
        
        // 2. Item novo: encontra o primeiro slot vazio
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (_inventorySlots[i] == null)
            {
                _inventorySlots[i] = new Item { itemData = itemData, quantity = quantity };
                OnInventoryChanged?.Invoke();
                return; 
            }
        }

        Debug.LogWarning("Inventário Cheio! Não há slots vazios.");
    }

    public void RemoveItem(ItemData itemData, int quantity)
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            Item item = _inventorySlots[i];
            
            if (item != null && item.itemData == itemData)
            {
                item.quantity -= quantity;
                
                if (item.quantity <= 0)
                {
                    _inventorySlots[i] = null; // Zera o slot
                }

                OnInventoryChanged?.Invoke(); 
                return;
            }
        }
    }

    // --- LÓGICA DE TROCA E SELEÇÃO ---
    
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0 || indexA >= MAX_SLOTS || indexB >= MAX_SLOTS)
        {
            Debug.LogError("Tentativa de troca em índice inválido.");
            return;
        }

        // Troca os itens no Array Fixo (Item ou null)
        Item temp = _inventorySlots[indexA];
        _inventorySlots[indexA] = _inventorySlots[indexB];
        _inventorySlots[indexB] = temp;

        OnInventoryChanged?.Invoke();
        SelectSlot(indexB); 
    }
    
    public void SelectSlot(int index)
    {
        if (index >= 0 && index < MAX_SLOTS)
        {
            selectedSlotIndex = index;
            OnSlotSelected?.Invoke(index);
            
            if (_inventorySlots[index] != null)
            {
                Debug.Log($"Slot {index + 1} selecionado: {_inventorySlots[index].itemData.itemName}");
            }
            else
            {
                Debug.Log($"Slot {index + 1} selecionado (Vazio).");
            }
        }
    }

    public Item GetSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < MAX_SLOTS)
        {
            return _inventorySlots[selectedSlotIndex];
        }
        return null; 
    }

    // --- LÓGICA DE CHECAGEM ---
    public bool HasItem(ItemData itemData, int requiredQuantity)
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            Item item = _inventorySlots[i];
            if (item != null && item.itemData == itemData && item.quantity >= requiredQuantity)
            {
                return true;
            }
        }
        return false;
    }
    
    // --- DEBUG ---
    public void DisplayInventory()
    {
        Debug.Log("--- Inventário ---");
        int filledSlots = _inventorySlots.Count(i => i != null);

        if (filledSlots == 0)
        {
            Debug.Log("O inventário está vazio.");
            return;
        }

        foreach (var item in _inventorySlots)
        {
            if (item != null)
            {
                Debug.Log($"SLOT: {item.itemData.itemName}: {item.quantity}");
            }
        }
        Debug.Log("------------------");
    }
}