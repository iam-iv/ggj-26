using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Tooltip("Panel de créditos que se mostrará cuando el jugador toque la meta.")]
    public GameObject creditsPanel;

    [Tooltip("Tag del objeto jugador (por defecto: Player)")]
    public string playerTag = "Player";

    [Tooltip("Nombre de la escena del menú principal a cargar al cerrar los créditos")]
    public string mainMenuSceneName = "MainMenu";

    bool triggered = false;

    void Start()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

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
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }

    // Conectar este método al botón "Cerrar" del panel de créditos
    public void CloseCredits()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
