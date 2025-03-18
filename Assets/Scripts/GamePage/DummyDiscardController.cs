using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DummyDiscardController : MonoBehaviour
{
    [Header("버림패 처리 담당 DiscardManager (참조 연결)")]
    [SerializeField] private DiscardManager discardManager;

    [Header("버림 타일 진행용 버튼 (Inspector에서 연결)")]
    [SerializeField] private Button discardButton;

    // 남(S), 서(W), 북(N) 순서로 타일 버림
    private PlayerSeat[] seatOrder = { PlayerSeat.S, PlayerSeat.W, PlayerSeat.N };
    private int currentSeatIndex = 0;

    // 자리별 더미 손패 (테스트용으로 만("m") 타일만 사용)
    private Dictionary<PlayerSeat, List<TileData>> seatHands = new Dictionary<PlayerSeat, List<TileData>>();

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        if (discardButton != null)
        {
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }

        // 더미 데이터 초기화 (만("m") 타일만 사용)
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

    private void OnDiscardButtonClicked()
    {
        // 현재 차례의 플레이어 (남, 서, 북 순서)
        PlayerSeat seatToDiscard = seatOrder[currentSeatIndex];

        // 해당 자리 손패가 남아있는지 확인
        if (seatHands.ContainsKey(seatToDiscard) && seatHands[seatToDiscard].Count > 0)
        {
            // 랜덤으로 한 장 선택
            var hand = seatHands[seatToDiscard];
            int randomIndex = Random.Range(0, hand.Count);
            TileData tileToDiscard = hand[randomIndex];

            // 선택한 타일 제거
            hand.RemoveAt(randomIndex);

            // DiscardManager를 호출하여 3D 버림패 생성
            discardManager.DiscardTile(seatToDiscard, tileToDiscard);
            Debug.Log($"[{seatToDiscard}] 버린 패: {tileToDiscard.suit} {tileToDiscard.value}");
        }
        else
        {
            Debug.LogWarning($"{seatToDiscard} 자리에는 버릴 패가 없습니다!");
        }

        // 다음 플레이어로 순서 이동 (S → W → N → S ...)
        currentSeatIndex = (currentSeatIndex + 1) % seatOrder.Length;
    }
}
