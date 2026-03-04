using UnityEngine;

public class SwordHit : MonoBehaviour
{
    private int damageAmount = 20; // можно сделать публичным или брать из SwordHandler

    void Start()
    {
        // Если хочешь брать урон из SwordHandler:
        // var handler = FindObjectOfType<SwordHandler>();
        // if (handler != null) damageAmount = handler.damageAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"⚔️ Попадание по врагу: {damageAmount} урона");
            }
        }
    }
}