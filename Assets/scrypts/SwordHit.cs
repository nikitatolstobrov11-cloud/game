using UnityEngine;

public class SwordHit : MonoBehaviour
{
    public int damageAmount = 20;

    [Header("Звуки")]
    public AudioClip swooshSound;   // взмах (из анимации)
    public AudioClip hitSound;      // удар (при попадании)

    private AudioSource swooshSource;
    private AudioSource hitSource;

    void Start()
    {
        // Создаём два независимых источника звука
        swooshSource = gameObject.AddComponent<AudioSource>();
        hitSource = gameObject.AddComponent<AudioSource>();

        // Настройки для 3D-звука
        foreach (var src in new[] { swooshSource, hitSource })
        {
            src.playOnAwake = false;
            src.spatialBlend = 1f;    // 3D
            src.volume = 1f;
        }
    }

    // Вызывается из анимации (через SwordHandler)
    public void PlaySwoosh()
    {
        if (swooshSound != null)
        {
            swooshSource.PlayOneShot(swooshSound);
            Debug.Log("💨 Взмах");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"⚔️ Удар по врагу: {damageAmount}");

                // Просто играем звук удара, не трогая взмах
                if (hitSound != null)
                {
                    hitSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}