using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public Sprite[] growthSprites; // Sprites para cada estágio de crescimento
    public int daysToGrow; // O tempo total de crescimento
    public ItemData yieldItem; // O item que será colhido
    public int yieldAmount; // A quantidade do item
}