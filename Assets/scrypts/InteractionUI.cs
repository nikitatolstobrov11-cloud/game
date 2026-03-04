using UnityEngine;
using UnityEngine.UI; // если используешь обычный Text (Legacy)
// using TMPro; // если используешь TextMeshPro — раскомментируй и замени Text на TMP_Text

public class InteractionUI : MonoBehaviour
{
    public float rayDistance = 20f;
    public LayerMask interactableLayer;
    public GameObject promptText; // перетащи сюда текст из Canvas

    private RaycastHit hit;

    void Update()
    {
        // Луч из центра экрана
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            // Навели на предмет — показываем текст
            promptText.SetActive(true);
        }
        else
        {
            // Ничего не навели — скрываем
            promptText.SetActive(false);
        }
    }
}