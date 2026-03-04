using UnityEngine;

public class Interaction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayDistance = 20f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Sword Settings")]
    public Transform swordHolder;          // точка в правой руке
    private GameObject currentSword;

    [Header("Shield Settings")]
    public Transform shieldHolder;         // точка в левой руке
    private GameObject currentShield;

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

    void PickUpSword(GameObject sword)
    {
        // Анимация подбора
        skeleton skel = FindObjectOfType<skeleton>();
        if (skel != null) skel.StartPickUp();

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
        if (skel != null) skel.StartPickUp();

        if (currentShield != null) Destroy(currentShield);
        AttachItem(shield, shieldHolder);
        currentShield = shield;

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