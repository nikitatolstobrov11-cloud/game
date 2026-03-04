using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab; // префаб с цифрами
    public Transform popupSpawnPoint; // откуда вылетают цифры (если пусто — из центра врага)

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"💔 Враг получил {damage} урона. Осталось {currentHealth} HP");
        ShowDamage(damage);
        if (currentHealth <= 0) Die();
    }

    void ShowDamage(int damage)
    {
    if (damagePopupPrefab == null)
    {
        Debug.LogError("❌ Нет префаба для цифр!");
        return;
    }

    // Находим Canvas
    Canvas canvas = FindObjectOfType<Canvas>();
    if (canvas == null)
    {
        Debug.LogError("❌ Canvas не найден на сцене!");
        return;
    }

    // Создаём экземпляр префаба как дочерний Canvas
    GameObject popup = Instantiate(damagePopupPrefab, canvas.transform);
    
    // Получаем позицию НАД врагом в МИРОВЫХ координатах
    Vector3 worldPos = transform.position + Vector3.up * 2f;
    
    // Конвертируем МИРОВЫЕ координаты в ЭКРАННЫЕ (пиксели)
    Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
    
    // Получаем RectTransform у созданного префаба
    RectTransform rectTransform = popup.GetComponent<RectTransform>();
    if (rectTransform != null)
    {
        // Устанавливаем позицию в ЭКРАННЫХ координатах
        rectTransform.position = screenPos;
    }
    else
    {
        Debug.LogError("❌ У префаба нет RectTransform!");
        return;
    }

    // Устанавливаем текст
    Text text = popup.GetComponentInChildren<Text>();
    if (text != null)
    {
        text.text = damage.ToString();
    }
    else
    {
        TMPro.TMP_Text tmp = popup.GetComponentInChildren<TMPro.TMP_Text>();
        if (tmp != null)
        {
            tmp.text = damage.ToString();
        }
        else
        {
            Debug.LogError("❌ В префабе нет компонента Text или TMP_Text!");
            return;
        }
    }

    // Уничтожаем через 1 секунду
    Destroy(popup, 1f);
    }

    void Die()
    {
        Debug.Log("💀 Враг умер!");
        Destroy(gameObject, 0.5f); // через 0.5 сек удаляем
    }
}