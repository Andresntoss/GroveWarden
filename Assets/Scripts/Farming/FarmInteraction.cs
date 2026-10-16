using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// A single input reader picks exactly one target, even when triggers overlap.
public class FarmInteraction : MonoBehaviour
{
    static FarmInteraction instance;
    [Min(.1f)] public float interactionRange = 2.5f;
    public Camera worldCamera;
    public Color selectionColor = new Color(1f,.9f,.25f,1f);
    PlotSlot selectedSlot;
    BuildSign selectedSign;
    LineRenderer outline;
    Material lineMaterial;
    Transform player;
    Collider2D playerBody;
    float nextSearch;
    public static void EnsureExists()
    {
        if (!Application.isPlaying || instance != null) return;
        if (FindAnyObjectByType<FarmInteraction>() != null) return;
        new GameObject("Farm Interaction").AddComponent<FarmInteraction>();
    }
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        var go = new GameObject("Selected farm cell");
        go.transform.SetParent(transform,false);
        outline = go.AddComponent<LineRenderer>();
        outline.useWorldSpace = true; outline.loop = true; outline.positionCount = 4;
        outline.startWidth = outline.endWidth = .035f;
        outline.startColor = outline.endColor = selectionColor;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) { lineMaterial = new Material(shader); outline.sharedMaterial = lineMaterial; }
        outline.sortingOrder = 100;
        outline.enabled = false;
    }
    void Update()
    {
        if (player == null && Time.unscaledTime >= nextSearch)
        {
            nextSearch = Time.unscaledTime+1;
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) { player = found.transform; playerBody = found.GetComponent<Collider2D>(); }
        }
        if (worldCamera == null) worldCamera = Camera.main;
        ClearSelection();
        if (player == null || worldCamera == null || HotbarUi.IsInventoryOpen || Time.timeScale == 0 || PointerBlocked()) return;
        var health = player.GetComponent<Health>();
        if (health != null && health.IsDead) return;
        // Intersect camera ray with the player's XY plane (also supports perspective).
        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.forward,new Vector3(0,0,player.position.z));
        if (!plane.Raycast(ray,out float distance)) return;
        Vector2 point = ray.GetPoint(distance);
        Vector2 playerPoint = playerBody != null && playerBody.enabled ? (Vector2)playerBody.bounds.center : (Vector2)player.position;
        if (Vector2.Distance(playerPoint,point) > interactionRange) return;
        Collider2D bestCollider = null;
        float best = float.PositiveInfinity;
        int bestId = int.MaxValue;
        foreach (var collider in Physics2D.OverlapPointAll(point))
        {
            PlotSlot slot = collider.GetComponent<PlotSlot>();
            BuildSign sign = collider.GetComponent<BuildSign>();
            if ((slot == null || !slot.isActiveAndEnabled) && (sign == null || !sign.isActiveAndEnabled)) continue;
            float score = ((Vector2)collider.bounds.center-point).sqrMagnitude;
            int id = collider.GetInstanceID();
            if (score > best || (Mathf.Approximately(score,best) && id >= bestId)) continue;
            best = score; bestId = id; bestCollider = collider;
            selectedSlot = slot; selectedSign = sign;
        }
        if (selectedSlot != null) selectedSlot.SetSelected(true);
        if (selectedSign != null) selectedSign.SetSelected(true);
        if (bestCollider != null)
        {
            Bounds b = bestCollider.bounds;
            float z = player.position.z;
            outline.SetPosition(0,new Vector3(b.min.x,b.min.y,z));
            outline.SetPosition(1,new Vector3(b.min.x,b.max.y,z));
            outline.SetPosition(2,new Vector3(b.max.x,b.max.y,z));
            outline.SetPosition(3,new Vector3(b.max.x,b.min.y,z));
            outline.enabled = true;
        }
        if (!Input.GetKeyDown(KeyCode.E) && !Input.GetMouseButtonDown(1)) return;
        // One action per input; a purchase never plants in the same frame.
        if (selectedSign != null) selectedSign.Purchase();
        else if (selectedSlot != null) selectedSlot.Interact();
    }
    bool PointerBlocked()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        var focus = es.currentSelectedGameObject;
        if (focus != null && (focus.GetComponent<TMP_InputField>() != null || focus.GetComponent<InputField>() != null)) return true;
        // Ignore legacy transparent panels; only actual inventory slots block the mouse.
        var pointer = new PointerEventData(es) { position = Input.mousePosition };
        var hits = new System.Collections.Generic.List<RaycastResult>();
        es.RaycastAll(pointer,hits);
        foreach (var hit in hits) if (hit.gameObject.GetComponentInParent<GroveSlotView>() != null) return true;
        return false;
    }
    void ClearSelection()
    {
        if (selectedSlot != null) selectedSlot.SetSelected(false);
        if (selectedSign != null) selectedSign.SetSelected(false);
        selectedSlot = null; selectedSign = null;
        if (outline != null) outline.enabled = false;
    }
    void OnDisable() { ClearSelection(); }
    void OnDestroy() { if (instance == this) instance = null; if (lineMaterial != null) Destroy(lineMaterial); }
    void OnGUI()
    {
        if (selectedSlot == null && selectedSign == null) return;
        string text = selectedSign != null ? "Comprar área: "+selectedSign.CurrentPrice+" moedas" : selectedSlot.ActionName;
        GUI.Label(new Rect(12,Screen.height-42,600,28),text+"  |  E / botão direito");
    }
}
