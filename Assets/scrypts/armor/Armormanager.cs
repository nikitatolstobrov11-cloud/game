using UnityEngine;
using TMPro;

public class ArmorManager : MonoBehaviour
{
    [Header("Кости для статичных мешей (шлем)")]
    public Transform helmetBone;

    [Header("Смещение шлема (только для статичного меша)")]
    public Vector3 helmetOffset   = Vector3.zero;
    public Vector3 helmetRotation = Vector3.zero;

    [Header("UI слоты экипировки (опционально)")]
    public UnityEngine.UI.Image helmetSlotIcon;
    public UnityEngine.UI.Image chestSlotIcon;
    public UnityEngine.UI.Image glovesSlotIcon;
    public UnityEngine.UI.Image pantsSlotIcon;
    public UnityEngine.UI.Image bootsSlotIcon;
    public TextMeshProUGUI      defenseText;

    // Текущая надетая броня
    private InventoryItem _equippedHelmet;
    private InventoryItem _equippedChest;
    private InventoryItem _equippedGloves;
    private InventoryItem _equippedPants;
    private InventoryItem _equippedBoots;

    // Визуальные объекты
    private GameObject _helmetVisual;
    private GameObject _chestVisual;
    private GameObject _glovesVisual;
    private GameObject _pantsVisual;
    private GameObject _bootsVisual;

    private skeleton  _skeleton;
    private Inventory _inventory;

    public int TotalArmorDefense =>
        (_equippedHelmet?.defenseBonus ?? 0) +
        (_equippedChest?.defenseBonus  ?? 0) +
        (_equippedGloves?.defenseBonus ?? 0) +
        (_equippedPants?.defenseBonus  ?? 0) +
        (_equippedBoots?.defenseBonus  ?? 0);

    public int TotalArmorHealth =>
        (_equippedHelmet?.healthBonus ?? 0) +
        (_equippedChest?.healthBonus  ?? 0) +
        (_equippedGloves?.healthBonus ?? 0) +
        (_equippedPants?.healthBonus  ?? 0) +
        (_equippedBoots?.healthBonus  ?? 0);

    public static ArmorManager Instance { get; private set; }

    void Awake()
    {
        Instance   = this;
        _skeleton  = GetComponent<skeleton>();
        _inventory = GetComponent<Inventory>();
    }

    // ── Надеть броню ──
    public void EquipArmor(InventoryItem item)
    {
        if (item == null || item.itemType != ItemType.Armor) return;

        switch (item.armorSlot)
        {
            case ArmorSlot.Helmet: EquipToSlot(item, ref _equippedHelmet, ref _helmetVisual, helmetSlotIcon); break;
            case ArmorSlot.Chest:  EquipToSlot(item, ref _equippedChest,  ref _chestVisual,  chestSlotIcon);  break;
            case ArmorSlot.Gloves: EquipToSlot(item, ref _equippedGloves, ref _glovesVisual, glovesSlotIcon); break;
            case ArmorSlot.Pants:  EquipToSlot(item, ref _equippedPants,  ref _pantsVisual,  pantsSlotIcon);  break;
            case ArmorSlot.Boots:  EquipToSlot(item, ref _equippedBoots,  ref _bootsVisual,  bootsSlotIcon);  break;
            default:
                Debug.LogWarning($"ArmorManager: у предмета {item.itemName} не задан ArmorSlot!");
                return;
        }

        ApplyStats();
        UpdateUI();
        Debug.Log($"🛡️ Надета броня: {item.itemName} (+{item.defenseBonus} защиты)");
    }

    // ── Снять броню ──
    public void UnequipArmor(ArmorSlot slot)
    {
        switch (slot)
        {
            case ArmorSlot.Helmet: UnequipSlot(ref _equippedHelmet, ref _helmetVisual, helmetSlotIcon); break;
            case ArmorSlot.Chest:  UnequipSlot(ref _equippedChest,  ref _chestVisual,  chestSlotIcon);  break;
            case ArmorSlot.Gloves: UnequipSlot(ref _equippedGloves, ref _glovesVisual, glovesSlotIcon); break;
            case ArmorSlot.Pants:  UnequipSlot(ref _equippedPants,  ref _pantsVisual,  pantsSlotIcon);  break;
            case ArmorSlot.Boots:  UnequipSlot(ref _equippedBoots,  ref _bootsVisual,  bootsSlotIcon);  break;
        }
        ApplyStats();
        UpdateUI();
    }

    public InventoryItem GetEquipped(ArmorSlot slot) => slot switch
    {
        ArmorSlot.Helmet => _equippedHelmet,
        ArmorSlot.Chest  => _equippedChest,
        ArmorSlot.Gloves => _equippedGloves,
        ArmorSlot.Pants  => _equippedPants,
        ArmorSlot.Boots  => _equippedBoots,
        _                => null
    };

    // ─────────────────────────────────────────

    void EquipToSlot(InventoryItem item, ref InventoryItem slot,
                     ref GameObject visual, UnityEngine.UI.Image slotIcon)
    {
        if (slot != null)
        {
            RemoveVisual(ref visual);
            _inventory?.AddItem(slot, 1);
        }

        slot = item;
        _inventory?.RemoveItem(item, 1);
        SpawnVisual(item, ref visual);

        if (slotIcon != null)
        {
            slotIcon.sprite  = item.icon;
            slotIcon.enabled = true;
            slotIcon.color   = Color.white;
        }
    }

    void UnequipSlot(ref InventoryItem slot, ref GameObject visual,
                     UnityEngine.UI.Image slotIcon)
    {
        if (slot == null) return;
        _inventory?.AddItem(slot, 1);
        RemoveVisual(ref visual);
        slot = null;

        if (slotIcon != null)
        {
            slotIcon.sprite  = null;
            slotIcon.enabled = false;
        }
    }

    void SpawnVisual(InventoryItem item, ref GameObject visual)
    {
        if (item.armorPrefab == null) return;

        SkinnedMeshRenderer skinned = item.armorPrefab.GetComponentInChildren<SkinnedMeshRenderer>();

        if (skinned != null && SkinnedArmorEquip.Instance != null)
        {
            // Skinned меш — переназначаем кости
            visual = SkinnedArmorEquip.Instance.EquipSkinnedArmor(item.armorPrefab);
        }
        else if (helmetBone != null)
        {
            // Статичный меш — крепим к кости головы
            visual = Instantiate(item.armorPrefab, helmetBone);
            visual.transform.localPosition = item.armorOffset;
            visual.transform.localRotation = Quaternion.Euler(item.armorRotation);
            visual.transform.localScale    = Vector3.one;
        }
    }

    void RemoveVisual(ref GameObject visual)
    {
        if (visual != null) { Destroy(visual); visual = null; }
    }

    void ApplyStats()
    {
        if (_skeleton == null) return;
        _skeleton.currentHealth = Mathf.Min(
            _skeleton.currentHealth,
            _skeleton.maxHealth + TotalArmorHealth
        );
        Debug.Log($"🛡️ Защита: {TotalArmorDefense} | Бонус HP: +{TotalArmorHealth}");
    }

    void UpdateUI()
    {
        if (defenseText != null)
            defenseText.text = $"🛡 {TotalArmorDefense}";
    }

    void Update()
    {
        // Шлем (статичный меш) — обновляем в реальном времени
        if (_helmetVisual != null && _equippedHelmet != null)
        {
            bool isSkinned = _equippedHelmet.armorPrefab != null &&
                             _equippedHelmet.armorPrefab.GetComponentInChildren<SkinnedMeshRenderer>() != null;
            if (!isSkinned)
            {
                _helmetVisual.transform.localPosition = helmetOffset;
                _helmetVisual.transform.localRotation = Quaternion.Euler(helmetRotation);
            }
        }
    }
}