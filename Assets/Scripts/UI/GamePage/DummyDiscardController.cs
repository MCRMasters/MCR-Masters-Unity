using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MCRGame.Common;  // RelativeSeat, GameTile, etc.

namespace MCRGame.UI
{
    public class DummyDiscardController : MonoBehaviour
    {
        [Header("버림패 처리 담당 DiscardManager (참조 연결)")]
        [SerializeField] private DiscardManager discardManager;

        [Header("버림 타일 진행용 버튼 (Inspector에서 연결)")]
        [SerializeField] private Button discardButton;

        // SHIMO(下家), TOI(対家), KAMI(上家) 순서로 타일 버림
        private RelativeSeat[] seatOrder = {
            RelativeSeat.SHIMO,
            RelativeSeat.TOI,
            RelativeSeat.KAMI
        };
        private int currentSeatIndex = 0;

        // 자리별 더미 손패 (서버에서 수신한 데이터처럼 목업)
        private Dictionary<RelativeSeat, List<GameTile>> seatHands = new Dictionary<RelativeSeat, List<GameTile>>();

        void Start()
        {
            // 버튼 클릭 이벤트 등록
            if (discardButton != null)
                discardButton.onClick.AddListener(OnDiscardButtonClicked);

            // 목업 데이터 초기화 (manzu 타일만 사용)
            seatHands[RelativeSeat.SHIMO] = new List<GameTile>
            {
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5
            };
            seatHands[RelativeSeat.TOI] = new List<GameTile>
            {
                GameTile.M3, GameTile.M7, GameTile.M9,
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5
            };
            seatHands[RelativeSeat.KAMI] = new List<GameTile>
            {
                GameTile.M2, GameTile.M4, GameTile.M8,
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5,
                GameTile.M1, GameTile.M9, GameTile.M5
            };
        }

        // 0부터 NormalTiles.Count 미만 사이에서 랜덤으로 GameTile 선택
        public static GameTile CreateRandomGameTile()
        {
            var normals = new List<GameTile>(GameTileExtensions.NormalTiles());
            return normals[Random.Range(0, normals.Count)];
        }

        private void OnDiscardButtonClicked()
        {
            // 현재 차례의 상대 (下家→対家→上家 순환)
            RelativeSeat seatToDiscard = seatOrder[currentSeatIndex];

            // 목업 데이터에서 꺼낼 수 있는지 확인
            if (seatHands.ContainsKey(seatToDiscard) && seatHands[seatToDiscard].Count > 0)
            {
                // 서버 메시지처럼 랜덤 타일 생성
                GameTile tileToDiscard = CreateRandomGameTile();

                // DiscardManager 호출 (RelativeSeat, GameTile)
                discardManager.DiscardTile(seatToDiscard, tileToDiscard);
                Debug.Log($"[Server Mock] [{seatToDiscard}] 버림: {tileToDiscard}");
            }
            else
            {
                Debug.LogWarning($"[{seatToDiscard}] 자리에는 버릴 패가 없습니다!");
            }

            // 다음 상대로 순서 이동
            currentSeatIndex = (currentSeatIndex + 1) % seatOrder.Length;
        }
    }
}
