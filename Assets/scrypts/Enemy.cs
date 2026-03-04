using UnityEngine;
using TMPro;  // для TextMeshPro

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public float destroyDelay = 1f;

    [Header("Damage Popup 3D")]
    public GameObject damagePopupPrefab;  // префаб 3D-текста
    public Transform popupSpawnPoint;      // точка появления (необязательно)

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"💔 Враг получил {damage} урона. Осталось {health} HP");

        ShowDamage(damage);

        if (health <= 0)
            Die();
    }

    void ShowDamage(int damage)
{
    if (damagePopupPrefab == null) return;
    Vector3 spawnPos = popupSpawnPoint != null ? popupSpawnPoint.position : transform.position + Vector3.up * 0.1f;
    GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
    TextMeshPro tmp = popup.GetComponentInChildren<TextMeshPro>();
    if (tmp != null) tmp.text = damage.ToString();
}

    void Die()
    {
        Debug.Log("💀 Враг умер!");
        Destroy(gameObject, destroyDelay);
    }
}