using UnityEngine;
using MCRGame.Common;
using DG.Tweening;

namespace MCRGame.UI
{
    public class ScorePopupManager : MonoBehaviour
    {
        //public Canvas subCanvas;
        public GameObject winningScorePrefab;
        public GameObject popup;
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

        public void DeleteWinningPopup()
        {
            GameObject oldPopup = GameObject.Find("Score Popup");
            if (oldPopup != null)
            {
                Destroy(oldPopup);
                oldPopup = null;
            }
        }

        public void ShowButton()
        {
            if (!popup.TryGetComponent<WinningScorePopup>(out var popupComponent))
            {
                Debug.LogError("WinningScorePopup component missing!");
            }
            popupComponent.SetOKButtonActive();
        }
        public Sequence ShowWinningPopup(WinningScoreData data)
        {

            if (winningScorePrefab == null)
            {
                Debug.LogError("ScorePopupManager references not set!");
                return DOTween.Sequence();
            }

            GameObject oldPopup = GameObject.Find("Score Popup");
            if (oldPopup != null)
            {
                Destroy(oldPopup);
                oldPopup = null;
            }

            popup = Instantiate(winningScorePrefab);
            popup.name = "Score Popup";
            if (!popup.TryGetComponent<WinningScorePopup>(out var popupComponent))
            {
                Debug.LogError("WinningScorePopup component missing!");
                return DOTween.Sequence();
            }

            return popupComponent.Initialize(data);
        }
    }
}