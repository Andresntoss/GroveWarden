using System;
using UnityEditor;
using UnityEngine;

public static class GroveInventoryChecks
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Inventory check failed: " + message);
    }
    [MenuItem("Grove Warden/Validar inventario (Play Mode)")]
    static void Run()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Entre em Play Mode antes de validar."); return; }
        // Temporarily use an isolated manager; restore the real singleton even on failure.
        InventoryManager original = InventoryManager.instance;
        GameObject go = null;
        ItemData a = ScriptableObject.CreateInstance<ItemData>();
        ItemData b = ScriptableObject.CreateInstance<ItemData>();
        try
        {
            InventoryManager.instance = null;
            go = new GameObject("Temporary inventory validation");
            var m = go.AddComponent<InventoryManager>();
            a.maxStackSize = 64; b.maxStackSize = 1;
            Check(m.TryAddItem(a, 70), "add 70");
            Check(m.InventorySlots[0].quantity == 64 && m.InventorySlots[1].quantity == 6, "split stacks");
            m.SwapItems(1, 9);
            Check(m.InventorySlots[1] == null && m.InventorySlots[9].quantity == 6, "move to bag");
            Check(m.TryRemoveItem(a, 60), "remove");
            m.SwapItems(9,0);
            Check(m.InventorySlots[0].quantity == 10 && m.InventorySlots[9] == null, "merge");
            Check(!m.TryRemoveItem(a,11) && m.CountItem(a)==10, "atomic failed removal");
            Check(m.TryAddItem(b,35), "fill all remaining slots");
            Check(!m.TryAddItem(b,1) && m.CountItem(b)==35, "full inventory retains counts");
            Check(m.TryAddItem(a,54) && m.CountItem(a)==64, "fill existing stack when no empty slots");
            Check(!m.TryAddItem(a,1), "reject full stacks");
            Check(!m.TryAddItem(null,1) && !m.TryAddItem(a,-1), "reject invalid input");
            m.SelectSlot(8); m.SelectSlot(9);
            Check(m.selectedSlotIndex==8, "selection limited to hotbar");
            m.SwapItems(0,1);
            Check(m.InventorySlots[1].itemData==a && m.InventorySlots[0].itemData==b, "swap distinct items");
            Debug.Log("Grove Warden: todas as verificações de inventário passaram.");
        }
        finally
        {
            if(go!=null) UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(a);
            UnityEngine.Object.DestroyImmediate(b);
            InventoryManager.instance = original;
        }
    }
}
