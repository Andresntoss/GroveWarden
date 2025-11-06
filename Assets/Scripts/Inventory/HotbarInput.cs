using UnityEngine;

public class HotbarInput : MonoBehaviour
{
    private InventoryManager _inventoryManager;

    // NOVO: Usar Awake() para garantir que a referência seja capturada cedo
    void Awake()
    {
        // Encontra a instância do InventoryManager (singleton)
        // Usamos FindObjectOfType, pois a instância pode não ter se definido ainda.
        _inventoryManager = FindAnyObjectByType<InventoryManager>();
        
        if (_inventoryManager == null) 
        {
             Debug.LogError("[HotbarInput] InventoryManager não encontrado! Verifique se está na cena.");
        }
    }
    
    // Deixe o Start() vazio.
    void Start()
    {
        // Se o InventoryManager for um Singleton de cena, esta referência já está segura.
    }

    void Update()
    {
        if (_inventoryManager == null) return;
        
        // ... (resto do seu código Update() aqui, está correto)
        for (int i = 0; i < 9; i++)
        {
             if (Input.GetKeyDown(KeyCode.Alpha1 + i))
             {
                 _inventoryManager.SelectSlot(i);
                 break; 
             }
        }
    }
}