using UnityEngine;

public class HotbarInput : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    void Start()
    {
        _inventoryManager = InventoryManager.instance;
    }

    void Update()
    {
        if (_inventoryManager == null) return;
        
        // Loop que verifica as teclas Alpha1 (tecla 1) até Alpha9 (tecla 9)
        for (int i = 0; i < 9; i++)
        {
            // Verifica se a tecla numérica correspondente (1, 2, 3...) foi pressionada
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Chama o método SelectSlot do InventoryManager, passando o índice (0 a 8)
                _inventoryManager.SelectSlot(i);
                break; 
            }
        }
    }
}