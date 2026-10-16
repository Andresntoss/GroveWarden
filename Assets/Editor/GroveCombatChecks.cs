using System;
using UnityEditor;
using UnityEngine;

public static class GroveCombatChecks
{
    static void Check(bool result, string description)
    {
        if (!result) throw new Exception("Combat check failed: " + description);
    }
    [MenuItem("Grove Warden/Validar vida e dano (Play Mode)")]
    static void Run()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Entre em Play Mode para validar."); return; }
        var go = new GameObject("Temporary combat validation");
        try
        {
            go.AddComponent<BoxCollider2D>();
            var health = go.AddComponent<Health>();
            Check(health.CurrentHealth == 100 && !health.IsDead, "initial health");
            Check(!health.TryTakeDamage(-1) && !health.TryTakeDamage(float.NaN), "invalid damage");
            Check(health.TryTakeDamage(20) && health.CurrentHealth == 80, "accepted damage");
            Check(!health.TryTakeDamage(20) && health.CurrentHealth == 80, "invulnerability");
            health.RestoreFullHealth();
            Check(health.CurrentHealth == 100 && health.TryTakeDamage(10), "restore clears invulnerability");
            health.RestoreFullHealth();
            Check(health.TryTakeDamage(200) && health.IsDead && health.CurrentHealth == 0, "lethal damage");
            Check(!go.GetComponent<BoxCollider2D>().enabled, "dead body collider disabled");
            Check(!health.TryTakeDamage(20), "reject repeated death damage");
            health.RestoreFullHealth();
            Check(!health.IsDead && go.GetComponent<BoxCollider2D>().enabled, "revival restores body");
            Debug.Log("Grove Warden: verificações de vida e dano passaram. Teste também ataques e animações na cena.");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
}
