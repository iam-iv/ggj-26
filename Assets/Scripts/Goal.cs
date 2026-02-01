using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Tooltip("Tag del objeto jugador (por defecto: Player)")]
    public string playerTag = "Player";

    [Tooltip("Nombre de la escena del menú principal a cargar")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [Tooltip("Clip que se reproduce al ganar (opcional)")]
    public AudioClip winClip;
    [Tooltip("AudioSource usado para reproducir el clip (opcional). Si está vacío se creará uno.")]
    public AudioSource audioSource;
    [Tooltip("Delay extra después del clip (segundos, usa 0 para ninguno)")]
    public float postWinDelay = 0f;

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
            if (winClip != null)
            {
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();

                audioSource.PlayOneShot(winClip);
                StartCoroutine(WaitThenLoad(winClip.length + postWinDelay));
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }

    private IEnumerator WaitThenLoad(float waitSeconds)
    {
        yield return new WaitForSecondsRealtime(waitSeconds);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Método público opcional para botones
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
