using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Two painted maps act as templates. Runtime displays only the wet cells that were watered.
public class SoilTilemapVisual : MonoBehaviour
{
    public Tilemap dryTilemap;
    public Tilemap wetTilemap;
    public PlotSlot[] slots;
    struct TileSnapshot
    {
        public TileBase tile;
        public Color color;
        public Matrix4x4 matrix;
        public TileFlags flags;
    }
    class Binding
    {
        public PlotSlot slot;
        public Vector3Int dryCell, wetCell;
        public TileSnapshot dry, wet;
    }
    readonly List<Binding> bindings = new List<Binding>();
    bool initialized;
    [ContextMenu("Preencher os slots deste Plot")]
    void FillSlots() { slots = GetComponentsInChildren<PlotSlot>(true); }
    static TileSnapshot Capture(Tilemap map,Vector3Int cell)
    {
        return new TileSnapshot { tile=map.GetTile(cell),color=map.GetColor(cell),matrix=map.GetTransformMatrix(cell),flags=map.GetTileFlags(cell) };
    }
    static void Paint(Tilemap map,Vector3Int cell,TileSnapshot snapshot,bool visible)
    {
        map.SetTile(cell,visible ? snapshot.tile : null);
        if (!visible || snapshot.tile == null) return;
        map.SetTileFlags(cell,TileFlags.None);
        map.SetColor(cell,snapshot.color);
        map.SetTransformMatrix(cell,snapshot.matrix);
        map.SetTileFlags(cell,snapshot.flags);
    }
    void Awake()
    {
        if (dryTilemap == null || wetTilemap == null || dryTilemap == wetTilemap)
        { Debug.LogError("SoilTilemapVisual: atribua dois Tilemaps diferentes, seco e molhado.",this); enabled=false; return; }
        if (slots == null || slots.Length == 0) FillSlots();
        var seenDry = new HashSet<Vector3Int>();
        var seenWet = new HashSet<Vector3Int>();
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            // One cell per slot; place slot centers inside their tile, not across cell boundaries.
            Vector3 point = slot.transform.position;
            Vector3Int dryCell = dryTilemap.WorldToCell(point);
            Vector3Int wetCell = wetTilemap.WorldToCell(point);
            if (!seenDry.Add(dryCell) || !seenWet.Add(wetCell))
            { Debug.LogError("Dois PlotSlots correspondem ao mesmo tile. Confira alinhamento e escala do Grid.",slot); continue; }
            TileSnapshot dry = Capture(dryTilemap,dryCell), wet = Capture(wetTilemap,wetCell);
            if (dry.tile == null || wet.tile == null)
            { Debug.LogError("Falta tile seco ou molhado sob o centro de "+slot.name+". Confira os dois Tilemaps.",slot); continue; }
            bindings.Add(new Binding { slot=slot,dryCell=dryCell,wetCell=wetCell,dry=dry,wet=wet });
        }
        initialized = true;
        // Templates remain unchanged in Edit Mode. Wet cells are removed only from the Play instance.
        wetTilemap.ClearAllTiles();
        dryTilemap.gameObject.SetActive(true);
        wetTilemap.gameObject.SetActive(true);
        var dryRenderer = dryTilemap.GetComponent<TilemapRenderer>();
        var wetRenderer = wetTilemap.GetComponent<TilemapRenderer>();
        if (dryRenderer != null) dryRenderer.enabled = true;
        if (wetRenderer != null) wetRenderer.enabled = true;
    }
    void OnEnable()
    {
        if (!initialized) return;
        foreach (var binding in bindings) binding.slot.OnStateChanged += Refresh;
        Refresh();
    }
    void OnDisable()
    {
        foreach (var binding in bindings) if (binding.slot != null) binding.slot.OnStateChanged -= Refresh;
    }
    void Refresh()
    {
        foreach (var binding in bindings)
        {
            if (binding.slot == null) continue;
            bool watered = binding.slot.IsWatered;
            Paint(dryTilemap,binding.dryCell,binding.dry,!watered);
            Paint(wetTilemap,binding.wetCell,binding.wet,watered);
        }
    }
}
