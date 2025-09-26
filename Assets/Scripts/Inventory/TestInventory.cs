using UnityEngine;

public class TestInventory : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    void Start()
{
    _inventoryManager = InventoryManager.instance;
    if (_inventoryManager == null)
    {
        Debug.LogError("InventoryManager não encontrado! Certifique-se de que ele está na cena e com o script anexado.");
    }
}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_inventoryManager != null)
            {
                _inventoryManager.DisplayInventory();
            }
        }
    }
}