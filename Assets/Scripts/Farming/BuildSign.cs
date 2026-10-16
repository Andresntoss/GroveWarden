using UnityEngine;

public class BuildSign : MonoBehaviour
{
    public GameObject modulePrefab;
    [Tooltip("Legado: o preço agora é calculado no FarmManager.")]
    public int moduleCost;
    public string moduleName = "Canteiro de Plantio";
    public GameObject interactionPrompt;
    bool purchased;
    public int CurrentPrice => FarmManager.instance != null ? FarmManager.instance.CurrentPrice : moduleCost;
    void OnEnable() { FarmInteraction.EnsureExists(); SetSelected(false); }
    void OnDisable() { SetSelected(false); }
    public void SetSelected(bool selected)
    {
        if (interactionPrompt == null) return;
        var zone = GetComponentInParent<FarmZone>();
        if (zone != null)
        {
            // A permanent purchase marker must not be hidden as a hover prompt.
            if (zone.purchaseSign == interactionPrompt) return;
            if (zone.purchaseVisuals != null)
                foreach (var visual in zone.purchaseVisuals) if (visual == interactionPrompt) return;
        }
        interactionPrompt.SetActive(selected && !purchased);
    }
    public bool Purchase()
    {
        if (purchased || !isActiveAndEnabled || FarmManager.instance == null) return false;
        FarmZone zone = GetComponentInParent<FarmZone>();
        if (zone != null)
        {
            bool success = FarmManager.instance.TryPurchase(zone);
            if (success) purchased = true;
            return success;
        }
        if (!FarmManager.instance.TryBuildModule(transform.position,modulePrefab,moduleCost)) return false;
        purchased = true; gameObject.SetActive(false); Destroy(gameObject); return true;
    }
}
