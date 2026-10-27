using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class GroveSaveData
{
    public int version = 1;
    public string scenePath;
    public Vector3 playerPosition;
    public float playerHealth;
    public int coins, day, purchasedAreas, selectedSlot;
    public float elapsedDay, clockTime;
    public bool timeRunning;
    public List<SavedItem> inventory = new List<SavedItem>();
    public List<SavedArea> areas = new List<SavedArea>();
    public List<SavedWorldItem> drops = new List<SavedWorldItem>();
    public List<SavedNpc> npcs = new List<SavedNpc>();
}
[Serializable] public class SavedItem { public int slot, quantity; public string itemId; }
[Serializable] public class SavedArea { public string id; public bool unlocked; public List<SavedPlot> plots = new List<SavedPlot>(); }
[Serializable] public class SavedPlot { public string path, cropId; public int days; public bool planted, watered, harvestable; }
[Serializable] public class SavedWorldItem { public string itemId; public int quantity; public Vector3 position; }
[Serializable] public class SavedNpc { public string id; public bool known, upgrade; public int delivered; }
