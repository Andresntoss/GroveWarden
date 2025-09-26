using UnityEngine;

public class PlotManager : MonoBehaviour
{
    public PlotSlot[] plotSlots;
    public GameObject plotSign; // A placa para interagir

    private void Start()
    {
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

    public void PassDay()
    {
        foreach (PlotSlot slot in plotSlots)
        {
            slot.AdvanceDay();
        }
    }
}