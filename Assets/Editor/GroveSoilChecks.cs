using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class GroveSoilChecks
{
    static void Check(bool ok,string text) { if(!ok) throw new Exception("Soil check failed: "+text); }
    [MenuItem("Grove Warden/Validar terra seca e molhada (Play Mode)")]
    static void Run()
    {
        if(!Application.isPlaying) { Debug.LogWarning("Entre em Play Mode para validar."); return; }
        var root = new GameObject("Temporary soil validation"); root.SetActive(false);
        var dryTile = ScriptableObject.CreateInstance<Tile>();
        var wetTile = ScriptableObject.CreateInstance<Tile>();
        var seed = ScriptableObject.CreateInstance<ItemData>();
        var product = ScriptableObject.CreateInstance<ItemData>();
        var crop = ScriptableObject.CreateInstance<CropData>();
        try
        {
            var zone = root.AddComponent<FarmZone>();
            var sign = new GameObject("Placa");sign.transform.SetParent(root.transform);
            var outline = new GameObject("Demarcacao");outline.transform.SetParent(root.transform);
            var plot = new GameObject("Plot",typeof(Grid));plot.transform.SetParent(root.transform);
            plot.SetActive(false);
            zone.unlockedContent = plot; zone.purchaseVisuals = new[] {sign,outline};
            var buyCollider = root.AddComponent<BoxCollider2D>();
            zone.purchaseColliders = new Collider2D[] {buyCollider};
            Tilemap MakeMap(string name)
            {
                var go = new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));go.transform.SetParent(plot.transform);
                return go.GetComponent<Tilemap>();
            }
            var dry = MakeMap("Dry");var wet = MakeMap("Wet");
            var slots = new PlotSlot[6];
            for(int i=0;i<6;i++)
            {
                var cell = new Vector3Int(i%3,i/3,0);dry.SetTile(cell,dryTile);wet.SetTile(cell,wetTile);
                var go = new GameObject("Slot"+i);go.transform.SetParent(plot.transform);
                go.transform.localPosition = new Vector3(cell.x+.5f,cell.y+.5f,0);
                go.AddComponent<BoxCollider2D>();slots[i] = go.AddComponent<PlotSlot>();
            }
            var visual = plot.AddComponent<SoilTilemapVisual>(); visual.dryTilemap = dry;visual.wetTilemap = wet;visual.slots = slots;
            crop.growthSprites = new Sprite[2];crop.daysToGrow=1;crop.seedItem=seed;crop.yieldItem=product;crop.yieldAmount=1;
            root.SetActive(true);
            Check(!plot.activeSelf && sign.activeSelf && outline.activeSelf,"initial locked state");
            Check(zone.Unlock() && root.activeSelf && !sign.activeSelf && !outline.activeSelf && !buyCollider.enabled,"unlock preserves root and disables purchase");
            Check(!zone.Unlock(),"no duplicate unlock");
            for(int i=0;i<6;i++) slots[i].Plant(crop);
            slots[0].Water();
            Check(dry.GetTile(Vector3Int.zero)==null && wet.GetTile(Vector3Int.zero)==wetTile,"one watered cell");
            Check(dry.GetTile(Vector3Int.right)==dryTile && wet.GetTile(Vector3Int.right)==null,"neighbour stays dry");
            for(int i=1;i<6;i++) slots[i].Water();
            for(int i=0;i<6;i++)
            {
                var c=new Vector3Int(i%3,i/3,0);Check(wet.GetTile(c)==wetTile && dry.GetTile(c)==null,"all cells wet");
                slots[i].AdvanceDay();
                Check(dry.GetTile(c)==dryTile && wet.GetTile(c)==null,"new day dries soil");
            }
            Debug.Log("Grove Warden: verificações de compra visual e rega individual passaram.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(dryTile);UnityEngine.Object.DestroyImmediate(wetTile);
            UnityEngine.Object.DestroyImmediate(seed);UnityEngine.Object.DestroyImmediate(product);UnityEngine.Object.DestroyImmediate(crop);
        }
    }
}
