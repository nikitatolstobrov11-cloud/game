using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    public int health = 100;

    [Header("Damage Popup 3D")]
    public GameObject damagePopupPrefab;
    public Transform popupSpawnPoint;
    public float popupHeight = 1.2f;

    private Rigidbody rb;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("❌ На враге нет компонента Animator!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"💔 Враг получил {damage} урона. Осталось {health} HP");

        ShowDamage(damage);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Hit");
        }
    }

    void ShowDamage(int damage)
    {
        if (damagePopupPrefab == null) return;

        Vector3 spawnPos = popupSpawnPoint != null 
            ? popupSpawnPoint.position 
            : transform.position + Vector3.up * popupHeight;

        GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
        TextMeshPro tmp = popup.GetComponentInChildren<TextMeshPro>();
        if (tmp != null) tmp.text = damage.ToString();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 Враг умер!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
            // Можно отключить коллайдер, чтобы сквозь труп не проходили
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            // Rigidbody можно оставить или тоже отключить
            rb.isKinematic = true; // уже true
        }
        // Объект НЕ УДАЛЯЕМ!
    }
}