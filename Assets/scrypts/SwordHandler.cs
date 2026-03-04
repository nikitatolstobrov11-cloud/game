using UnityEngine;

public class SwordHandler : MonoBehaviour
{
    public int damageAmount = 20;
    private GameObject sword;
    private Collider swordCollider;

    public void SetSword(GameObject newSword)
    {
        sword = newSword;
        swordCollider = sword.GetComponent<Collider>();

        if (swordCollider == null)
        {
            Debug.LogError("❌ На мече нет коллайдера!");
            return;
        }

        swordCollider.enabled = false;
        Debug.Log("✅ Меч привязан к SwordHandler: " + sword.name);
    }

    public void EnableSwordCollider()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
            Debug.Log("⚔️ Коллайдер ВКЛЮЧЕН");
        }
    }

    public void DisableSwordCollider()
    {
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            Debug.Log("⚔️ Коллайдер ВЫКЛЮЧЕН");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("🔍 Триггер сработал с: " + other.name + ", тег: " + other.tag);

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"💥 Урон {damageAmount} НАНЕСЁН!");
            }
            else
            {
                Debug.LogError("❌ На объекте Enemy нет компонента Enemy!");
            }
        }
    }
}