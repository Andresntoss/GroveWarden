using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance; // Singleton
    public static int coins; // Total de moedas

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinsText;

    private void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        // Opcional: persistir entre cenas
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        coins = 0; // inicia com 0 moedas
        UpdateCoinsUI();
    }

    // Adiciona moedas
    public static void AddCoins(int amount)
    {
        coins += amount;
        if (instance != null)
            instance.UpdateCoinsUI();
        else
            Debug.LogWarning("[CurrencyManager] Nenhuma instância encontrada para atualizar UI!");
    }

    // Remove moedas
    public static void RemoveCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            if (instance != null)
                instance.UpdateCoinsUI();
            else
                Debug.LogWarning("[CurrencyManager] Nenhuma instância encontrada para atualizar UI!");
        }
    }

    // Atualiza a UI
    private void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = coins.ToString();
        }
        else
        {
            Debug.LogWarning("[CurrencyManager] coinsText não atribuído no Inspector!");
        }
    }
}
