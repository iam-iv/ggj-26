using UnityEngine;
using Managers;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        public void OnPlayButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogError("GameManager Instance is null! Make sure GameManager is in the scene.");
            }
        }

        public void OnQuitButtonClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
