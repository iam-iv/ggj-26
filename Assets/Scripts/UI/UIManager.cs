using UnityEngine;
using System.Collections.Generic;
using Managers;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private CanvasGroupFader hudScreen;
        [SerializeField] private CanvasGroupFader pauseScreen;
        [SerializeField] private CanvasGroupFader endOfGameScreen;

        // Track the currently visible screen so we can hide it later
        private CanvasGroupFader _currentScreen;

        private void Start()
        {
            // Initialize: Start with HUD visible, others hidden
            InitializeScreen(hudScreen, true);
            InitializeScreen(pauseScreen, false);
            InitializeScreen(endOfGameScreen, false);

            _currentScreen = hudScreen;

            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnGameOver += HandleGameOver;
                Managers.GameManager.Instance.OnGameStart += HandleGameStart;
            }
        }

        private void OnDestroy()
        {
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnGameOver -= HandleGameOver;
                Managers.GameManager.Instance.OnGameStart -= HandleGameStart;
            }
        }

        private void InitializeScreen(CanvasGroupFader screen, bool visible)
        {
            if (screen != null)
            {
                screen.SetVisible(visible);
            }
        }

        /// <summary>
        /// Generic method to switch from the current screen to a new one
        /// </summary>
        public void SwitchTo(CanvasGroupFader newScreen)
        {
            if (_currentScreen == newScreen) return;

            // Fade out the old one
            if (_currentScreen != null)
            {
                _currentScreen.FadeOut();
            }

            // Fade in the new one
            if (newScreen != null)
            {
                newScreen.FadeIn();
                _currentScreen = newScreen;
            }
        }

        // --- Helper Methods for specific state changes ---

        public void ShowPauseMenu()
        {
            SwitchTo(pauseScreen);
            // Optional: Pause time here if not handled elsewhere
            // Time.timeScale = 0f; 
        }

        private void ResumeGame()
        {
            SwitchTo(hudScreen);
            // Time.timeScale = 1f;
        }

        private void HandleGameOver(bool win)
        {
            ShowGameOver();
        }

        private void HandleGameStart(GameState gameState)
        {
            ResumeGame();
        }

        private void ShowGameOver()
        {
            SwitchTo(endOfGameScreen);
        }
    }
}
