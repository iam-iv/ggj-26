using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("Level duration in seconds")]
    [SerializeField] private float levelDuration = 60f;

    private float remainingTime;
    private bool running = true;
    private bool expired = false;

    // Public getter for remaining time in seconds.
    public float RemainingTime => remainingTime;

    void Awake()
    {
        remainingTime = Mathf.Max(0f, levelDuration);
    }

    void Start()
    {
        running = true;
        expired = false;
    }

    void Update()
    {
        if (!running || expired)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            expired = true;
            OnTimerExpired();
        }
    }

    private void OnTimerExpired()
    {
        // Player loses: notify GameManager if present, otherwise reload current scene
        if (Managers.GameManager.Instance != null)
        {
            Managers.GameManager.Instance.TriggerGameOver(false);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Optional controls
    public void StartTimer() => running = true; // in case it was stopped
    public void StopTimer() => running = false; // pause
    public void ResetTimer() // to initial level duration
    {
        remainingTime = Mathf.Max(0f, levelDuration);
        expired = false;
    }
}
