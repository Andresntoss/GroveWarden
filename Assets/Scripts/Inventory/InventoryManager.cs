using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public const int MAX_SLOTS = 36;
    public const int HOTBAR_SLOTS = 9;
    public int selectedSlotIndex;
    [SerializeField] private Item[] _inventorySlots = new Item[MAX_SLOTS];
    public Item[] InventorySlots => _inventorySlots;
    public event Action OnInventoryChanged;
    public event Action<int> OnSlotSelected;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        if (_inventorySlots == null) _inventorySlots = new Item[MAX_SLOTS];
        else if (_inventorySlots.Length != MAX_SLOTS) Array.Resize(ref _inventorySlots, MAX_SLOTS);
        for (int i = 0; i < MAX_SLOTS; i++)
            if (_inventorySlots[i] != null && (_inventorySlots[i].itemData == null || _inventorySlots[i].quantity <= 0))
                _inventorySlots[i] = null;
        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, HOTBAR_SLOTS - 1);
    }
    void OnDestroy() { if (instance == this) instance = null; }
    static int Limit(ItemData data) => Mathf.Max(1, data.maxStackSize);
    public Item GetSelectedItem() => _inventorySlots[selectedSlotIndex];
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_SLOTS) return;
        selectedSlotIndex = index;
        OnSlotSelected?.Invoke(index);
    }
    public int CountItem(ItemData data)
    {
        int count = 0;
        if (data == null) return count;
        foreach (Item item in _inventorySlots)
            if (item != null && item.itemData == data) count += item.quantity;
        return count;
    }
    public bool HasItem(ItemData data, int quantity) => data != null && quantity > 0 && CountItem(data) >= quantity;
    public bool CanAddItem(ItemData data, int quantity)
    {
        if (data == null || quantity <= 0) return false;
        long capacity = 0;
        foreach (Item item in _inventorySlots)
            capacity += item == null ? Limit(data) : item.itemData == data ? Mathf.Max(0, Limit(data) - item.quantity) : 0;
        return capacity >= quantity;
    }
    // All-or-nothing: callers can preserve the source when the inventory is full.
    public bool TryAddItem(ItemData data, int quantity)
    {
        if (!CanAddItem(data, quantity)) return false;
        int remaining = quantity;
        foreach (Item item in _inventorySlots)
        {
            if (item == null || item.itemData != data) continue;
            int amount = Mathf.Min(remaining, Mathf.Max(0, Limit(data) - item.quantity));
            item.quantity += amount;
            remaining -= amount;
        }
        for (int i = 0; i < MAX_SLOTS && remaining > 0; i++)
        {
            if (_inventorySlots[i] != null) continue;
            int amount = Mathf.Min(remaining, Limit(data));
            _inventorySlots[i] = new Item { itemData = data, quantity = amount };
            remaining -= amount;
        }
        OnInventoryChanged?.Invoke();
        return true;
    }
    public void AddItem(ItemData data, int quantity)
    {
        if (!TryAddItem(data, quantity)) Debug.LogWarning("Não foi possível receber o item: dados inválidos ou inventário cheio.");
    }
    public bool TryRemoveItem(ItemData data, int quantity)
    {
        if (!HasItem(data, quantity)) return false;
        int remaining = quantity;
        for (int i = 0; i < MAX_SLOTS && remaining > 0; i++)
        {
            Item item = _inventorySlots[i];
            if (item == null || item.itemData != data) continue;
            int amount = Mathf.Min(remaining, item.quantity);
            item.quantity -= amount;
            remaining -= amount;
            if (item.quantity == 0) _inventorySlots[i] = null;
        }
        OnInventoryChanged?.Invoke();
        return true;
    }
    public void RemoveItem(ItemData data, int quantity) { TryRemoveItem(data, quantity); }
    public void SwapItems(int from, int to)
    {
        if (from < 0 || to < 0 || from >= MAX_SLOTS || to >= MAX_SLOTS || from == to) return;
        Item source = _inventorySlots[from], target = _inventorySlots[to];
        if (source == null) return;
        if (target != null && source.itemData == target.itemData)
        {
            int amount = Mathf.Min(source.quantity, Mathf.Max(0, Limit(target.itemData) - target.quantity));
            target.quantity += amount;
            source.quantity -= amount;
            if (source.quantity == 0) _inventorySlots[from] = null;
        }
        else { _inventorySlots[to] = source; _inventorySlots[from] = target; }
        OnInventoryChanged?.Invoke();
    }
    public void DisplayInventory()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
            if (_inventorySlots[i] != null) Debug.Log($"{i + 1}: {_inventorySlots[i].itemData.itemName} x{_inventorySlots[i].quantity}");
    }
}
