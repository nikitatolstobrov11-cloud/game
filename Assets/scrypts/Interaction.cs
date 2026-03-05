using UnityEngine;

public class Interaction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayDistance = 20f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Sword Settings")]
    public Transform swordHolder;
    private GameObject currentSword;

    [Header("Shield Settings")]
    public Transform shieldHolder;
    private GameObject currentShield;

    [Header("Sounds")]
    public AudioClip pickUpSound; // звук подбора

    private AudioSource audioSource;

    void Start()
    {
        // Добавляем AudioSource, если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer) && Input.GetKeyDown(interactKey))
        {
            if (hit.collider.CompareTag("Sword"))
            {
                PickUpSword(hit.collider.gameObject);
            }
            else if (hit.collider.CompareTag("Shield"))
            {
                PickUpShield(hit.collider.gameObject);
            }
            else
            {
                Debug.Log("Взаимодействие с: " + hit.collider.name);
            }
        }
    }

    void PlayPickUpSound()
    {
        if (pickUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickUpSound);
        }
    }

    void PickUpSword(GameObject sword)
    {
        skeleton skel = FindObjectOfType<skeleton>();
        if (skel != null) skel.StartPickUp();

        PlayPickUpSound(); // звук подбора

        if (currentSword != null) Destroy(currentSword);
        AttachItem(sword, swordHolder);
        currentSword = sword;

        SwordHandler handler = FindObjectOfType<SwordHandler>();
        if (handler != null) handler.SetSword(sword);

        Debug.Log("Меч поднят!");
    }

    void PickUpShield(GameObject shield)
    {
    skeleton skel = FindObjectOfType<skeleton>();

    // Если уже есть щит – сначала убираем флаг у старого
    if (currentShield != null)
    {
        if (skel != null) skel.hasShield = false;
        Destroy(currentShield);
    }

    // Запускаем анимацию подбора (если нужно)
    if (skel != null) skel.StartPickUp();

    AttachItem(shield, shieldHolder);
    currentShield = shield;

    // Устанавливаем флаг, что щит теперь есть
    if (skel != null) skel.hasShield = true;

    ShieldHandler handler = FindObjectOfType<ShieldHandler>();
    if (handler != null) handler.SetShield(shield);

    Debug.Log("Щит поднят!");
    }

    void AttachItem(GameObject item, Transform holder)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        item.transform.SetParent(holder);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }
}