using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Добавь этот компонент на префаб слота
public class InventorySlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private float _lastClickTime;
    private const float DoubleClickInterval = 0.3f;



    [Header("UI References")]
    public Image        borderImage;   // внешняя рамка слота (Image на корневом объекте)
    public Image        iconImage;     // иконка предмета
    public TextMeshProUGUI countText;  // количество (TMP)

    [Header("Rarity Colors")]
    public Color colorCommon    = new Color(0.78f, 0.78f, 0.78f);
    public Color colorUncommon  = new Color(0.30f, 0.73f, 0.37f);
    public Color colorRare      = new Color(0.30f, 0.62f, 1.00f);
    public Color colorEpic      = new Color(0.70f, 0.40f, 1.00f);
    public Color colorLegendary = new Color(1.00f, 0.60f, 0.13f);
    public Color colorEmpty     = new Color(0.25f, 0.18f, 0.08f, 0.8f);

    // ───── внутренние данные ─────
    private InventoryItem currentItem;
    private int           currentCount;

    // Режим сундука — одиночный клик забирает предмет
    public bool isChestSlot = false;

    // ссылка на тултип-менеджер (найдётся автоматически)
    private static InventoryTooltip _tooltip;

    void Awake()
    {
        if (_tooltip == null)
            _tooltip = FindObjectOfType<InventoryTooltip>();

        // Убеждаемся что на корневом объекте есть Image с Raycast Target
        // Без этого IPointerClickHandler не получает события
        Image rootImage = GetComponent<Image>();
        if (rootImage == null)
        {
            rootImage = gameObject.AddComponent<Image>();
            rootImage.color = new Color(0, 0, 0, 0); // прозрачный
        }
        rootImage.raycastTarget = true;
    }



    // ───── публичный API (вызывается из Inventory.cs) ─────

    public void SetItem(InventoryItem item, int count)
    {
        currentItem  = item;
        currentCount = count;

        if (item != null)
        {
            iconImage.sprite  = item.icon;
            iconImage.enabled = true;
            countText.text    = count > 1 ? count.ToString() : "";
            SetBorderColor(item.rarity);
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        currentItem      = null;
        currentCount     = 0;
        iconImage.sprite  = null;
        iconImage.enabled = false;
        countText.text    = "";
        if (borderImage != null)
            borderImage.color = colorEmpty;
    }

    // ───── клики ─────

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // ── Слот сундука — одиночный клик забирает предмет в инвентарь ──
        if (isChestSlot)
        {
            ChestUI.Instance?.TakeItem(currentItem, currentCount);
            if (_tooltip != null) _tooltip.Hide();
            return;
        }

        // ── Слот инвентаря — двойной клик надевает/использует ──
        float timeSinceLastClick = Time.unscaledTime - _lastClickTime;
        _lastClickTime = Time.unscaledTime;

        if (timeSinceLastClick <= DoubleClickInterval)
        {
            if (currentItem.itemType == ItemType.Armor)
            {
                ArmorManager armor = ArmorManager.Instance;
                if (armor != null)
                {
                    armor.EquipArmor(currentItem);
                    if (_tooltip != null) _tooltip.Hide();
                }
            }
        }
    }

    // ───── тултип ─────

    public void OnPointerEnter(PointerEventData _)
    {
        if (currentItem != null && _tooltip != null)
            _tooltip.Show(currentItem, currentCount);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_tooltip != null)
            _tooltip.Hide();
    }

    // ───── вспомогательное ─────

    void SetBorderColor(ItemRarity rarity)
    {
        if (borderImage == null) return;
        borderImage.color = rarity switch
        {
            ItemRarity.Common    => colorCommon,
            ItemRarity.Uncommon  => colorUncommon,
            ItemRarity.Rare      => colorRare,
            ItemRarity.Epic      => colorEpic,
            ItemRarity.Legendary => colorLegendary,
            _                    => colorEmpty
        };
    }

    // Используется фильтрацией вкладок
    public ItemType GetItemType() =>
        currentItem != null ? currentItem.itemType : ItemType.All;

    // Для ChestUI — получить текущий предмет
    public InventoryItem GetCurrentItem() => currentItem;
}