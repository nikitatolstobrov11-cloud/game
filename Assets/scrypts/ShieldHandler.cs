using UnityEngine;

public class ShieldHandler : MonoBehaviour
{
    private GameObject shieldObject;
    private Collider shieldCollider;

    public void SetShield(GameObject shield)
    {
        shieldObject = shield;
        shieldCollider = shield.GetComponent<Collider>();

        if (shieldCollider == null)
        {
            Debug.LogError("❌ ShieldHandler: на щите нет коллайдера!");
            return;
        }

        if (!shieldCollider.isTrigger)
        {
            Debug.LogWarning("⚠️ ShieldHandler: коллайдер щита не Trigger. Включаем IsTrigger.");
            shieldCollider.isTrigger = true;
        }

        shieldCollider.enabled = false;
        Debug.Log("✅ ShieldHandler: щит привязан.");
    }
}