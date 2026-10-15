using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public class InventoryPickup : MonoBehaviour
{
    public ItemData itemData;
    [Min(1)] public int quantity = 1;
    public static void Spawn(ItemData data, int amount, Vector3 position)
    {
        if (data == null || amount <= 0) return;
        var go = new GameObject("Item - " + data.itemName);
        go.transform.position = position;
        var pickup = go.AddComponent<InventoryPickup>();
        pickup.itemData = data; pickup.quantity = amount;
        go.GetComponent<SpriteRenderer>().sprite = data.itemIcon;
        go.GetComponent<SpriteRenderer>().sortingOrder = 10;
        var collider = go.GetComponent<CircleCollider2D>();
        collider.isTrigger = true; collider.radius = .45f;
    }
    void OnTriggerEnter2D(Collider2D other) { TryCollect(other); }
    void OnTriggerStay2D(Collider2D other) { TryCollect(other); }
    void TryCollect(Collider2D other)
    {
        if (quantity <= 0 || !other.CompareTag("Player") || InventoryManager.instance == null) return;
        if (!InventoryManager.instance.TryAddItem(itemData, quantity)) return;
        quantity = 0;
        Destroy(gameObject);
    }
}
