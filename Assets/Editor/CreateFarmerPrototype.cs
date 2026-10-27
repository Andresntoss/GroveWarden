using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateFarmerPrototype
{
    const string ProfilePath = "Assets/GameData/NPC/FazendeiraTeste.asset";
    const string CarrotPath = "Assets/Scripts/Inventory/ItemDataCollection/Carrot.asset";
    const string ArtPath = "Assets/Sprites/Pacote Custom/Entities/Npc's/Rogue/Idle/Idle-Sheet.png";
    [MenuItem("Grove Warden/NPC/Criar fazendeira de teste na cena")]
    static void Create()
    {
        if (Application.isPlaying) { EditorUtility.DisplayDialog("NPC", "Saia do Play Mode antes de criar o NPC.", "OK"); return; }
        var existing = Object.FindObjectsByType<NpcCharacter>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach (var npc in existing)
            if (npc.profile != null && npc.profile.npcId == "fazendeira_01")
            { Selection.activeGameObject = npc.gameObject; EditorUtility.DisplayDialog("NPC", "Este NPC já existe na cena. Ele foi selecionado na Hierarchy.", "OK"); return; }
        NpcProfile profile = AssetDatabase.LoadAssetAtPath<NpcProfile>(ProfilePath);
        if (profile == null)
        {
            Directory.CreateDirectory("Assets/GameData/NPC"); AssetDatabase.Refresh();
            profile = ScriptableObject.CreateInstance<NpcProfile>();
            profile.npcId = "fazendeira_01"; profile.displayName = "Fazendeira";
            profile.requestedItem = AssetDatabase.LoadAssetAtPath<ItemData>(CarrotPath);
            foreach (Object item in AssetDatabase.LoadAllAssetsAtPath(ArtPath))
                if (item is Sprite sprite) { profile.portrait = sprite; profile.fullArt = sprite; profile.galleryArt = sprite; break; }
            AssetDatabase.CreateAsset(profile,ProfilePath); AssetDatabase.SaveAssets();
        }
        var obj = new GameObject("NPC - Fazendeira de teste");
        Undo.RegisterCreatedObjectUndo(obj,"Criar fazendeira");
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) obj.transform.position = player.transform.position + new Vector3(2,0,0);
        else if (SceneView.lastActiveSceneView != null) obj.transform.position = SceneView.lastActiveSceneView.pivot;
        obj.transform.position = new Vector3(obj.transform.position.x,obj.transform.position.y,0);
        var spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = profile.portrait; spriteRenderer.sortingOrder = 10;
        var npcCharacter = obj.AddComponent<NpcCharacter>(); npcCharacter.profile = profile;
        EditorSceneManager.MarkSceneDirty(obj.scene);
        Selection.activeGameObject = obj;
        if (profile.requestedItem == null) Debug.LogWarning("Defina a cenoura no perfil do NPC.",profile);
        if (profile.portrait == null) Debug.LogWarning("Defina um sprite provisório no perfil e no SpriteRenderer do NPC.",profile);
        EditorUtility.DisplayDialog("NPC de teste criado", "O NPC foi criado próximo do jogador. Arraste-o para onde quiser na cena e salve. Em Play Mode, aproxime-se e pressione F; abra o álbum com N. Configure as artes no perfil.", "OK");
    }
}
