using System.Collections.Generic;
using UnityEngine;
using MCRGame.Common;
using MCRGame.UI;

namespace MCRGame.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        // 게임 관련 데이터
        public List<Player> Players { get; private set; }
        public AbsoluteSeat MySeat { get; private set; }
        public AbsoluteSeat CurrentTurnSeat { get; private set; }
        public Round CurrentRound { get; private set; }

        // Inspector에서 할당하는 GameHandManager 오브젝트를 통해 GameHand를 관리합니다.
        [SerializeField]
        private GameHandManager gameHandManager;
        public GameHand GameHand => gameHandManager != null ? gameHandManager.GameHand : null;

        public const int MAX_TILES = 144;
        public const int MAX_PLAYERS = 4;
        private int leftTiles;
        [SerializeField]
        private UnityEngine.UI.Text leftTilesText;

        // 추가: Inspector에서 할당할 수 있는 4개의 Hand3DField 배열 (index 0~3 은 각 상대 좌석에 대응)
        [SerializeField]
        private Hand3DField[] playersHand3DFields;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Players = new List<Player>();
            leftTiles = MAX_TILES - (GameHand.FULL_HAND_SIZE - 1) * MAX_PLAYERS;
            UpdateLeftTiles(leftTiles);
        }

        public void UpdateLeftTiles(int newValue)
        {
            leftTiles = newValue;
            if (leftTilesText != null)
            {
                leftTilesText.text = leftTiles.ToString();
            }
            else
            {
                Debug.LogWarning("leftTilesText UI가 할당되지 않았습니다.");
            }
        }

        public void SetPlayers(List<Player> players)
        {
            Players = players;
            Debug.Log("GameManager: Players updated with " + players.Count + " players.");
        }

        /// <summary>
        /// INIT_EVENT 메시지를 통해 받은 초기 손패 데이터를 SELF의 손패와 플레이어들의 3D 손패 필드에 반영합니다.
        /// 각 Hand3DField 자식 오브젝트가 있다면 모두 삭제한 후,
        /// GameHand.FULL_HAND_SIZE - 1 만큼의 타일을 생성하고 tsumoTile은 null로 초기화합니다.
        /// </summary>
        /// <param name="initTiles">자신의 초기 손패 타일 데이터 리스트</param>
        public void InitHandFromMessage(List<GameTile> initTiles)
        {
            Debug.Log("GameManager: Initializing hand with received data for SELF.");

            // 자신의 손패 초기화는 기존 GameHandManager를 이용합니다.
            if (gameHandManager != null)
            {
                gameHandManager.InitHand(initTiles);
            }
            else
            {
                Debug.LogWarning("GameManager: GameHandManager 인스턴스가 없습니다.");
            }

            // Inspector에서 할당된 각 Hand3DField에 대해 초기화 처리 (SELF 포함)
            if (playersHand3DFields == null || playersHand3DFields.Length < MAX_PLAYERS)
            {
                Debug.LogError("playersHand3DFields 배열이 4개로 할당되어 있지 않습니다.");
                return;
            }

            for (int i = 0; i < playersHand3DFields.Length; i++)
            {
                Hand3DField hand3DField = playersHand3DFields[i];
                if (hand3DField == null)
                {
                    Debug.LogWarning($"Hand3DField가 배열의 index {i}에서 할당되지 않았습니다.");
                    continue;
                }

                // 기존 자식 타일 오브젝트가 있다면 모두 파괴하고 리스트를 클리어
                if (hand3DField.handTiles != null && hand3DField.handTiles.Count > 0)
                {
                    foreach (GameObject tileObj in hand3DField.handTiles)
                    {
                        if (tileObj != null)
                        {
                            Destroy(tileObj);
                        }
                    }
                    hand3DField.handTiles.Clear();
                }
                // tsumoTile을 null로 초기화
                hand3DField.tsumoTile = null;

                int tilesToCreate = GameHand.FULL_HAND_SIZE - 1;
                if (i != (int)RelativeSeat.SELF)
                for (int j = 0; j < tilesToCreate; j++)
                {
                    hand3DField.AddTile();
                }
            }
        }
    }
}
