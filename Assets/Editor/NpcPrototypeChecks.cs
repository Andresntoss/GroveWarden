using System;
using UnityEditor;
using UnityEngine;

public static class NpcPrototypeChecks
{
    static void Check(bool value, string description) { if (!value) throw new Exception("Falha no teste NPC: "+description); }
    [MenuItem("Grove Warden/NPC/Validar protótipo (Play Mode)")]
    static void Run()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Entre em Play Mode para validar o NPC."); return; }
        var originalInventory = InventoryManager.instance;
        int originalCoins = CurrencyManager.coins;
        var originalProgress = NpcProgress.Capture();
        var go = new GameObject("Teste temporário de NPC");
        var profile = ScriptableObject.CreateInstance<NpcProfile>();
        var carrot = ScriptableObject.CreateInstance<ItemData>();
        profile.npcId = "test_" + Guid.NewGuid().ToString("N");
        profile.requestedItem=carrot; profile.galleryTarget=50; profile.upgradeCoinCost=100; profile.upgradeItemCost=10;
        carrot.maxStackSize=20;
        string prefix="GroveWarden.NpcPrototype.v1."+profile.npcId+".";
        try
        {
            InventoryManager.instance=null;
            var inventory=go.AddComponent<InventoryManager>();
            CurrencyManager.coins=100;
            Check(NpcProgress.Register(profile),"registro");
            Check(!NpcProgress.Known(profile),"ainda desconhecido");
            NpcProgress.Meet(profile);
            Check(inventory.TryAddItem(carrot,70),"adicionar em várias pilhas");
            Check(NpcProgress.Deliver(profile,20)==20 && NpcProgress.Delivered(profile)==20,"entrega parcial");
            Check(NpcProgress.Deliver(profile,99)==30 && NpcProgress.Delivered(profile)==50,"limite da entrega");
            Check(inventory.CountItem(carrot)==20,"quantidade restante");
            Check(NpcProgress.Deliver(profile,1)==0,"entrega após completar");
            CurrencyManager.coins=99;
            Check(!NpcProgress.BuyUpgrade(profile) && inventory.CountItem(carrot)==20,"saldo insuficiente não desconta itens");
            CurrencyManager.coins=100;
            Check(NpcProgress.BuyUpgrade(profile) && inventory.CountItem(carrot)==10 && CurrencyManager.coins==0,"compra e descontos");
            Check(NpcProgress.Bonus(profile)==1 && !NpcProgress.BuyUpgrade(profile),"bônus e compra única");
            Debug.Log("Grove Warden: verificações de entregas e melhorias passaram.");
        }
        finally
        {
            NpcProgress.Restore(originalProgress);
            UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(profile); UnityEngine.Object.DestroyImmediate(carrot);
            InventoryManager.instance=originalInventory; CurrencyManager.coins=originalCoins;
            if (CurrencyManager.instance != null) CurrencyManager.AddCoins(0);
        }
    }
}
