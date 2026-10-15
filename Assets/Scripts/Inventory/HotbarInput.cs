using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class HotbarInput : MonoBehaviour
{
    void Update()
    {
        var manager = InventoryManager.instance;
        if (manager == null || Time.timeScale == 0) return;
        var focus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (focus != null && (focus.GetComponent<TMP_InputField>() != null || focus.GetComponent<InputField>() != null)) return;
        for (int i = 0; i < 9; i++)
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) manager.SelectSlot(i);
        float wheel = Input.mouseScrollDelta.y;
        if (wheel != 0) manager.SelectSlot((manager.selectedSlotIndex + (wheel > 0 ? 8 : 1)) % 9);
    }
}
