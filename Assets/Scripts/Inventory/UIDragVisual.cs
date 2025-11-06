using UnityEngine;
using UnityEngine.UI;
using TMPro; // Certifique-se de ter este using

public class UIDragVisual : MonoBehaviour
{
    public static UIDragVisual instance;
    
    // Referências aos componentes visuais
    public Image itemIcon;
    public TextMeshProUGUI quantityText; 

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        // Garante que o objeto comece invisível
        gameObject.SetActive(false); 
    }

    public void SetVisual(Sprite sprite, int quantity)
    {
        itemIcon.sprite = sprite;
        quantityText.text = quantity.ToString();
        gameObject.SetActive(true);
    }

    public void HideVisual()
    {
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        // Garante que o item fantasma siga o mouse
        if (gameObject.activeSelf)
        {
            transform.position = Input.mousePosition;
        }
    }
}