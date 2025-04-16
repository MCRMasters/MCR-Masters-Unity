using UnityEngine;
using MCRGame.Common;

namespace MCRGame.UI
{
    public class ScorePopupManager : MonoBehaviour
    {
        //public Canvas subCanvas;
        public GameObject winningScorePrefab;

        public static ScorePopupManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }

        public void ShowWinningPopup(WinningScoreData data)
        {

            if (winningScorePrefab == null)
            {
                Debug.LogError("ScorePopupManager references not set!");
                return;
            }

            GameObject oldPopup = GameObject.Find("Score Popup");
            if (oldPopup != null){
                Destroy(oldPopup);
            }
            
            var popup = Instantiate(winningScorePrefab);
            popup.name = "Score Popup";
            if (!popup.TryGetComponent<WinningScorePopup>(out var popupComponent))
            {
                Debug.LogError("WinningScorePopup component missing!");
                return;
            }
            
            popupComponent.Initialize(data);
        }
    }
}