using System;
using UnityEditor;
using UnityEngine;

public static class GroveFarmChecks
{
    static void Check(bool value,string text) { if(!value) throw new Exception("Farm check failed: "+text); }
    static FarmZone MakeZone(Transform parent)
    {
        var root = new GameObject("Temporary zone"); root.SetActive(false); root.transform.SetParent(parent);
        var zone = root.AddComponent<FarmZone>();
        zone.unlockedContent = new GameObject("Plantio"); zone.unlockedContent.transform.SetParent(root.transform);
        zone.purchaseSign = new GameObject("Placa"); zone.purchaseSign.transform.SetParent(root.transform);
        root.SetActive(true); return zone;
    }
    [MenuItem("Grove Warden/Validar compras e plantio (Play Mode)")]
    static void Run()
    {
        if(!Application.isPlaying) { Debug.LogWarning("Entre em Play Mode para validar."); return; }
        FarmManager originalFarm = FarmManager.instance;
        InventoryManager originalInventory = InventoryManager.instance;
        int coins = CurrencyManager.coins;
        var root = new GameObject("Temporary farm validation");
        ItemData seed = ScriptableObject.CreateInstance<ItemData>();
        ItemData product = ScriptableObject.CreateInstance<ItemData>();
        CropData crop = ScriptableObject.CreateInstance<CropData>();
        try
        {
            FarmManager.instance = null; InventoryManager.instance = null;
            var farm = root.AddComponent<FarmManager>();
            var inventory = root.AddComponent<InventoryManager>();
            CurrencyManager.coins = 100;
            var a = MakeZone(root.transform); var b = MakeZone(root.transform);
            Check(!a.unlockedContent.activeSelf && a.purchaseSign.activeSelf,"locked visuals");
            Check(farm.TryPurchase(a) && CurrencyManager.coins==90 && farm.CurrentPrice==20,"first purchase");
            Check(a.unlockedContent.activeSelf && !a.purchaseSign.activeSelf,"unlocked visuals");
            Check(!farm.TryPurchase(a) && CurrencyManager.coins==90,"duplicate purchase");
            Check(farm.TryPurchase(b) && CurrencyManager.coins==70 && farm.CurrentPrice==30,"global progressive price");
            var c = MakeZone(root.transform); CurrencyManager.coins = 0;
            Check(!farm.TryPurchase(c) && !c.IsUnlocked && farm.CurrentPrice==30,"insufficient money");
            var slotObject = new GameObject("Temporary plot"); slotObject.transform.SetParent(root.transform);
            slotObject.AddComponent<BoxCollider2D>();
            var slot = slotObject.AddComponent<PlotSlot>();
            seed.itemType = ItemType.Seed; seed.relatedCropData = crop;
            crop.growthSprites = new Sprite[2]; crop.daysToGrow = 1; crop.yieldItem = product; crop.yieldAmount = 2;
            Check(inventory.TryAddItem(seed,2),"seed setup");
            slot.Interact();
            Check(slot.IsPlanted && inventory.CountItem(seed)==1,"one seed planted");
            slot.AdvanceDay(); Check(!slot.IsHarvestable,"no growth without water");
            slot.Interact(); Check(slot.IsWatered,"watering");
            slot.AdvanceDay(); Check(slot.IsHarvestable,"growth");
            slot.Interact(); Check(!slot.IsPlanted && inventory.CountItem(product)==2,"harvest");
            slot.Interact(); slot.Water(); slot.AdvanceDay();
            Check(inventory.TryAddItem(product,64*36-2),"fill inventory");
            slot.Harvest(); Check(slot.IsHarvestable,"full inventory preserves crop");
            Debug.Log("Grove Warden: verificações de compra e plantio passaram. Confira mouse e visual na cena.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(seed); UnityEngine.Object.DestroyImmediate(product); UnityEngine.Object.DestroyImmediate(crop);
            FarmManager.instance = originalFarm; InventoryManager.instance = originalInventory;
            CurrencyManager.coins = coins;
            if(CurrencyManager.instance != null) CurrencyManager.AddCoins(0);
        }
    }
}
