using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrepareGroveSave
{
    [MenuItem("Grove Warden/Salvar jogo/Preparar cena e catálogo")]
    static void Prepare()
    {
        if(Application.isPlaying) { EditorUtility.DisplayDialog("Preparação","Saia do Play Mode.","OK"); return; }
        Scene scene=SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path)) { EditorUtility.DisplayDialog("Preparação","Salve a cena antes de preparar o salvamento.","OK"); return; }
        var used = new HashSet<string>(); int changed=0, areas=0;
        foreach(var root in scene.GetRootGameObjects())
        {
            foreach(var zone in root.GetComponentsInChildren<FarmZone>(true))
            {
                areas++;
                if (string.IsNullOrWhiteSpace(zone.saveId) || !used.Add(zone.saveId))
                { Undo.RecordObject(zone,"Atribuir ID de área"); zone.saveId=Guid.NewGuid().ToString("N"); used.Add(zone.saveId); EditorUtility.SetDirty(zone); PrefabUtility.RecordPrefabInstancePropertyModifications(zone); changed++; }
            }
            foreach(var plot in root.GetComponentsInChildren<PlotManager>(true))
            {
                if(plot.GetComponentInParent<FarmZone>(true)!=null) continue;
                areas++;
                if(string.IsNullOrWhiteSpace(plot.saveId) || !used.Add(plot.saveId))
                { Undo.RecordObject(plot,"Atribuir ID de canteiro"); plot.saveId=Guid.NewGuid().ToString("N"); used.Add(plot.saveId); EditorUtility.SetDirty(plot); PrefabUtility.RecordPrefabInstancePropertyModifications(plot); changed++; }
            }
        }
        Directory.CreateDirectory("Assets/Resources"); AssetDatabase.Refresh();
        const string path="Assets/Resources/GroveSaveCatalog.asset";
        var catalog=AssetDatabase.LoadAssetAtPath<GroveSaveCatalog>(path);
        if(catalog==null) { catalog=ScriptableObject.CreateInstance<GroveSaveCatalog>(); AssetDatabase.CreateAsset(catalog,path); }
        Undo.RecordObject(catalog,"Atualizar catálogo de save");
        catalog.items.Clear(); catalog.crops.Clear(); catalog.npcs.Clear();
        foreach(string guid in AssetDatabase.FindAssets("t:ItemData"))
        { var value=AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid)); if(value!=null) catalog.items.Add(new GroveSaveCatalog.ItemEntry{id=guid,item=value}); }
        foreach(string guid in AssetDatabase.FindAssets("t:CropData"))
        { var value=AssetDatabase.LoadAssetAtPath<CropData>(AssetDatabase.GUIDToAssetPath(guid)); if(value!=null) catalog.crops.Add(new GroveSaveCatalog.CropEntry{id=guid,crop=value}); }
        var npcIds = new HashSet<string>();
        foreach(string guid in AssetDatabase.FindAssets("t:NpcProfile"))
        {
            var value=AssetDatabase.LoadAssetAtPath<NpcProfile>(AssetDatabase.GUIDToAssetPath(guid));
            if(value==null) continue;
            if(string.IsNullOrWhiteSpace(value.npcId) || !npcIds.Add(value.npcId)) { Debug.LogError("Perfil de NPC com ID vazio ou repetido: "+value.name,value); continue; }
            catalog.npcs.Add(value);
        }
        EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Preparação concluída",$"{areas} áreas encontradas; {changed} IDs atribuídos.\n{catalog.items.Count} itens, {catalog.crops.Count} plantações e {catalog.npcs.Count} perfis no catálogo.\n\nSalve a cena com Ctrl+S. Repita após duplicar áreas ou criar novos itens, plantas ou NPCs.","OK");
    }
}
