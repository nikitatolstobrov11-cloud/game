using UnityEngine;
using TMPro;

// Повесь на игрока (тот же объект что и skeleton.cs)
public class SoulManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI soulsText;     // HUD — всегда виден
    public TextMeshProUGUI soulsTextInv;  // текст в инвентаре (GoldBar)

    [Header("Soul Puddle")]
    public GameObject soulPuddlePrefab; // префаб "лужи душ" на земле

    [Header("Настройки")]
    public int currentSouls = 0;

    // Позиция где игрок умер — туда спавним лужу
    private Vector3 deathPosition;
    private int     lostSouls = 0;
    private GameObject activePuddle; // только одна лужа одновременно

    // Singleton для удобного доступа из Enemy
    public static SoulManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();

        // Подписываемся на смерть игрока
        skeleton playerSkel = GetComponent<skeleton>();
        if (playerSkel == null)
            Debug.LogError("SoulManager: skeleton не найден на этом объекте!");
    }

    // ── Добавить души (вызывается из Enemy при смерти) ──
    public void AddSouls(int amount)
    {
        currentSouls += amount;
        UpdateUI();
        Debug.Log($"✨ +{amount} душ. Всего: {currentSouls}");
    }

    // ── Вызывается из skeleton.Die() ──
    public void OnPlayerDeath(Vector3 position)
    {
        if (currentSouls <= 0) return;

        lostSouls    = currentSouls;
        deathPosition = position;
        currentSouls  = 0;
        UpdateUI();

        // Уничтожаем старую лужу если была
        if (activePuddle != null)
            Destroy(activePuddle);

        // Спавним новую лужу в точке смерти
        if (soulPuddlePrefab != null)
        {
            activePuddle = Instantiate(soulPuddlePrefab, deathPosition, Quaternion.identity);
            SoulPuddle puddle = activePuddle.GetComponent<SoulPuddle>();
            if (puddle != null)
                puddle.Initialize(lostSouls, this);
        }

        Debug.Log($"💀 Игрок умер. Потеряно {lostSouls} душ.");
    }

    // ── Вызывается из SoulPuddle при подборе ──
    public void RecoverSouls(int amount)
    {
        currentSouls += amount;
        lostSouls     = 0;
        activePuddle  = null;
        UpdateUI();
        Debug.Log($"✨ Восстановлено {amount} душ! Всего: {currentSouls}");
    }

    void UpdateUI()
    {
    string display = $"✨ {currentSouls:N0}";
    if (soulsText    != null) soulsText.text    = display;
    if (soulsTextInv != null) soulsTextInv.text = display;
    }
    public void ForceUpdateUI()
    {
    UpdateUI();
    }
}