using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public int damage = 25;              // сколько урона наносит меч
    private bool canDealDamage = false;  // включён ли урон

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"⚡ Удар по {other.name} нанес {damage} урона");
            }
        }
    }

    public void EnableSwordDamage()
    {
        canDealDamage = true;
        Debug.Log("🟢 Урон включён");
    }

    public void DisableSwordDamage()
    {
        canDealDamage = false;
        Debug.Log("🔴 Урон выключен");
    }
}