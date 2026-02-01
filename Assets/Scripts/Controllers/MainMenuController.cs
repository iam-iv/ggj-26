using UnityEngine;
using Managers;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private SceneManagement sceneManagement;

        private void Start()
        {
            if (sceneManagement == null)
            {
                sceneManagement = FindObjectOfType<SceneManagement>();
            }
        }

        public void OnPlayButtonClicked()
        {
            if (sceneManagement != null)
            {
                sceneManagement.LoadGameScene();
            }
            else
            {
                Debug.LogError("SceneManagement reference is missing in MainMenuController!");
            }
        }

        public void OnQuitButtonClicked()
        {
            if (sceneManagement != null)
            {
                sceneManagement.QuitGame();
            }
        }
    }
}
