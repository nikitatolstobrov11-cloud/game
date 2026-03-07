using UnityEngine;

public enum ItemRarity  { Common, Uncommon, Rare, Epic, Legendary }
public enum ItemType    { All, Weapon, Armor, Consumable }
public enum ArmorSlot   { None, Helmet, Chest, Gloves, Pants, Boots }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Основное")]
    public string itemName;
    public Sprite icon;
    public int maxStack = 99;
    [TextArea] public string description;

    [Header("Классификация")]
    public ItemType   itemType  = ItemType.All;
    public ItemRarity rarity    = ItemRarity.Common;
    public ArmorSlot  armorSlot = ArmorSlot.None;

    [Header("Характеристики (0 = не используется)")]
    public int attackBonus  = 0;
    public int defenseBonus = 0;
    public int healthBonus  = 0;

    [Header("Визуал брони")]
    public GameObject armorPrefab;

    [Header("Смещение (только для статичных мешей)")]
    public Vector3 armorOffset   = Vector3.zero;
    public Vector3 armorRotation = Vector3.zero;
}