using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;

using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;
using MCRGame.View;
using MCRGame.Audio;
using DG.Tweening;


namespace MCRGame.Game
{
    public partial class GameManager
    {
        /*──────────────────────────────────────────────*/
        /*            ROUND 초기화 & 좌석 매핑           */
        /*──────────────────────────────────────────────*/
        #region 🔄 ROUND & INIT

        #region ▶ InitRoundSub - 라운드별 리셋

        private void InitRoundSub(Round round)
        {
            leftTiles = MAX_TILES - (GameHand.FULL_HAND_SIZE - 1) * MAX_PLAYERS;
            IsFlowerConfirming = false;
            isActionUIActive = false;
            isAfterTsumoAction = false;

            AutoHuFlag = IsAutoHuDefault;
            PreventCallFlag = false;
            AutoFlowerFlag = IsAutoFlowerDefault;
            TsumogiriFlag = false;

            CanClick = false;
            NowHoverTile = null;
            NowHoverSource = null;
            tenpaiAssistDict.Clear();
            NowTenpaiAssistList.Clear();

            SetUIActive(true);
            ClearActionUI();

            discardManager.InitRound();
            UpdateLeftTiles(leftTiles);

            foreach (var cb in callBlockFields) cb.InitializeCallBlockField();
            gameHandManager.clear();
            foreach (var h in playersHand3DFields) h.clear();

            CurrentRound = round;

            ResetAllBlinkTurnEffects();
            UpdateCurrentRoundUI();

            InitSeatIndexMapping();          // ① 좌석-인덱스 매핑
            UpdateSeatLabels();              // ② 자리 라벨
            UpdateScoreText();               // ③ 점수 라벨
            InitializeProfileUI();           // ④ 프로필
            InitializeFlowerUI();            // ⑤ 화패 UI
        }

        #endregion
        #region ▶ InitRound - 첫 라운드 & 다음 라운드

        private void InitRound()
        {
            if (isGameStarted)
            {
                if (CurrentRound.NextRound() != Round.END)
                    CurrentRound = CurrentRound.NextRound();
            }
            else
            {
                CurrentRound = Round.E1;
                isGameStarted = true;
            }
            InitRoundSub(CurrentRound);
        }

        #endregion
        #region ▶ 좌석 매핑 (GetSeatMappings / InitSeatIndexMapping)

        /// <summary>deal(1~16)에 대한 좌석-플레이어Index 매핑 테이블 작성</summary>
        public static void GetSeatMappings(
            int deal,
            out Dictionary<AbsoluteSeat, int> seatToPlayer,
            out Dictionary<int, AbsoluteSeat> playerToSeat)
        {
            AbsoluteSeat[] order = DEAL_TABLE[deal];

            seatToPlayer = new Dictionary<AbsoluteSeat, int>(4);
            playerToSeat = new Dictionary<int, AbsoluteSeat>(4);

            for (int i = 0; i < 4; i++)
            {
                seatToPlayer[order[i]] = i;
                playerToSeat[i] = order[i];
            }
        }

        /* 사용 예
            DealSeatMapper.GetSeatMappings(1, out var seat2idx, out var idx2seat);
           // seat2idx[AbsoluteSeat.EAST] == 0,  idx2seat[2] == AbsoluteSeat.WEST
        */
        public void InitSeatIndexMapping()
        {
            GetSeatMappings((int)CurrentRound, out seatToPlayerIndex, out playerIndexToSeat);

            /* 내 절대좌석 & 현재 턴 좌석 계산 */
            MySeat = playerIndexToSeat[playerUidToIndex[PlayerDataManager.Instance.Uid]];
            CurrentTurnSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(MySeat, AbsoluteSeat.EAST);
        }

        #endregion
        #region ▶ InitGame - 플레이어 리스트 세팅

        /// <summary>
        /// 서버에서 받은 플레이어 정보로 Players 리스트와 UID→Index 매핑 초기화
        /// </summary>
        public void InitGame(List<Player> players)
        {
            Players = players.Select(p => new Player(p.Uid, p.Nickname, p.Index, p.Score)).ToList();
            playerUidToIndex = Players.ToDictionary(p => p.Uid, p => p.Index);

            Debug.Log($"GameManager: Game initialized with {Players.Count} players.");
        }

        #endregion

        #endregion /* ROUND & INIT */
    }
}
