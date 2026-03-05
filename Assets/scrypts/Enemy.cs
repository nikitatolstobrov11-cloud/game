using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    public int health = 100;

    [Header("Damage Popup 3D")]
    public GameObject damagePopupPrefab;
    public Transform popupSpawnPoint;
    public float popupHeight = 1.2f;

    [Header("Sounds")]
    public AudioClip deathSound;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public float healthBarHeight = 2.5f;
    public float maxShowDistance = 15f;
    public Transform playerRef;  // будет установлено из EnemyAI

    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDead = false;
    private GameObject healthBarInstance;
    private Slider healthSlider;

    public bool IsDead => isDead;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D-звук

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * healthBarHeight, Quaternion.identity);
            healthBarInstance.transform.SetParent(transform);
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = health;
                healthSlider.value = health;
            }
            healthBarInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (healthBarInstance != null && playerRef != null)
        {
            float dist = Vector3.Distance(transform.position, playerRef.position);
            healthBarInstance.SetActive(dist <= maxShowDistance);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"💔 Враг получил {damage} урона. Осталось {health} HP");

        if (healthSlider != null)
            healthSlider.value = health;

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

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        if (animator != null)
            animator.SetTrigger("Die");

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}