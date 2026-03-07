using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public InventoryItem item;
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inv = other.GetComponent<Inventory>();
            if (inv != null && inv.AddItem(item, amount))
            {
                Destroy(gameObject);
            }
        }
    }
}