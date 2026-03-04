using UnityEngine;

public class SwordHandler : MonoBehaviour
{
    private GameObject swordObject;      // ссылка на меч в руке
    private Collider swordCollider;       // коллайдер меча

    // Вызывается из Interaction при подборе меча
    public void SetSword(GameObject sword)
    {
        swordObject = sword;
        swordCollider = sword.GetComponent<Collider>();

        if (swordCollider == null)
        {
            Debug.LogError("❌ SwordHandler: на мече нет коллайдера!");
            return;
        }

        // Убеждаемся, что коллайдер помечен как триггер (иначе физика будет мешать)
        if (!swordCollider.isTrigger)
        {
            Debug.LogWarning("⚠️ SwordHandler: коллайдер меча не Trigger. Включаем IsTrigger.");
            swordCollider.isTrigger = true;
        }

        // Изначально коллайдер выключен — урон только в момент анимации
        swordCollider.enabled = false;
        Debug.Log("✅ SwordHandler: меч привязан, коллайдер готов.");
    }

    // Вызывается из анимации удара (EnableSwordDamage)
    public void EnableSwordDamage()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            Debug.Log("⚔️ Коллайдер меча ВКЛЮЧЕН");
        }
    }

    // Вызывается из анимации удара (DisableSwordDamage)
    public void DisableSwordDamage()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            Debug.Log("⚔️ Коллайдер меча ВЫКЛЮЧЕН");
        }
    }
}