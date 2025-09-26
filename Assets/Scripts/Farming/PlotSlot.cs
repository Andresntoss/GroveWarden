using UnityEngine;

public class PlotSlot : MonoBehaviour
{
    public CropData cropData;
    public int daysSincePlanted;
    public bool isPlanted = false;
    public bool isWatered = false;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // Começa com o slot vazio
        _spriteRenderer.sprite = null;
    }

    public void Plant(CropData data)
    {
        this.cropData = data;
        isPlanted = true;
        daysSincePlanted = 0;
        isWatered = false;
        
        // Exibe o sprite da semente
        if (cropData.growthSprites.Length > 0)
        {
            _spriteRenderer.sprite = cropData.growthSprites[0];
        }
    }

    public void Water()
    {
        isWatered = true;
    }

    public void Harvest()
    {
        // ... (Lógica de colheita será adicionada aqui)
    }

    public void AdvanceDay()
    {
        if (isPlanted && isWatered)
        {
            daysSincePlanted++;
            UpdateGrowthVisual();
        }
    }

    private void UpdateGrowthVisual()
    {
        if (cropData.growthSprites.Length > daysSincePlanted)
        {
            _spriteRenderer.sprite = cropData.growthSprites[daysSincePlanted];
        }
    }
}