using UnityEngine;

public class FarmManager : MonoBehaviour
{
    public static FarmManager instance;
    
    [Header("Estruturas")]
    public GameObject plotModulePrefab; // Prefab do canteiro de 6 slots
    public int plotCost = 10; // Custo do módulo de plantio

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    // NOVO MÉTODO: Executa a transação de construção
    public bool TryBuildModule(Vector3 buildPosition, GameObject prefabToBuild, int cost)
    {
        // 1. Verifica se o jogador tem dinheiro suficiente
        if (CurrencyManager.coins >= cost)
        {
            // 2. Tenta remover o custo
            CurrencyManager.RemoveCoins(cost);

            // 3. Cria o módulo na posição da placa
            Instantiate(prefabToBuild, buildPosition, Quaternion.identity, transform);
            
            Debug.Log($"Módulo construído! {prefabToBuild.name} custou {cost} moedas.");
            return true;
        }
        else
        {
            Debug.Log("Você não tem moedas suficientes para construir isso!");
            return false;
        }
    }
}