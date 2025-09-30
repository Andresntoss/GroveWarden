using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    private Item _item;
    private bool _isSelected = false;

    // Configura o slot para exibir o item
    public void SetItem(Item item)
    {
        _item = item;
        itemIcon.sprite = item.itemData.itemIcon;
        quantityText.text = item.quantity.ToString();
        
        // Ativa o slot se ele não estiver vazio
        gameObject.SetActive(true);
    }
    
    // Limpa o slot (quando o item é removido ou a quantidade é zero)
    public void ClearSlot()
    {
        _item = null;
        itemIcon.sprite = null;
        quantityText.text = "";
        gameObject.SetActive(false);
    }

    // Chamado quando o jogador clica no slot
    public void OnClickSelect()
    {
        if (_item != null)
        {
            // Lógica de seleção será adicionada aqui.
            // O ideal é que este slot avise ao PlayerManager que foi selecionado.
            Debug.Log($"Slot {gameObject.name} clicado: {_item.itemData.itemName}");
        }
    }
}