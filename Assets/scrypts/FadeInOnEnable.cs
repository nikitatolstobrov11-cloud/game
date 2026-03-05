using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInOnEnable : MonoBehaviour
{
    public Image fadeImage;        // ссылка на чёрное изображение
    public float fadeDuration = 1f; // длительность затемнения

    private void OnEnable()
    {
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;
    }
}