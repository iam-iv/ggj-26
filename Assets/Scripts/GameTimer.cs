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

    /// <summary>
    /// Public getter for remaining time in seconds.
    /// </summary>
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
        // Player loses; restart the active scene immediately
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Optional controls
    public void StartTimer() => running = true;
    public void StopTimer() => running = false;
    public void ResetTimer()
    {
        remainingTime = Mathf.Max(0f, levelDuration);
        expired = false;
    }
}
