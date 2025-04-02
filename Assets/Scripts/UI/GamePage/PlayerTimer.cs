using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MCRGame.UI
{
    public class PlayerTimer : MonoBehaviour
    {
        [Header("UI 연결")]
        [SerializeField] private Text remainingTimeText;  // 남은 시간 텍스트 (숫자만 표시)
        [SerializeField] private Button testRandomTimeButton; // 테스트 버튼
        [SerializeField] private ButtonManager buttonManager; // ButtonManager와 연계

        private int remainingTime;      // 남은 시간 (초 단위)
        private Coroutine countdownCo;  // 카운트다운 코루틴 추적

        private void Start()
        {
            if (testRandomTimeButton != null)
            {
                testRandomTimeButton.onClick.AddListener(StartRandomCountdown);
            }
        }

        /// <summary>
        /// 1~60초 사이 랜덤 시간을 설정하고 카운트다운을 시작합니다.
        /// (실제 환경에서는 서버에서 플레이어의 남은 시간을 받아와야 함)
        /// </summary>
        private void StartRandomCountdown()
        {
            if (countdownCo != null)
            {
                StopCoroutine(countdownCo);
            }
            remainingTime = Random.Range(1, 61);
            // UI가 꺼져있다면 활성화
            if (remainingTimeText != null)
            {
                remainingTimeText.gameObject.SetActive(true);
            }
            countdownCo = StartCoroutine(CountdownRoutine());
        }

        /// <summary>
        /// 매 초마다 1초씩 줄어드는 카운트다운 코루틴
        /// </summary>
        private IEnumerator CountdownRoutine()
        {
            while (remainingTime > 0)
            {
                UpdateRemainingTimeText();
                yield return new WaitForSeconds(1f);
                remainingTime--;
            }
            // 마지막 0초 업데이트
            UpdateRemainingTimeText();

            // 시간이 0초가 되면 타임아웃 처리: 서버 전송(디버그로그) 및 자동 스킵
            Debug.Log("Timeout, sending server signal: timeout");
            // UI 숨김 처리
            if (remainingTimeText != null)
            {
                remainingTimeText.gameObject.SetActive(false);
            }
            // ButtonManager를 통해 자동 스킵 실행
            if (buttonManager != null)
            {
                buttonManager.AutoSkip();
            }
            countdownCo = null;
        }

        /// <summary>
        /// 남은 시간을 숫자만 UI Text에 표시합니다.
        /// </summary>
        private void UpdateRemainingTimeText()
        {
            if (remainingTimeText != null)
            {
                remainingTimeText.text = remainingTime.ToString();
            }
        }

        /// <summary>
        /// 버튼매니저의 액션 버튼 클릭 시 호출되어 남은 시간을 서버로 전송(디버그로그로 대체)하고 UI를 숨깁니다.
        /// </summary>
        public void SubmitRemainingTime()
        {
            Debug.Log("Sending remaining time to server: " + remainingTime);
            if (remainingTimeText != null)
            {
                remainingTimeText.gameObject.SetActive(false);
            }
            if (countdownCo != null)
            {
                StopCoroutine(countdownCo);
                countdownCo = null;
            }
        }
    }
}
