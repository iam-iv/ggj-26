using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace Managers
{
    public struct GameSceneStruct
    {
        public int index;
        public string sceneName;
        public GameState mappedGameState;
    }
   
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

        // Global State
        private GameState currentState;

        public event Action<bool> OnGameOver;
        public event Action<GameState> OnStateChanged;
        public event Action<GameState> OnGameStart;

        private Dictionary<int, GameSceneStruct> _sceneDictionary = new()
        {
            { 0, new GameSceneStruct { index = 0, sceneName = "MainMenu" , mappedGameState = GameState.MainMenu} },
            { 1, new GameSceneStruct { index = 1, sceneName = "Gameplay",mappedGameState =  GameState.Gameplay} },

        };

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
           if (_sceneDictionary.TryGetValue(scene.buildIndex, out var structData))
           {
               UpdateState(structData.mappedGameState);
               if (structData.mappedGameState == GameState.Gameplay)
               {
                   OnGameStart?.Invoke(GameState.Gameplay);
               }
           }
           else
           {
               // Fallback if scene is not in dictionary
               if (scene.name == "MainMenu") UpdateState(GameState.MainMenu);
               else if (scene.name == "Gameplay") UpdateState(GameState.Gameplay);
           }
        }

        private void Start()
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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
                    Time.timeScale = 0f;
                    break;
            }
        }

        /// <summary>
        /// Trigger Game Over Logic
        /// </summary>
        public void TriggerGameOver(bool win)
        {
            UpdateState(GameState.GameOver);
            Debug.Log(win ? "Game Over: YOU WON!" : "Game Over: YOU LOST!");
            OnGameOver?.Invoke(win);
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

        public GameState GetState() => currentState;
    }
}