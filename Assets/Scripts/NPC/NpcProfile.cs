using System;
using UnityEngine;

[CreateAssetMenu(menuName="Grove Warden/NPC/Perfil")]
public class NpcProfile : ScriptableObject
{
    [Tooltip("Identificador permanente e exclusivo para salvar o progresso.")]
    public string npcId = "fazendeira_01";
    public string displayName = "Fazendeira";
    [TextArea] public string description = "Cuida da fazenda e ajuda a melhorar as colheitas.";
    public Sprite portrait;
    public Sprite fullArt;
    [TextArea] public string[] dialogue = { "Olá! Posso ajudar você a cuidar da fazenda.", "Traga cenouras para conhecer mais sobre mim." };
    public ItemData requestedItem;
    [Min(1)] public int galleryTarget = 50;
    public string galleryTitle = "Uma tarde na fazenda";
    public Sprite galleryArt;
    [Min(0)] public int upgradeCoinCost = 100;
    [Min(0)] public int upgradeItemCost = 10;
    [Min(1)] public int harvestBonus = 1;
}
