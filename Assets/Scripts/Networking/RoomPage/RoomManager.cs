using UnityEngine;
using System.Linq;

namespace MCRGame.Net
{
    public class RoomManager : MonoBehaviour
    {
        // 게스트 3명의 Ready 상태를 저장
        // guestReady[0] = 플레이어 인덱스1, [1] = 인덱스2, [2] = 인덱스3
        public bool[] guestReady = new bool[3];

        // 게스트가 Ready 버튼(실제로는 단일 버튼이지만 게스트 역할일 때) 클릭 시 호출
        // guestIndex = 1, 2, 또는 3
        public void OnGuestToggleReady(int guestIndex)
        {
            int arrIndex = guestIndex - 1;  // 1->0, 2->1, 3->2
            if (arrIndex < 0 || arrIndex >= guestReady.Length)
            {
                Debug.LogError($"잘못된 게스트 인덱스: {guestIndex}");
                return;
            }

            // Ready 상태 토글
            guestReady[arrIndex] = !guestReady[arrIndex];
            bool nowReady = guestReady[arrIndex];

            Debug.Log($"[RoomManager] Guest {guestIndex} => Ready: {nowReady}");
        }

        // 호스트(인덱스 0)가 Start 버튼을 누를 때 호출
        public void OnHostStartGame()
        {
            // 게스트 3명 중 준비된 사람 수
            int readyCount = guestReady.Count(r => r);
            Debug.Log($"[RoomManager] 현재 {readyCount}/3 게스트가 Ready 상태.");

            if (readyCount == 3)
            {
                Debug.Log("모든 게스트가 레디! → 게임 시작!!");
                // 여기서 실제 게임 씬 전환 or 서버 통신 등 진행
            }
            else
            {
                Debug.LogWarning("아직 준비되지 않은 게스트가 있습니다!");
            }
        }
    }
}
