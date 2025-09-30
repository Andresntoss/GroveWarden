using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identificação")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string itemDescription;

    // NOVO: Define o tipo de item (necessário para a lógica de plantio)
    public ItemType itemType = ItemType.General;

    // NOVO: Liga a semente ao seu modelo de crescimento (somente para Seed)
    public CropData relatedCropData; 
}