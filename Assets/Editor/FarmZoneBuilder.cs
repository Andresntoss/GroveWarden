using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmZoneBuilder : EditorWindow
{
    PlotManager referencePlot;
    GameObject signPrefab;
    Tilemap sourceSoil;
    [MenuItem("Grove Warden/Criar prefab FarmZone")]
    static void Open() { GetWindow<FarmZoneBuilder>("Criar FarmZone"); }
    void OnEnable()
    {
        signPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FunctionPrefabs/BuildPlotSign.prefab");
        if (Selection.activeGameObject != null) referencePlot = Selection.activeGameObject.GetComponent<PlotManager>();
    }
    void OnGUI()
    {
        EditorGUILayout.HelpBox("Fora do Play Mode: use um Plot existente na cena, alinhado com a terra arada do Tilemap. A ferramenta copia os canteiros e os tiles sob seus centros. Os originais não são apagados.",MessageType.Info);
        referencePlot = (PlotManager)EditorGUILayout.ObjectField("Plot de referência",referencePlot,typeof(PlotManager),true);
        signPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab da placa",signPrefab,typeof(GameObject),false);
        sourceSoil = (Tilemap)EditorGUILayout.ObjectField("Tilemap da terra arada",sourceSoil,typeof(Tilemap),true);
        EditorGUILayout.HelpBox("Depois confira a terra no prefab criado. Desative/remova o Plot antigo e a placa antiga e apague somente os tiles de terra arada do cenário. As áreas novas passam a carregar sua própria terra.",MessageType.None);
        using (new EditorGUI.DisabledScope(Application.isPlaying))
            if (GUILayout.Button("Criar prefab e colocar uma instância")) Build();
    }
    void Build()
    {
        if (referencePlot == null || EditorUtility.IsPersistent(referencePlot) || signPrefab == null || sourceSoil == null || sourceSoil.layoutGrid == null)
        { EditorUtility.DisplayDialog("Configuração incompleta","Atribua um Plot da cena, o prefab da placa e o Tilemap com terra arada.","OK"); return; }
        var slots = referencePlot.GetComponentsInChildren<PlotSlot>(true);
        if (slots.Length == 0) { EditorUtility.DisplayDialog("Sem canteiros","O Plot precisa conter PlotSlots.","OK"); return; }
        Vector3Int min = sourceSoil.WorldToCell(slots[0].transform.position), max = min;
        foreach (var slot in slots)
        {
            var cell = sourceSoil.WorldToCell(slot.transform.position);
            min = Vector3Int.Min(min,cell); max = Vector3Int.Max(max,cell);
        }
        if ((long)(max.x-min.x+1)*(max.y-min.y+1) > 10000)
        { EditorUtility.DisplayDialog("Área inválida","Verifique a posição dos canteiros e a escala do Tilemap.","OK"); return; }
        int tileCount = 0;
        for (int y=min.y;y<=max.y;y++) for (int x=min.x;x<=max.x;x++) if(sourceSoil.HasTile(new Vector3Int(x,y,min.z))) tileCount++;
        if (tileCount==0) { EditorUtility.DisplayDialog("Sem terra","Não há tiles sob os canteiros no Tilemap escolhido. Confira o alinhamento.","OK"); return; }
        GameObject root = new GameObject("FarmZone");
        try
        {
            var zone = root.AddComponent<FarmZone>();
            GameObject plot = Instantiate(referencePlot.gameObject,root.transform);
            plot.name = "Plantio";
            plot.transform.position -= referencePlot.transform.position;
            var pm = plot.GetComponent<PlotManager>();
            pm.plotSlots = plot.GetComponentsInChildren<PlotSlot>(true);
            pm.plotSign = null;
            foreach(var slot in pm.plotSlots) slot.SetSelected(false);
            GameObject sign = (GameObject)PrefabUtility.InstantiatePrefab(signPrefab);
            sign.transform.SetParent(root.transform,false); sign.transform.localPosition = Vector3.zero;
            sign.name = "PlacaCompra";
            var buildSign = sign.GetComponent<BuildSign>();
            if (buildSign == null) buildSign = sign.AddComponent<BuildSign>();
            if (buildSign.interactionPrompt != null) buildSign.interactionPrompt.SetActive(false);
            CopySoil(plot.transform,min,max);
            zone.unlockedContent = plot; zone.purchaseSign = sign; zone.startsUnlocked = false;
            plot.SetActive(false);
            const string folder = "Assets/Prefabs/FunctionPrefabs";
            System.IO.Directory.CreateDirectory(folder); AssetDatabase.Refresh();
            string path = AssetDatabase.GenerateUniqueAssetPath(folder+"/FarmZone.prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root,path);
            if (prefab == null) throw new System.Exception("Não foi possível salvar o prefab.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance,"Criar FarmZone");
            instance.transform.position = referencePlot.transform.position;
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log("FarmZone criado em "+path+". Confira o alinhamento antes de remover os originais.",instance);
        }
        finally { DestroyImmediate(root); }
    }
    void CopySoil(Transform parent,Vector3Int min,Vector3Int max)
    {
        var originalGrid = sourceSoil.layoutGrid;
        var gridObject = new GameObject("TerraGrid",typeof(Grid));
        var grid = gridObject.GetComponent<Grid>();
        grid.cellSize = originalGrid.cellSize; grid.cellGap = originalGrid.cellGap;
        grid.cellLayout = originalGrid.cellLayout; grid.cellSwizzle = originalGrid.cellSwizzle;
        gridObject.transform.position = originalGrid.transform.position-referencePlot.transform.position;
        gridObject.transform.rotation = originalGrid.transform.rotation;
        gridObject.transform.localScale = originalGrid.transform.lossyScale;
        gridObject.transform.SetParent(parent,true);
        var go = new GameObject("TerraArada",typeof(Tilemap),typeof(TilemapRenderer));
        go.transform.position = sourceSoil.transform.position-referencePlot.transform.position;
        go.transform.rotation = sourceSoil.transform.rotation;
        go.transform.localScale = sourceSoil.transform.lossyScale;
        go.transform.SetParent(gridObject.transform,true);
        var map = go.GetComponent<Tilemap>();
        map.tileAnchor = sourceSoil.tileAnchor; map.color = sourceSoil.color;
        map.orientation = sourceSoil.orientation; map.orientationMatrix = sourceSoil.orientationMatrix;
        var renderer = go.GetComponent<TilemapRenderer>();
        var sourceRenderer = sourceSoil.GetComponent<TilemapRenderer>();
        if (sourceRenderer != null)
        {
            renderer.sharedMaterials = sourceRenderer.sharedMaterials;
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            renderer.sortingOrder = sourceRenderer.sortingOrder;
        }
        for(int z=min.z;z<=max.z;z++) for(int y=min.y;y<=max.y;y++) for(int x=min.x;x<=max.x;x++)
        {
            var cell = new Vector3Int(x,y,z);
            TileBase tile = sourceSoil.GetTile(cell);
            if(tile==null) continue;
            map.SetTile(cell,tile); map.SetTileFlags(cell,TileFlags.None);
            map.SetColor(cell,sourceSoil.GetColor(cell)); map.SetTransformMatrix(cell,sourceSoil.GetTransformMatrix(cell));
        }
        map.CompressBounds();
    }
}
