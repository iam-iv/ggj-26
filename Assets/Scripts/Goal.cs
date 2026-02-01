using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Tooltip("Tag del objeto jugador (por defecto: Player)")]
    public string playerTag = "Player";

    [Tooltip("Nombre de la escena del menú principal a cargar")]
    public string mainMenuSceneName = "MainMenu";

    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        HandleEnter(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleEnter(other.gameObject);
    }

    void HandleEnter(GameObject other)
    {
        if (triggered) return;
        if (other.CompareTag(playerTag))
        {
            triggered = true;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // Método público opcional para botones
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
