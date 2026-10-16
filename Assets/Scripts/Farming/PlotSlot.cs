using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlotSlot : MonoBehaviour
{
    public CropData cropData;
    [Header("Slot State")]
    [SerializeField] int daysSincePlanted;
    [SerializeField] bool isPlanted;
    [SerializeField] bool isWatered;
    [SerializeField] bool isHarvestable;
    public GameObject interactionPrompt;
    public GameObject waterPrompt;
    SpriteRenderer cropRenderer;
    bool selected;
    public event System.Action OnStateChanged;
    public bool IsPlanted => isPlanted;
    public bool IsWatered => isWatered;
    public bool IsHarvestable => isHarvestable;
    public string ActionName => !isPlanted ? "Plantar" : isHarvestable ? "Colher" : !isWatered ? "Regar" : "Regado";
    void Awake() { cropRenderer = GetComponent<SpriteRenderer>(); RefreshVisual(); }
    void OnEnable() { FarmInteraction.EnsureExists(); SetSelected(false); }
    void OnDisable() { SetSelected(false); }
    public void SetSelected(bool value)
    {
        selected = value;
        if (interactionPrompt != null) interactionPrompt.SetActive(value && (!isPlanted || isHarvestable));
        if (waterPrompt != null) waterPrompt.SetActive(value && isPlanted && !isHarvestable && !isWatered);
    }
    public void Interact()
    {
        if (!isActiveAndEnabled) return;
        if (!isPlanted)
        {
            Item item = InventoryManager.instance != null ? InventoryManager.instance.GetSelectedItem() : null;
            if (item == null || item.itemData == null || item.itemData.itemType != ItemType.Seed || item.itemData.relatedCropData == null)
            { Debug.Log("Selecione uma semente na hotbar."); return; }
            CropData crop = item.itemData.relatedCropData;
            if (!ValidCrop(crop)) { Debug.LogWarning("CropData sem sprites de crescimento ou produto de colheita.",this); return; }
            // Consume the selected seed type, even if CropData.seedItem is misconfigured.
            if (InventoryManager.instance.TryRemoveItem(item.itemData,1)) Plant(crop);
        }
        else if (isHarvestable) Harvest();
        else if (!isWatered) Water();
    }
    static bool ValidCrop(CropData data) => data != null && data.growthSprites != null && data.growthSprites.Length > 0 && data.yieldItem != null && data.yieldAmount > 0;
    public void Plant(CropData data)
    {
        if (isPlanted || !ValidCrop(data)) return;
        cropData = data; daysSincePlanted = 0; isPlanted = true; isWatered = false; isHarvestable = false;
        RefreshVisual(); SetSelected(selected);
    }
    public void TryPlanting(CropData data)
    {
        if (isPlanted || !ValidCrop(data) || InventoryManager.instance == null) return;
        if (InventoryManager.instance.TryRemoveItem(data.seedItem,1)) Plant(data);
    }
    public void Water()
    {
        if (!isPlanted || isHarvestable) return;
        isWatered = true; SetSelected(selected); OnStateChanged?.Invoke();
    }
    public void Harvest()
    {
        if (!isPlanted || !isHarvestable || !ValidCrop(cropData) || InventoryManager.instance == null) return;
        if (!InventoryManager.instance.TryAddItem(cropData.yieldItem,cropData.yieldAmount))
        { Debug.Log("Inventário cheio: libere espaço para colher."); return; }
        isPlanted = false; isWatered = false; isHarvestable = false; daysSincePlanted = 0;
        cropData = null; RefreshVisual(); SetSelected(selected);
    }
    public void AdvanceDay()
    {
        if (!isPlanted) return;
        if (isWatered && !isHarvestable)
        {
            daysSincePlanted++;
            isHarvestable = daysSincePlanted >= Mathf.Max(1,cropData.daysToGrow);
        }
        isWatered = false; RefreshVisual(); SetSelected(selected);
    }
    void RefreshVisual()
    {
        OnStateChanged?.Invoke();
        if (cropRenderer == null) cropRenderer = GetComponent<SpriteRenderer>();
        if (!isPlanted || cropData == null || cropData.growthSprites == null || cropData.growthSprites.Length == 0)
        { cropRenderer.sprite = null; return; }
        int index = isHarvestable ? cropData.growthSprites.Length-1 : Mathf.Clamp(daysSincePlanted,0,cropData.growthSprites.Length-1);
        cropRenderer.sprite = cropData.growthSprites[index];
    }
}
