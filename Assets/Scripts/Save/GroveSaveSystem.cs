using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroveSaveSystem : MonoBehaviour
{
    static GroveSaveSystem instance;
    static bool loadFailed;
    static int selectedSlot;
    bool menuOpen, choosingNew, ready, failedLoad;
    int confirmSlot;
    float previousTimeScale;
    string menuError;
    public static bool MenuOpen => instance != null && instance.menuOpen;
    public static int SelectedSlot => selectedSlot;
    GroveSaveCatalog catalog;
    string status; float statusUntil;
    void Status(string value) { status=value; statusUntil=Time.unscaledTime+5f; }
    void OnGUI()
    {
        if(menuOpen) { DrawMenu(); return; }
        if(!string.IsNullOrEmpty(status) && Time.unscaledTime<statusUntil) GUI.Box(new Rect(12,12,Mathf.Min(650,Screen.width-24),40),status);
    }
    static string PathForSlot(int slot)
    {
        if(slot != 1 && slot != 2) throw new ArgumentOutOfRangeException(nameof(slot));
        // Slot 1 preserves the save from the original system without migration.
        return Path.Combine(Application.persistentDataPath,slot==1 ? "grovewarden-save-v1.json" : "grovewarden-save-slot2-v1.json");
    }
    public static string SavePath => PathForSlot(selectedSlot);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession() { instance=null; loadFailed=false; selectedSlot=0; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if(instance!=null || FindAnyObjectByType<GameManager>()==null) return;
        new GameObject("Grove Save System").AddComponent<GroveSaveSystem>();
    }
    void Awake()
    {
        if(instance != null && instance != this) { Destroy(gameObject); return; }
        instance=this; selectedSlot=0; menuOpen=true; previousTimeScale=Time.timeScale; Time.timeScale=0f;
    }
    IEnumerator Start()
    {
        // Wait until Start has initialized clocks, coins, inventory and UI.
        yield return null;
        catalog=Resources.Load<GroveSaveCatalog>("GroveSaveCatalog");
        if(catalog==null)
        {
            loadFailed=true; failedLoad=true;
            menuError="Catálogo ausente. Saia do Play Mode e use Grove Warden > Salvar jogo > Preparar cena e catálogo.";
            Debug.LogError("[Save] "+menuError); yield break;
        }
        foreach(var p in catalog.npcs) if(p!=null && !NpcProgress.Register(p))
        { loadFailed=true; failedLoad=true; menuError="Erro no catálogo de NPCs. Consulte o Console."; yield break; }
        ready=true;
    }
    void OnDestroy()
    {
        if(instance!=this) return;
        if(menuOpen) Time.timeScale=previousTimeScale;
        instance=null;
    }
    void DrawMenu()
    {
        float width=Mathf.Min(490,Screen.width-24), height=Mathf.Min(390,Screen.height-24);
        if(width<200 || height<200) return;
        Rect panel=new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height);
        GUI.Box(panel,GUIContent.none);
        GUILayout.BeginArea(new Rect(panel.x+18,panel.y+15,panel.width-36,panel.height-30));
        var title=new GUIStyle(GUI.skin.label) {fontSize=24,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
        var label=new GUIStyle(GUI.skin.label) {fontSize=15,wordWrap=true};
        var button=new GUIStyle(GUI.skin.button) {fontSize=16,wordWrap=true};
        GUILayout.Label("GROVE WARDEN",title,GUILayout.Height(40));
        if(!ready || failedLoad)
        {
            GUILayout.Label(failedLoad ? menuError : "Preparando o jogo...",label);
            GUILayout.EndArea(); return;
        }
        if(confirmSlot!=0)
        {
            GUILayout.Label("Iniciar um jogo novo no Slot "+confirmSlot+"?",label);
            GUILayout.Label("A partida atual só será substituída ao dormir. Uma cópia anterior será mantida como backup.",label);
            if(GUILayout.Button("Confirmar novo jogo",button,GUILayout.Height(45))) StartNew(confirmSlot);
            if(GUILayout.Button("Cancelar",button,GUILayout.Height(38))) confirmSlot=0;
        }
        else if(choosingNew)
        {
            GUILayout.Label("Escolha onde guardar a nova partida:",label);
            for(int i=1;i<=2;i++)
            {
                int slot=i;
                string labelText="Slot "+slot+ (File.Exists(PathForSlot(slot)) ? " — ocupado" : " — vazio");
                if(GUILayout.Button(labelText,button,GUILayout.Height(46)))
                { if(File.Exists(PathForSlot(slot))) confirmSlot=slot; else StartNew(slot); }
            }
            if(GUILayout.Button("Voltar",button,GUILayout.Height(38))) choosingNew=false;
        }
        else
        {
            if(GUILayout.Button("Novo jogo",button,GUILayout.Height(46))) choosingNew=true;
            GUILayout.Label("Continuar uma partida:",label);
            for(int i=1;i<=2;i++)
            {
                int slot=i; bool exists=File.Exists(PathForSlot(slot));
                GUI.enabled=exists;
                if(GUILayout.Button("Save Slot "+slot+(exists ? " — continuar" : " — vazio"),button,GUILayout.Height(46))) ContinueSlot(slot);
                GUI.enabled=true;
            }
            if(!string.IsNullOrEmpty(menuError)) GUILayout.Label(menuError,label);
        }
        GUILayout.EndArea();
    }
    void StartNew(int slot)
    {
        selectedSlot=slot; loadFailed=false; menuOpen=false; Time.timeScale=previousTimeScale;
        Status("Novo jogo: Slot "+slot+". Durma para salvar.");
    }
    void ContinueSlot(int slot)
    {
        selectedSlot=slot;
        try
        {
            Load(); loadFailed=false; menuOpen=false; Time.timeScale=previousTimeScale;
            Status("Slot "+slot+" carregado."); Debug.Log("[Save] Slot "+slot+" carregado: "+SavePath);
        }
        catch(Exception e)
        {
            loadFailed=true; failedLoad=true; selectedSlot=0;
            menuError="Não foi possível carregar o slot. Arquivo preservado. Saia do Play Mode e confira o Console.";
            Debug.LogError("[Save] "+menuError+" "+e);
        }
    }
    public static bool SaveAtBed()
    {
        if(instance==null || instance.catalog==null || loadFailed || instance.menuOpen || selectedSlot==0)
        { if(instance!=null) instance.Status("Save indisponível. Veja o Console."); Debug.LogError("[Save] Salvamento indisponível. Verifique as mensagens anteriores no Console."); return false; }
        try { instance.Save(); instance.Status("Jogo salvo no Slot "+selectedSlot+"."); Debug.Log("[Save] Jogo salvo ao dormir: "+SavePath); return true; }
        catch(Exception e) { instance.Status("Erro ao salvar. Veja o Console."); Debug.LogError("[Save] Não foi possível salvar. O arquivo anterior foi preservado. "+e); return false; }
    }
    static string SlotPath(Transform root,Transform child)
    {
        string result=""; var cursor=child;
        while(cursor!=root && cursor!=null) { result="/"+cursor.name+result; cursor=cursor.parent; }
        if(cursor!=root) throw new Exception("Slot fora da área de plantio.");
        return result;
    }
    static Dictionary<string,Transform> Areas()
    {
        var result=new Dictionary<string,Transform>();
        var scene=SceneManager.GetActiveScene();
        foreach(var root in scene.GetRootGameObjects())
        {
            foreach(var zone in root.GetComponentsInChildren<FarmZone>(true)) Add(zone.saveId,zone.transform,result);
            foreach(var plot in root.GetComponentsInChildren<PlotManager>(true))
                if(plot.GetComponentInParent<FarmZone>(true)==null) Add(plot.saveId,plot.transform,result);
        }
        return result;
    }
    static void Add(string id,Transform root,Dictionary<string,Transform> areas)
    {
        if(string.IsNullOrWhiteSpace(id) || areas.ContainsKey(id)) throw new Exception("Área sem ID ou com ID duplicado: "+root.name+". Prepare a cena no editor.");
        areas.Add(id,root);
    }
    static Dictionary<string,PlotSlot> Slots(Transform area)
    {
        var result=new Dictionary<string,PlotSlot>();
        foreach(var slot in area.GetComponentsInChildren<PlotSlot>(true))
        {
            string key=SlotPath(area,slot.transform);
            if(result.ContainsKey(key)) throw new Exception("Nomes de slots duplicados na área "+area.name);
            result.Add(key,slot);
        }
        return result;
    }
    static void Require(bool condition,string message) { if(!condition) throw new Exception(message); }
    GroveSaveData Capture()
    {
        var player=PlayerController.instance; var inv=InventoryManager.instance; var game=GameManager.instance; var farm=FarmManager.instance;
        Require(player!=null && inv!=null && game!=null && farm!=null,"Jogador ou gerenciador ausente.");
        var health=player.GetComponent<Health>(); Require(health!=null && !health.IsDead,"Jogador sem vida válida.");
        var data=new GroveSaveData {scenePath=SceneManager.GetActiveScene().path,playerPosition=player.transform.position,playerHealth=health.CurrentHealth,
            coins=CurrencyManager.coins,day=game.SaveDay,elapsedDay=game.SaveElapsed,clockTime=GameClock.instance!=null ? GameClock.instance.SaveTime : 0,timeRunning=game.SaveTimeRunning,purchasedAreas=farm.PurchasedAreas,selectedSlot=inv.selectedSlotIndex};
        for(int i=0;i<inv.InventorySlots.Length;i++)
        {
            var item=inv.InventorySlots[i]; if(item==null) continue;
            string id=catalog.ItemId(item.itemData); Require(!string.IsNullOrEmpty(id) && item.quantity>0,"Item do inventário ausente do catálogo.");
            data.inventory.Add(new SavedItem {slot=i,itemId=id,quantity=item.quantity});
        }
        foreach(var entry in Areas())
        {
            var zone=entry.Value.GetComponent<FarmZone>();
            var saved=new SavedArea {id=entry.Key,unlocked=zone==null || zone.IsUnlocked};
            foreach(var part in Slots(entry.Value))
            {
                var slot=part.Value; string id=slot.IsPlanted ? catalog.CropId(slot.cropData) : null;
                Require(!slot.IsPlanted || !string.IsNullOrEmpty(id),"Planta ausente do catálogo em "+slot.name);
                saved.plots.Add(new SavedPlot {path=part.Key,cropId=id,days=slot.DaysSincePlanted,planted=slot.IsPlanted,watered=slot.IsWatered,harvestable=slot.IsHarvestable});
            }
            data.areas.Add(saved);
        }
        foreach(var pickup in FindObjectsByType<InventoryPickup>(FindObjectsSortMode.None))
        {
            if(pickup.quantity<=0) continue;
            string id=catalog.ItemId(pickup.itemData); Require(!string.IsNullOrEmpty(id),"Item no chão ausente do catálogo.");
            data.drops.Add(new SavedWorldItem {itemId=id,quantity=pickup.quantity,position=pickup.transform.position});
        }
        data.npcs=NpcProgress.Capture(); Validate(data); return data;
    }
    void Save()
    {
        string json=JsonUtility.ToJson(Capture(),true);
        string tmp=SavePath+".tmp", backup=SavePath+".bak";
        Directory.CreateDirectory(Application.persistentDataPath);
        try
        {
            using(var stream=new FileStream(tmp,FileMode.Create,FileAccess.Write,FileShare.None))
            using(var writer=new StreamWriter(stream)) { writer.Write(json); writer.Flush(); stream.Flush(true); }
            if(File.Exists(SavePath)) File.Replace(tmp,SavePath,backup);
            else File.Move(tmp,SavePath);
        }
        finally { if(File.Exists(tmp)) File.Delete(tmp); }
    }
    void Validate(GroveSaveData data)
    {
        Require(data!=null && data.version==1 && data.scenePath==SceneManager.GetActiveScene().path,"Arquivo inválido, versão ou cena incompatível.");
        Require(data.inventory!=null && data.areas!=null && data.npcs!=null && data.drops!=null,"Arquivo incompleto.");
        Require(data.coins>=0 && data.day>=1 && data.purchasedAreas>=0 && data.selectedSlot>=0 && data.selectedSlot<9 && data.playerHealth>0 && !float.IsNaN(data.playerHealth) && !float.IsInfinity(data.playerHealth) && !float.IsNaN(data.elapsedDay) && !float.IsInfinity(data.elapsedDay) && data.elapsedDay>=0,"Dados de jogador inválidos.");
        Require(Finite(data.playerPosition) && !float.IsNaN(data.clockTime) && !float.IsInfinity(data.clockTime) && data.clockTime>=0,"Posição ou relógio inválido.");
        var usedSlots=new HashSet<int>();
        foreach(var i in data.inventory) Require(i!=null && i.slot>=0 && i.slot<InventoryManager.MAX_SLOTS && i.quantity>0 && usedSlots.Add(i.slot) && catalog.Item(i.itemId)!=null,"Item inválido ou ausente do catálogo.");
        var current=Areas(); var usedAreas=new HashSet<string>();
        foreach(var area in data.areas)
        {
            Require(area!=null && !string.IsNullOrEmpty(area.id) && usedAreas.Add(area.id) && current.ContainsKey(area.id) && area.plots!=null,"Área salva ausente da cena ou duplicada.");
            var configuredZone=current[area.id].GetComponent<FarmZone>();
            Require(configuredZone==null || configuredZone.IsConfigured,"FarmZone com referências inválidas: "+current[area.id].name);
            var slots=Slots(current[area.id]); var usedPaths=new HashSet<string>();
            foreach(var slot in area.plots)
                Require(slot!=null && slot.path!=null && usedPaths.Add(slot.path) && slots.ContainsKey(slot.path) && slot.days>=0 && (!slot.planted || catalog.Crop(slot.cropId)!=null) && (slot.planted || (!slot.watered && !slot.harvestable)),"Canteiro ou planta salva inválida.");
            Require(usedPaths.Count==slots.Count,"A estrutura dos canteiros mudou desde o último save.");
        }
        var npcIds=new HashSet<string>();
        foreach(var p in catalog.npcs) { Require(p!=null && !string.IsNullOrWhiteSpace(p.npcId) && npcIds.Add(p.npcId),"Catálogo de NPCs inválido."); }
        var savedNpcIds=new HashSet<string>();
        foreach(var n in data.npcs) Require(n!=null && !string.IsNullOrWhiteSpace(n.id) && npcIds.Contains(n.id) && savedNpcIds.Add(n.id) && n.delivered>=0,"NPC salvo ausente do catálogo ou inválido.");
        foreach(var drop in data.drops) Require(drop!=null && drop.quantity>0 && catalog.Item(drop.itemId)!=null && Finite(drop.position),"Item no chão inválido ou ausente do catálogo.");
        Require(PlayerController.instance!=null && PlayerController.instance.GetComponent<Health>()!=null && InventoryManager.instance!=null && GameManager.instance!=null && FarmManager.instance!=null,"Gerenciador necessário ausente.");
    }
    static bool Finite(Vector3 p) => !float.IsNaN(p.x) && !float.IsNaN(p.y) && !float.IsNaN(p.z) && !float.IsInfinity(p.x) && !float.IsInfinity(p.y) && !float.IsInfinity(p.z);
    void Load()
    {
        var data=JsonUtility.FromJson<GroveSaveData>(File.ReadAllText(SavePath)); Validate(data);
        var areas=Areas();
        NpcProgress.Restore(data.npcs);
        foreach(var profile in catalog.npcs) NpcProgress.Register(profile);
        foreach(var area in data.areas)
        {
            var root=areas[area.id]; var zone=root.GetComponent<FarmZone>();
            if(zone!=null) zone.RestoreUnlocked(area.unlocked);
            var slots=Slots(root);
            foreach(var slot in area.plots) slots[slot.path].RestorePlot(slot.planted ? catalog.Crop(slot.cropId) : null,slot.days,slot.planted,slot.watered,slot.harvestable);
        }
        var items=new Item[InventoryManager.MAX_SLOTS];
        foreach(var i in data.inventory) items[i.slot]=new Item {itemData=catalog.Item(i.itemId),quantity=i.quantity};
        InventoryManager.instance.RestoreSlots(items,data.selectedSlot);
        CurrencyManager.RestoreCoins(data.coins);
        FarmManager.instance.RestorePurchasedAreas(data.purchasedAreas);
        GameManager.instance.RestoreClock(data.day,data.elapsedDay,data.timeRunning);
        if(GameClock.instance!=null) GameClock.instance.RestoreTime(data.clockTime);
        var player=PlayerController.instance;
        var body=player.GetComponent<Rigidbody2D>();
        if(body!=null) { body.linearVelocity=Vector2.zero; body.position=data.playerPosition; }
        player.transform.position=data.playerPosition;
        player.GetComponent<Health>().RestoreSavedHealth(data.playerHealth);
        foreach(var pickup in FindObjectsByType<InventoryPickup>(FindObjectsSortMode.None)) Destroy(pickup.gameObject);
        foreach(var drop in data.drops) InventoryPickup.Spawn(catalog.Item(drop.itemId),drop.quantity,drop.position);
    }
}
