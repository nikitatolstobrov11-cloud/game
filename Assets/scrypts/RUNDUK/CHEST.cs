using UnityEngine;
using System.Collections.Generic;

// Повесь на объект сундука
// Tag = "Chest", Collider (Is Trigger = false), Layer = interactableLayer
public class Chest : MonoBehaviour
{
    [Header("Содержимое сундука")]
    public List<ChestSlotData> items = new List<ChestSlotData>();
    public int slotCount = 20;

    [Header("Анимация (опционально)")]
    public Animator animator;
    public string openTrigger  = "Open";
    public string closeTrigger = "Close";

    public bool IsOpen { get; private set; }

    void Start()
    {
        while (items.Count < slotCount)
            items.Add(new ChestSlotData());

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        if (animator != null) animator.SetTrigger(openTrigger);
        ChestUI.Instance?.OpenChest(this);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        if (animator != null) animator.SetTrigger(closeTrigger);
    }

    public bool AddItem(InventoryItem item, int amount = 1)
    {
        foreach (var slot in items)
        {
            if (!slot.IsEmpty && slot.item == item && slot.count < item.maxStack)
            {
                int add = Mathf.Min(amount, item.maxStack - slot.count);
                slot.count += add;
                amount -= add;
                if (amount <= 0) return true;
            }
        }
        foreach (var slot in items)
        {
            if (slot.IsEmpty) { slot.item = item; slot.count = amount; return true; }
        }
        return false;
    }

    public bool RemoveItem(InventoryItem item, int amount = 1)
    {
        foreach (var slot in items)
        {
            if (!slot.IsEmpty && slot.item == item)
            {
                slot.count -= amount;
                if (slot.count <= 0) { slot.item = null; slot.count = 0; }
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class ChestSlotData
{
    public InventoryItem item;
    public int count;
    public bool IsEmpty => item == null;
}