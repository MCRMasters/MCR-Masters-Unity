using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace MCRGame.UI
{
    public class ScorePopupManager : MonoBehaviour
    {
        public Canvas subCanvas; // 서브 캔버스 (Sort Order = 100)
        public GameObject winningScorePrefab; // WinningScreen-ScorePanel 프리팹

        public void ShowWinningPopup(WinningScoreData data)
        {
            // 기존 팝업 제거 (중복 방지)
            foreach (Transform child in subCanvas.transform)
            {
                Destroy(child.gameObject);
            }

            // 팝업 생성 및 초기화
            var popup = Instantiate(winningScorePrefab, subCanvas.transform);
            popup.GetComponent<WinningScorePopup>().Initialize(data);
        }
    }
}