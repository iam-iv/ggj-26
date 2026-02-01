using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// A local scene-based proxy for handling scene transitions.
    /// Attach this to a GameObject in your scene (e.g., 'SceneManager' or 'UI').
    /// UI Buttons should reference this script's methods.
    /// </summary>
    public class SceneManagement : MonoBehaviour
    {
        [Header("Scene Settings")]
        [Tooltip("Name of the Main Menu Scene")]
        [SerializeField] private string menuSceneName = "MainMenu";
        [Tooltip("Name of the Gameplay Scene")]
        [SerializeField] private string gameSceneName = "Gameplay";

        // No Singleton Instance. This script is meant to live in the scene to be referenced by UI.

        public void LoadGameScene()
        {
            // Just load the scene. GameManager (Singleton) will detect the change via OnSceneLoaded.
            SceneManager.LoadScene(gameSceneName);
        }

        public void LoadMenuScene()
        {
            SceneManager.LoadScene(menuSceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}