using System;
using System.Collections.Generic;
using UnityEngine;

public class GroveSaveCatalog : ScriptableObject
{
    [Serializable] public class ItemEntry { public string id; public ItemData item; }
    [Serializable] public class CropEntry { public string id; public CropData crop; }
    public List<ItemEntry> items = new List<ItemEntry>();
    public List<CropEntry> crops = new List<CropEntry>();
    public List<NpcProfile> npcs = new List<NpcProfile>();
    public string ItemId(ItemData item) { foreach(var e in items) if(e.item == item) return e.id; return null; }
    public string CropId(CropData crop) { foreach(var e in crops) if(e.crop == crop) return e.id; return null; }
    public ItemData Item(string id) { foreach(var e in items) if(e.id == id) return e.item; return null; }
    public CropData Crop(string id) { foreach(var e in crops) if(e.id == id) return e.crop; return null; }
}
