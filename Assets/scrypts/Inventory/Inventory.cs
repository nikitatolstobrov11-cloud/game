using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class InventorySlotData
{
    public InventoryItem item;
    public int count;

    public bool IsEmpty => item == null;
}

public class Inventory : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject slotPrefab;
    public int slotCount = 20;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.I;

    private List<InventorySlotData> items = new List<InventorySlotData>();
    private List<InventorySlot> slotUIs = new List<InventorySlot>();
    private bool isOpen = false;
    public bool IsOpen => isOpen;

    void Start()
    {
        // Инициализация данных
        for (int i = 0; i < slotCount; i++)
        {
            items.Add(new InventorySlotData());
        }

        // Создание UI слотов
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slotUIs.Add(slot);
            slot.SetItem(null, 0); // очищаем
        }
        inventoryPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = isOpen;

        skeleton playerSkel = GetComponent<skeleton>();
        if (playerSkel != null)
            playerSkel.SetInputLocked(isOpen);

        if (isOpen) RequestUIRefresh();
    }

    public bool AddItem(InventoryItem item, int amount = 1)
    {
        // Попытка сложить в существующий стек
        foreach (var slotData in items)
        {
            if (!slotData.IsEmpty && slotData.item == item && slotData.count < item.maxStack)
            {
                int space = item.maxStack - slotData.count;
                int add = Mathf.Min(amount, space);
                slotData.count += add;
                amount -= add;
                if (amount <= 0)
                {
                    UpdateUI();
                    return true;
                }
            }
        }

        // Поиск пустого слота
        foreach (var slotData in items)
        {
            if (slotData.IsEmpty)
            {
                slotData.item = item;
                slotData.count = amount;
                UpdateUI();
                return true;
            }
        }

        Debug.Log("Инвентарь полон!");
        return false;
    }

    // ── Удалить предмет (нужен для ArmorManager) ──
    public bool RemoveItem(InventoryItem item, int amount = 1)
    {
        foreach (var slotData in items)
        {
            if (!slotData.IsEmpty && slotData.item == item)
            {
                slotData.count -= amount;
                if (slotData.count <= 0)
                {
                    slotData.item  = null;
                    slotData.count = 0;
                }
                UpdateUI();
                RequestUIRefresh();
                return true;
            }
        }
        return false;
    }

    public void RequestUIRefresh()
    {
        UpdateUI();
    }

    public void ForceOpen()
    {
        if (isOpen) return;
        isOpen = true;
        inventoryPanel.SetActive(true);
        RequestUIRefresh();
    }

    public void ForceClose()
    {
        if (!isOpen) return;
        isOpen = false;
        inventoryPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void UpdateUI()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (items[i].IsEmpty)
                slotUIs[i].Clear();
            else
                slotUIs[i].SetItem(items[i].item, items[i].count);
        }
    }
}