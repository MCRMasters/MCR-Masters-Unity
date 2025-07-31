using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;

using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;
using MCRGame.Audio;
using MCRGame.Effect;
using DG.Tweening;


namespace MCRGame.Game
{
    public partial class GameManager
    {
        /*────────────────────────────────────────────────────*/
        /*          📡  WS  MESSAGE  HANDLERS                 */
        /*────────────────────────────────────────────────────*/
        #region 📡 WS 메시지 핸들러
        /*  ──  Update / Timer  ───────────────────────────── */
        #region ▶ 상태 업데이트

        public void UpdateActionId(int actionId)
        {
            currentActionId = actionId;
        }

        public void UpdatePlayerScores(List<int> playersScores)
        {
            for (int i = 0; i < playersScores.Count; ++i)
            {
                Players[i].Score = playersScores[i];
            }
        }

        public void SetTimer(object data)
        {
            try
            {
                // 1) data를 JObject로 변환
                var jData = data as JObject;
                if (jData == null)
                {
                    Debug.LogError("[SetTimer] data가 JObject가 아닙니다.");
                    return;
                }

                remainingTime = jData.Value<float>("remaining_time");

                // 3) 파싱된 값 사용 예시
                Debug.Log($"[SetTimer] remaining_time: {remainingTime}");

                if (timerText != null)
                {
                    timerText.gameObject.SetActive(remainingTime > 0f);
                    timerText.text = Mathf.FloorToInt(remainingTime).ToString();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SetTimer] JSON 파싱 중 오류: {ex.Message}");
            }
        }
        #endregion

        /*  ──  서버 요청 / DISCARDS  ──────────────────────── */
        #region ▶ 클라이언트 → 서버 요청

        /// <summary> TileManager 클릭 시 호출: 서버로 검증 요청 </summary>
        public void RequestDiscard(GameTile tile, bool is_tsumogiri)
        {
            var payload = new
            {
                event_type = (int)GameEventType.DISCARD,
                action_id = currentActionId,
                data = new
                {
                    tile = (int)tile,
                    is_tsumogiri,
                }
            };
            GameWS.Instance.SendGameEvent(GameWSActionType.GAME_EVENT, payload);
        }

        #endregion

        /*  ──  Confirm ● 서버 Broadcast  ─────────────────── */
        #region ▶ Confirm 메시지

        public IEnumerator WaitAndProcessTsumo(JObject data)
        {
            // GameManager 가 아직 뜨지 않은 경우도 고려
            yield return new WaitUntil(() =>
                !IsFlowerConfirming);

            yield return ProcessTsumoActions(data);
        }




        /// <summary>
        /// 서버에서 전달한 호출(CallBlock) 데이터를 파싱합니다.
        /// data는 JSON 형태로 { "seat": <seat>, "call_block_data": <CallBlockData JSON> }를 포함합니다.
        /// </summary>
        public void ConfirmCallBlock(object data)
        {
            ClearActionUI();
            try
            {
                Debug.Log("ConfirmCallBlock: Step 1 - Casting data to JObject");
                JObject jData = data as JObject;
                if (jData == null)
                {
                    Debug.LogWarning("ConfirmCallBlock: Data is not a valid JObject");
                    return;
                }

                Debug.Log("ConfirmCallBlock: Step 2 - Parsing 'seat' value");
                int seatInt = jData["seat"].ToObject<int>();
                Debug.Log("ConfirmCallBlock: seatInt = " + seatInt);
                AbsoluteSeat seat = (AbsoluteSeat)seatInt;
                Debug.Log("ConfirmCallBlock: seat = " + seat.ToString());

                Debug.Log("ConfirmCallBlock: Step 3 - Parsing 'call_block_data'");
                JToken callBlockToken = jData["call_block_data"];
                CallBlockData callBlockData = null;
                if (callBlockToken != null && callBlockToken.Type != JTokenType.Null)
                {
                    Debug.Log("ConfirmCallBlock: call_block_data token exists, type: " + callBlockToken.Type);
                    callBlockData = callBlockToken.ToObject<CallBlockData>();
                    if (callBlockData != null)
                    {
                        Debug.Log("ConfirmCallBlock: callBlockData parsed successfully. FirstTile = " + callBlockData.FirstTile.ToString());
                    }
                    else
                    {
                        Debug.LogWarning("ConfirmCallBlock: callBlockData parsed as null.");
                    }
                }
                else
                {
                    Debug.Log("ConfirmCallBlock: 'call_block_data' is null.");
                }

                // call block audio
                ActionAudioManager.Instance?.EnqueueCallSound(callBlockData.Type);

                Debug.Log("ConfirmCallBlock: Step 4 - Parsing 'has_tsumo_tile'");
                bool has_tsumo_tile = false;
                JToken hasTsumoTileToken = jData["has_tsumo_tile"];
                if (hasTsumoTileToken != null)
                {
                    Debug.Log("ConfirmCallBlock: has_tsumo_tile token exists.");
                }
                else
                {
                    Debug.Log("ConfirmCallBlock: has_tsumo_tile token is null.");
                }
                if (hasTsumoTileToken != null && callBlockData != null &&
                    (callBlockData.Type == CallBlockType.AN_KONG || callBlockData.Type == CallBlockType.SHOMIN_KONG))
                {
                    has_tsumo_tile = hasTsumoTileToken.ToObject<bool>();
                    Debug.Log("ConfirmCallBlock: has_tsumo_tile parsed as " + has_tsumo_tile);
                }
                else
                {
                    Debug.Log("ConfirmCallBlock: Skipping has_tsumo_tile parsing.");
                }

                Debug.Log("ConfirmCallBlock: Step 5 - Logging parsed values");
                Debug.Log($"ConfirmCallBlock: seat = {seat}, callBlockData = {(callBlockData != null ? callBlockData.FirstTile.ToString() : "null")}");

                if (jData.TryGetValue("action_id", out JToken actionIDToken))
                {
                    if (actionIDToken != null)
                    {
                        currentActionId = actionIDToken.ToObject<int>();
                    }
                }

                Debug.Log("ConfirmCallBlock: Step 6 - Determining relative seat");
                RelativeSeat relativeSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(currentSeat: MySeat, targetSeat: seat);
                Debug.Log("ConfirmCallBlock: relativeSeat = " + relativeSeat);


                Debug.Log("ConfirmCallBlock: Step 7 - Accessing callBlockData.SourceSeat");
                RelativeSeat CallBlockSourceSeat = RelativeSeat.SELF;
                if (callBlockData != null)
                {
                    CallBlockSourceSeat = callBlockData.SourceSeat;
                }
                AbsoluteSeat sourceAbsoluteSeat = CallBlockSourceSeat.ToAbsoluteSeat(mySeat: seat);
                Debug.Log("ConfirmCallBlock: sourceAbsoluteSeat = " + sourceAbsoluteSeat);
                RelativeSeat sourceRelativeSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(currentSeat: MySeat, targetSeat: sourceAbsoluteSeat);
                Debug.Log("ConfirmCallBlock: sourceRelativeSeat = " + sourceRelativeSeat);



                if (relativeSeat == RelativeSeat.SELF)
                {
                    if (callBlockData.Type == CallBlockType.CHII || callBlockData.Type == CallBlockType.PUNG)
                    {
                        tenpaiAssistDict.Clear();
                        if (jData.TryGetValue("tenpai_assist", out JToken assistToken)
                            && assistToken.Type == JTokenType.Object)
                        {
                            tenpaiAssistDict = BuildTenpaiAssistDict((JObject)assistToken);
                        }
                    }
                    Debug.Log("ConfirmCallBlock: Applying call for SELF.");
                    gameHandManager.ApplyCall(cbData: callBlockData);
                }
                else
                {
                    Debug.Log("ConfirmCallBlock: Adding call block to callBlockFields for relativeSeat: " + relativeSeat);
                    if (callBlockData != null)
                    {
                        callBlockFields[(int)relativeSeat].AddCallBlock(data: callBlockData);
                    }
                    if (callBlockData.Type == CallBlockType.CHII || callBlockData.Type == CallBlockType.PUNG)
                    {
                        Debug.Log("ConfirmCallBlock: Starting RequestDiscardMultiple(count: 2)");
                        playersHand3DFields[(int)relativeSeat]
                            .RequestDiscardMultipleSequence(count: 2)
                            .Play();
                    }
                    else if (callBlockData.Type == CallBlockType.DAIMIN_KONG)
                    {
                        Debug.Log("ConfirmCallBlock: Starting RequestDiscardMultiple(count: 3)");
                        playersHand3DFields[(int)relativeSeat]
                            .RequestDiscardMultipleSequence(count: 3)
                            .Play();
                    }
                    else if (callBlockData.Type == CallBlockType.AN_KONG)
                    {
                        if (has_tsumo_tile)
                        {
                            Debug.Log("ConfirmCallBlock: AN_KONG with tsumo tile, starting RequestDiscardRightmost and RequestDiscardMultiple(count: 3)");
                            playersHand3DFields[(int)relativeSeat]
                                .RequestDiscardRightmostSequence()
                                .Play();
                            playersHand3DFields[(int)relativeSeat]
                                .RequestDiscardMultipleSequence(count: 3)
                                .Play();
                        }
                        else
                        {
                            Debug.Log("ConfirmCallBlock: AN_KONG without tsumo tile, starting RequestDiscardMultiple(count: 4)");
                            playersHand3DFields[(int)relativeSeat]
                                .RequestDiscardMultipleSequence(count: 4)
                                .Play();
                        }
                    }
                    else if (callBlockData.Type == CallBlockType.SHOMIN_KONG)
                    {
                        if (has_tsumo_tile)
                        {
                            Debug.Log("ConfirmCallBlock: SHOMIN_KONG with tsumo tile, starting RequestDiscardRightmost");
                            playersHand3DFields[(int)relativeSeat]
                                .RequestDiscardRightmostSequence()
                                .Play();
                        }
                        else
                        {
                            Debug.Log("ConfirmCallBlock: SHOMIN_KONG without tsumo tile, starting RequestDiscardMultiple(count: 1)");
                            playersHand3DFields[(int)relativeSeat]
                                .RequestDiscardMultipleSequence(count: 1)
                                .Play();
                        }
                    }
                }
                if (callBlockData.Type == CallBlockType.CHII ||
                    callBlockData.Type == CallBlockType.PUNG ||
                    callBlockData.Type == CallBlockType.DAIMIN_KONG)
                {
                    Debug.Log("ConfirmCallBlock: Removing last discard for sourceRelativeSeat = " + sourceRelativeSeat);
                    discardManager.RemoveLastDiscard(seat: sourceRelativeSeat);
                }

                moveTurn(relativeSeat);
            }
            catch (Exception ex)
            {
                Debug.LogError("ConfirmCallBlock parsing error: " + ex.Message);
            }
        }


        public void ConfirmTsumo(JObject data)
        {
            ClearActionUI();
            AbsoluteSeat TsumoSeat = (AbsoluteSeat)data["seat"].ToObject<int>();
            UpdateLeftTilesByDelta(-1);
            if (TsumoSeat == MySeat)
            {
                return;
            }

            RelativeSeat relativeTsumoSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(currentSeat: MySeat, targetSeat: TsumoSeat);

            moveTurn(seat: relativeTsumoSeat);

            if (playersHand3DFields[(int)relativeTsumoSeat].handTiles.Count >= GameHandManager.FULL_HAND_SIZE)
            {
                return;
            }
            playersHand3DFields[(int)relativeTsumoSeat]
                .RequestTsumoSequence()
                .Play();
        }
        public IEnumerator ConfirmFlower(JObject data)
        {
            ClearActionUI();
            GameTile floweredTile = (GameTile)data["tile"].ToObject<int>();
            AbsoluteSeat floweredSeat = (AbsoluteSeat)data["seat"].ToObject<int>();
            RelativeSeat floweredRelativeSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(currentSeat: MySeat, targetSeat: floweredSeat);

            ActionAudioManager.Instance?.EnqueueFlowerSound();

            if (floweredRelativeSeat == RelativeSeat.SELF)
            {
                bool animateDone = false;
                yield return gameHandManager.RunExclusive(gameHandManager.ApplyFlowerSequence(tile: floweredTile));
                int previousCount = flowerCountMap[floweredRelativeSeat];
                int currentFlowerCount = previousCount + 1;
                flowerCountMap[floweredRelativeSeat] = currentFlowerCount; // update early so next requests see the increment

                PlayFlowerCountAnimation(floweredRelativeSeat, previousCount, currentFlowerCount, () => { animateDone = true; });
                yield return new WaitUntil(() => animateDone);
                SetFlowerCount(floweredRelativeSeat, flowerCountMap[floweredRelativeSeat]);
            }
            else
            {
                // 상대의 경우: Hand3DField를 이용해 요청
                Hand3DField handField = playersHand3DFields[(int)floweredRelativeSeat];
                int previousCount = flowerCountMap[floweredRelativeSeat];
                int currentFlowerCount = previousCount + 1;
                flowerCountMap[floweredRelativeSeat] = currentFlowerCount;

                bool animateDone = false;

                // 동시에 꽃 카운트 애니메이션 실행
                PlayFlowerCountAnimation(floweredRelativeSeat, previousCount, currentFlowerCount, () => { animateDone = true; });
                // Hand3DField의 RequestDiscardRandom과 RequestInitFlowerTsumo를 순차 실행하는 코루틴
                yield return handField.RequestDiscardRandomSequence().WaitForCompletion();
                yield return new WaitUntil(() => animateDone);

                SetFlowerCount(floweredRelativeSeat, flowerCountMap[floweredRelativeSeat]);
            }
            IsFlowerConfirming = false;
        }

        public void ConfirmDiscard(JObject data)
        {
            // discard sound audio
            DiscardSoundManager.Instance.PlayDiscardSound();

            Debug.Log($"[GameManager.ConfirmDiscard] discard tile successfully");
            ClearActionUI();
            GameTile discardTile = (GameTile)data["tile"].ToObject<int>();



            AbsoluteSeat discardedSeat = (AbsoluteSeat)data["seat"].ToObject<int>();
            bool is_tsumogiri = data["is_tsumogiri"].ToObject<bool>();
            if (discardedSeat == MySeat)
            {
                if (tenpaiAssistDict != null
                && tenpaiAssistDict.TryGetValue(discardTile, out var list)
                && list != null
                && list.Count > 0)
                {
                    NowTenpaiAssistList = new List<TenpaiAssistEntry>(list);
                }
                else
                {
                    NowTenpaiAssistList = new List<TenpaiAssistEntry>();
                }
                tenpaiAssistDict.Clear();
                gameHandManager.ConfirmDiscard(tile: discardTile);
            }
            else
            {
                RelativeSeat enemyDiscardSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(currentSeat: MySeat, targetSeat: discardedSeat);
                if (is_tsumogiri)
                {
                    playersHand3DFields[(int)enemyDiscardSeat]
                        .RequestDiscardRightmostSequence()
                        .Play();
                }
                else
                {
                    playersHand3DFields[(int)enemyDiscardSeat]
                        .RequestDiscardRandomSequence()
                        .Play();
                }
                discardManager.DiscardTile(seat: enemyDiscardSeat, tile: discardTile);
            }
        }

        #endregion

        /*  ──  Process & Reload  ─────────────────────────── */
        #region ▶ Reload 처리

        public void ReloadDiscardActions(List<GameAction> list)
        {
            ClearActionUI();

            isAfterTsumoAction = false;
            if (timerText != null)
            {
                timerText.gameObject.SetActive(remainingTime > 0f);
                timerText.text = Mathf.FloorToInt(remainingTime).ToString();
            }

            list.Sort();

            // 3) SKIP 버튼 (항상 제일 먼저)
            if (list.Count > 0)
            {
                isActionUIActive = true;

                var skip = Instantiate(actionButtonPrefab, actionButtonPanel);
                skip.GetComponent<Image>().sprite = skipButtonSprite;
                skip.GetComponent<Button>().onClick.AddListener(OnSkipButtonClicked);
            }

            // 4) CHII/KAN 그룹별 분기
            var groups = list.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in groups)
            {
                var type = kv.Key;
                var actionsOfType = kv.Value;

                // CHII 또는 KAN 이고 선택지가 2개 이상이면 "추가 선택지" 버튼 생성
                if ((type == GameActionType.CHII || type == GameActionType.KAN)
                    && actionsOfType.Count > 1)
                {
                    var button = Instantiate(actionButtonPrefab, actionButtonPanel);
                    button.GetComponent<Image>().sprite = GetSpriteForAction(type);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                        ShowAdditionalActionChoices(type, actionsOfType));
                }
                else
                {
                    // 단일 혹은 그 외 행동: 바로 버튼 생성
                    foreach (var act in actionsOfType)
                    {
                        var btnObj = Instantiate(actionButtonPrefab, actionButtonPanel);
                        btnObj.GetComponent<Image>().sprite = GetSpriteForAction(act.Type);
                        btnObj.GetComponent<Button>().onClick.AddListener(() =>
                            OnActionButtonClicked(act));
                    }
                }
            }
        }

        public void ReloadTsumoActions(List<GameAction> list)
        {
            ClearActionUI();
            isAfterTsumoAction = true;
            if (timerText != null)
            {
                timerText.gameObject.SetActive(remainingTime > 0f);
                timerText.text = Mathf.FloorToInt(remainingTime).ToString();
            }

            list.Sort();


            // Skip 버튼
            if (list.Count > 0)
            {
                isActionUIActive = true;
                var skip = Instantiate(actionButtonPrefab, actionButtonPanel);
                skip.GetComponent<Image>().sprite = skipButtonSprite;
                skip.GetComponent<Button>().onClick.AddListener(OnSkipButtonClickedAfterTsumo);
            }

            // CHII/KAN 추가선택지 로직
            var groups = list.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in groups)
            {
                var type = kv.Key;
                var actionsOfType = kv.Value;

                if ((type == GameActionType.CHII || type == GameActionType.KAN)
                    && actionsOfType.Count > 1)
                {
                    var button = Instantiate(actionButtonPrefab, actionButtonPanel);
                    button.GetComponent<Image>().sprite = GetSpriteForAction(type);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                        ShowAdditionalActionChoices(type, actionsOfType));
                }
                else
                {
                    foreach (var act in actionsOfType)
                    {
                        var btnObj = Instantiate(actionButtonPrefab, actionButtonPanel);
                        btnObj.GetComponent<Image>().sprite = GetSpriteForAction(act.Type);
                        btnObj.GetComponent<Button>().onClick.AddListener(() =>
                            OnActionButtonClicked(act));
                    }
                }
            }

            moveTurn(RelativeSeat.SELF);
        }

        public void ReloadData(JObject data)
        {
            isGameStarted = true;
            Players = data["player_list"]
                .ToObject<List<Player>>();

            CurrentRound = (Round)data["current_round"].Value<int>();
            InitRoundSub(CurrentRound);

            CurrentTurnSeat = (RelativeSeat)data["current_turn_seat"].Value<int>();

            leftTiles = data["tiles_remaining"].Value<int>();
            UpdateLeftTiles(leftTiles);

            List<List<GameTile>> kawas = data["kawas"]
                .Select(arr => arr.ToObject<List<GameTile>>())
                .ToList();

            discardManager.ReloadAllDiscards(allTilesBySeat: kawas);

            List<List<CallBlockData>> CallBlocksList = data["call_blocks_list"]
                .Select(arr => arr.ToObject<List<CallBlockData>>())
                .ToList();

            List<GameTile> RawHand = data["hand"]
                .Select(t => (GameTile)t.Value<int>())
                .ToList();
            List<CallBlockData> RawCallBockList = CallBlocksList[(int)MySeat];


            var tsumoToken = data["tsumo_tile"];
            GameTile? TsumoTile = (tsumoToken.Type == JTokenType.Null)
                ? (GameTile?)null
                : (GameTile)tsumoToken.Value<int>();

            gameHandManager.ReloadInitHand(rawTiles: RawHand, rawCallBlocks: RawCallBockList, rawTsumoTile: TsumoTile);

            List<int> HandsCount = data["hands_count"]
                .ToObject<List<int>>();

            List<int> TsumoTilesCount = data["tsumo_tiles_count"]
                .ToObject<List<int>>();

            for (int seat = 0; seat < MAX_PLAYERS; ++seat)
            {
                if ((RelativeSeat)seat == RelativeSeat.SELF) continue;
                AbsoluteSeat absoluteSeat = RelativeSeatExtensions.ToAbsoluteSeat(rel: (RelativeSeat)seat, mySeat: MySeat);
                callBlockFields[seat].ReloadCallBlockListImmediate(CallBlocksList[(int)absoluteSeat]);
                playersHand3DFields[seat].ReloadInitHand(handCount: HandsCount[(int)absoluteSeat], includeTsumo: TsumoTilesCount[(int)absoluteSeat] == 1);
            }

            List<int> FlowersCount = data["flowers_count"]
                .ToObject<List<int>>();
            InitializeFlowerUI();
            for (int seat = 0; seat < MAX_PLAYERS; ++seat)
            {
                AbsoluteSeat absoluteSeat = RelativeSeatExtensions.ToAbsoluteSeat(rel: (RelativeSeat)seat, mySeat: MySeat);
                SetFlowerCount(rel: (RelativeSeat)seat, FlowersCount[(int)absoluteSeat]);
            }

            moveTurn(CurrentTurnSeat);
            currentActionId = data["action_id"].Value<int>();

            // remaining time 처리
            List<List<GameAction>> ActionList = data["action_choices_list"]
                .Select(arr => arr.ToObject<List<GameAction>>())
                .ToList();
            remainingTime = data["remaining_time"].Value<float>();

            if (CurrentTurnSeat == RelativeSeat.SELF)
            {
                ReloadTsumoActions(ActionList[(int)MySeat]);
            }
            else
            {
                if (ActionList[(int)MySeat].Count > 0)
                {
                    ReloadDiscardActions(ActionList[(int)MySeat]);
                }
            }
            Debug.Log($"[GameManager] ReloadData 완료 - 남은 시간: {remainingTime:F2}s");
        }

        #endregion

        #region ▶ Process 처리

        public void ProcessDiscardActions(JObject data)
        {
            ClearActionUI();

            isAfterTsumoAction = false;


            // 1) action_id
            currentActionId = data["action_id"].ToObject<int>();
            // 2) GameAction 리스트로 변환 후 정렬
            var list = data["actions"].ToObject<List<GameAction>>();
            list.Sort();

            bool has_hu_action = false;

            foreach (var action in list)
            {
                if (action.Type == GameActionType.HU)
                {
                    has_hu_action = true;
                    if (AutoHuFlag)
                    {
                        SendSelectedAction(action);
                        return;
                    }
                }
            }
            if (PreventCallFlag && !has_hu_action)
            {
                OnSkipButtonClicked();
                return;
            }

            remainingTime = data["left_time"].ToObject<float>();
            if (timerText != null)
            {
                timerText.gameObject.SetActive(remainingTime > 0f);
                timerText.text = Mathf.FloorToInt(remainingTime).ToString();
            }


            // 3) SKIP 버튼 (항상 제일 먼저)
            if (list.Count > 0)
            {
                isActionUIActive = true;

                var skip = Instantiate(actionButtonPrefab, actionButtonPanel);
                skip.GetComponent<Image>().sprite = skipButtonSprite;
                skip.GetComponent<Button>().onClick.AddListener(OnSkipButtonClicked);
            }

            // 4) CHII/KAN 그룹별 분기
            var groups = list.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in groups)
            {
                var type = kv.Key;
                var actionsOfType = kv.Value;
                if (PreventCallFlag && has_hu_action && type != GameActionType.HU)
                {
                    continue;
                }
                // CHII 또는 KAN 이고 선택지가 2개 이상이면 "추가 선택지" 버튼 생성
                if ((type == GameActionType.CHII || type == GameActionType.KAN)
                    && actionsOfType.Count > 1)
                {
                    var button = Instantiate(actionButtonPrefab, actionButtonPanel);
                    button.GetComponent<Image>().sprite = GetSpriteForAction(type);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                        ShowAdditionalActionChoices(type, actionsOfType));
                }
                else
                {
                    // 단일 혹은 그 외 행동: 바로 버튼 생성
                    foreach (var act in actionsOfType)
                    {
                        var btnObj = Instantiate(actionButtonPrefab, actionButtonPanel);
                        btnObj.GetComponent<Image>().sprite = GetSpriteForAction(act.Type);
                        btnObj.GetComponent<Button>().onClick.AddListener(() =>
                            OnActionButtonClicked(act));
                    }
                }
            }
        }

        public IEnumerator ProcessTsumoActions(JObject data)
        {
            UpdateLeftTilesByDelta(-1);

            ClearActionUI();

            isAfterTsumoAction = true;

            currentActionId = data["action_id"].ToObject<int>();
            remainingTime = data["left_time"].ToObject<float>();

            tenpaiAssistDict.Clear();
            if (data.TryGetValue("tenpai_assist", out JToken assistToken)
                && assistToken.Type == JTokenType.Object)
            {
                tenpaiAssistDict = BuildTenpaiAssistDict((JObject)assistToken);
            }

            GameTile newTsumoTile = (GameTile)data["tile"].ToObject<int>();
            if (gameHandManager.GameHandPublic.HandSize < GameHand.FULL_HAND_SIZE)
            {
                yield return gameHandManager.RunExclusive(gameHandManager.AddTsumoSequence(newTsumoTile));
            }

            var list = data["actions"].ToObject<List<GameAction>>();
            list.Sort();
            foreach (var action in list)
            {
                if (action.Type == GameActionType.HU)
                {
                    if (AutoHuFlag)
                    {
                        moveTurn(RelativeSeat.SELF);
                        CanClick = false;
                        SendSelectedAction(action);
                        yield break;
                    }
                }
                else if (action.Type == GameActionType.FLOWER)
                {
                    if (AutoFlowerFlag)
                    {
                        moveTurn(RelativeSeat.SELF);
                        CanClick = false;
                        SendSelectedAction(action);
                        yield break;
                    }
                }
            }
            if (TsumogiriFlag)
            {
                moveTurn(RelativeSeat.SELF);
                CanClick = false;
                yield return gameHandManager.RunExclusive(gameHandManager.RequestDiscardRightmostTile());
                yield break;
            }

            if (timerText != null)
            {
                timerText.gameObject.SetActive(remainingTime > 0f);
                timerText.text = Mathf.FloorToInt(remainingTime).ToString();
            }
            // Skip 버튼
            if (list.Count > 0)
            {
                isActionUIActive = true;
                var skip = Instantiate(actionButtonPrefab, actionButtonPanel);
                skip.GetComponent<Image>().sprite = skipButtonSprite;
                skip.GetComponent<Button>().onClick.AddListener(OnSkipButtonClickedAfterTsumo);
            }

            // CHII/KAN 추가선택지 로직
            var groups = list.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var kv in groups)
            {
                var type = kv.Key;
                var actionsOfType = kv.Value;

                if ((type == GameActionType.CHII || type == GameActionType.KAN)
                    && actionsOfType.Count > 1)
                {
                    var button = Instantiate(actionButtonPrefab, actionButtonPanel);
                    button.GetComponent<Image>().sprite = GetSpriteForAction(type);
                    button.GetComponent<Button>().onClick.AddListener(() =>
                        ShowAdditionalActionChoices(type, actionsOfType));
                }
                else
                {
                    foreach (var act in actionsOfType)
                    {
                        var btnObj = Instantiate(actionButtonPrefab, actionButtonPanel);
                        btnObj.GetComponent<Image>().sprite = GetSpriteForAction(act.Type);
                        btnObj.GetComponent<Button>().onClick.AddListener(() =>
                            OnActionButtonClicked(act));
                    }
                }
            }

            moveTurn(RelativeSeat.SELF);
        }

        #endregion
        /*────────────────────────────────────────────────────*/

        /*  ──  기타 헬퍼  ────────────────────────────────── */
        #region ▶ Flower Replacement & Result

        public void ProcessInitFlowerReplacement(GameWSMessage message)
        {
            List<GameTile> newTiles = null;
            List<GameTile> appliedFlowers = null;
            List<int> flowerCounts = null;

            if (message.Data.TryGetValue("new_tiles", out JToken tilesToken))
            {
                var newTilesInts = tilesToken.ToObject<List<int>>();
                newTiles = newTilesInts.Select(i => (GameTile)i).ToList();
            }

            if (message.Data.TryGetValue("applied_flowers", out JToken appliedFlowersToken))
            {
                appliedFlowers = appliedFlowersToken.ToObject<List<GameTile>>();
            }

            if (message.Data.TryGetValue("flower_count", out JToken countToken))
            {
                flowerCounts = countToken.ToObject<List<int>>();
            }

            if (newTiles != null && appliedFlowers != null && flowerCounts != null)
            {
                Debug.Log("[GameManager] Starting flower replacement coroutine.");
                StartCoroutine(gameHandManager.RunExclusive(FlowerReplacementController.Instance
                    .StartFlowerReplacement(newTiles, appliedFlowers, flowerCounts)));
            }
            else
            {
                Debug.LogWarning("[GameManager] One or more flower replacement parameters were missing.");
            }
        }

        public IEnumerator ProcessDraw(
            List<List<GameTile>> anKanInfos
        )
        {
            isInitHandDone = false;
            yield return StartCoroutine(cameraResultAnimator.PlayResultAnimation());
            yield return new WaitForSeconds(3f);
            cameraResultAnimator.ResetCameraState();
            if (GameWS.Instance != null)
            {
                GameWS.Instance.SendGameEvent(GameWSActionType.GAME_EVENT, new
                {
                    event_type = (int)GameEventType.NEXT_ROUND_CONFIRM,
                    data = new Dictionary<string, object>()
                });
            }
            ResetAllBlinkTurnEffects();
            // if (CurrentRound == Round.N4)
            // {
            //     EndScorePopup();
            // }
        }

        public IEnumerator ProcessHuHand(
            List<GameTile> handTiles,
            List<CallBlockData> callBlocks,
            ScoreResult scoreResult,
            AbsoluteSeat winPlayerSeat,
            AbsoluteSeat currentPlayerSeat,
            int flowerCount,
            GameTile? tsumoTile,
            List<List<GameTile>> anKanInfos,
            GameTile winningTile
        )
        {
            ActionAudioManager.Instance?.EnqueueHuSound();


            isInitHandDone = false;
            ClearActionUI();
            CanClick = false;
            gameHandManager.IsAnimating = true;
            handTiles.Sort();
            int singleScore = scoreResult.total_score + flowerCount;
            int total_score = (winPlayerSeat == currentPlayerSeat ? singleScore * 3 : singleScore) + 24;
            scoreResult.yaku_score_list.Add(new YakuScore(yid: (int)Yaku.FlowerPoint, score: flowerCount));
            WinningScoreData wsd = new WinningScoreData(handTiles, callBlocks, singleScore, total_score, scoreResult.yaku_score_list, winPlayerSeat, flowerCount, winningTile);


            // 3D 핸드 필드 새로 생성: 승리한 플레이어의 핸드 필드를 클리어하고 실제 타일로 재구성
            // (tsumoTile과 동일한 타일 한 개는 표시하지 않고, 마지막에 tsumoTile을 추가하여 extra gap이 적용되도록)
            Hand3DField targetHandField = playersHand3DFields[(int)RelativeSeatExtensions.CreateFromAbsoluteSeats(currentSeat: MySeat, targetSeat: winPlayerSeat)];
            targetHandField.clear();
            GameObject winningTileGo = targetHandField.MakeRealHand(winningTile, handTiles, tsumoTile.HasValue);
            if (winningTileGo == null)
            {
                // 창깡이면 그 타일 위치, 론이면 강에 있는 그 타일 위치 반환
                winningTileGo = NowFocus3DTile;
            }
            // "Main 2D Canvas" 이름의 GameObject 찾기
            GameObject canvas = _canvasInstance;
            if (canvas == null)
            {
                Debug.LogWarning("Main 2D Canvas를 찾을 수 없습니다.");
                yield break;
            }
            canvas.SetActive(false);
            Debug.Log("Canvas 비활성화 완료.");
            yield return StartCoroutine(cameraResultAnimator.PlayResultAnimation());
            Sequence seq = targetHandField.AnimateAllTilesRotationDomino(baseDuration: 0.4f, handScore: singleScore);
            yield return seq.WaitForCompletion();
            if (winningTileGo != null)
            {
                // 1) Players 리스트에서 Index가 일치하는 Player 찾기
                var winningPlayer = Players
                    .FirstOrDefault(p => p.Index == seatToPlayerIndex[winPlayerSeat]);
                var winningru = PlayerInfo.FirstOrDefault(p => p.uid == winningPlayer.Uid);
                if (winningPlayer == null)
                {
                    Debug.LogError($"[ProcessHuHand] No player found with Index={winPlayerSeat}");
                    yield break;
                }

                // 2) CurrentCharacter null 체크
                var winningChar = winningru.current_character;
                if (winningChar == null)
                {
                    Debug.LogError($"[ProcessHuHand] CurrentCharacter is null for player Index={winPlayerSeat}");
                    yield break;
                }

                // 3) effect 이름 결정
                string winnersEffectName = (winningChar.code == "c1")
                    ? "KunaiDrop"
                    : "WaterColumn";

                // 4) 연출 실행
                Sequence winningEffectSeq =
                    WinningEffectManager.Instance
                        .PlayEffectAtTile(winnersEffectName, winningTileGo);

                yield return winningEffectSeq.WaitForCompletion();
            }
            yield return new WaitForSeconds(0.5f);
            yield return ScorePopupManager.Instance.ShowWinningPopup(wsd).WaitForCompletion();
            ScorePopupManager.Instance.ShowButton();
            Debug.Log("processed hu hand.");
            yield return new WaitForSeconds(5f);
            cameraResultAnimator.ResetCameraState();
            ScorePopupManager.Instance.DeleteWinningPopup();
            canvas.SetActive(true);
            Debug.Log("Canvas 활성화 완료.");

            UpdateScoreText();
            ResetAllBlinkTurnEffects();
            if (GameWS.Instance != null)
            {
                GameWS.Instance.SendGameEvent(GameWSActionType.GAME_EVENT, new
                {
                    event_type = (int)GameEventType.NEXT_ROUND_CONFIRM,
                    data = new Dictionary<string, object>()
                });
            }
            // if (CurrentRound == Round.N4)
            // {
            //     EndScorePopup();
            // }
        }
        #endregion
        #endregion /* 📡 WS 메시지 핸들러 */
    }
}
