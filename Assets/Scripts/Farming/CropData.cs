using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public Sprite[] growthSprites; // Sprites para cada estágio de crescimento
    public int daysToGrow; // O tempo total de crescimento

    [Header("Itens")]
    public ItemData seedItem; // <--- NOVO: O ItemData da semente (o que o jogador planta)
    public ItemData yieldItem; // O item que será colhido (o produto final)
    public int yieldAmount; // A quantidade do item
}