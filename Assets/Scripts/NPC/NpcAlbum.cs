using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-50)]
public class NpcAlbum : MonoBehaviour
{
    static NpcAlbum instance;
    static readonly List<NpcCharacter> Characters = new List<NpcCharacter>();
    static readonly List<NpcProfile> Profiles = new List<NpcProfile>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession() { instance=null; Characters.Clear(); Profiles.Clear(); }
    public static bool IsOpen => instance != null && instance.mode != Mode.Closed;
    enum Mode { Closed, Dialogue, Album, DeliveryConfirm, UpgradeConfirm }
    Mode mode;
    NpcProfile selected;
    int dialogueIndex;
    int proposedDelivery;
    string message = "";
    Vector2 scroll;
    GUIStyle title, text, button;
    public static void EnsureExists()
    {
        if (!Application.isPlaying || instance != null) return;
        if (FindAnyObjectByType<NpcAlbum>() != null) return;
        new GameObject("NPC Album").AddComponent<NpcAlbum>();
    }
    public static void Register(NpcCharacter character)
    {
        if (character == null || character.profile == null || !NpcProgress.Register(character.profile)) return;
        if (!Characters.Contains(character)) Characters.Add(character);
        if (!Profiles.Contains(character.profile)) Profiles.Add(character.profile);
    }
    public static void Unregister(NpcCharacter character) { Characters.Remove(character); }
    void Awake() { if (instance != null && instance != this) { Destroy(gameObject); return; } instance = this; }
    void OnDestroy() { if (instance == this) instance = null; }
    void Update()
    {
        if (Time.timeScale == 0) return;
        var es = EventSystem.current;
        var focus = es != null ? es.currentSelectedGameObject : null;
        if (focus != null && (focus.GetComponent<TMP_InputField>() != null || focus.GetComponent<InputField>() != null)) return;
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpen) { mode = Mode.Closed; return; }
        if (HotbarUi.IsInventoryOpen) return;
        if (Input.GetKeyDown(KeyCode.N)) { mode = mode == Mode.Closed ? Mode.Album : Mode.Closed; message = ""; return; }
        if (mode != Mode.Closed || !Input.GetKeyDown(KeyCode.F)) return;
        var player = PlayerController.instance;
        if (player == null || !player.isActiveAndEnabled) return;
        NpcCharacter closest = null;
        float best = float.PositiveInfinity;
        foreach (var character in Characters)
        {
            if (character == null || !character.isActiveAndEnabled || character.profile == null) continue;
            float distance = Vector2.Distance(player.transform.position,character.transform.position);
            if (distance <= character.interactionDistance && distance < best) { closest=character; best=distance; }
        }
        if (closest == null) return;
        selected=closest.profile; NpcProgress.Meet(selected); dialogueIndex=0; mode=Mode.Dialogue;
    }
    static void DrawSprite(Sprite sprite, Rect rect)
    {
        if (sprite == null) { GUI.Box(rect,"Arte provisória"); return; }
        Rect t = sprite.textureRect; Texture tex = sprite.texture;
        var uv = new Rect(t.x/tex.width,t.y/tex.height,t.width/tex.width,t.height/tex.height);
        float scale = Mathf.Min(rect.width/t.width,rect.height/t.height);
        var dest = new Rect(rect.center.x-t.width*scale/2,rect.center.y-t.height*scale/2,t.width*scale,t.height*scale);
        GUI.DrawTextureWithTexCoords(dest,tex,uv,true);
    }
    void Styles()
    {
        if (title != null) return;
        title = new GUIStyle(GUI.skin.label) { fontSize=22, fontStyle=FontStyle.Bold, wordWrap=true };
        text = new GUIStyle(GUI.skin.label) { fontSize=16, wordWrap=true };
        button = new GUIStyle(GUI.skin.button) { fontSize=15, wordWrap=true };
    }
    void OnGUI()
    {
        if (!IsOpen) return;
        Styles();
        float width = Mathf.Min(850,Screen.width-24), height = Mathf.Min(610,Screen.height-24);
        if (width < 200 || height < 200) return;
        Rect panel = new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height);
        GUI.Box(panel,GUIContent.none);
        GUILayout.BeginArea(new Rect(panel.x+16,panel.y+12,panel.width-32,panel.height-24));
        GUILayout.BeginHorizontal(); GUILayout.Label(mode == Mode.Dialogue ? "Conversa" : "Álbum de NPCs",title);
        if (GUILayout.Button("Fechar",button,GUILayout.Width(80),GUILayout.Height(30))) mode=Mode.Closed;
        GUILayout.EndHorizontal();
        if (mode == Mode.Dialogue) DrawDialogue();
        else if (mode == Mode.DeliveryConfirm) DrawDelivery();
        else if (mode == Mode.UpgradeConfirm) DrawUpgradeConfirm();
        else if (mode == Mode.Album) DrawAlbum();
        GUILayout.EndArea();
    }
    void DrawDialogue()
    {
        if (selected == null) { mode=Mode.Closed; return; }
        GUILayout.Label(selected.displayName,title);
        Rect portrait = GUILayoutUtility.GetRect(150,150,GUILayout.Width(150),GUILayout.Height(150)); DrawSprite(selected.portrait,portrait);
        string[] lines = selected.dialogue;
        GUILayout.Label(lines != null && dialogueIndex < lines.Length ? lines[dialogueIndex] : "Até logo!",text,GUILayout.MinHeight(55));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Continuar",button,GUILayout.Height(38)))
        { dialogueIndex++; if (lines == null || dialogueIndex >= lines.Length) mode=Mode.Closed; }
        if (GUILayout.Button("Abrir ficha",button,GUILayout.Height(38))) mode=Mode.Album;
        GUILayout.EndHorizontal();
    }
    void DrawAlbum()
    {
        GUILayout.Label("N: abrir ou fechar   |   F: conversar   |   Esc: fechar",text);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(180)); GUILayout.Label("Conhecidos",title);
        foreach (var p in Profiles)
        {
            if (p == null || !NpcProgress.Known(p)) continue;
            if (GUILayout.Button(p.displayName,button,GUILayout.Height(36))) { selected=p; message=""; }
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if (selected == null || !NpcProgress.Known(selected)) GUILayout.Label("Conheça um NPC para preencher o álbum.",text);
        else
        {
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label(selected.displayName,title);
            var art = GUILayoutUtility.GetRect(150,150,GUILayout.Width(150),GUILayout.Height(150)); DrawSprite(selected.fullArt,art);
            GUILayout.Label(selected.description,text);
            GUILayout.Space(8); GUILayout.Label("Galeria",title);
            GUILayout.Label("Primeiro encontro — desbloqueada",text);
            int delivered = NpcProgress.Delivered(selected), target = Mathf.Max(1,selected.galleryTarget);
            GUILayout.Label(selected.galleryTitle + " — entregue " + target + " " + (selected.requestedItem != null ? selected.requestedItem.itemName : "itens") + " ("+delivered+"/"+target+")",text);
            if (delivered >= target)
            { var art2 = GUILayoutUtility.GetRect(150,150,GUILayout.Width(150),GUILayout.Height(150)); DrawSprite(selected.galleryArt,art2); }
            else if (GUILayout.Button("Entregar itens",button,GUILayout.Height(35)))
            {
                proposedDelivery = InventoryManager.instance != null && selected.requestedItem != null ? Mathf.Min(target-delivered,InventoryManager.instance.CountItem(selected.requestedItem)) : 0;
                mode=Mode.DeliveryConfirm;
            }
            GUILayout.Space(8); GUILayout.Label("Melhoria da fazenda",title);
            GUILayout.Label("+"+selected.harvestBonus+" produto por colheita. Custo: "+selected.upgradeCoinCost+" moedas e "+selected.upgradeItemCost+" "+(selected.requestedItem != null ? selected.requestedItem.itemName : "itens")+".",text);
            if (NpcProgress.Upgraded(selected)) GUILayout.Label("Melhoria adquirida",text);
            else if (GUILayout.Button("Comprar melhoria",button,GUILayout.Height(35))) mode=Mode.UpgradeConfirm;
            if (!string.IsNullOrEmpty(message)) GUILayout.Label(message,text);
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical(); GUILayout.EndHorizontal();
    }
    void DrawDelivery()
    {
        if (selected == null) { mode=Mode.Album; return; }
        string item = selected.requestedItem != null ? selected.requestedItem.itemName : "itens";
        GUILayout.Label("Entregar " + proposedDelivery + " " + item + "?",title);
        GUILayout.Label("Esses itens serão retirados do inventário. Progresso: " + NpcProgress.Delivered(selected) + "/"+Mathf.Max(1,selected.galleryTarget)+".",text);
        GUI.enabled = proposedDelivery > 0;
        if (GUILayout.Button("Confirmar entrega",button,GUILayout.Height(40)))
        { int count=NpcProgress.Deliver(selected,proposedDelivery); message=count>0 ? count+" itens entregues." : "Entrega não realizada. Confira o inventário."; mode=Mode.Album; }
        GUI.enabled = true;
        if (GUILayout.Button("Cancelar",button,GUILayout.Height(35))) mode=Mode.Album;
    }
    void DrawUpgradeConfirm()
    {
        if (selected == null) { mode=Mode.Album; return; }
        GUILayout.Label("Confirmar melhoria?",title);
        GUILayout.Label("Consome "+selected.upgradeCoinCost+" moedas e "+selected.upgradeItemCost+" "+(selected.requestedItem != null ? selected.requestedItem.itemName : "itens")+". Bônus: +"+selected.harvestBonus+" produto por colheita.",text);
        if (GUILayout.Button("Confirmar compra",button,GUILayout.Height(40)))
        { message=NpcProgress.BuyUpgrade(selected) ? "Melhoria adquirida." : "Compra não realizada: confira os recursos."; mode=Mode.Album; }
        if (GUILayout.Button("Cancelar",button,GUILayout.Height(35))) mode=Mode.Album;
    }
}
