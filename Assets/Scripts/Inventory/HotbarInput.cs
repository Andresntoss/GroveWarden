using UnityEngine;

public class HotbarInput : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    void Start()
    {
        // Encontra a instância do InventoryManager (singleton)
        _inventoryManager = InventoryManager.instance;
        
        if (_inventoryManager == null) 
        {
             Debug.LogError("[HotbarInput] InventoryManager não encontrado! Verifique se está na cena.");
        }
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
                // O índice é 0 (para a tecla 1) até 8 (para a tecla 9)
                Debug.Log("Tecla " + (i + 1) + " OK!");
                _inventoryManager.SelectSlot(i);
                break; 
            }
        }
    }
}