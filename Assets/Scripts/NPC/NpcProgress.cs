using System.Collections.Generic;
using UnityEngine;

// Progress is held in memory and saved together with the rest of the game at the bed.
public static class NpcProgress
{
    static readonly Dictionary<string,NpcProfile> Profiles = new Dictionary<string,NpcProfile>();
    static readonly Dictionary<string,SavedNpc> States = new Dictionary<string,SavedNpc>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession() { Profiles.Clear(); States.Clear(); }
    static SavedNpc State(NpcProfile p)
    {
        if (!States.TryGetValue(p.npcId,out var state)) { state = new SavedNpc { id=p.npcId }; States[p.npcId]=state; }
        return state;
    }
    public static bool Register(NpcProfile p)
    {
        if (p == null || string.IsNullOrWhiteSpace(p.npcId)) { Debug.LogError("NPC precisa de um ID estável."); return false; }
        if (Profiles.TryGetValue(p.npcId,out var previous) && previous != null && previous != p)
        { Debug.LogError("IDs de NPC duplicados: "+p.npcId,p); return false; }
        Profiles[p.npcId]=p; return true;
    }
    public static bool Known(NpcProfile p) => p != null && State(p).known;
    public static int Delivered(NpcProfile p) => Mathf.Clamp(State(p).delivered,0,Mathf.Max(1,p.galleryTarget));
    public static bool Upgraded(NpcProfile p) => p != null && State(p).upgrade;
    public static void Meet(NpcProfile p) { if (Register(p)) State(p).known=true; }
    public static int Bonus(NpcProfile p) => p != null && Upgraded(p) ? Mathf.Max(0,p.harvestBonus) : 0;
    public static int TotalHarvestBonus()
    {
        int total=0; foreach(var p in Profiles.Values) if(p != null && Known(p)) total += Bonus(p); return total;
    }
    public static int Deliver(NpcProfile p,int requested)
    {
        if (!Known(p) || p.requestedItem == null || requested <= 0 || InventoryManager.instance == null) return 0;
        int amount=Mathf.Min(requested,Mathf.Max(1,p.galleryTarget)-Delivered(p),InventoryManager.instance.CountItem(p.requestedItem));
        if (amount<=0 || !InventoryManager.instance.TryRemoveItem(p.requestedItem,amount)) return 0;
        State(p).delivered += amount; return amount;
    }
    public static bool BuyUpgrade(NpcProfile p)
    {
        if(!Known(p) || Upgraded(p) || InventoryManager.instance==null || p.requestedItem==null) return false;
        int money=Mathf.Max(0,p.upgradeCoinCost), items=Mathf.Max(0,p.upgradeItemCost);
        if(CurrencyManager.coins<money || (items>0 && !InventoryManager.instance.HasItem(p.requestedItem,items))) return false;
        if(items>0 && !InventoryManager.instance.TryRemoveItem(p.requestedItem,items)) return false;
        if(money>0) CurrencyManager.RemoveCoins(money);
        State(p).upgrade=true; return true;
    }
    public static List<SavedNpc> Capture()
    {
        var result=new List<SavedNpc>();
        foreach(var s in States.Values) result.Add(new SavedNpc {id=s.id,known=s.known,delivered=s.delivered,upgrade=s.upgrade});
        return result;
    }
    public static void Restore(List<SavedNpc> saved)
    {
        States.Clear();
        foreach(var s in saved) States[s.id]=new SavedNpc {id=s.id,known=s.known,delivered=s.delivered,upgrade=s.upgrade};
    }
}
