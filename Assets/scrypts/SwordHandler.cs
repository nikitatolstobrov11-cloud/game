using UnityEngine;

public class SwordHandler : MonoBehaviour
{
    [Header("Sword Settings")]
    public GameObject swordObject;        // Меч в руке
    public int damageAmount = 20;

    private Collider swordCollider;
    private bool isDamaging = false;

    void Start()
    {
        if (swordObject != null)
            InitializeSword();
    }

    public void SetSword(GameObject newSword)
    {
        swordObject = newSword;
        InitializeSword();
        Debug.Log("✅ SwordHandler: меч привязан через SetSword: " + swordObject.name);
    }

    void InitializeSword()
    {
        if (swordObject == null) return;

        swordCollider = swordObject.GetComponent<Collider>();
        if (swordCollider == null)
        {
            Debug.LogError("❌ SwordHandler: на мече нет коллайдера!");
            return;
        }

        if (!swordCollider.isTrigger)
        {
            Debug.LogWarning("⚠️ SwordHandler: коллайдер меча не Trigger. Включаем IsTrigger автоматически.");
            swordCollider.isTrigger = true;
        }

        swordCollider.enabled = false;
    }

    public void EnableSwordDamage()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            isDamaging = true;
            Debug.Log("⚔️ Урон ВКЛЮЧЕН (коллайдер активирован)");
        }
    }

    public void DisableSwordDamage()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            isDamaging = false;
            Debug.Log("⚔️ Урон ВЫКЛЮЧЕН (коллайдер деактивирован)");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Игнорируем, если урон сейчас не активен
        if (!isDamaging) return;

        // Игнорируем самого себя (меч не должен реагировать на свой коллайдер)
        if (other.gameObject == swordObject) return;

        // Игнорируем объекты с тегом Sword (на всякий случай, если где-то ещё есть такой тег)
        if (other.CompareTag("Sword")) return;

        Debug.Log($"🔍 SwordHandler: триггер с {other.name}, тег: {other.tag}");

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"💥 Урон {damageAmount} нанесён врагу {other.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ SwordHandler: у объекта {other.name} тег Enemy, но нет компонента Enemy!");
            }
        }
    }
}