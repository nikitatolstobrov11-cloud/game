using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    // Перезапуск текущей сцены
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Выход в главное меню (если есть)
    public void ExitToMenu()
    {
        // Замени "MainMenu" на имя твоей сцены меню
        SceneManager.LoadScene("MainMenu");
    }

    // Выход из игры
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}