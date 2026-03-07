using UnityEngine;
using TMPro;

public class Interaction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayDistance = 20f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("References")]
    public GameObject player;

    [Header("Sword Settings")]
    public Transform swordHolder;
    private GameObject currentSword;

    [Header("Shield Settings")]
    public Transform shieldHolder;
    private GameObject currentShield;

    [Header("Подсказка [E]")]
    public GameObject interactPrompt;        // ← перетащи сюда объект с текстом "[E] Взять"
    public TextMeshProUGUI promptText;        // ← TMP текст внутри него (опционально — для смены надписи)

    [Header("Sounds")]
    public AudioClip pickUpSound;

    private AudioSource audioSource;
    private Inventory inventory;
    private skeleton playerSkeleton;

    // Текущий объект под прицелом (обновляется каждый кадр)
    private GameObject currentTarget;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (player != null)
        {
            inventory      = player.GetComponent<Inventory>();
            playerSkeleton = player.GetComponent<skeleton>();
        }
        else
        {
            Debug.LogError("Player reference not set in Interaction!");
        }

        // Прячем подсказку при старте
        SetPrompt(false);
    }

    void Update()
    {
        // ── 1. Raycast каждый кадр — определяем что под прицелом ──
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.yellow);

        bool hitSomething = Physics.Raycast(ray, out hit, rayDistance, interactableLayer);

        if (hitSomething)
        {
            // Показываем подсказку и обновляем надпись под тип предмета
            currentTarget = hit.collider.gameObject;
            SetPrompt(true, GetPromptLabel(hit.collider));
        }
        else
        {
            // Ничего под прицелом — прячем подсказку
            currentTarget = null;
            SetPrompt(false);
        }

        // ── 2. Нажатие E — обрабатываем отдельно ──
        // ИСПРАВЛЕНО: раньше raycast && GetKeyDown стояли вместе,
        // из-за чего подбор срабатывал только если луч попал
        // ровно в тот же кадр что и нажатие — очень ненадёжно.
        if (Input.GetKeyDown(interactKey) && currentTarget != null)
        {
            TryInteract(currentTarget);
        }
    }

    // ── Определяем что делать с объектом ──
    void TryInteract(GameObject target)
    {
        Collider col = target.GetComponent<Collider>();
        if (col == null) return;

        if (col.CompareTag("Sword"))
            PickUpSword(target);
        else if (col.CompareTag("Shield"))
            PickUpShield(target);
        else if (col.CompareTag("Item"))
            PickUpInventoryItem(target);
        else if (col.CompareTag("Soul"))
            CollectSoul(target);
        else if (col.CompareTag("Chest"))
            OpenChest(target);
        else
            Debug.Log("Взаимодействие с: " + target.name);
    }

    // ── Текст подсказки зависит от типа предмета ──
    string GetPromptLabel(Collider col)
    {
        if (col.CompareTag("Sword"))  return "[E]  Поднять меч";
        if (col.CompareTag("Shield")) return "[E]  Поднять щит";
        if (col.CompareTag("Item"))   return "[E]  Подобрать";
        if (col.CompareTag("Soul"))   return "[E]  Забрать души";
        if (col.CompareTag("Chest"))  return "[E]  Открыть сундук";
        return "[E]  Взаимодействовать";
    }

    // ── Показать / скрыть подсказку ──
    void SetPrompt(bool visible, string label = "")
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(visible);

        if (promptText != null && visible)
            promptText.text = label;
    }

    // ──────────────────────────────────────────
    //  Подбор предметов (логика не изменилась)
    // ──────────────────────────────────────────

    void PlayPickUpSound()
    {
        if (pickUpSound != null && audioSource != null)
            audioSource.PlayOneShot(pickUpSound);
    }

    void PickUpSword(GameObject sword)
    {
        if (playerSkeleton == null) { Debug.LogError("playerSkeleton не найден!"); return; }

        playerSkeleton.StartPickUp();
        PlayPickUpSound();

        if (currentSword != null) Destroy(currentSword);
        AttachItem(sword, swordHolder);
        currentSword = sword;

        SwordHandler handler = FindObjectOfType<SwordHandler>();
        if (handler != null) handler.SetSword(sword);

        currentTarget = null;
        SetPrompt(false);
        Debug.Log("Меч поднят!");
    }

    void PickUpShield(GameObject shield)
    {
        if (playerSkeleton == null) { Debug.LogError("playerSkeleton не найден!"); return; }

        playerSkeleton.StartPickUp();
        PlayPickUpSound();

        if (currentShield != null)
        {
            playerSkeleton.hasShield = false;
            Destroy(currentShield);
        }

        AttachItem(shield, shieldHolder);
        currentShield = shield;
        playerSkeleton.hasShield = true;

        ShieldHandler handler = FindObjectOfType<ShieldHandler>();
        if (handler != null) handler.SetShield(shield);

        currentTarget = null;
        SetPrompt(false);
        Debug.Log("Щит поднят!");
    }

    void PickUpInventoryItem(GameObject itemObj)
    {
        PickupItem pickup = itemObj.GetComponent<PickupItem>();
        if (pickup == null)
        {
            Debug.LogWarning("На объекте нет компонента PickupItem");
            return;
        }
        if (inventory == null)
        {
            Debug.LogWarning("У игрока нет компонента Inventory");
            return;
        }

        if (inventory.AddItem(pickup.item, pickup.amount))
        {
            PlayPickUpSound();
            if (playerSkeleton != null) playerSkeleton.StartPickUp();

            currentTarget = null;
            SetPrompt(false);
            Destroy(itemObj);
            Debug.Log($"Предмет {pickup.item.itemName} поднят");
        }
        else
        {
            Debug.Log("Инвентарь полон!");
        }
    }

    void OpenChest(GameObject chestObj)
    {
        Chest chest = chestObj.GetComponent<Chest>();
        if (chest == null)
        {
            Debug.LogWarning("На объекте нет компонента Chest!");
            return;
        }
        if (chest.IsOpen)
        {
            ChestUI.Instance?.CloseChest();
            return;
        }
        chest.Open();
        currentTarget = null;
        SetPrompt(false);
    }

    void CollectSoul(GameObject soulObj)
    {
        SoulPuddle puddle = soulObj.GetComponent<SoulPuddle>();
        if (puddle == null)
        {
            Debug.LogWarning("На объекте нет компонента SoulPuddle");
            return;
        }
        puddle.Collect();
        currentTarget = null;
        SetPrompt(false);
    }

    void AttachItem(GameObject item, Transform holder)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        item.transform.SetParent(holder);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }
}