using UnityEngine;
using TMPro; // Importe para usar TextMeshPro

public class CurrencyManager : MonoBehaviour
{
    public static int coins; // O total de moedas

    [SerializeField] private TextMeshProUGUI coinsText;

    void Start()
    {
        coins = 0; // Inicia com 0 moedas
        UpdateCoinsUI();
    }

    // Adiciona moedas ao total
    public static void AddCoins(int amount)
    {
        coins += amount;
        FindAnyObjectByType<CurrencyManager>().UpdateCoinsUI();
    }

    // Remove moedas do total
    public static void RemoveCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            FindAnyObjectByType<CurrencyManager>().UpdateCoinsUI();
        }
    }

    // Atualiza o texto da UI
    private void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = coins.ToString();
        }
    }
}