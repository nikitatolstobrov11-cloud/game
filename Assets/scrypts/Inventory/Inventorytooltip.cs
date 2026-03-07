using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryTooltip : MonoBehaviour
{
    [Header("UI References")]
    public GameObject      tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descText;
    public Image           rarityBar;

    [Header("← Перетащи сюда именно тот Canvas на котором висит инвентарь")]
    public Canvas targetCanvas;   // назначь вручную в Inspector!

    [Header("Rarity Colors")]
    public Color colorCommon    = new Color(0.78f, 0.78f, 0.78f);
    public Color colorUncommon  = new Color(0.30f, 0.73f, 0.37f);
    public Color colorRare      = new Color(0.30f, 0.62f, 1.00f);
    public Color colorEpic      = new Color(0.70f, 0.40f, 1.00f);
    public Color colorLegendary = new Color(1.00f, 0.60f, 0.13f);

    private RectTransform _rect;

    void Awake()
    {
        _rect = tooltipPanel.GetComponent<RectTransform>();

        // Якоря фиксируем один раз
        _rect.anchorMin = Vector2.zero;
        _rect.anchorMax = Vector2.zero;
        _rect.pivot     = new Vector2(0f, 1f);

        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (!tooltipPanel.activeSelf) return;
        FollowMouse();
    }

    public void Show(InventoryItem item, int count)
    {
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling();

        nameText.text = item.itemName;

        string rarityName = item.rarity switch
        {
            ItemRarity.Common    => "Обычный",
            ItemRarity.Uncommon  => "Необычный",
            ItemRarity.Rare      => "Редкий",
            ItemRarity.Epic      => "Эпический",
            ItemRarity.Legendary => "Легендарный",
            _                    => ""
        };
        Color rarityColor = GetRarityColor(item.rarity);
        rarityText.text   = rarityName;
        rarityText.color  = rarityColor;
        if (rarityBar != null) rarityBar.color = rarityColor;

        var sb = new System.Text.StringBuilder();
        if (item.attackBonus  > 0) sb.AppendLine($"⚔  Урон:      +{item.attackBonus}");
        if (item.defenseBonus > 0) sb.AppendLine($"🛡  Защита:   +{item.defenseBonus}");
        if (item.healthBonus  > 0) sb.AppendLine($"❤  Здоровье: +{item.healthBonus}");
        if (count > 1)             sb.AppendLine($"📦  Кол-во:    {count}");
        statsText.text = sb.ToString().TrimEnd();
        descText.text  = item.description;

        FollowMouse();
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }

    void FollowMouse()
    {
        if (targetCanvas == null)
        {
            Debug.LogError("InventoryTooltip: targetCanvas не назначен в Inspector!");
            return;
        }

        // Конвертируем позицию мыши в координаты нужного Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
            out Vector2 localPoint
        );

        float w = _rect.rect.width;
        float h = _rect.rect.height;
        Vector2 canvasSize = targetCanvas.GetComponent<RectTransform>().rect.size;

        localPoint.x += 14f;
        localPoint.y -= 14f;

        // Не вылезаем за правый край
        if (localPoint.x + w > canvasSize.x * 0.5f)
            localPoint.x -= w + 28f;

        // Не вылезаем за нижний край
        if (localPoint.y - h < -canvasSize.y * 0.5f)
            localPoint.y += h + 14f;

        _rect.anchoredPosition = localPoint;
    }

    Color GetRarityColor(ItemRarity r) => r switch
    {
        ItemRarity.Common    => colorCommon,
        ItemRarity.Uncommon  => colorUncommon,
        ItemRarity.Rare      => colorRare,
        ItemRarity.Epic      => colorEpic,
        ItemRarity.Legendary => colorLegendary,
        _                    => Color.white
    };
}