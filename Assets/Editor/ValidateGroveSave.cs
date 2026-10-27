using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ValidateGroveSave
{
    [MenuItem("Grove Warden/Salvar jogo/Verificar configuração")]
    static void Check()
    {
        var scene=SceneManager.GetActiveScene();
        var catalog=Resources.Load<GroveSaveCatalog>("GroveSaveCatalog");
        var ids=new HashSet<string>(); int errors=0, areas=0, slots=0;
        if(catalog==null) { Debug.LogError("[Save] Catálogo ausente: prepare a cena primeiro."); errors++; }
        foreach(var root in scene.GetRootGameObjects())
        {
            foreach(var zone in root.GetComponentsInChildren<FarmZone>(true))
            {
                areas++;
                if(string.IsNullOrWhiteSpace(zone.saveId) || !ids.Add(zone.saveId) || !zone.IsConfigured)
                { Debug.LogError("[Save] Área inválida, duplicada ou não configurada: "+zone.name,zone); errors++; }
                slots += zone.GetComponentsInChildren<PlotSlot>(true).Length;
            }
            foreach(var plot in root.GetComponentsInChildren<PlotManager>(true))
            {
                if(plot.GetComponentInParent<FarmZone>(true)!=null) continue;
                areas++;
                if(string.IsNullOrWhiteSpace(plot.saveId) || !ids.Add(plot.saveId))
                { Debug.LogError("[Save] Canteiro sem ID ou duplicado: "+plot.name,plot); errors++; }
                slots += plot.GetComponentsInChildren<PlotSlot>(true).Length;
            }
        }
        if(Object.FindFirstObjectByType<GameManager>()==null || Object.FindFirstObjectByType<FarmManager>()==null || Object.FindFirstObjectByType<InventoryManager>()==null || Object.FindFirstObjectByType<PlayerController>()==null)
        { Debug.LogError("[Save] Faltam jogador ou gerenciadores ativos."); errors++; }
        EditorUtility.DisplayDialog("Verificação de save",$"Áreas: {areas}. Slots: {slots}. Erros: {errors}.\n\nEsta verificação não substitui o teste de salvar e carregar em Play Mode. Consulte o Console.","OK");
    }
}
