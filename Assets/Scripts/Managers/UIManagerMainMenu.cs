using UI;
using UnityEngine;

namespace Managers
{
    public class UIManagerMainMenu : MonoBehaviour
    {
        [Header("Screens")] [SerializeField] private CanvasGroupFader creditsScreen;

        public void SetCreditsScreen( bool visible)
        {
            if (creditsScreen != null)
            {
                creditsScreen.SetVisible(visible);
            }
        }
        
    }
}
