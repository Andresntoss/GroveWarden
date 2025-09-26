using UnityEngine;

public class PlotManager : MonoBehaviour
{
    public PlotSlot[] plotSlots;
    public GameObject plotSign; // A placa para interagir

    private void Start()
    {
        // NOVO: O registro acontece no Start(), que é mais seguro
        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterPlotManager(this);
            Debug.Log("[PlotManager] Registro BEM-SUCEDIDO no Start()."); 
        }
        else
        {
            Debug.LogError("[PlotManager] FALHA. GameManager.instance é NULL.");
        }
        // Desativa a placa de interação no início
        if (plotSign != null)
        {
            plotSign.SetActive(false);
        }
    }

    public void InteractWithSlot(int slotIndex, GameObject player)
    {
        // A lógica de plantio e rega será adicionada aqui
    }
    private void Awake()
    {
        
    }
    public void PassDay() // Método que o GameManager chama para avançar o dia ---

    {
        foreach (PlotSlot slot in plotSlots)
        {
            slot.AdvanceDay();
        }
    }
}