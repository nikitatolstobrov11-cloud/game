using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChestUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject      chestPanel;
    public Transform       slotsParent;
    public TextMeshProUGUI titleText;
    public Button          closeButton;
    public Button          takeAllButton;

    [Header("Ссылки")]
    public Inventory playerInventory;

    public static ChestUI Instance { get; private set; }

    private Chest           _currentChest;
    private InventorySlot[] _slots;
    private bool            _initialized;

    public bool IsOpen => _currentChest != null && chestPanel != null && chestPanel.activeSelf;

    void Awake()
    {
        Instance = this;
        if (closeButton   != null) closeButton.onClick.AddListener(CloseChest);
        if (takeAllButton != null) takeAllButton.onClick.AddListener(TakeAll);
        if (chestPanel    != null) chestPanel.SetActive(false);
    }

    void InitSlots()
    {
        if (_initialized) return;
        _initialized = true;

        if (slotsParent == null) { Debug.LogError("ChestUI: slotsParent не подключён!"); return; }

        var list = new List<InventorySlot>();
        foreach (Transform child in slotsParent)
        {
            InventorySlot s = child.GetComponent<InventorySlot>();
            if (s != null)
            {
                // Добавляем Button на слот для обработки кликов
                Button btn = child.GetComponent<Button>();
                if (btn == null) btn = child.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;

                // Захватываем слот в замыкание
                InventorySlot captured = s;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSlotClick(captured));

                // Включаем Raycast Target
                Image img = child.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;

                s.Clear();
                list.Add(s);
            }
        }
        _slots = list.ToArray();
        Debug.Log($"ChestUI: {_slots.Length} слотов");
    }

    void OnSlotClick(InventorySlot slot)
    {
        Debug.Log($"OnSlotClick! currentItem={slot.GetCurrentItem()?.itemName ?? "null"}, chest={_currentChest?.name ?? "null"}");
        if (_currentChest == null) return;
        foreach (var data in _currentChest.items)
        {
            if (data.IsEmpty) continue;
            Debug.Log($"Сравниваем: slot={slot.GetCurrentItem()?.itemName} == data={data.item?.itemName}  → {slot.GetCurrentItem() == data.item}");
            if (slot.GetCurrentItem() == data.item)
            {
                TakeItem(data.item, data.count);
                return;
            }
        }
        Debug.Log("Предмет не найден в данных сундука!");
    }

    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseChest();
    }

    public void OpenChest(Chest chest)
    {
        _currentChest = chest;
        chestPanel.SetActive(true);
        InitSlots(); // вызываем после SetActive — Awake слотов уже сработал

        if (titleText != null) titleText.text = "СУНДУК";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        var skel = playerInventory?.GetComponent<skeleton>();
        if (skel != null) skel.SetInputLocked(true);

        Refresh();
    }

    public void CloseChest()
    {
        if (_currentChest == null) return;
        _currentChest.Close();
        _currentChest = null;
        chestPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        var skel = playerInventory?.GetComponent<skeleton>();
        if (skel != null) skel.SetInputLocked(false);
    }

    public void TakeAll()
    {
        if (_currentChest == null || playerInventory == null) return;
        foreach (var data in _currentChest.items)
        {
            if (data.IsEmpty) continue;
            if (playerInventory.AddItem(data.item, data.count))
            { data.item = null; data.count = 0; }
        }
        Refresh();
    }

    public void TakeItem(InventoryItem item, int count)
    {
        if (_currentChest == null || playerInventory == null) return;
        if (playerInventory.AddItem(item, count))
        {
            _currentChest.RemoveItem(item, count);
            Refresh();
            Debug.Log($"📦 Взято: {item.itemName} x{count}");
        }
        else Debug.Log("Инвентарь полон!");
    }

    void Refresh()
    {
        if (_slots == null || _currentChest == null) return;
        int max = Mathf.Min(_slots.Length, _currentChest.items.Count);
        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < max && !_currentChest.items[i].IsEmpty)
                _slots[i].SetItem(_currentChest.items[i].item, _currentChest.items[i].count);
            else
                _slots[i].Clear();
        }
    }
}