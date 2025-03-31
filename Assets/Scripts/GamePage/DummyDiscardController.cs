using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace MCRGame
{
    public class DummyDiscardController : MonoBehaviour
    {
        [Header("버림패 처리 담당 DiscardManager (참조 연결)")]
        [SerializeField] private DiscardManager discardManager;

        [Header("버림 타일 진행용 버튼 (Inspector에서 연결)")]
        [SerializeField] private Button discardButton;

        // 남(S), 서(W), 북(N) 순서로 타일 버림
        private PlayerSeat[] seatOrder = { PlayerSeat.S, PlayerSeat.W, PlayerSeat.N };
        private int currentSeatIndex = 0;

        // 자리별 더미 손패 (서버에서 수신한 데이터처럼 목업)
        // 테스트용으로 만("m") 타일만 사용합니다.
        private Dictionary<PlayerSeat, List<TileData>> seatHands = new Dictionary<PlayerSeat, List<TileData>>();

        void Start()
        {
            // 버튼 클릭 이벤트 등록
            if (discardButton != null)
            {
                discardButton.onClick.AddListener(OnDiscardButtonClicked);
            }

            // 목업 데이터 초기화 (각 자리의 손패를 서버 수신 데이터처럼 미리 준비)
            seatHands[PlayerSeat.S] = new List<TileData>()
            {
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
            };
            seatHands[PlayerSeat.W] = new List<TileData>()
            {
                new TileData(){ suit = "m", value = 3 },
                new TileData(){ suit = "m", value = 7 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
            };
            seatHands[PlayerSeat.N] = new List<TileData>()
            {
                new TileData(){ suit = "m", value = 2 },
                new TileData(){ suit = "m", value = 4 },
                new TileData(){ suit = "m", value = 8 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
                new TileData(){ suit = "m", value = 1 },
                new TileData(){ suit = "m", value = 9 },
                new TileData(){ suit = "m", value = 5 },
            };
        }

        
        public static TileData CreateRandomTileData()
        {
            int randomIndex = Random.Range(0, 34); // 0 이상 34 미만 → 총 34가지 경우

            TileData tileData = new TileData();
            if (randomIndex < 9)
            {
                tileData.value = randomIndex + 1;
                tileData.suit = "m";
            }
            else if (randomIndex < 18)
            {
                tileData.value = randomIndex - 9 + 1;
                tileData.suit = "s";
            }
            else if (randomIndex < 27)
            {
                tileData.value = randomIndex - 18 + 1;
                tileData.suit = "p";
            }
            else
            {
                tileData.value = randomIndex - 27 + 1;
                tileData.suit = "z";
            }
            return tileData;
        }

        private void OnDiscardButtonClicked()
        {
            // 현재 차례의 플레이어 (남, 서, 북 순서)
            PlayerSeat seatToDiscard = seatOrder[currentSeatIndex];

            // 목업 서버 메시지처럼, 미리 준비한 손패에서 첫 번째 타일을 꺼내 처리합니다.
            if (seatHands.ContainsKey(seatToDiscard) && seatHands[seatToDiscard].Count > 0)
            {
                // 정해진 순서대로 첫 타일을 꺼냄 (서버에서 받은 데이터처럼)
                TileData tileToDiscard = CreateRandomTileData();

                // DiscardManager를 호출하여 3D 버림패 생성
                discardManager.DiscardTile(seatToDiscard, tileToDiscard);
                Debug.Log($"[Server Mock] [{seatToDiscard}] 버림: {tileToDiscard.suit}{tileToDiscard.value}");
            }
            else
            {
                Debug.LogWarning($"[{seatToDiscard}] 자리에는 버릴 패가 없습니다!");
            }

            // 다음 플레이어로 순서 이동 (S → W → N → S ...)
            currentSeatIndex = (currentSeatIndex + 1) % seatOrder.Length;
        }
    }
}