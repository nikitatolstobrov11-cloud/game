using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Этот скрипт управляет панелью инвентаря: вкладки, обновление слотов
// Повесь на корневой объект InventoryPanel
public class InventoryUI : MonoBehaviour
{
    [Header("Slots")]
    public Transform slotsParent;      // Grid объект где лежат слоты

    [Header("Tabs")]
    public Button btnAll;
    public Button btnWeapon;
    public Button btnArmor;
    public Button btnConsumable;

    [Header("Tab Active Colors")]
    public Color tabActiveColor   = new Color(0.85f, 0.70f, 0.35f);
    public Color tabInactiveColor = new Color(0.20f, 0.10f, 0.03f, 0.85f);

    [Header("Gold")]
    public TextMeshProUGUI goldText;

    // ───── внутренние ─────
    private List<InventorySlot> _slots = new List<InventorySlot>();
    private ItemType _currentTab = ItemType.All;

    void Awake()
    {
        // Берём только прямых детей slotsParent (не рекурсивно!)
        foreach (Transform child in slotsParent)
        {
            InventorySlot s = child.GetComponent<InventorySlot>();
            if (s != null) _slots.Add(s);
        }

        // Подключаем вкладки
        btnAll        .onClick.AddListener(() => SetTab(ItemType.All));
        btnWeapon     .onClick.AddListener(() => SetTab(ItemType.Weapon));
        btnArmor      .onClick.AddListener(() => SetTab(ItemType.Armor));
        btnConsumable .onClick.AddListener(() => SetTab(ItemType.Consumable));

        RefreshTabColors();
    }

    // Вызывается из Inventory.cs после каждого изменения
    public void Refresh(List<InventorySlotData> items)
    {
        // Показываем/скрываем слоты по вкладке
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i >= items.Count)
            {
                _slots[i].Clear();
                _slots[i].gameObject.SetActive(true);
                continue;
            }

            var data = items[i];

            // Фильтр по вкладке
            bool show = _currentTab == ItemType.All
                        || data.IsEmpty
                        || data.item.itemType == _currentTab;

            if (!show)
            {
                _slots[i].Clear();
                // Можно скрыть слот совсем или просто очистить — оставляем пустым
                continue;
            }

            if (data.IsEmpty)
                _slots[i].Clear();
            else
                _slots[i].SetItem(data.item, data.count);
        }
    }

    public void SetGold(int amount)
    {
        if (goldText != null)
            goldText.text = amount.ToString("N0") + " 🪙";
    }

    // ───── вкладки ─────

    void SetTab(ItemType tab)
    {
        _currentTab = tab;
        RefreshTabColors();

        // Запрашиваем обновление у Inventory
        Inventory inv = GetComponentInParent<Inventory>();
        if (inv == null) inv = FindObjectOfType<Inventory>();
        if (inv != null) inv.RequestUIRefresh();
    }

    void RefreshTabColors()
    {
        SetTabColor(btnAll,         _currentTab == ItemType.All);
        SetTabColor(btnWeapon,      _currentTab == ItemType.Weapon);
        SetTabColor(btnArmor,       _currentTab == ItemType.Armor);
        SetTabColor(btnConsumable,  _currentTab == ItemType.Consumable);
    }

    void SetTabColor(Button btn, bool active)
    {
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = active ? tabActiveColor : tabInactiveColor;

        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.color = active ? Color.black : new Color(0.85f, 0.70f, 0.35f);
    }
}