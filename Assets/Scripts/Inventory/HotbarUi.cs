using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Keep this file's existing .meta so the scene retains its component reference.
public class HotbarUi : MonoBehaviour
{
    public List<InventorySlotUI> slots = new List<InventorySlotUI>();
    public Image selectionIndicator;
    public static bool IsInventoryOpen { get; private set; }
    InventoryManager manager;
    GameObject root, bag;
    readonly List<GroveSlotView> views = new List<GroveSlotView>();
    GroveSlotView dragging;
    Image ghost;

    void Start()
    {
        manager = InventoryManager.instance;
        if (manager == null) manager = new GameObject("InventoryManager").AddComponent<InventoryManager>();
        // Legacy slot visuals and EventTriggers no longer take part in dragging.
        foreach (var slot in slots) if (slot != null) slot.gameObject.SetActive(false);
        if (selectionIndicator != null) selectionIndicator.gameObject.SetActive(false);
        foreach (var old in FindObjectsByType<InventoryMain>(FindObjectsSortMode.None)) old.enabled = false;
        if (FindAnyObjectByType<HotbarInput>() == null) manager.gameObject.AddComponent<HotbarInput>();
        Build();
        manager.OnInventoryChanged += UpdateInventoryDisplay;
        manager.OnSlotSelected += HighlightSlot;
        UpdateInventoryDisplay();
    }
    RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return rt;
    }
    TextMeshProUGUI Label(string text, Transform parent, Vector2 size, Vector2 pos, int fontSize)
    {
        var rt = Rect("Label", parent, size, new Vector2(.5f,.5f), pos);
        var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }
    void Build()
    {
        root = new GameObject("Grove Inventory UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        // Independent canvas avoids inheriting the old inventory layout.
        root.transform.SetParent(null, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.matchWidthOrHeight = .5f;
        if (EventSystem.current == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var bar = Rect("Hotbar", root.transform, new Vector2(610,78), new Vector2(.5f,0), new Vector2(0,52));
        bar.gameObject.AddComponent<Image>().color = new Color(.07f,.09f,.08f,.97f);
        Label("1–9: selecionar   |   Rodinha: alternar   |   I / Tab: mochila", root.transform, new Vector2(700,24), new Vector2(0,-269), 16);
        var panel = Rect("Mochila", root.transform, new Vector2(650,310), new Vector2(.5f,.5f), new Vector2(0,0));
        bag = panel.gameObject;
        panel.gameObject.AddComponent<Image>().color = new Color(.07f,.09f,.08f,.98f);
        Label("MOCHILA  •  Arraste para mover ou juntar pilhas", panel, new Vector2(620,36), new Vector2(0,124), 21);
        for (int i = 0; i < 36; i++)
        {
            Transform parent = i < 9 ? bar : panel;
            int n = i < 9 ? i : i - 9;
            Vector2 pos = i < 9 ? new Vector2((n-4)*66,0) : new Vector2((n%9-4)*66,63-(n/9)*70);
            var rt = Rect("Slot " + i, parent, new Vector2(60,60), new Vector2(.5f,.5f), pos);
            var background = rt.gameObject.AddComponent<Image>();
            var view = rt.gameObject.AddComponent<GroveSlotView>();
            view.owner = this; view.index = i; view.background = background;
            var iconRect = Rect("Icon", rt, new Vector2(42,42), new Vector2(.5f,.5f), Vector2.zero);
            view.icon = iconRect.gameObject.AddComponent<Image>();
            view.icon.preserveAspect = true; view.icon.raycastTarget = false;
            view.quantity = Label("", rt, new Vector2(55,23), new Vector2(0,-18), 17);
            view.quantity.alignment = TextAlignmentOptions.Right;
            if (i < 9) Label((i+1).ToString(), rt, new Vector2(16,18), new Vector2(-21,21), 12);
            views.Add(view);
        }
        var ghostRect = Rect("Dragged item", root.transform, new Vector2(46,46), new Vector2(.5f,.5f), Vector2.zero);
        ghost = ghostRect.gameObject.AddComponent<Image>();
        ghost.raycastTarget = false; ghost.preserveAspect = true;
        ghost.gameObject.SetActive(false);
        SetOpen(false);
    }
    void Update()
    {
        var focus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        bool typing = focus != null && (focus.GetComponent<TMP_InputField>() != null || focus.GetComponent<InputField>() != null);
        if (!typing && (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))) SetOpen(!IsInventoryOpen);
        if (Input.GetKeyDown(KeyCode.Escape)) SetOpen(false);
    }
    void SetOpen(bool value)
    {
        CancelDrag(); IsInventoryOpen = value;
        if (bag != null) bag.SetActive(value);
    }
    public void UpdateInventoryDisplay()
    {
        if (manager == null) return;
        foreach (var view in views)
        {
            Item item = manager.InventorySlots[view.index];
            bool valid = item != null && item.itemData != null && item.quantity > 0;
            view.icon.enabled = valid && item.itemData.itemIcon != null;
            view.icon.sprite = valid ? item.itemData.itemIcon : null;
            view.quantity.text = valid && item.quantity > 1 ? item.quantity.ToString() : "";
            view.background.color = view.index == manager.selectedSlotIndex ? new Color(.65f,.49f,.22f,1) : new Color(.22f,.27f,.23f,1);
        }
    }
    void HighlightSlot(int index) { UpdateInventoryDisplay(); }
    public void Click(int index) { if (manager != null && index < 9) manager.SelectSlot(index); }
    public void BeginDrag(GroveSlotView source, PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || manager.InventorySlots[source.index] == null) return;
        dragging = source;
        ghost.sprite = manager.InventorySlots[source.index].itemData.itemIcon;
        ghost.gameObject.SetActive(ghost.sprite != null);
        ghost.transform.SetAsLastSibling();
        Drag(e);
    }
    public void Drag(PointerEventData e)
    {
        if (dragging == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)root.transform, e.position, null, out Vector2 point);
        ghost.rectTransform.anchoredPosition = point;
    }
    public void Drop(int target)
    {
        if (dragging != null) manager.SwapItems(dragging.index, target);
        CancelDrag();
    }
    public void CancelDrag() { dragging = null; if (ghost != null) ghost.gameObject.SetActive(false); }
    void OnDisable() { SetOpen(false); }
    void OnDestroy()
    {
        if (manager != null) { manager.OnInventoryChanged -= UpdateInventoryDisplay; manager.OnSlotSelected -= HighlightSlot; }
        IsInventoryOpen = false;
        if (root != null) Destroy(root);
    }
}

public class GroveSlotView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public HotbarUi owner;
    public int index;
    public Image background, icon;
    public TextMeshProUGUI quantity;
    public void OnPointerClick(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) owner.Click(index); }
    public void OnBeginDrag(PointerEventData e) { owner.BeginDrag(this,e); }
    public void OnDrag(PointerEventData e) { owner.Drag(e); }
    public void OnEndDrag(PointerEventData e) { owner.CancelDrag(); }
    public void OnDrop(PointerEventData e) { owner.Drop(index); }
}
