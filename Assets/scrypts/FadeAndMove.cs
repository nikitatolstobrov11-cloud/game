using UnityEngine;
using TMPro;   // <--- Этой строки не хватало!

public class FadeAndMove : MonoBehaviour
{
    public float speed = 1f;
    public float duration = 1f;

    private TextMeshPro tmp;
    private float timer;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
        timer = duration;
    }

    void Update()
    {
        // Поднимаем вверх
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Уменьшаем прозрачность
        timer -= Time.deltaTime;
        float alpha = timer / duration;
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);

        // Уничтожаем по истечении времени
        if (timer <= 0) Destroy(gameObject);
    }
}