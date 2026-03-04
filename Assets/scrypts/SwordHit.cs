using UnityEngine;

public class SwordHit : MonoBehaviour
{
    public int damageAmount = 20;

    [Header("Звуки")]
    public AudioClip swooshSound;   // взмах (из анимации)
    public AudioClip hitSound;      // удар (при попадании)

    private AudioSource swooshSource;
    private AudioSource hitSource;
    private bool hasHit; // флаг, чтобы не дублировать удар

    void Start()
    {
        // Создаём два независимых источника
        swooshSource = gameObject.AddComponent<AudioSource>();
        hitSource = gameObject.AddComponent<AudioSource>();

        // Настройки
        foreach (var src in new[] { swooshSource, hitSource })
        {
            src.playOnAwake = false;
            src.spatialBlend = 1f;      // 3D
            src.volume = 1f;
        }
    }

    // Вызывается из анимации (каждый удар)
    public void PlaySwoosh()
    {
        // Сбрасываем флаг удара для нового взмаха
        hasHit = false;

        if (swooshSound != null)
        {
            swooshSource.Stop(); // на всякий случай обрываем предыдущий
            swooshSource.PlayOneShot(swooshSound);
            Debug.Log("💨 Взмах");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если уже ударили в этом взмахе — игнорируем (защита от двойного попадания)
        if (hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            hasHit = true; // помечаем, что удар уже был

            // Останавливаем взмах (чтобы не накладывался)
            swooshSource.Stop();

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"⚔️ Удар по врагу: {damageAmount}");

                if (hitSound != null)
                {
                    hitSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}