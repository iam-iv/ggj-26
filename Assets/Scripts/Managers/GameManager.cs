using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Serialization;

namespace Managers
{
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Name of the Main Menu Scene")]
        [SerializeField] private string menuSceneName = "MainMenu";
        [Tooltip("Name of the Gameplay Scene")]
        [SerializeField] private string gameSceneName = "Gameplay";

        // Global State
        [SerializeField] private GameState currentState;

        public event Action<bool> OnGameOver;
        public event Action<GameState> OnStateChanged;
        public event Action<GameState> OnGameStart;

        private void Awake()
        {
            // Singleton Pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Initialize state based on active scene
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == menuSceneName)
            {
                UpdateState(GameState.MainMenu);
            }
            else if (currentScene.name == gameSceneName)
            {
                UpdateState(GameState.Gameplay);
            }
        }

        public void UpdateState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);

            switch (currentState)
            {
                case GameState.MainMenu:
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    // Logic for game over state can be added here
                    break;
            }
        }

        /// <summary>
        /// Called by the 'Play' button in the Menu
        /// </summary>
        public void StartNewGame()
        {
            UpdateState(GameState.Gameplay);
            OnGameStart?.Invoke(GameState.Gameplay);
        }

        /// <summary>
        /// Called when the player reaches the goal or dies
        /// </summary>
        /// <param name="win">True if reached goal, False if caught/time out</param>
        public void TriggerGameOver(bool win)
        {
            UpdateState(GameState.GameOver);
            
            // Log for debugging before UI is ready
            Debug.Log(win ? "Game Over: YOU WON!" : "Game Over: YOU LOST!");

            OnGameOver?.Invoke(win);
        }

        public void LoadMainMenu()
        {
            UpdateState(GameState.MainMenu);
            SceneManager.LoadScene(menuSceneName);
        }

        public void TogglePause()
        {
            if (currentState == GameState.Gameplay)
            {
                UpdateState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                UpdateState(GameState.Gameplay);
            }
        }

        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Quit Game"); // Visible in Editor
        }
    }
}