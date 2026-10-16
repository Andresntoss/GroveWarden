using UnityEngine;

[DefaultExecutionOrder(-100)]
public class FarmManager : MonoBehaviour
{
    public static FarmManager instance;
    [Header("Preço global de expansão")]
    public int plotCost = 10;
    [Min(0)] public int priceIncrease = 10;
    [SerializeField, Min(0)] int purchasedAreas;
    [Header("Compatibilidade com placas antigas")]
    public GameObject plotModulePrefab;
    bool purchasing;
    public int PurchasedAreas => purchasedAreas;
    public int CurrentPrice => (int)System.Math.Min(int.MaxValue,(long)Mathf.Max(0,plotCost)+(long)Mathf.Max(0,priceIncrease)*Mathf.Max(0,purchasedAreas));
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
    }
    void Start() { FarmInteraction.EnsureExists(); }
    void OnDestroy() { if (instance == this) instance = null; }
    public bool TryPurchase(FarmZone zone)
    {
        if (zone == null || zone.IsUnlocked || !zone.IsConfigured || purchasing) return false;
        int cost = CurrentPrice;
        if (CurrencyManager.coins < cost) { Debug.Log("Moedas insuficientes. Preço: "+cost); return false; }
        purchasing = true;
        try
        {
            if (!zone.Unlock()) return false;
            CurrencyManager.RemoveCoins(cost);
            purchasedAreas = Mathf.Min(purchasedAreas, int.MaxValue-1)+1;
            Debug.Log("Área comprada por "+cost+" moedas. Próxima: "+CurrentPrice);
            return true;
        }
        finally { purchasing = false; }
    }
    // Old signs still work during migration; individual prices are superseded.
    public bool TryBuildModule(Vector3 position, GameObject prefab, int ignoredCost)
    {
        if (prefab == null || purchasing) return false;
        int cost = CurrentPrice;
        if (CurrencyManager.coins < cost) { Debug.Log("Moedas insuficientes. Preço: "+cost); return false; }
        purchasing = true;
        try
        {
            GameObject module = Instantiate(prefab,position,Quaternion.identity,transform);
            if (module == null) return false;
            CurrencyManager.RemoveCoins(cost);
            purchasedAreas = Mathf.Min(purchasedAreas,int.MaxValue-1)+1;
            return true;
        }
        finally { purchasing = false; }
    }
}
