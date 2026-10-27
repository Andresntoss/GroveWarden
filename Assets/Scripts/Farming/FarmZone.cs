using UnityEngine;

public class FarmZone : MonoBehaviour
{
    [Tooltip("Arraste o filho Plot, com terra e seis canteiros.")]
    public GameObject unlockedContent;
    [Tooltip("Opcional: grupo filho contendo todos os visuais da compra. Nunca use a raiz.")]
    public GameObject purchaseSign;
    [Tooltip("Na estrutura atual: InteractionPoint e InteractionPoint2.")]
    public GameObject[] purchaseVisuals = new GameObject[0];
    [Tooltip("Somente os colliders de compra, não os colliders dos Slot1...Slot6.")]
    public Collider2D[] purchaseColliders = new Collider2D[0];
    [SerializeField, HideInInspector] public string saveId;
    public bool startsUnlocked;
    public bool IsUnlocked { get; private set; }
    public bool IsConfigured
    {
        get
        {
            if (unlockedContent == null || unlockedContent == gameObject || !unlockedContent.transform.IsChildOf(transform)) return false;
            bool hasVisual = false;
            if (purchaseSign != null) { if (!ValidVisual(purchaseSign)) return false; hasVisual = true; }
            if (purchaseVisuals != null) foreach (var visual in purchaseVisuals)
            {
                if (visual == null) continue;
                if (!ValidVisual(visual)) return false;
                hasVisual = true;
            }
            return hasVisual;
        }
    }
    bool ValidVisual(GameObject visual)
    {
        return visual != gameObject && visual != unlockedContent && visual.transform.IsChildOf(transform) && !unlockedContent.transform.IsChildOf(visual.transform);
    }
    void Awake()
    {
        IsUnlocked = startsUnlocked;
        ApplyState();
    }
    public bool Unlock()
    {
        if (IsUnlocked || !IsConfigured) return false;
        IsUnlocked = true;
        ApplyState();
        return true;
    }
    public void RestoreUnlocked(bool value)
    {
        IsUnlocked = value; ApplyState();
    }
    void ApplyState()
    {
        if (!IsConfigured) { Debug.LogWarning("FarmZone: atribua Plot e os visuais da placa como filhos da raiz.",this); return; }
        unlockedContent.SetActive(IsUnlocked);
        if (purchaseSign != null) purchaseSign.SetActive(!IsUnlocked);
        if (purchaseVisuals != null) foreach (var visual in purchaseVisuals) if (visual != null) visual.SetActive(!IsUnlocked);
        // The root remains active so the Plot child survives the purchase.
        if (purchaseColliders != null) foreach (var collider in purchaseColliders)
        {
            if (collider != null && !collider.transform.IsChildOf(unlockedContent.transform)) collider.enabled = !IsUnlocked;
        }
        var sign = GetComponent<BuildSign>();
        if (sign != null)
        {
            foreach (var collider in sign.GetComponents<Collider2D>()) collider.enabled = !IsUnlocked;
            if (IsUnlocked) sign.SetSelected(false);
        }
    }
}
