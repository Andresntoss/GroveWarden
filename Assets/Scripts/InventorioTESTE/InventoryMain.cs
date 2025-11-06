using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Necessário para a limpeza

public class InventoryMain : MonoBehaviour
{
    public static InventoryMain instance;

    public GameObject mouseItem; // O Button/Item que está sendo arrastado visualmente

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    // CHAMADO POR: Pointer Down no Botão do Item
    public void StartDrag(GameObject buttonItem)
    {
        // 1. Define o item que está sendo arrastado
        mouseItem = buttonItem;
        
        // 2. Desliga o Centralize (libera o item para o mouse)
        Centralize centralizeScript = mouseItem.GetComponent<Centralize>();
        if (centralizeScript != null)
        {
            centralizeScript.enabled = false;
        }
        
        // 3. Coloca o item no topo da UI para que ele fique sempre visível
        mouseItem.transform.SetAsLastSibling(); 
    }
    
    // CHAMADO POR: Drag no Botão do Item
    public void DragItem(GameObject buttonItem)
    {
        if (mouseItem != null)
        {
            mouseItem.transform.position = Input.mousePosition;
        }
    }

    // CHAMADO POR: Evento Drop no Slot de Destino
    public void DropItem(GameObject targetSlot)
    {
        if (mouseItem == null) return;

        // 1. Encontra os slots de origem e destino
        // O slot de ORIGEM é o PAI do mouseItem (o item arrastado)
        InventorySlotUI slotA = mouseItem.transform.parent.GetComponent<InventorySlotUI>(); 
        
        // O slot de DESTINO é o Slot que está sendo passado pelo EventTrigger
        InventorySlotUI slotB = targetSlot.GetComponent<InventorySlotUI>();

        if (slotA != null && slotB != null && InventoryManager.instance != null)
        {
            // CRUCIAL: Troca os dados no array de 36 slots
            InventoryManager.instance.SwapItems(slotA.fixedSlotIndex, slotB.fixedSlotIndex);
            
            // O InventoryManager.OnInventoryChanged se encarregará de redesenhar tudo.
        }

        // 2. Finalização Visual (Colocando o item arrastado no slot de destino)
        if (slotB != null)
        {
            // O Item (mouseItem) se torna filho do novo Slot (slotB)
            mouseItem.transform.SetParent(slotB.transform); 
            
            // 3. Liga o Centralize (Faz o item dar snap ao centro do novo slot)
            Centralize centralizeScript = mouseItem.GetComponent<Centralize>();
            if (centralizeScript != null)
            {
                centralizeScript.enabled = true;
            }
        }
        
        // 4. Limpeza
        mouseItem = null;
    }
}