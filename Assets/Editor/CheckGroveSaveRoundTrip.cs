using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CheckGroveSaveRoundTrip
{
    [MenuItem("Grove Warden/Salvar jogo/Validar serialização (Play Mode)")]
    static void Run()
    {
        if(!Application.isPlaying) { Debug.LogWarning("Entre em Play Mode para validar o save."); return; }
        var system=UnityEngine.Object.FindFirstObjectByType<GroveSaveSystem>();
        if(system==null) { Debug.LogError("[Save] Sistema não encontrado."); return; }
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var capture=typeof(GroveSaveSystem).GetMethod("Capture",flags);
        var validate=typeof(GroveSaveSystem).GetMethod("Validate",flags);
        try
        {
            var original=(GroveSaveData)capture.Invoke(system,null);
            string json=JsonUtility.ToJson(original);
            var copy=JsonUtility.FromJson<GroveSaveData>(json);
            validate.Invoke(system,new object[]{copy});
            if(copy.day!=original.day || copy.coins!=original.coins || copy.areas.Count!=original.areas.Count || copy.inventory.Count!=original.inventory.Count)
                throw new Exception("Dados mudaram na serialização.");
            copy.inventory.Add(new SavedItem {slot=-1,quantity=1,itemId="invalid"});
            bool rejected=false;
            try { validate.Invoke(system,new object[]{copy}); }
            catch(TargetInvocationException) { rejected=true; }
            if(!rejected) throw new Exception("Um item inválido não foi rejeitado.");
            Debug.Log("[Save] Serialização e rejeição de dados inválidos passaram. Faça também o teste de dormir e recarregar.");
        }
        catch(Exception e) { Debug.LogError("[Save] Falha na validação de serialização: "+e); }
    }
}
