using UnityEngine;

public class NpcCharacter : MonoBehaviour
{
    public NpcProfile profile;
    [Min(.1f)] public float interactionDistance = 2.5f;
    void OnEnable() { NpcAlbum.EnsureExists(); NpcAlbum.Register(this); }
    void OnDisable() { NpcAlbum.Unregister(this); }
}
