using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Game.Shared;
using System.Linq;
using System.Collections;
using System;
using DataTransfer;
using TMPro;
using Unity.VisualScripting;

public class PlayerManager : NetworkBehaviour
{
    [SyncVar]
    public int PlayerIndex;

    [SyncVar]
    public string PlayerName;

    [SyncVar(hook = nameof(OnPlayerStatusChanged))]
    public PlayerStatus playerStatus;

    [SyncVar]
    public bool PlayerTurnForLight;

    private float remainingTime;
    private float remainingTimeActionId;

    private List<int> PlayerHandTiles = new();
    private List<Block> PlayerCallBlocksList = new();
    private List<int>[] EnemyCallBlocksList = new List<int>[3];
    private int[] EnemyHandTilesCount = new int[3];
    private WinningCondition PlayerWinningCondition = new WinningCondition();
    private List<int> PlayerKawaTiles = new List<int>();
    private List<int>[] EnemyKawaTiles = new List<int>[3];
    private int[] EnemyIndexMap = new int[3];

    public GameObject actionSoundManager;    
    private ActionSoundManager soundManager;


    private static ServerManager serverManager;

    public GameObject[] TilePrefabArray;
    public GameObject TileBackPrefab;
    public GameObject EnemyHaipaiToi;
    public GameObject EnemyHaipaiKami;
    public GameObject EnemyHaipaiShimo;
    public GameObject[] EnemyHaipaiList;
    public GameObject PlayerHaipai;

    public GameObject PlayerKawa;
    public GameObject EnemyKawaToi;
    public GameObject EnemyKawaKami;
    public GameObject EnemyKawaShimo;
    public GameObject[] EnemyKawaList;

    public GameObject EnemyFuroFieldShimo;
    public GameObject EnemyFuroFieldToi;
    public GameObject EnemyFuroFieldKami;
    public GameObject PlayerFuroField;

    public GameObject PlayerFlowerField;
    public GameObject EnemyFlowerFieldShimo;
    public GameObject EnemyFlowerFieldToi;
    public GameObject EnemyFlowerFieldKami;

    public GameObject GameStatusUI;


    private int isValidDiscardResponse = -1; // Default is -1
    private bool isWaitingForResponse = false;

    private int tilesLeft;
    private int roundIndex;

    public GameObject huButtonPrefab;
    public GameObject skipButtonPrefab;
    public GameObject flowerButtonPrefab;
    public GameObject chiiButtonPrefab;
    public GameObject ponButtonPrefab;
    public GameObject kanButtonPrefab;
    public GameObject popupPrefab;

    private bool IsPopupConfirmed = false;
    private GameObject popupObject;
    private GameObject huButton;
    private GameObject skipButton;
    private GameObject flowerButton;
    private GameObject chiiButton;
    private GameObject ponButton;
    private GameObject kanButton;

    public GameObject[] discardHandPrefabs;  // Animator가 포함된 Prefab 배열
    private Animator animator;

    [Command]
    public void CmdSetPlayerName(string newName)
    {
        PlayerName = newName;
    }

    private void DestoryAllChildrenOfFuroFields()
    {
        DestroyAllChildren(PlayerFuroField);
        DestroyAllChildren(EnemyFuroFieldShimo);
        DestroyAllChildren(EnemyFuroFieldToi);
        DestroyAllChildren(EnemyFuroFieldKami);
    }

    public static void DestroyAllChildren(GameObject parent)
    {
        if (parent == null)
        {
            Debug.LogError("부모 GameObject가 null입니다.");
            return;
        }

        // 부모 오브젝트의 Transform을 기준으로 모든 자식 오브젝트 순회
        foreach (Transform child in parent.transform)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }

        Debug.Log($"{parent.name}의 모든 자식 오브젝트가 삭제되었습니다.");
    }

    private PlayerManager GetOwnedPlayerManager()
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return null;
        }

        Debug.Log($"[RpcDisplayEnemyTsumoTile] Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager.isOwned)
            {
                return playerManager;
            }
        }
        return null;
    }

    [TargetRpc]
    public void TargetClearButtons(NetworkConnection conn)
    {
        PlayerManager playerManager = GetOwnedPlayerManager();
        if (playerManager == null)
        {
            return;
        }
        DeleteButtons();
        DestroyAdditionalChoices();
    }

    public void ClearButtonsAndDoCallAction(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, KanType kanType)
    {
        if (PlayerIndex != playerIndex)
        {
            DeleteButtons();
            DestroyAdditionalChoices();

            // 완료 응답 보내기
            CmdNotifyActionCompleted();
            return;
        }

        // 액션 실행
        ExecuteAction(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, kanType);

        // 액션 실행 완료 후 서버에 응답
        CmdNotifyActionCompleted();
    }


    [TargetRpc]
    public void TargetClearButtonsAndDoCallAction(NetworkConnection conn, ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, KanType kanType)
    {
        PlayerManager playerManager = GetOwnedPlayerManager();

        if(playerManager != null)
        {
            playerManager.ClearButtonsAndDoCallAction(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, kanType);
        }
    }

    [Command]
   public void CmdNotifyActionCompleted()
    {
        serverManager.NotifyActionCompleted();
    }

    [Command]
    public void CmdPerformKan(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, KanType kanType)
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("[CmdPerformKan] No PlayerManager instances found in the scene.");
            return;
        }
        foreach (var pm in allPlayerManagers)
        {
            if (kanType == KanType.ANKAN && pm.PlayerIndex != playerIndex)
            {
                ActionPriorityInfo hideTileIdAction = new ActionPriorityInfo(action.Type, action.Priority, -1);
                TargetPerformKan(pm.connectionToClient, hideTileIdAction, -1, playerIndex, sourcePlayerIndex, isDiscarded, kanType);
            }
            else
            {
                TargetPerformKan(pm.connectionToClient, action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, kanType);
            }
        }
        
    }

    [TargetRpc]
    public void TargetPerformKan(NetworkConnection conn, ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, KanType kanType)
    {
        // 현재 씬에서 모든 PlayerManager 객체를 찾음
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        Debug.Log($"[RpcPerformKan] Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager == null)
                continue;
            if (playerManager.isOwned)
            {
                playerManager.PerformKan(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, kanType);
                return;
            }
        }
    }

    public void PerformKan(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, KanType kanType)
    {
        soundManager.PlayActionSound("kan");
        if (playerIndex == PlayerIndex)
        {
            animator = discardHandPrefabs[3].GetComponent<Animator>();
            animator.SetTrigger("101FuroTrigger");
            if (kanType == KanType.DAIMINKAN)
            {
                PerformDaiminKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, PlayerFuroField, PlayerHaipai, GetKawaByIndex(sourcePlayerIndex));
            }
            else if (kanType == KanType.ANKAN)
            {
                PerformAnKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, PlayerFuroField, PlayerHaipai);
            }
        }
        else
        {
            int relativeIndex = GetRelativeIndex(playerIndex);
            animator = discardHandPrefabs[relativeIndex].GetComponent<Animator>();
            if (relativeIndex == 0)
            {
                animator.SetTrigger("201FuroTrigger");
                if (kanType == KanType.DAIMINKAN)
                {
                    PerformDaiminKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldShimo, EnemyHaipaiShimo, GetKawaByIndex(sourcePlayerIndex));
                }
                else if (kanType == KanType.ANKAN)
                {
                    PerformAnKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldShimo, EnemyHaipaiShimo);
                }
            }
            else if (relativeIndex == 1)
            {
                animator.SetTrigger("301FuroTrigger");

                if (kanType == KanType.DAIMINKAN)
                {
                    PerformDaiminKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldToi, EnemyHaipaiToi, GetKawaByIndex(sourcePlayerIndex));
                }
                else if (kanType == KanType.ANKAN)
                {
                    PerformAnKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldToi, EnemyHaipaiToi);
                }
            }
            else if (relativeIndex == 2)
            {
                animator.SetTrigger("401FuroTrigger");

                if (kanType == KanType.DAIMINKAN)
                {
                    PerformDaiminKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldKami, EnemyHaipaiKami, GetKawaByIndex(sourcePlayerIndex));
                }
                else if (kanType == KanType.ANKAN)
                {
                    PerformAnKanSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldKami, EnemyHaipaiKami);
                }
            }
        }
    }

    private void PerformAnKanSub(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, GameObject FuroField, GameObject HaiPaiField)
    {
        // 기본 타일 크기 및 간격 설정
        float tileWidth = 50;
        float tileHeight = 75;
        float spacing = 0f; // 타일 간격 추가

        float groupSpacing = 10f;

        //GameObject FuroField = PlayerFuroField;

        // FuroField 확인
        if (FuroField == null)
        {
            Debug.LogError("FuroField not found in the scene.");
            return;
        }

        // 그룹 생성 및 부모 설정
        GameObject tileGroup = new GameObject("TileGroup", typeof(RectTransform));
        tileGroup.transform.SetParent(FuroField.transform, false);
        RectTransform tileGroupRect = tileGroup.GetComponent<RectTransform>();

        // 타일 묶음 크기 설정
        tileGroupRect.sizeDelta = new Vector2(tileWidth * 4 + spacing * 3, tileHeight);
        tileGroupRect.anchorMin = new Vector2(0, 0.5f);  // 왼쪽 정렬
        tileGroupRect.anchorMax = new Vector2(0, 0.5f);
        tileGroupRect.pivot = new Vector2(0, 0.5f);

        // 기존 자식 오브젝트 개수 확인
        int childCount = FuroField.transform.childCount;

        DebugFuroFieldChildren(FuroField.transform);


        // 마지막 자식의 위치를 기반으로 위치 조정
        if (childCount > 1)
        {
            Transform lastChild = FuroField.transform.GetChild(childCount - 2);
            RectTransform lastChildRect = lastChild.GetComponent<RectTransform>();

            // 디버깅: 마지막 자식의 위치와 크기 출력
            Debug.Log($"[PerformPonSub] Last Child Position: {lastChildRect.anchoredPosition.x}, Size: {lastChildRect.sizeDelta.x}");

            // 새 그룹을 마지막 그룹의 왼쪽으로 한 칸 더 떨어져 위치시키기
            float newX = lastChildRect.anchoredPosition.x - (lastChildRect.sizeDelta.x + groupSpacing + tileWidth + spacing);

            tileGroupRect.anchoredPosition = new Vector2(newX, 0);

            // 디버깅: 새 그룹 위치 출력
            Debug.Log($"[PerformPonSub] New TileGroup Position: {tileGroupRect.anchoredPosition.x}");
        }
        else
        {
            // 첫 번째 그룹일 경우, FuroField의 왼쪽 끝에 배치
            tileGroupRect.anchoredPosition = new Vector2(-tileGroupRect.sizeDelta.x, 0);
            Debug.Log("[PerformPonSub] First TileGroup positioned at (0,0)");
        }


        Debug.Log($"[PerformPonSub] PlayerIndex: {PlayerIndex}, playerIndex: {playerIndex}, sourcePlayerIndex: {sourcePlayerIndex}, source tile: {sourceTileId}");

        float tile_space = 0;
        TileGrid haipaiTileGrid = HaiPaiField.GetComponent<TileGrid>();



        //int relativeIndexOfSource = GetRelativeIndex(sourcePlayerIndex);

        bool DidTsumoGiriFlag = false;
        // 나머지 타일 추가
        for (int i = 0; i < 4; i++)
        {
            GameObject tile;
            if (action.TileId < 0)
            {
                tile = Instantiate(TileBackPrefab, tileGroup.transform);
                tile.name = "TileBack";
            }
            else
            {
                tile = Instantiate(TilePrefabArray[action.TileId], tileGroup.transform);
                tile.name = TileDictionary.NumToString[action.TileId];
            }
            if (haipaiTileGrid != null)
            {
                if (HaiPaiField == PlayerHaipai)
                {
                    haipaiTileGrid.DestoryByTileId(action.TileId);
                }
                else
                {
                    if (sourceTileId == -1)
                    {
                        if (!DidTsumoGiriFlag)
                        {
                            DidTsumoGiriFlag = true;
                            haipaiTileGrid.ShowTedashi(false);
                        }
                        else
                        {
                            //haipaiTileGrid.ShowTedashi(true);
                            haipaiTileGrid.DestoryLastTile();
                        }
                    }
                    else
                    {
                        //haipaiTileGrid.ShowTedashi(true);
                        haipaiTileGrid.DestoryLastTile();
                    }
                }
            }
            RectTransform tileRect = tile.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

            tileRect.anchoredPosition = new Vector2(tile_space, 0);
            tile_space += tileWidth + spacing;

            

            TileEvent tileEvent = tile.GetComponent<TileEvent>();
            if (tileEvent != null)
            {
                tileEvent.SetUndraggable();
            }
        }

    }
    private void PerformDaiminKanSub(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, GameObject FuroField, GameObject HaiPaiField, GameObject KawaField)
    {
        // 기본 타일 크기 및 간격 설정
        float tileWidth = 50;
        float tileHeight = 75;
        float spacing = 0f; // 타일 간격 추가

        float groupSpacing = 10f;

        //GameObject FuroField = PlayerFuroField;

        // FuroField 확인
        if (FuroField == null)
        {
            Debug.LogError("FuroField not found in the scene.");
            return;
        }

        if (KawaField == null)
        {
            Debug.LogError("KawaField not found in the scene.");
            return;
        }

        TileGrid tileGridKawa = KawaField.GetComponent<TileGrid>();
        if (tileGridKawa == null)
        {
            Debug.LogError("TileGrid is not in KawaField.");
            return;
        }

        tileGridKawa.DestoryLastKawaTile();

        // 그룹 생성 및 부모 설정
        GameObject tileGroup = new GameObject("TileGroup", typeof(RectTransform));
        tileGroup.transform.SetParent(FuroField.transform, false);
        RectTransform tileGroupRect = tileGroup.GetComponent<RectTransform>();

        // 타일 묶음 크기 설정
        tileGroupRect.sizeDelta = new Vector2(tileWidth * 3 + tileHeight + spacing * 2, tileHeight);
        tileGroupRect.anchorMin = new Vector2(0, 0.5f);  // 왼쪽 정렬
        tileGroupRect.anchorMax = new Vector2(0, 0.5f);
        tileGroupRect.pivot = new Vector2(0, 0.5f);

        // 기존 자식 오브젝트 개수 확인
        int childCount = FuroField.transform.childCount;

        DebugFuroFieldChildren(FuroField.transform);


        // 마지막 자식의 위치를 기반으로 위치 조정
        if (childCount > 1)
        {
            Transform lastChild = FuroField.transform.GetChild(childCount - 2);
            RectTransform lastChildRect = lastChild.GetComponent<RectTransform>();

            // 디버깅: 마지막 자식의 위치와 크기 출력
            Debug.Log($"[PerformDaiminKanSub] Last Child Position: {lastChildRect.anchoredPosition.x}, Size: {lastChildRect.sizeDelta.x}");

            // 새 그룹을 마지막 그룹의 왼쪽으로 한 칸 더 떨어져 위치시키기
            float newX = lastChildRect.anchoredPosition.x - (lastChildRect.sizeDelta.x + groupSpacing + tileWidth + spacing);

            tileGroupRect.anchoredPosition = new Vector2(newX, 0);

            // 디버깅: 새 그룹 위치 출력
            Debug.Log($"[PerformDaiminKanSub] New TileGroup Position: {tileGroupRect.anchoredPosition.x}");
        }
        else
        {
            // 첫 번째 그룹일 경우, FuroField의 왼쪽 끝에 배치
            tileGroupRect.anchoredPosition = new Vector2(-tileGroupRect.sizeDelta.x, 0);
            Debug.Log("[PerformDaiminKanSub] First TileGroup positioned at (0,0)");
        }


        Debug.Log($"[PerformDaiminKanSub] PlayerIndex: {PlayerIndex}, playerIndex: {playerIndex}, sourcePlayerIndex: {sourcePlayerIndex}, source tile: {TileDictionary.NumToString[sourceTileId]}");

        float tile_space = 0;
        TileGrid haipaiTileGrid = HaiPaiField.GetComponent<TileGrid>();

        //int relativeIndexOfSource = GetRelativeIndex(sourcePlayerIndex);

        // 나머지 타일 추가
        for (int i = 0; i < 4; i++)
        {
            if (((int)action.Priority == 3 && i == 3) || ((int)action.Priority != 3 && i == (int)action.Priority - 1))
            {
                GameObject tile = Instantiate(TilePrefabArray[action.TileId], tileGroup.transform);
                tile.name = TileDictionary.NumToString[action.TileId];
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                tileRect.anchoredPosition = new Vector2(tile_space, -tileHeight); // 아래로 배치
                tile_space += tileHeight + spacing;
                tileRect.localRotation = Quaternion.Euler(0, 0, 90); // 눕히기

                TileEvent tileEvent = tile.GetComponent<TileEvent>();
                if (tileEvent != null)
                {
                    tileEvent.SetUndraggable();
                }
            }
            else
            {
                GameObject tile = Instantiate(TilePrefabArray[action.TileId], tileGroup.transform);
                tile.name = TileDictionary.NumToString[action.TileId];
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                tileRect.anchoredPosition = new Vector2(tile_space, 0);
                tile_space += tileWidth + spacing;

                if (haipaiTileGrid != null)
                {
                    if (HaiPaiField == PlayerHaipai)
                    {
                        haipaiTileGrid.DestoryByTileId(action.TileId);
                    }
                    else
                    {
                        haipaiTileGrid.DestoryLastTile();
                    }
                }

                TileEvent tileEvent = tile.GetComponent<TileEvent>();
                if (tileEvent != null)
                {
                    tileEvent.SetUndraggable();
                }
            }
        }
    }




    [Command]
    public void CmdPerformPon(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded)
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("[CmdPerformPon] No PlayerManager instances found in the scene.");
            return;
        }
        RpcPerformPon(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded);
    }

    [ClientRpc]
    public void RpcPerformPon(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded)
    {
        // 현재 씬에서 모든 PlayerManager 객체를 찾음
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        Debug.Log($"[RpcPerformPon] Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager == null)
                continue;
            if (playerManager.isOwned)
            {
                playerManager.PerformPon(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded);
                return;
            }
        }
    }

    private GameObject GetKawaByIndex(int sourcePlayerIndex)
    {
        if (PlayerIndex == sourcePlayerIndex)
        {
            return PlayerKawa;
        }
        else
        {
            int relativeIndex = GetRelativeIndex(sourcePlayerIndex);
            if (relativeIndex == 0)
            {
                return EnemyKawaShimo;
            }
            else if(relativeIndex == 1)
            {
                return EnemyKawaToi;
            }
            else if (relativeIndex == 2)
            {
                return EnemyKawaKami;
            }
            else
            {
                return null;
            }
        }
    }

    public void PerformPon(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded)
    {
        soundManager.PlayActionSound("pon");
        if (playerIndex == PlayerIndex)
        {
            animator = discardHandPrefabs[3].GetComponent<Animator>();
            animator.SetTrigger("101FuroTrigger");
            PerformPonSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, PlayerFuroField, PlayerHaipai, GetKawaByIndex(sourcePlayerIndex));
        }
        else
        {
            int relativeIndex = GetRelativeIndex(playerIndex);
            animator = discardHandPrefabs[relativeIndex].GetComponent<Animator>();
            if (relativeIndex == 0)
            {
                animator.SetTrigger("201FuroTrigger");

                PerformPonSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldShimo, EnemyHaipaiShimo, GetKawaByIndex(sourcePlayerIndex));
            }
            else if (relativeIndex == 1)
            {
                animator.SetTrigger("301FuroTrigger");

                PerformPonSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldToi, EnemyHaipaiToi, GetKawaByIndex(sourcePlayerIndex));
            }
            else if (relativeIndex == 2)
            {
                animator.SetTrigger("401FuroTrigger");

                PerformPonSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldKami, EnemyHaipaiKami, GetKawaByIndex(sourcePlayerIndex));
            }
        }
    }

    private void PerformPonSub(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, GameObject FuroField, GameObject HaiPaiField, GameObject KawaField)
    {
        // 기본 타일 크기 및 간격 설정
        float tileWidth = 50;
        float tileHeight = 75;
        float spacing = 0f; // 타일 간격 추가

        float groupSpacing = 10f;

        //GameObject FuroField = PlayerFuroField;

        // FuroField 확인
        if (FuroField == null)
        {
            Debug.LogError("FuroField not found in the scene.");
            return;
        }

        if (KawaField == null)
        {
            Debug.LogError("KawaField not found in the scene.");
            return;
        }

        TileGrid tileGridKawa = KawaField.GetComponent<TileGrid>();
        if (tileGridKawa == null)
        {
            Debug.LogError("TileGrid is not in KawaField.");
            return;
        }

        tileGridKawa.DestoryLastKawaTile();

        // 그룹 생성 및 부모 설정
        GameObject tileGroup = new GameObject("TileGroup", typeof(RectTransform));
        tileGroup.transform.SetParent(FuroField.transform, false);
        RectTransform tileGroupRect = tileGroup.GetComponent<RectTransform>();

        // 타일 묶음 크기 설정
        tileGroupRect.sizeDelta = new Vector2(tileWidth * 2 + tileHeight + spacing * 2, tileHeight);
        tileGroupRect.anchorMin = new Vector2(0, 0.5f);  // 왼쪽 정렬
        tileGroupRect.anchorMax = new Vector2(0, 0.5f);
        tileGroupRect.pivot = new Vector2(0, 0.5f);

        // 기존 자식 오브젝트 개수 확인
        int childCount = FuroField.transform.childCount;

        DebugFuroFieldChildren(FuroField.transform);


        // 마지막 자식의 위치를 기반으로 위치 조정
        if (childCount > 1)
        {
            Transform lastChild = FuroField.transform.GetChild(childCount - 2);
            RectTransform lastChildRect = lastChild.GetComponent<RectTransform>();

            // 디버깅: 마지막 자식의 위치와 크기 출력
            Debug.Log($"[PerformPonSub] Last Child Position: {lastChildRect.anchoredPosition.x}, Size: {lastChildRect.sizeDelta.x}");

            // 새 그룹을 마지막 그룹의 왼쪽으로 한 칸 더 떨어져 위치시키기
            float newX = lastChildRect.anchoredPosition.x - (lastChildRect.sizeDelta.x + groupSpacing + tileWidth + spacing);

            tileGroupRect.anchoredPosition = new Vector2(newX, 0);

            // 디버깅: 새 그룹 위치 출력
            Debug.Log($"[PerformPonSub] New TileGroup Position: {tileGroupRect.anchoredPosition.x}");
        }
        else
        {
            // 첫 번째 그룹일 경우, FuroField의 왼쪽 끝에 배치
            tileGroupRect.anchoredPosition = new Vector2(-tileGroupRect.sizeDelta.x, 0);
            Debug.Log("[PerformPonSub] First TileGroup positioned at (0,0)");
        }


        Debug.Log($"[PerformPonSub] PlayerIndex: {PlayerIndex}, playerIndex: {playerIndex}, sourcePlayerIndex: {sourcePlayerIndex}, source tile: {TileDictionary.NumToString[sourceTileId]}");

        float tile_space = 0;
        TileGrid haipaiTileGrid = HaiPaiField.GetComponent<TileGrid>();

        //int relativeIndexOfSource = GetRelativeIndex(sourcePlayerIndex);

        // 나머지 타일 추가
        for (int i = 0; i < 3; i++)
        {
            if (i == (int)action.Priority - 1)
            {
                GameObject tile = Instantiate(TilePrefabArray[action.TileId], tileGroup.transform);
                tile.name = TileDictionary.NumToString[action.TileId];
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                tileRect.anchoredPosition = new Vector2(tile_space, -tileHeight); // 아래로 배치
                tile_space += tileHeight + spacing;
                tileRect.localRotation = Quaternion.Euler(0, 0, 90); // 눕히기

                TileEvent tileEvent = tile.GetComponent<TileEvent>();
                if (tileEvent != null)
                {
                    tileEvent.SetUndraggable();
                }
            }
            else
            {
                GameObject tile = Instantiate(TilePrefabArray[action.TileId], tileGroup.transform);
                tile.name = TileDictionary.NumToString[action.TileId];
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                tileRect.anchoredPosition = new Vector2(tile_space, 0);
                tile_space += tileWidth + spacing;

                if (haipaiTileGrid != null)
                {
                    if (HaiPaiField == PlayerHaipai)
                    {
                        haipaiTileGrid.DestoryByTileId(action.TileId);
                    }
                    else
                    {
                        haipaiTileGrid.DestoryLastTile();
                    }
                }

                TileEvent tileEvent = tile.GetComponent<TileEvent>();
                if (tileEvent != null)
                {
                    tileEvent.SetUndraggable();
                }
            }
        }
    }



    [Command]
    public void CmdPerformChii(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded)
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("[CmdPerformChii] No PlayerManager instances found in the scene.");
            return;
        }
        RpcPerformChii(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded);
    }

    [ClientRpc]
    public void RpcPerformChii(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded)
    {
        // 현재 씬에서 모든 PlayerManager 객체를 찾음
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        Debug.Log($"[RpcPerformChii] Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager == null)
                continue;
            if (playerManager.isOwned)
            {
                playerManager.PerformChii(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded);
                return;
            }
        }

    }
    
    public void PerformChii(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded)
    {
        soundManager.PlayActionSound("chii");
        if (playerIndex == PlayerIndex)
        {
            animator = discardHandPrefabs[3].GetComponent<Animator>();
            animator.SetTrigger("101FuroTrigger");
            PerformChiiSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, PlayerFuroField, PlayerHaipai, GetKawaByIndex(sourcePlayerIndex));
        }
        else
        {
            int relativeIndex = GetRelativeIndex(playerIndex);
            animator = discardHandPrefabs[relativeIndex].GetComponent<Animator>();
            
            if (relativeIndex == 0)
            {
                PerformChiiSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldShimo, EnemyHaipaiShimo, GetKawaByIndex(sourcePlayerIndex));
                animator.SetTrigger("201FuroTrigger");
            }
            else if (relativeIndex == 1)
            {
                PerformChiiSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldToi, EnemyHaipaiToi, GetKawaByIndex(sourcePlayerIndex));
                animator.SetTrigger("301FuroTrigger");
            }
            else if (relativeIndex == 2)
            {
                PerformChiiSub(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, EnemyFuroFieldKami, EnemyHaipaiKami, GetKawaByIndex(sourcePlayerIndex));
                animator.SetTrigger("401FuroTrigger");
            }
        }
    }

    private void PerformChiiSub(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, GameObject FuroField, GameObject HaiPaiField, GameObject KawaField)
    {
        int relativeIndexOfSource = sourceTileId - action.TileId;

        // 기본 타일 크기 및 간격 설정
        float tileWidth = 50;
        float tileHeight = 75;
        float spacing = 0f; // 타일 간격 추가

        float groupSpacing = 10f;

        //GameObject FuroField = PlayerFuroField;

        // FuroField 확인
        if (FuroField == null)
        {
            Debug.LogError("FuroField not found in the scene.");
            return;
        }

        if (KawaField == null)
        {
            Debug.LogError("KawaField not found in the scene.");
            return;
        }

        TileGrid tileGridKawa = KawaField.GetComponent<TileGrid>();
        if (tileGridKawa == null)
        {
            Debug.LogError("TileGrid is not in KawaField.");
            return;
        }
        tileGridKawa.DestoryLastKawaTile();

        // 그룹 생성 및 부모 설정
        GameObject tileGroup = new GameObject("TileGroup", typeof(RectTransform));
        tileGroup.transform.SetParent(FuroField.transform, false);
        RectTransform tileGroupRect = tileGroup.GetComponent<RectTransform>();

        // 타일 묶음 크기 설정
        tileGroupRect.sizeDelta = new Vector2(tileWidth * 2 + tileHeight + spacing * 2, tileHeight);
        tileGroupRect.anchorMin = new Vector2(0, 0.5f);  // 왼쪽 정렬
        tileGroupRect.anchorMax = new Vector2(0, 0.5f);
        tileGroupRect.pivot = new Vector2(0, 0.5f);

        // 기존 자식 오브젝트 개수 확인
        int childCount = FuroField.transform.childCount;

        DebugFuroFieldChildren(FuroField.transform);


        // 마지막 자식의 위치를 기반으로 위치 조정
        if (childCount > 1)
        {
            Transform lastChild = FuroField.transform.GetChild(childCount - 2);
            RectTransform lastChildRect = lastChild.GetComponent<RectTransform>();

            // 디버깅: 마지막 자식의 위치와 크기 출력
            Debug.Log($"[PerformChii] Last Child Position: {lastChildRect.anchoredPosition.x}, Size: {lastChildRect.sizeDelta.x}");

            // 새 그룹을 마지막 그룹의 왼쪽으로 한 칸 더 떨어져 위치시키기
            float newX = lastChildRect.anchoredPosition.x - (lastChildRect.sizeDelta.x + groupSpacing + tileWidth + spacing);

            tileGroupRect.anchoredPosition = new Vector2(newX, 0);

            // 디버깅: 새 그룹 위치 출력
            Debug.Log($"[PerformChii] New TileGroup Position: {tileGroupRect.anchoredPosition.x}");
        }
        else
        {
            // 첫 번째 그룹일 경우, FuroField의 왼쪽 끝에 배치
            tileGroupRect.anchoredPosition = new Vector2(-tileGroupRect.sizeDelta.x, 0);
            Debug.Log("[PerformChii] First TileGroup positioned at (0,0)");
        }


        Debug.Log($"[PerformChii] PlayerIndex: {PlayerIndex}, playerIndex: {playerIndex}, sourcePlayerIndex: {sourcePlayerIndex}, relativeindex: {relativeIndexOfSource}, source tile: {TileDictionary.NumToString[sourceTileId]}");

        float tile_space = 0;
        TileGrid haipaiTileGrid = HaiPaiField.GetComponent<TileGrid>();

        // 눕혀진 타일(가져온 타일) 추가
        for (int i = 0; i < 3; i++)
        {
            if (i == relativeIndexOfSource)
            {
                GameObject tile = Instantiate(TilePrefabArray[action.TileId + i], tileGroup.transform);
                tile.name = TileDictionary.NumToString[action.TileId];
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                tileRect.anchoredPosition = new Vector2(tile_space, -tileHeight); // 아래로 배치
                tile_space += tileHeight + spacing;
                tileRect.localRotation = Quaternion.Euler(0, 0, 90); // 눕히기

                TileEvent tileEvent = tile.GetComponent<TileEvent>();
                if (tileEvent != null)
                {
                    tileEvent.SetUndraggable();
                }
            }
        }

        // 나머지 타일 추가
        for (int i = 0; i < 3; i++)
        {
            if (i == relativeIndexOfSource)
                continue;

            GameObject tile = Instantiate(TilePrefabArray[action.TileId + i], tileGroup.transform);
            tile.name = TileDictionary.NumToString[action.TileId];
            RectTransform tileRect = tile.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

            tileRect.anchoredPosition = new Vector2(tile_space, 0);
            tile_space += tileWidth + spacing;

            if (haipaiTileGrid != null)
            {
                if (HaiPaiField == PlayerHaipai)
                {
                    haipaiTileGrid.DestoryByTileId(action.TileId + i);
                }
                else
                {
                    haipaiTileGrid.DestoryLastTile();
                }
            }

            TileEvent tileEvent = tile.GetComponent<TileEvent>();
            if (tileEvent != null)
            {
                tileEvent.SetUndraggable();
            }
        }
    }

    // FuroField의 자식 오브젝트 목록 출력
    void DebugFuroFieldChildren(Transform furoFieldTransform)
    {
        int childCount = furoFieldTransform.childCount;

        if (childCount == 0)
        {
            Debug.Log("[DebugFuroFieldChildren] FuroField has no child objects.");
            return;
        }

        Debug.Log($"[DebugFuroFieldChildren] FuroField has {childCount} child objects:");

        for (int i = 0; i < childCount; i++)
        {
            Transform child = furoFieldTransform.GetChild(i);
            RectTransform childRect = child.GetComponent<RectTransform>();

            if (childRect != null)
            {
                Debug.Log($"[Child {i}] Name: {child.name}, Position: {childRect.anchoredPosition}, Size: {childRect.sizeDelta}");
            }
            else
            {
                Debug.Log($"[Child {i}] Name: {child.name}, Position: {child.position}, No RectTransform component.");
            }
        }
    }

    private void ExecuteAction(ActionPriorityInfo action, int sourceTileId, int playerIndex, int sourcePlayerIndex, bool isDiscarded, KanType kanType)
    {
        switch (action.Type)
        {
            case ActionType.KAN:
                CmdPerformKan(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded, kanType);
                break;
            case ActionType.PON:
                CmdPerformPon(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded);
                break;
            case ActionType.CHII:
                CmdPerformChii(action, sourceTileId, playerIndex, sourcePlayerIndex, isDiscarded);
                break;
        }

    }


    [TargetRpc]
    public void TargetShowActionButtons(NetworkConnection conn, int playerWindIndex, List<ActionPriorityInfo> actions, int actionTurnId, int tileId)
    {
        //DeleteButtons();
        StartCoroutine(MakeButtonsAndHandlePlayerDecision(playerWindIndex, actions, actionTurnId, tileId));

    }

    public void DisableButtons()
    {
        List<GameObject> allButtons = new List<GameObject> { huButton, skipButton, flowerButton, chiiButton, ponButton, kanButton };
        foreach (var button in allButtons)
        {
            if (button != null && button.activeSelf)
            {
                button.SetActive(false);
            }
        }

        Debug.Log("[DisableButtons] All active buttons have been disabled.");
    }

    public void EnableButtons()
    {
        List<GameObject> allButtons = new List<GameObject> { huButton, skipButton, flowerButton, chiiButton, ponButton, kanButton };
        foreach (var button in allButtons)
        {
            if (button != null && !button.activeSelf)
            {
                button.SetActive(true);
            }
        }

        Debug.Log("[EnableButtons] All valid buttons have been enabled.");
    }

    public void DeleteButtons()
    {
        //remainingTime = -1;
        huButton = DeleteButton(huButton);
        skipButton = DeleteButton(skipButton);
        flowerButton = DeleteButton(flowerButton);
        chiiButton = DeleteButton(chiiButton);
        ponButton = DeleteButton(ponButton);
        kanButton = DeleteButton(kanButton);
        DestroyAdditionalChoices();
    }

    private GameObject DeleteButton(GameObject button)
    {
        if (button != null)
        {
            Destroy(button);
            button = null;
        }
        return button;
    }


    private void ShowHand(GameObject handField, HandData originalHand)
    {
        if (handField == null)
        {
            Debug.LogError("[ShowHand] HandField not found.");
            return;
        }
        HandData hand = originalHand.DeepCopy();
        hand.PrintHandNames();
        //return;
        // 기본 타일 크기 및 간격 설정
        float tileWidth = 50f;
        float tileHeight = 75f;
        float spacing = 0f;  // 타일 간격
        float groupSpacing = 20f;  // 블록 간격

        // HandField 초기화 (기존 타일 삭제)
        foreach (Transform child in handField.transform)
        {
            Destroy(child.gameObject);
        }
        //hand.ClosedTiles[hand.WinningTile] -= 1;

        float currentXPosition = 0f;
        foreach (BlockData block in hand.CallBlocks)
        {
            if (block.Type == (int)BlockType.SINGLETILE)
            {
                hand.ClosedTiles[block.Tile] += 1;
            }
            else if (block.Type == (int)BlockType.PAIR)
            {
                hand.ClosedTiles[block.Tile] += 2;
            }
            else if (block.Type == (int)BlockType.KNITTED)
            {
                hand.ClosedTiles[block.Tile] += 1;
                hand.ClosedTiles[block.Tile + 3] += 1;
                hand.ClosedTiles[block.Tile + 6] += 1;
            }
        }

        // 1. ClosedTiles 나열
        for (int i = 0; i < hand.ClosedTiles.Length; i++)
        {
            int count = hand.ClosedTiles[i];
            for (int j = 0; j < count; j++)
            {
                GameObject tilePrefab = TilePrefabArray[i];
                if (tilePrefab != null)
                {
                    GameObject tile = Instantiate(tilePrefab, handField.transform);
                    tile.name = TileDictionary.NumToString[i];

                    RectTransform tileRect = tile.GetComponent<RectTransform>();
                    tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);
                    tileRect.anchoredPosition = new Vector2(currentXPosition, 0);

                    currentXPosition += tileWidth + spacing;

                    TileEvent tileEvent = tile.GetComponent<TileEvent>();
                    if (tileEvent != null)
                    {
                        tileEvent.SetUndraggable();
                    }
                }
            }
        }
        currentXPosition += groupSpacing;
        // 2. CallBlocks 나열
        for (int idx = 0; idx < hand.CallBlockCount; idx++) 
        {
            BlockData block = hand.CallBlocks[idx];
            if (block.Type == (int)BlockType.SINGLETILE || block.Type == (int)BlockType.PAIR || block.Type == (int)BlockType.KNITTED)
            {
                continue;
            }
            // 블록 그룹 생성
            GameObject blockGroup = new GameObject("BlockGroup", typeof(RectTransform));
            blockGroup.transform.SetParent(handField.transform, false);
            RectTransform blockGroupRect = blockGroup.GetComponent<RectTransform>();
            blockGroupRect.anchorMin = new Vector2(0, 0.5f);  // 왼쪽 정렬
            blockGroupRect.anchorMax = new Vector2(0, 0.5f);
            blockGroupRect.pivot = new Vector2(0, 0.5f);

            int blockTileCount = GetBlockTileCount((BlockType)block.Type);
            blockGroupRect.anchoredPosition = new Vector2(currentXPosition, 0);
            if (block.Type == (int)BlockType.SEQUENCE)
            {
                blockGroupRect.sizeDelta = new Vector2((tileWidth + spacing) * 2 + tileHeight, tileHeight);
                float offsetx = 0f;
                for (int i = 0; i < 3; i++)
                {
                    if (i == block.SourceTileIndex)
                    {
                        GameObject tilePrefab = TilePrefabArray[block.Tile + i];
                        if (tilePrefab != null)
                        {

                            GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                            tile.name = TileDictionary.NumToString[block.Tile];

                            RectTransform tileRect = tile.GetComponent<RectTransform>();
                            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);
                            tileRect.anchoredPosition = new Vector2(offsetx, -tileHeight);
                            offsetx += tileHeight + spacing;
                            tileRect.localRotation = Quaternion.Euler(0, 0, 90);

                            TileEvent tileEvent = tile.GetComponent<TileEvent>();
                            if (tileEvent != null)
                            {
                                tileEvent.SetUndraggable();
                            }
                        }
                        break;
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    if (i != block.SourceTileIndex)
                    {
                        GameObject tilePrefab = TilePrefabArray[block.Tile + i];
                        if (tilePrefab != null)
                        {

                            GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                            tile.name = TileDictionary.NumToString[block.Tile];

                            RectTransform tileRect = tile.GetComponent<RectTransform>();
                            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);
                            tileRect.anchoredPosition = new Vector2(offsetx, 0);
                            offsetx += tileWidth + spacing;


                            TileEvent tileEvent = tile.GetComponent<TileEvent>();
                            if (tileEvent != null)
                            {
                                tileEvent.SetUndraggable();
                            }
                        }
                    }
                }
            }
            else if (block.Type == (int)BlockType.TRIPLET)
            {
                blockGroupRect.sizeDelta = new Vector2((tileWidth + spacing) * 2 + tileHeight, tileHeight);
                float tile_space = 0;
                // 나머지 타일 추가
                for (int i = 0; i < 3; i++)
                {
                    GameObject tilePrefab = TilePrefabArray[block.Tile];
                    if (i == 3 - (int)block.Source)
                    {
                        GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                        tile.name = TileDictionary.NumToString[block.Tile];
                        RectTransform tileRect = tile.GetComponent<RectTransform>();
                        tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                        tileRect.anchoredPosition = new Vector2(tile_space, -tileHeight); // 아래로 배치
                        tile_space += tileHeight + spacing;
                        tileRect.localRotation = Quaternion.Euler(0, 0, 90); // 눕히기

                        TileEvent tileEvent = tile.GetComponent<TileEvent>();
                        if (tileEvent != null)
                        {
                            tileEvent.SetUndraggable();
                        }
                    }
                    else
                    {
                        GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                        tile.name = TileDictionary.NumToString[block.Tile];
                        RectTransform tileRect = tile.GetComponent<RectTransform>();
                        tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                        tileRect.anchoredPosition = new Vector2(tile_space, 0);
                        tile_space += tileWidth + spacing;


                        TileEvent tileEvent = tile.GetComponent<TileEvent>();
                        if (tileEvent != null)
                        {
                            tileEvent.SetUndraggable();
                        }
                    }
                }
            }
            else if (block.Type == (int)BlockType.QUAD)
            {
                if (block.Source == (int)BlockSource.SELF)
                {
                    blockGroupRect.sizeDelta = new Vector2(tileWidth * 4 + spacing * 3, tileHeight);
                    float tile_space = 0;
                    GameObject tilePrefab = TilePrefabArray[block.Tile];
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                        tile.name = TileDictionary.NumToString[block.Tile];
                        RectTransform tileRect = tile.GetComponent<RectTransform>();
                        tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                        tileRect.anchoredPosition = new Vector2(tile_space, 0);
                        tile_space += tileWidth + spacing;

                        TileEvent tileEvent = tile.GetComponent<TileEvent>();
                        if (tileEvent != null)
                        {
                            tileEvent.SetUndraggable();
                        }
                    }
                }
                else
                {
                    blockGroupRect.sizeDelta = new Vector2((tileWidth + spacing) * 3 + tileHeight, tileHeight);
                    float tile_space = 0;
                    GameObject tilePrefab = TilePrefabArray[block.Tile];
                    for (int i = 0; i < 4; i++)
                    {
                        if (((int)block.Source == 3 && i == 3) || ((int)block.Source != 3 && i == (int)block.Source - 1))
                        {
                            GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                            tile.name = TileDictionary.NumToString[block.Tile];
                            RectTransform tileRect = tile.GetComponent<RectTransform>();
                            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                            tileRect.anchoredPosition = new Vector2(tile_space, -tileHeight); // 아래로 배치
                            tile_space += tileHeight + spacing;
                            tileRect.localRotation = Quaternion.Euler(0, 0, 90); // 눕히기

                            TileEvent tileEvent = tile.GetComponent<TileEvent>();
                            if (tileEvent != null)
                            {
                                tileEvent.SetUndraggable();
                            }
                        }
                        else
                        {
                            GameObject tile = Instantiate(tilePrefab, blockGroup.transform);
                            tile.name = TileDictionary.NumToString[block.Tile];
                            RectTransform tileRect = tile.GetComponent<RectTransform>();
                            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);

                            tileRect.anchoredPosition = new Vector2(tile_space, 0);
                            tile_space += tileWidth + spacing;

                            TileEvent tileEvent = tile.GetComponent<TileEvent>();
                            if (tileEvent != null)
                            {
                                tileEvent.SetUndraggable();
                            }
                        }
                    }
                }
            }

            currentXPosition += blockGroupRect.sizeDelta.x + groupSpacing;
        }

        // 3. 쯔모패 표시
        if (true)
        {
            currentXPosition += groupSpacing;
            GameObject tile = Instantiate(TilePrefabArray[hand.WinningTile], handField.transform);
            tile.name = TileDictionary.NumToString[hand.WinningTile];


            RectTransform tileRect = tile.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);
            tileRect.anchoredPosition = new Vector2(currentXPosition, 0);



            TileEvent tileEvent = tile.GetComponent<TileEvent>();
            if (tileEvent != null)
            {
                tileEvent.SetUndraggable();
            }
        }
    }

    // BlockType에 따른 타일 개수 반환
    private int GetBlockTileCount(BlockType type)
    {
        return type switch
        {
            BlockType.SEQUENCE => 3,
            BlockType.TRIPLET => 3,
            BlockType.QUAD => 4,
            BlockType.PAIR => 2,
            BlockType.KNITTED => 3,
            _ => 1
        };
    }


    public GameObject[] FuEffectAnimatorPrefabs;

    public IEnumerator PlayFuEffectCoroutine(PlayerManager HuPlayerManager, HandData hand, int score)
    {
        soundManager.PlayActionSound("hu");

        Debug.Log($"[PlayFuEffectCoroutine] 후 효과 시작 - 플레이어: {HuPlayerManager.PlayerName}, 점수: {score}");

        if (HuPlayerManager.PlayerIndex != PlayerIndex)
        {
            int relativeIndex = GetRelativeIndex(HuPlayerManager.PlayerIndex);
            Debug.Log($"[PlayFuEffectCoroutine] 상대방 플레이어 상대 인덱스: {relativeIndex}");

            if (relativeIndex >= 0)
            {
                TileGrid tileGrid = EnemyHaipaiList[relativeIndex].GetComponent<TileGrid>();
                hand.ClosedTiles[hand.WinningTile] -= 1;
                Debug.Log($"[PlayFuEffectCoroutine] 상대방의 패 초기화 및 승리 타일 제거. 승리 타일: {hand.WinningTile}");

                if (tileGrid != null)
                {
                    tileGrid.EmptyAll();
                    Debug.Log($"[PlayFuEffectCoroutine] 상대방 타일 그리드 초기화 완료.");

                    for (int i = 0; i < hand.ClosedTiles.Length; ++i)
                    {
                        for (int j = 0; j < hand.ClosedTiles[i]; ++j)
                        {
                            GameObject spawnTile = Instantiate(TilePrefabArray[i], EnemyHaipaiList[relativeIndex].transform);
                            tileGrid.AddTileToLastIndex(spawnTile);
                        }
                    }
                    Debug.Log($"[PlayFuEffectCoroutine] 상대방 손패 다시 그리기 완료.");

                    tileGrid.ShowTsumoTile(Instantiate(TilePrefabArray[hand.WinningTile], EnemyHaipaiList[relativeIndex].transform));
                    Debug.Log($"[PlayFuEffectCoroutine] 승리 타일 표시 완료. 타일 ID: {hand.WinningTile}");
                }
                else
                {
                    Debug.LogWarning($"[PlayFuEffectCoroutine] TileGrid를 찾지 못했습니다. 상대 인덱스: {relativeIndex}");
                }
            }
        }
        AudioSource audioSource;
        // Fu Effect 애니메이션 실행 및 대기
        if (score >= 32)
        {
            Debug.Log($"[PlayFuEffectCoroutine] 고득점 효과 실행 (32점 이상).");
            RestoreTransparency(FuEffectAnimatorPrefabs[1]);
            animator = FuEffectAnimatorPrefabs[1].GetComponent<Animator>();
            audioSource = FuEffectAnimatorPrefabs[1].GetComponent<AudioSource>();
            StartCoroutine(PlayAudioSource(audioSource));
            animator.SetTrigger("DdingTrigger");
            yield return StartCoroutine(DeactivateEffectAfterDelay(1.557f, FuEffectAnimatorPrefabs[1]));
            Debug.Log($"[PlayFuEffectCoroutine] 고득점 효과 종료.");
        }
        else if (score >= 16)
        {
            Debug.Log($"[PlayFuEffectCoroutine] 중간 점수 효과 실행 (16~31점).");
            RestoreTransparency(FuEffectAnimatorPrefabs[0]);
            animator = FuEffectAnimatorPrefabs[0].GetComponent<Animator>();
            audioSource = FuEffectAnimatorPrefabs[0].GetComponent<AudioSource>();
            StartCoroutine(PlayAudioSource(audioSource));
            animator.SetTrigger("FakerTrigger");
            yield return StartCoroutine(DeactivateEffectAfterDelay(1.557f, FuEffectAnimatorPrefabs[0]));
            Debug.Log($"[PlayFuEffectCoroutine] 중간 점수 효과 종료.");
        }
        else
        {
            Debug.Log($"[PlayFuEffectCoroutine] 점수가 낮아 Hu 효과를 실행하지 않습니다. (점수: {score})");
        }

        // 추가 대기 시간
        yield return new WaitForSeconds(3f);
        Debug.Log($"[PlayFuEffectCoroutine] Hu 효과 전체 완료.");
    }

    private IEnumerator PlayAudioSource(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[PlayAudioSource] AudioSource가 비어있습니다.");
            yield break;
        }

        audioSource.Play();  // 오디오 재생
        Debug.Log("[PlayAudioSource] 오디오 재생 시작");

        // 오디오가 재생 중일 동안 대기
        while (audioSource.isPlaying)
        {
            yield return null;  // 매 프레임 대기
        }

        Debug.Log("[PlayAudioSource] 오디오 재생 완료");
    }


    /// <summary>
    /// 오브젝트를 즉시 투명하게 만드는 함수
    /// </summary>
    private IEnumerator DeactivateEffectAfterDelay(float delay, GameObject effectPrefab)
    {
        Debug.Log($"[DeactivateEffectAfterDelay] {delay}초 후 Hu 효과 즉시 투명화 예정.");

        // Renderer 가져오기
        Renderer renderer = effectPrefab.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("[DeactivateEffectAfterDelay] Renderer를 찾을 수 없습니다.");
            yield break;
        }

        // delay 후 투명화
        yield return new WaitForSeconds(delay);

        // 즉시 알파값을 0으로 설정 (완전 투명)
        Color color = renderer.material.color;
        color.a = 0f;  // 투명하게 설정
        renderer.material.color = color;

        Debug.Log("[DeactivateEffectAfterDelay] Hu 효과 즉시 투명화 완료.");
    }

    /// <summary>
    /// 오브젝트의 투명도를 완전히 불투명(알파값 1)으로 복구하는 함수
    /// </summary>
    private void RestoreTransparency(GameObject effectPrefab)
    {
        Renderer renderer = effectPrefab.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("[RestoreTransparency] Renderer를 찾을 수 없습니다.");
            return;
        }

        // 알파값을 1로 설정 (완전 불투명)
        Color color = renderer.material.color;
        color.a = 1f;  // 불투명하게 설정
        renderer.material.color = color;

        Debug.Log("[RestoreTransparency] 투명도 원상복구 완료.");
    }




    public IEnumerator TargetShowRoundScoreSub(NetworkConnection target, PlayerManager HuPlayerManager, List<YakuScoreData> huYakuScoreArray, int totalScore, HandData hand)
    {
        yield return StartCoroutine(PlayFuEffectCoroutine(HuPlayerManager, hand, totalScore));  // Wait for the Fu effect to complete

        // Main Canvas 찾기
        GameObject mainCanvas = GameObject.Find("Main Canvas");
        if (mainCanvas == null)
        {
            Debug.LogError("[TargetShowRoundScore] Main Canvas not found.");
            yield break;
        }

        // 팝업 UI 생성
        popupObject = Instantiate(popupPrefab, mainCanvas.transform);
        RectTransform popupRect = popupObject.GetComponent<RectTransform>();

        TMP_Text scoreText = popupObject.transform.Find("Canvas/ScoreListText").GetComponent<TMP_Text>();
        TMP_Text totalScoreText = popupObject.transform.Find("Canvas/TotalScoreText").GetComponent<TMP_Text>();
        TMP_Text nameText = popupObject.transform.Find("Canvas/NameText").GetComponent<TMP_Text>();
        GameObject HandField = popupObject.transform.Find("Canvas/HandField").gameObject;
        ShowHand(HandField, hand);

        if (scoreText == null || totalScoreText == null || nameText == null)
        {
            Debug.LogError("[TargetShowRoundScore] ScoreListText or TotalScoreText not found in popup prefab.");
            yield break;
        }

        nameText.text = HuPlayerManager.PlayerName;

        // 점수 데이터 표시
        int maxRows = huYakuScoreArray.Count >= 8 ? 8 : 5;
        int totalColumns = Mathf.CeilToInt((float)huYakuScoreArray.Count / maxRows);
        string[,] table = new string[maxRows, totalColumns];

        for (int i = 0; i < huYakuScoreArray.Count; i++)
        {
            int row = i % maxRows;
            int column = i / maxRows;
            table[row, column] = $"{YakuDictionary.dict[huYakuScoreArray[i].YakuId]} : {huYakuScoreArray[i].Score}";
        }

        string scoreDisplay = "";
        for (int row = 0; row < maxRows; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                if (!string.IsNullOrEmpty(table[row, column]))
                {
                    scoreDisplay += table[row, column].PadRight(50);
                }
            }
            scoreDisplay += "\n";
        }

        scoreText.text = scoreDisplay;
        scoreText.fontSize = huYakuScoreArray.Count >= 8 ? 32 : (huYakuScoreArray.Count >= 5 ? 40 : 48);
        totalScoreText.text = $"{totalScore} Points";

        // 총 점수 색상 설정
        if (totalScore < 16)
        {
            totalScoreText.color = Color.black;
        }
        else if (totalScore < 32)
        {
            totalScoreText.color = Color.blue;
        }
        else if (totalScore < 48)
        {
            totalScoreText.color = Color.red;
        }
        else
        {
            totalScoreText.color = new Color(1.0f, 0.84f, 0.0f); // Gold
        }

        // Material 색상 덮어쓰기
        Material material = totalScoreText.fontMaterial;
        material.SetColor(ShaderUtilities.ID_FaceColor, totalScoreText.color);
        totalScoreText.fontMaterial = material;

        // 확인 버튼 처리
        Button confirmButton = popupObject.transform.Find("Canvas/ConfirmButton").GetComponent<Button>();
        if (confirmButton == null)
        {
            Debug.LogError("[TargetShowRoundScore] ConfirmButton not found in popup prefab.");
            yield break;
        }
        DeleteButtons();
        IsPopupConfirmed = false;
        confirmButton.onClick.AddListener(() =>
        {
            IsPopupConfirmed = true;
        });

        // 10초 대기 또는 확인 버튼 클릭 대기
        yield return StartCoroutine(WaitForPopupConfirmation(10f));
    }



    [TargetRpc]
    public void TargetShowRoundScore(NetworkConnection target, PlayerManager HuPlayerManager, List<YakuScoreData> huYakuScoreArray, int totalScore, HandData hand)
    {

        StartCoroutine(TargetShowRoundScoreSub(target, HuPlayerManager, huYakuScoreArray, totalScore, hand));
        /*
        PlayFuEffectAndWait(HuPlayerManager, hand, totalScore);
        //DeleteButtons();
        // Main Canvas 찾기
        GameObject mainCanvas = GameObject.Find("Main Canvas");
        if (mainCanvas == null)
        {
            Debug.LogError("[TargetShowRoundScore] Main Canvas not found.");
            return;
        }


        // 팝업 UI 생성
        popupObject = Instantiate(popupPrefab, mainCanvas.transform); // Main Canvas에 추가
        RectTransform popupRect = popupObject.GetComponent<RectTransform>();

        TMP_Text scoreText = popupObject.transform.Find("Canvas/ScoreListText").GetComponent<TMP_Text>();
        TMP_Text totalScoreText = popupObject.transform.Find("Canvas/TotalScoreText").GetComponent<TMP_Text>();
        TMP_Text nameText = popupObject.transform.Find("Canvas/NameText").GetComponent<TMP_Text>();
        GameObject HandField = popupObject.transform.Find("Canvas/HandField").gameObject;
        ShowHand(HandField, hand);


        if (scoreText == null || totalScoreText == null || nameText == null)
        {
            Debug.LogError("[TargetShowRoundScore] ScoreListText or TotalScoreText not found in popup prefab.");
            return;
        }

        nameText.text = HuPlayerManager.PlayerName;

        // 점수 데이터 표시
        int maxRows = huYakuScoreArray.Count >= 8 ? 8 : 5;
        int totalColumns = Mathf.CeilToInt((float)huYakuScoreArray.Count / maxRows);
        string[,] table = new string[maxRows, totalColumns];

        for (int i = 0; i < huYakuScoreArray.Count; i++)
        {
            int row = i % maxRows;
            int column = i / maxRows;
            table[row, column] = $"{YakuDictionary.dict[huYakuScoreArray[i].YakuId]} : {huYakuScoreArray[i].Score}";
        }

        string scoreDisplay = "";
        for (int row = 0; row < maxRows; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                if (!string.IsNullOrEmpty(table[row, column]))
                {
                    scoreDisplay += table[row, column].PadRight(50);
                }
            }
            scoreDisplay += "\n";
        }

        scoreText.text = scoreDisplay;
        scoreText.fontSize = huYakuScoreArray.Count >= 8 ? 32 : (huYakuScoreArray.Count >= 5 ? 40 : 48);
        totalScoreText.text = $"{totalScore} Points";

        // 총 점수 표시 및 색상 설정
        totalScoreText.text = $"{totalScore} Points";
        if (totalScore < 16)
        {
            totalScoreText.color = Color.black;
        }
        else if (totalScore < 32)
        {
            totalScoreText.color = Color.blue;
        }
        else if (totalScore < 48)
        {
            totalScoreText.color = Color.red;
        }
        else
        {
            totalScoreText.color = new Color(1.0f, 0.84f, 0.0f); // Gold 색상
        }

        // Material 색상 덮어쓰기
        Material material = totalScoreText.fontMaterial;
        material.SetColor(ShaderUtilities.ID_FaceColor, totalScoreText.color);
        totalScoreText.fontMaterial = material;
        // 확인 버튼 처리
        Button confirmButton = popupObject.transform.Find("Canvas/ConfirmButton").GetComponent<Button>();
        if (confirmButton == null)
        {
            Debug.LogError("[TargetShowRoundScore] ConfirmButton not found in popup prefab.");
            return;
        }
        DeleteButtons();
        IsPopupConfirmed = false;
        confirmButton.onClick.AddListener(() =>
        {
            IsPopupConfirmed = true;
            //Destroy(popup); // 팝업 제거
        });

        // 10초 대기 또는 확인 버튼 클릭 대기
        StartCoroutine(WaitForPopupConfirmation(10f));
        */
    }

    [TargetRpc]
    public void TargetDeletePopup(NetworkConnection conn)
    {
        if (!!popupObject)
        {
            Destroy(popupObject);
            popupObject = null;
        }
    }

    [TargetRpc]
    public void TargetSetRemainingTimeZero(NetworkConnection conn)
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager == null)
                continue;
            if (playerManager.isOwned)
            {
                playerManager.setRemainingTimeZero();
                return;
            }
        }
    }

    public void setRemainingTimeZero()
    {
        remainingTime = -1f;
    }

    public float getRemainingTime()
    {
        return remainingTime;
    }

    private IEnumerator WaitForPopupConfirmation(float popupWaitTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < popupWaitTime && !IsPopupConfirmed)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //if (!IsPopupConfirmed)
        //{
            //Destroy(popup); // 팝업 제거
        //}

        CmdNotifyServerPopupComplete(); // 서버에 팝업 완료 알림
    }

    [Command]
    private void CmdNotifyServerPopupComplete()
    {
        if (serverManager == null)
        {
            Debug.LogError("[CmdNotifyServerPopupComplete] ServerManager component not found.");
            return;
        }

        serverManager.OnClientPopupComplete();
    }


    private void OnPlayerStatusChanged(PlayerStatus oldStatus, PlayerStatus newStatus)
    {
        Debug.Log($"on Player {PlayerIndex}, PlayerStatus changed from {oldStatus} to {newStatus}");
    }

    [ClientRpc]
    public void RpcUpdateTilesLeft(int updatedTilesLeft)
    {
        tilesLeft = updatedTilesLeft;
    }

    // 클라이언트에서 RoundIndex와 RoundWind 데이터를 업데이트
    [ClientRpc]
    public void RpcUpdateRoundIndex(int updatedRoundIndex)
    {
        roundIndex = updatedRoundIndex;
    }

    // RoundIndex와 RoundWind 값을 반환하는 메서드
    public int GetRoundIndex()
    {
        return roundIndex;
    }

    public int GetTilesLeft()
    {
        return tilesLeft;
    }


    public IEnumerator CheckVaildDiscardAsync(int tileID, Action<bool> callback)
    {
        if (!isOwned)
        {
            Debug.LogError("[CheckVaildDiscardAsync] This method can only be executed by the owning client.");
            callback?.Invoke(false);
            yield break;
        }

        Debug.Log($"[CheckVaildDiscardAsync] Sending discard check for tileID: {tileID}");

        isWaitingForResponse = true;
        isValidDiscardResponse = -1;

        // 서버에 유효성 검사 요청
        CmdCheckValidDiscard(tileID);
        Debug.Log($"[CheckVaildDiscardAsync] Requested CmdCheckValidDiscard Tile: {TileDictionary.NumToString[tileID]}, isWaitingForResponse: {isWaitingForResponse}, isValidDiscardResponse:  {isValidDiscardResponse}");
        // 응답이 올 때까지 대기
        while (isWaitingForResponse)
        {
            yield return null; // 한 프레임 대기
        }

        Debug.Log($"[CheckVaildDiscardAsync] Received discard validation response: {isValidDiscardResponse}");
        callback?.Invoke(isValidDiscardResponse == 1);
    }


    [Command]
    private void CmdCheckValidDiscard(int tileID)
    {
        if (serverManager == null)
        {
            Debug.LogError("[CmdCheckValidDiscard] ServerManager is not assigned!");
            return;
        }
        // debug code
        // Perform the server-side validation
        int result = serverManager.IsVaildDiscard(tileID, PlayerIndex);

        TargetUpdateDiscardResult(connectionToClient, result);
        Debug.Log($"[CmdCheckValidDiscard] Result of discard validation: {result}");
    }


    [TargetRpc]
    private void TargetUpdateDiscardResult(NetworkConnection target, int result)
    {
        isValidDiscardResponse = result;
        isWaitingForResponse = false;

        Debug.Log($"[TargetUpdateDiscardResult] Received discard validation result: {result}");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // GameObject 초기화
        PlayerHaipai = GameObject.Find("PlayerHaipai");
        EnemyHaipaiToi = GameObject.Find("EnemyHaipaiToi");
        EnemyHaipaiKami = GameObject.Find("EnemyHaipaiKami");
        EnemyHaipaiShimo = GameObject.Find("EnemyHaipaiShimo");

        PlayerKawa = GameObject.Find("PlayerKawa");
        EnemyKawaToi = GameObject.Find("EnemyKawaToi");
        EnemyKawaKami = GameObject.Find("EnemyKawaKami");
        EnemyKawaShimo = GameObject.Find("EnemyKawaShimo");
        EnemyKawaList = new GameObject[3];
        EnemyKawaList[0] = EnemyKawaShimo;
        EnemyKawaList[1] = EnemyKawaToi;
        EnemyKawaList[2] = EnemyKawaKami;
        EnemyHaipaiList = new GameObject[3];
        EnemyHaipaiList[0] = EnemyHaipaiShimo;
        EnemyHaipaiList[1] = EnemyHaipaiToi;
        EnemyHaipaiList[2] = EnemyHaipaiKami;

        PlayerFuroField = GameObject.Find("PlayerFuroField");
        EnemyFuroFieldShimo = GameObject.Find("EnemyFuroFieldShimo");
        EnemyFuroFieldToi = GameObject.Find("EnemyFuroFieldToi");
        EnemyFuroFieldKami = GameObject.Find("EnemyFuroFieldKami");
        
        DisableFlowerFields();
        GameStatusUI = GameObject.Find("GameStatusUI");
        discardHandPrefabs[0] = GameObject.Find("201");
        discardHandPrefabs[1] = GameObject.Find("301");
        discardHandPrefabs[2] = GameObject.Find("401");
        discardHandPrefabs[3] = GameObject.Find("101");

        FuEffectAnimatorPrefabs[0] = GameObject.Find("4_0");
        FuEffectAnimatorPrefabs[1] = GameObject.Find("DDing");
        //animator = GetComponent<Animator>();

        actionSoundManager = GameObject.Find("ActionSoundManager");

        soundManager = actionSoundManager.GetComponent<ActionSoundManager>();

        DeleteButtons();
    }

    void Awake()
    {
        Debug.Log("[Awake] Attempting to find FlowerFields...");

        PlayerFlowerField = GameObject.Find("PlayerFlowerField");
        Debug.Log($"[Awake] PlayerFlowerField: {PlayerFlowerField?.name}");

        EnemyFlowerFieldShimo = GameObject.Find("EnemyFlowerField1");
        Debug.Log($"[Awake] EnemyFlowerFieldShimo: {EnemyFlowerFieldShimo?.name}");

        EnemyFlowerFieldToi = GameObject.Find("EnemyFlowerField2");
        Debug.Log($"[Awake] EnemyFlowerFieldToi: {EnemyFlowerFieldToi?.name}");

        EnemyFlowerFieldKami = GameObject.Find("EnemyFlowerField3");
        Debug.Log($"[Awake] EnemyFlowerFieldKami: {EnemyFlowerFieldKami?.name}");
    }

    private void DisableFlowerFields()
    {
        Debug.Log("[DisableFlowerFields] Function called.");

        if (!isOwned)
        {
            Debug.Log("[DisableFlowerFields] is not owned");
            return;
        }

        if (PlayerFlowerField != null)
        {
            if (PlayerFlowerField.activeSelf)
            {
                Debug.Log($"[DisableFlowerFields] Disabling active PlayerFlowerField: {PlayerFlowerField.name}");
                PlayerFlowerField.SetActive(false);
            }
            else
            {
                Debug.Log($"[DisableFlowerFields] PlayerFlowerField: {PlayerFlowerField.name} is already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("[DisableFlowerFields] PlayerFlowerField is null.");
        }

        if (EnemyFlowerFieldShimo != null)
        {
            if (EnemyFlowerFieldShimo.activeSelf)
            {
                Debug.Log($"[DisableFlowerFields] Disabling active EnemyFlowerFieldShimo: {EnemyFlowerFieldShimo.name}");
                EnemyFlowerFieldShimo.SetActive(false);
            }
            else
            {
                Debug.Log($"[DisableFlowerFields] EnemyFlowerFieldShimo: {EnemyFlowerFieldShimo.name} is already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("[DisableFlowerFields] EnemyFlowerFieldShimo is null.");
        }

        if (EnemyFlowerFieldToi != null)
        {
            if (EnemyFlowerFieldToi.activeSelf)
            {
                Debug.Log($"[DisableFlowerFields] Disabling active EnemyFlowerFieldToi: {EnemyFlowerFieldToi.name}");
                EnemyFlowerFieldToi.SetActive(false);
            }
            else
            {
                Debug.Log($"[DisableFlowerFields] EnemyFlowerFieldToi: {EnemyFlowerFieldToi.name} is already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("[DisableFlowerFields] EnemyFlowerFieldToi is null.");
        }

        if (EnemyFlowerFieldKami != null)
        {
            if (EnemyFlowerFieldKami.activeSelf)
            {
                Debug.Log($"[DisableFlowerFields] Disabling active EnemyFlowerFieldKami: {EnemyFlowerFieldKami.name}");
                EnemyFlowerFieldKami.SetActive(false);
            }
            else
            {
                Debug.Log($"[DisableFlowerFields] EnemyFlowerFieldKami: {EnemyFlowerFieldKami.name} is already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("[DisableFlowerFields] EnemyFlowerFieldKami is null.");
        }

        Debug.Log("[DisableFlowerFields] All FlowerFields have been checked and disabled if active.");
    }



    [Server]
    public void UpdateCurrentScore(int newScore)
    {
        var newStatus = playerStatus;
        newStatus.CurrentScore = newScore;
        playerStatus = newStatus;

        Debug.Log($"Player {PlayerIndex}: score updated to {playerStatus.CurrentScore}");
    }

    [Server]
    public void SetPlayerTurnForLight(bool isTurn)
    {
        PlayerTurnForLight = isTurn;
    }

    [Server]
    public void SetPlayerTurn(bool isTurn)
    {
        var newStatus = playerStatus;
        newStatus.IsPlayerTurn = isTurn;
        playerStatus = newStatus;
        Debug.Log($"Player {PlayerIndex}: IsPlayerTurn set to {isTurn}");
    }

    private void SpawnEnemyFirstHand()
    {
        // 스폰할 타일의 개수
        const int tileCount = 13;

        // 각 상대의 패를 순서대로 스폰
        SpawnTilesForEnemy(EnemyHaipaiToi, tileCount);
        SpawnTilesForEnemy(EnemyHaipaiKami, tileCount);
        SpawnTilesForEnemy(EnemyHaipaiShimo, tileCount);

        Debug.Log("Enemy first hands spawned successfully.");
    }

    // 개별 상대에 타일 스폰 로직
    private void SpawnTilesForEnemy(GameObject enemyHaipai, int tileCount)
    {

        if (enemyHaipai == null)
        {
            Debug.LogWarning("EnemyHaipai is null. Skipping tile spawning.");
            return;
        }
        TileGrid tileGrid = enemyHaipai.GetComponent<TileGrid>();
        if (tileGrid == null)
        {
            Debug.LogError("TileGrid component not found on enemyHaipai!");
            return;
        }
        for (int i = 0; i < tileCount; i++)
        {
            GameObject spawnedTile = Instantiate(TileBackPrefab, enemyHaipai.transform);
            spawnedTile.name = $"TileBack_{i + 1}";
            var spawnedTileEvent = spawnedTile.GetComponent<TileEvent>();
            spawnedTileEvent.SetUndraggable();
            tileGrid.AddTileToLastIndex(spawnedTile);
        }
        //tileGrid.UpdateLayoutByName();
        Debug.Log($"Spawned {tileCount} TileBacks for {enemyHaipai.name}.");
    }


    [TargetRpc]
    public void TargetSpawnFirstHand(NetworkConnection target, List<int> closedTiles)
    {
        // 클라이언트에서 초기 손패를 스폰하거나 UI에 반영
        Debug.Log($"TargetSpawnFirstHand received. ");

        if (PlayerHaipai == null)
        {
            Debug.LogError("PlayerHaipai is null!");
            return;
        }
        else
        {
            Debug.Log($"PlayerHaipai is: {PlayerHaipai.name}");
            var components = PlayerHaipai.GetComponents<Component>();
            foreach (var component in components)
            {
                Debug.Log($"Component: {component.GetType().Name}");
            }

        }


        TileGrid tileGrid = PlayerHaipai.GetComponent<TileGrid>();
        if (tileGrid == null)
        {
            Debug.LogError("TileGrid component not found!");
            return;
        }
        for (int tileId = 0; tileId < closedTiles.Count; tileId++)
        {
            for (int tileCount = 0; tileCount < closedTiles[tileId]; tileCount++)
            {
                // 타일을 스폰하고 UI에 추가하는 로직
                GameObject tilePrefab = TilePrefabArray[tileId];
                if (tilePrefab != null && PlayerHaipai != null)
                {
                    GameObject spawnedTile = Instantiate(tilePrefab, PlayerHaipai.transform);
                    spawnedTile.name = TileDictionary.NumToString[tileId];
                }
            }
        }
        
        if (tileGrid)
        {
            tileGrid.UpdateLayoutByName();
        }
        SpawnEnemyFirstHand();
        Debug.Log("First hand spawned successfully on client.");
    }


    [TargetRpc]
    public void TargetShominKanTile(NetworkConnection target, int tile, int playerIndex)
    {
        GameObject FuroField = PlayerFuroField;
        GameObject HaipaiField = PlayerHaipai;
        if (playerIndex != PlayerIndex)
        {
            int relativeIndex = GetRelativeIndex(playerIndex);
            switch (relativeIndex)
            {
                case 0:
                    HaipaiField = EnemyHaipaiShimo;
                    FuroField = EnemyFuroFieldShimo;
                    break;
                case 1:
                    HaipaiField = EnemyHaipaiToi;
                    FuroField = EnemyFuroFieldToi;
                    break;
                case 2:
                    HaipaiField = EnemyHaipaiKami;
                    FuroField = EnemyFuroFieldKami;
                    break;
            }
        }
        Debug.Log($"[TargetShominKanTile] called with tile {tile}.");

        // 타일 생성
        GameObject spawnedTile = Instantiate(TilePrefabArray[tile], PlayerHaipai.transform);
        if (spawnedTile == null)
        {
            Debug.LogError("spawnedTile is null.");
            return;
        }
        spawnedTile.name = TileDictionary.NumToString[tile];
        // TileEvent의 isDraggable 설정
        TileEvent tileEvent = spawnedTile.GetComponent<TileEvent>();
        if (tileEvent != null)
        {
            tileEvent.SetUndraggable();
        }

        TileGrid tileGridHaipai = HaipaiField.GetComponent<TileGrid>();
        if (tileGridHaipai != null)
        {
            if (playerIndex == PlayerIndex)
            {
                tileGridHaipai.DestoryByTileId(tile);
            }
            else
            {
                tileGridHaipai.DestoryLastTile();
            }
        }
        // 조건에 맞는 하위 오브젝트 찾기
        Transform targetChild = FindMatchingChild(FuroField.transform, tile);

        if (targetChild != null)
        {
            // 회전된 자식을 찾기
            Transform rotatedChild = null;

            foreach (Transform grandChild in targetChild)
            {
                // Z축 회전이 90도 또는 -90도인 자식 찾기
                if (Mathf.Abs(grandChild.localRotation.eulerAngles.z - 90) < 1f || Mathf.Abs(grandChild.localRotation.eulerAngles.z - 270) < 1f)
                {
                    rotatedChild = grandChild;
                    break;  // 첫 번째 회전된 자식만 찾으면 종료
                }
            }

            if (rotatedChild != null)
            {
                // 찾은 회전된 자식의 하위로 타일 추가
                spawnedTile.transform.SetParent(rotatedChild.parent, false);  // 부모는 rotatedChild의 부모로 유지

                // RectTransform 가져오기
                RectTransform rotatedRect = rotatedChild.GetComponent<RectTransform>();
                RectTransform tileRect = spawnedTile.GetComponent<RectTransform>();

                if (rotatedRect != null && tileRect != null)
                {
                    // 회전된 자식의 크기와 동일하게 크기 조정
                    tileRect.sizeDelta = rotatedRect.sizeDelta;

                    // 회전된 자식의 위치와 동일하게 배치
                    float offsetY = rotatedRect.sizeDelta.x;
                    tileRect.anchoredPosition = new Vector2(rotatedRect.anchoredPosition.x, rotatedRect.anchoredPosition.y + offsetY);

                    // 회전된 자식과 동일하게 회전 적용
                    tileRect.localRotation = rotatedRect.localRotation;

                    Debug.Log($"[TargetShominKanTile] Tile placed, sized, and rotated above rotated child: {rotatedChild.name}");
                }
                else
                {
                    Debug.LogError("RectTransform is missing on rotatedChild or spawnedTile.");
                }
            }
            else
            {
                Debug.LogWarning("[TargetShominKanTile] No rotated child found.");
            }
        }
        else
        {
            Debug.LogWarning("[TargetShominKanTile] No matching child found.");
        }


    }

    /// <summary>
    /// FuroField의 하위에서 조건에 맞는 자식을 찾는 함수
    /// </summary>
    /// <param name="parent">탐색할 부모 Transform</param>
    /// <param name="tile">타일 ID</param>
    /// <returns>조건에 맞는 Transform (없으면 null)</returns>
    private Transform FindMatchingChild(Transform parent, int tile)
    {
        // 타일 이름 앞 두 글자 (예: "1m", "5p")
        string tilePrefix = TileDictionary.NumToString[tile].Substring(0, 2);
        Debug.Log($"[FindMatchingChild] Searching for children with prefix: {tilePrefix}");

        foreach (Transform child in parent)
        {
            Debug.Log($"[FindMatchingChild] Checking child: {child.name} with {child.childCount} children.");

            // 자식의 하위 오브젝트가 정확히 3개인지 확인
            if (child.childCount == 3)
            {
                bool allMatch = true;

                // 자식의 하위 오브젝트를 검사
                foreach (Transform grandChild in child)
                {
                    string grandChildPrefix = grandChild.name.Length >= 2 ? grandChild.name.Substring(0, 2) : grandChild.name;

                    Debug.Log($"[FindMatchingChild] Checking grandChild: {grandChild.name}, Prefix: {grandChildPrefix}");

                    // 하위 오브젝트의 이름 앞 두 글자가 tilePrefix와 일치하는지 확인
                    if (grandChildPrefix != tilePrefix)
                    {
                        Debug.LogWarning($"[FindMatchingChild] Mismatch found! Expected: {tilePrefix}, Found: {grandChildPrefix}");
                        allMatch = false;
                        break;  // 하나라도 다르면 중단
                    }
                }

                // 하위 오브젝트 3개가 모두 일치하면 반환
                if (allMatch)
                {
                    Debug.Log($"[FindMatchingChild] Found matching child: {child.name}");
                    return child;
                }
            }
            else
            {
                Debug.LogWarning($"[FindMatchingChild] Skipping {child.name} because it has {child.childCount} children (expected 3).");
            }
        }

        Debug.LogWarning("[FindMatchingChild] No matching child found.");
        return null;  // 조건을 만족하는 자식이 없으면 null 반환
    }




    [TargetRpc]
    public void TargetTsumoTile(NetworkConnection target, int tile, int playerIndex)
    {
        Debug.Log($"TargetTsumoTile called with tile {tile}.");



        TileGrid tileGrid = PlayerHaipai.GetComponent<TileGrid>();
        if (tileGrid == null) return;
        GameObject spawnedTile = Instantiate(TilePrefabArray[tile], PlayerHaipai.transform);
        if(spawnedTile == null) return;
        spawnedTile.name = TileDictionary.NumToString[tile];
        tileGrid.ShowTsumoTile(spawnedTile);
        Debug.Log($"Displaying Tsumo tile for player {playerIndex}.");
        Debug.Log($"[TargetTsumoTile] Is Player Turn: {playerStatus.IsPlayerTurn}");
        CmdDisplayEnemyTsumoTile(tile, playerIndex);
    }

    [Command]
    public void CmdDisplayEnemyTsumoTile(int tile, int playerIndex)
    {
        // 현재 씬에서 모든 PlayerManager 객체를 찾음
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }
        RpcDisplayEnemyTsumoTile(tile, playerIndex);
    }

    public void DisplayEnemyTsumoTile(int tile, int playerIndex)
    {
        if (isServer)
            return;
        int relativeIndex = GetRelativeIndex(playerIndex);
        if (relativeIndex < 0)
        {
            return;
        }
        EnemyHandTilesCount[relativeIndex]++;
        Debug.Log($"[RpcDisplayEnemyTsumoTile] Get relative index {relativeIndex}");
        GameObject haipaiPrefab = null;
        switch (relativeIndex)
        {
            case 0:
                haipaiPrefab = EnemyHaipaiShimo;
                break;
            case 1:
                haipaiPrefab = EnemyHaipaiToi;
                break;
            case 2:
                haipaiPrefab = EnemyHaipaiKami;
                break;
        }
        TileGrid tileGrid = haipaiPrefab.GetComponent<TileGrid>();
        if (tileGrid == null) return;
        var spawnedTile = Instantiate(TileBackPrefab, haipaiPrefab.transform);
        if (spawnedTile == null) return;
        spawnedTile.name = "TileBack_14";
        TileEvent tileEvent = spawnedTile.GetComponent<TileEvent>();
        if (tileEvent == null) return;
        tileEvent.SetUndraggable();
        tileGrid.ShowTsumoTile(spawnedTile);
        return;
    }

    [ClientRpc]
    public void RpcDisplayEnemyTsumoTile(int tile, int playerIndex)
    {
        Debug.Log($"[RpcDisplayEnemyTsumoTile] in function. Player Index here: {PlayerIndex}, Enemy's Index who tsumo: {playerIndex}, tile: {TileDictionary.NumToString[tile]}");
        // 현재 씬에서 모든 PlayerManager 객체를 찾음
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        Debug.Log($"[RpcDisplayEnemyTsumoTile] Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager == null)
                continue;
            if (playerManager.PlayerIndex == playerIndex)
                continue;
            if (playerManager.isOwned)
            {
                Debug.Log($"[RpcDisplayEnemyTsumoTile] player manager owned, Player Index here: {playerManager.PlayerIndex}, Enemy's Index who tsumo: {playerIndex}");
                playerManager.DisplayEnemyTsumoTile(tile, playerIndex);
                return;
            }
        }
        
    }


    public void DebugAllPlayerManagers()
    {
        // 현재 씬에서 모든 PlayerManager 객체를 찾음
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        Debug.Log($"Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            // PlayerManager의 정보를 출력
            Debug.Log($"PlayerManager: PlayerIndex = {playerManager.PlayerIndex}, PlayerName = {playerManager.PlayerName}, NetId = {playerManager.GetComponent<NetworkIdentity>()?.netId}");
        }
    }

    [Server]
    public void ForceDiscardByTileId(int tileId)
    {

        //string prefix = TileDictionary.NumToString[tileId].Substring(0, 2);

        TargetForceDiscardByTileId(connectionToClient, PlayerIndex, tileId);

    }

    [TargetRpc]
    public void TargetForceDiscardByTileId(NetworkConnection conn, int playerIndex, int tileId)
    {
        PlayerManager playerManager = FindMyPlayerManager(playerIndex);
        if (playerManager != null && playerManager.PlayerHaipai != null)
        {
            TileGrid tileGrid = playerManager.PlayerHaipai.GetComponent<TileGrid>();
            if (tileGrid != null)
            {
                tileGrid.DiscardTileByTileId(tileId);
            }
        }
    }


    [Server]
    public void ForceDiscardTile()
    {

        //string prefix = TileDictionary.NumToString[tileId].Substring(0, 2);

        TargetForceDiscardTsumoTile(connectionToClient, PlayerIndex);

    }

    [TargetRpc]
    public void TargetForceDiscardTsumoTile(NetworkConnection conn, int playerIndex)
    {
        PlayerManager playerManager = FindMyPlayerManager(playerIndex);
        if (playerManager != null && playerManager.PlayerHaipai != null)
        {
            TileGrid tileGrid = playerManager.PlayerHaipai.GetComponent<TileGrid>();
            if (tileGrid != null)
            {
                tileGrid.DiscardTsumoTile();
            }
        }
    }


    private PlayerManager FindMyPlayerManager(int playerIndex)
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return null;
        }

        Debug.Log($"Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager.PlayerIndex == playerIndex)
                return playerManager; 
        }
        return null;
    }


    [Command]
    public void CmdDiscardTile(string tileName, bool isTsumoTile)
    {
        string prefix = tileName.Substring(0, 2);

        if (!TileDictionary.StringToNum.TryGetValue(prefix, out int tileId))
        {
            Debug.LogError($"Invalid tile prefix: {prefix}");
            return;
        }

        if (PlayerKawa == null)
        {
            Debug.LogError("PlayerKawa is null. Cannot get TileGrid component.");
            return;
        }

        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        if (allPlayerManagers.Length == 0)
        {
            Debug.LogWarning("No PlayerManager instances found in the scene.");
            return;
        }

        Debug.Log($"Found {allPlayerManagers.Length} PlayerManager instances:");
        foreach (var playerManager in allPlayerManagers)
        {
            NetworkConnection networkConnection = playerManager.GetComponent<NetworkIdentity>().connectionToClient;
            Debug.Log($"Assigned PlayerIndex {playerManager.PlayerIndex} to PlayerManager with NetId: {playerManager.GetComponent<NetworkIdentity>().netId}");
            if (networkConnection != null)
            {
                TargetDiscardTile(networkConnection, tileId, PlayerIndex, isTsumoTile);
            }
        }
    }



    

    public IEnumerator HandleTileDiscardCoroutine(int tileId, int playerIndex, bool IsTsumoTile, Action onComplete)
    {
        // tileId로 TilePrefabArray에서 해당 프리팹 가져오기
        if (tileId < 0 || tileId >= TilePrefabArray.Length)
        {
            Debug.LogError($"Invalid tileId {tileId}. It is out of range: {TilePrefabArray.Length}.");
            onComplete?.Invoke();
            yield break;
        }

        GameObject kawaPrefab = null;
        GameObject haipaiPrefab = null;
        Debug.Log($"Discarded player is {playerIndex}, here player is {PlayerIndex}");
        if (playerIndex == PlayerIndex)
        {
            decisionMade = true;
            kawaPrefab = PlayerKawa;
            animator = discardHandPrefabs[3].GetComponent<Animator>();
            animator.SetTrigger("101AniTrigger");
        }
        else
        {
            int relativeIndex = GetRelativeIndex(playerIndex);
            Debug.Log($"Get relative index {relativeIndex}");
            animator = discardHandPrefabs[relativeIndex].GetComponent<Animator>();
            switch (relativeIndex)
            {
                case 0:
                    kawaPrefab = EnemyKawaShimo;
                    haipaiPrefab = EnemyHaipaiShimo;
                    animator.SetTrigger("201AniTrigger");
                    break;
                case 1:
                    kawaPrefab = EnemyKawaToi;
                    haipaiPrefab = EnemyHaipaiToi;
                    animator.SetTrigger("301AniTrigger");
                    break;
                case 2:
                    kawaPrefab = EnemyKawaKami;
                    haipaiPrefab = EnemyHaipaiKami;
                    animator.SetTrigger("401AniTrigger");
                    break;
                default:
                    Debug.LogError($"Invalid relative index: {relativeIndex}");
                    onComplete?.Invoke();
                    yield break;
            }
        }

        if (kawaPrefab == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (haipaiPrefab != null)
        {
            TileGrid haipaiTileGrid = haipaiPrefab.GetComponent<TileGrid>();
            if (haipaiTileGrid != null)
            {
                haipaiTileGrid.ShowTedashi(!IsTsumoTile);
            }
        }

        // 타일 생성
        GameObject spawnedTile = Instantiate(TilePrefabArray[tileId], kawaPrefab.transform);
        if (spawnedTile == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        spawnedTile.name = TileDictionary.NumToString[tileId];
        TileEvent tileEvent = spawnedTile.GetComponent<TileEvent>();
        if (tileEvent != null)
        {
            tileEvent.SetUndraggable();
        }

        Debug.Log($"Kawa Name: {kawaPrefab.name}, Components: {string.Join(", ", kawaPrefab.GetComponents<Component>().Select(c => c.GetType().Name))}");
        if (IsTsumoTile)
        {
            SetTileColorToGray(spawnedTile);
        }
        // TileGrid에 타일 추가
        TileGrid tileGrid = kawaPrefab.GetComponent<TileGrid>();
        if (tileGrid == null)
        {
            Debug.LogError("TileGrid component not found on PlayerKawa.");
            onComplete?.Invoke();
            yield break;
        }
        
        tileGrid.AddTileToLastIndex(spawnedTile);
        Debug.Log($"Tile {spawnedTile.name} successfully added to {kawaPrefab.name}.");
        onComplete?.Invoke(); // 작업 완료 알림
    }

    private void SetTileColorToGray(GameObject tile)
    {
        if (tile == null)
        {
            Debug.LogWarning("[SetTileColorToGray] Tile GameObject is null. Skipping color adjustment.");
            return;
        }

        // Renderer로 처리 (3D Object)
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    // 회색 (회색 RGB: 0.6, 0.6, 0.6)
                    material.color = new Color(0.6f, 0.6f, 0.6f, material.color.a);
                }
            }
            Debug.Log("[SetTileColorToGray] Applied gray color to Renderer.");
            return;
        }

        // Image로 처리 (UI Object)
        UnityEngine.UI.Image image = tile.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            // 약간 회색
            image.color = new Color(0.8f, 0.8f, 0.8f, image.color.a);
            Debug.Log("[SetTileColorToGray] Applied gray color to UI Image.");
            return;
        }

        Debug.LogWarning("[SetTileColorToGray] No Renderer or UI Image found on tile. Skipping color adjustment.");
    }

    private GameObject CreateButton(GameObject prefab, Transform parent, string buttonType)
    {
        if (prefab == null)
        {
            Debug.LogError($"[CreateButton] Prefab for {buttonType} is null.");
            return null;
        }

        GameObject button = Instantiate(prefab, parent);

        // 전역 변수에 버튼 할당
        switch (buttonType.ToUpper())
        {
            case "HU":
                huButton = button;
                break;
            case "SKIP":
                skipButton = button;
                break;
            case "FLOWER":
                flowerButton = button;
                break;
            case "CHII":
                chiiButton = button;
                break;
            case "PON":
                ponButton = button;
                break;
            case "KAN":
                kanButton = button;
                break;
            default:
                Debug.LogWarning($"[CreateButton] Unknown button type: {buttonType}");
                break;
        }

        return button;
    }


    [ClientRpc]
    public void RpcSetRemainingTimeDefault()
    {
        remainingTime = -1f;
    }

    [Command]
    public void CmdReturnActionDecision(int playerWindIndex, ActionPriorityInfo actionPriorityInfo, int actionTurnId, int tileId)
    {
        //remainingTime = -1;
        serverManager.StartCoroutine(serverManager.ReceiveActionDecisionCoroutine(playerWindIndex, actionPriorityInfo, actionTurnId, tileId));
    }

    private bool decisionMade = false; // 전역 변수로 이동하여 버튼 클릭 상태를 추적

    public const float remainingTimeForEveryTurn = 60f;


    [Command]
    public void CmdReturnDecisionTimeOut(int actionTurnId, int playerWindIndex)
    {
        Debug.Log($"[CmdReturnDecisionTimeOut] Called. ActionTurnId: {actionTurnId}, PlayerWindIndex: {playerWindIndex}");

        if (serverManager == null)
        {
            Debug.LogError("[CmdReturnDecisionTimeOut] serverManager is null.");
            return;
        }

        remainingTime = -1;
        serverManager.ReceiveTimeoutDecision(actionTurnId, playerWindIndex);
        Debug.Log($"[CmdReturnDecisionTimeOut] Decision timeout processed for PlayerWindIndex: {playerWindIndex}");
    }


    private IEnumerator WaitPlayerDecision(int actionTurnId, int playerWindIndex)
    {
        Debug.Log($"[WaitPlayerDecision] Started. ActionTurnId: {actionTurnId}, PlayerWindIndex: {playerWindIndex}");

        remainingTime = -1;

        remainingTime = remainingTimeForEveryTurn;
        remainingTimeActionId = actionTurnId;
        int localactionId = actionTurnId;
        while (remainingTime > 0 && remainingTimeActionId == localactionId)
        {
            Debug.Log($"[WaitPlayerDecision] Remaining Time: {remainingTime:F2}s for PlayerWindIndex: {playerWindIndex}");
            remainingTime -= Time.deltaTime;
            yield return null;
        }



        Debug.Log($"[WaitPlayerDecision] Time expired. Triggering CmdReturnDecisionTimeOut for PlayerWindIndex: {playerWindIndex}");
        //CmdReturnDecisionTimeOut(actionTurnId, playerWindIndex);
        DeleteButtons();
        CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.TIMEOUT, 0, -1), actionTurnId, -1);
        yield break;
    }


    [TargetRpc]
    public void TargetWaitPlayerDecision(NetworkConnection conn, int actionTurnId, int playerWindIndex)
    {
        Debug.Log($"[TargetWaitPlayerDecision] Called on client. ActionTurnId: {actionTurnId}, PlayerWindIndex: {playerWindIndex}");

        //if (conn == null)
        //{
        //    Debug.LogError("[TargetWaitPlayerDecision] NetworkConnection is null.");
        //    return;
        //}
        PlayerManager playerManager = GetOwnedPlayerManager();
        playerManager.StartCoroutine(playerManager.WaitPlayerDecision(actionTurnId, playerWindIndex));
        Debug.Log($"[TargetWaitPlayerDecision] Coroutine for player decision started.");
    }



    private IEnumerator MakeButtonsAndHandlePlayerDecision(int playerWindIndex, List<ActionPriorityInfo> actions, int actionTurnId, int tileId)
    {
        Debug.Log("[MakeButtonsAndHandlePlayerDecision] Starting decision coroutine.");

        decisionMade = false;
        if (actions.Count == 0)
        {
            remainingTime = -1;
            remainingTime = remainingTimeForEveryTurn;
            remainingTimeActionId = actionTurnId;
            int localactionId_1 = actionTurnId;
            while (remainingTime > 0 && remainingTimeActionId == localactionId_1 && !decisionMade)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (remainingTimeActionId == localactionId_1 && !decisionMade)
            {
                Debug.Log("[MakeButtonsAndHandlePlayerDecision] Time is up. Defaulting to TIMEOUT.");
                remainingTime = -1;
                DeleteButtons();
                //CmdReturnDecisionTimeOut(actionTurnId, playerWindIndex);
                CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.TIMEOUT, 0, -1), actionTurnId, tileId);
            }
            yield break;
        }
        int priority = actions[0].Priority;
        // `Main Canvas` 찾기
        Canvas mainCanvas = GameObject.Find("Main Canvas").GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("[MakeButtonsAndHandlePlayerDecision] Main Canvas not found.");
            remainingTime = -1;
            DeleteButtons();
            
            CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.SKIP, priority, -1), actionTurnId, tileId);
            yield break;
        }

        // `PlayerHaipai`의 RectTransform 가져오기
        RectTransform playerHaipaiRect = PlayerHaipai.GetComponent<RectTransform>();
        if (playerHaipaiRect == null)
        {
            Debug.LogError("[MakeButtonsAndHandlePlayerDecision] PlayerHaipai RectTransform not found.");
            remainingTime = -1;
            DeleteButtons();
            CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.SKIP, priority, -1), actionTurnId, tileId);
            yield break;
        }

        Vector3 playerHaipaiPosition = playerHaipaiRect.localPosition;
        Vector3 playerHaipaiScale = playerHaipaiRect.localScale;

        // Skip 버튼 생성 및 위치 설정
        skipButton = CreateButton(skipButtonPrefab, mainCanvas.transform, "SKIP");
        RectTransform skipButtonRect = skipButton.GetComponent<RectTransform>();
        if (skipButtonRect == null)
        {
            Debug.LogError("[MakeButtonsAndHandlePlayerDecision] SkipButton RectTransform not found.");
            DeleteButtons();
            if (priority != 0)
            {
                remainingTime = -1;
            }
            CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.SKIP, priority, -1), actionTurnId, tileId);
            yield break;
        }

        skipButtonRect.localPosition = new Vector3(
            playerHaipaiPosition.x + playerHaipaiRect.sizeDelta.x * playerHaipaiScale.x,
            playerHaipaiPosition.y + (playerHaipaiRect.sizeDelta.y / 2) * playerHaipaiScale.y,
            0
        );

        Debug.Log($"[MakeButtonsAndHandlePlayerDecision] SkipButton position set to {skipButtonRect.localPosition}.");

        // ActionType별로 선택지를 분류
        Dictionary<ActionType, List<ActionPriorityInfo>> ActionCountDictionary = new Dictionary<ActionType, List<ActionPriorityInfo>>();
        foreach (ActionType action in System.Enum.GetValues(typeof(ActionType)))
        {
            ActionCountDictionary[action] = new List<ActionPriorityInfo>();
        }
        foreach (var action in actions)
        {
            ActionCountDictionary[action.Type].Add(action);
        }

        // 버튼 생성 및 선택 처리
        float buttonSpacing = skipButtonRect.sizeDelta.x * skipButtonRect.localScale.x; // 버튼 간격
        int buttonIndex = 0;

        foreach (var actionPair in ActionCountDictionary)
        {
            if (actionPair.Value.Count == 0) continue; // 선택지가 없는 경우 버튼 생성 안 함
            string str = $"action type: {actionPair.Key.ToString()}, anctions:";
            for (int i = 0; i < actionPair.Value.Count; i++)
            {
                str += $" {actionPair.Value[i]}";
            }
            Debug.Log(str);
            GameObject buttonPrefab = null;
            string buttonType = actionPair.Key.ToString(); // 전역 변수 이름과 매핑
            switch (actionPair.Key)
            {
                case ActionType.HU:
                    buttonPrefab = huButtonPrefab;
                    break;
                case ActionType.KAN:
                    buttonPrefab = kanButtonPrefab;
                    break;
                case ActionType.PON:
                    buttonPrefab = ponButtonPrefab;
                    break;
                case ActionType.CHII:
                    buttonPrefab = chiiButtonPrefab;
                    break;
                case ActionType.FLOWER:
                    buttonPrefab = flowerButtonPrefab;
                    break;
            }

            if (buttonPrefab != null)
            {
                // 버튼 생성 및 전역 변수에 할당
                GameObject actionButton = CreateButton(buttonPrefab, mainCanvas.transform, buttonType);
                RectTransform actionButtonRect = actionButton.GetComponent<RectTransform>();
                if (actionButtonRect != null)
                {
                    // 버튼 위치 계산
                    actionButtonRect.localPosition = new Vector3(
                        skipButtonRect.localPosition.x - (buttonSpacing * (buttonIndex + 1)),
                        skipButtonRect.localPosition.y,
                        0
                    );
                    Debug.Log($"[MakeButtonsAndHandlePlayerDecision] Button for {actionPair.Key} placed at {actionButtonRect.localPosition}.");
                }

                // 클릭 이벤트 처리
                if (actionPair.Value.Count == 1)
                {
                    // 선택지가 하나인 경우 바로 실행
                    var selectedAction = actionPair.Value[0];
                    actionButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log($"[MakeButtonsAndHandlePlayerDecision] {selectedAction.Type} button clicked with single choice.");
                        remainingTime = -1;
                        decisionMade = true;
                        DeleteButtons();
                        CmdReturnActionDecision(playerWindIndex, selectedAction, actionTurnId, tileId);
                    });
                }
                else
                {
                    // 선택지가 여러 개인 경우 추가 선택지 UI 표시
                    actionButton.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log($"[MakeButtonsAndHandlePlayerDecision] {actionPair.Key} button clicked with multiple choices.");
                        ShowAdditionalChoices(playerWindIndex, actionPair.Key, actionPair.Value, actionTurnId, tileId);
                    });
                }

                buttonIndex++;
            }
        }

        // Skip 버튼 클릭 이벤트 추가
        skipButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("[MakeButtonsAndHandlePlayerDecision] Skip button clicked.");
            //remainingTime = -1;
            if (priority != 0)
            {
                remainingTime = -1;
            }
            decisionMade = true;
            DeleteButtons();
            CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.SKIP, priority, -1), actionTurnId, tileId);
        });
        remainingTime = -1;
        // 20초 타이머 시작
        remainingTime = remainingTimeForEveryTurn;
        remainingTimeActionId = actionTurnId;
        int localactionId = actionTurnId;
        bool buttonsDeleted = false; // 버튼 삭제 여부 체크

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTimeActionId != localactionId)
            {
                break;
            }
            // 결정이 완료되었고 버튼이 아직 삭제되지 않았다면 버튼 삭제
            if (decisionMade && !buttonsDeleted)
            {
                DeleteButtons();
                buttonsDeleted = true;
                Debug.Log("[MakeButtonsAndHandlePlayerDecision] Decision made. Buttons destroyed.");
            }

            // 시간이 다 되었고 결정이 안 되었을 때만 Timeout 처리
            if (remainingTime <= 0 && !decisionMade && remainingTimeActionId == localactionId)
            {
                Debug.Log("[MakeButtonsAndHandlePlayerDecision] Time is up. Defaulting to Skip.");
                //CmdReturnDecisionTimeOut(actionTurnId, playerWindIndex);
                CmdReturnActionDecision(playerWindIndex, new ActionPriorityInfo(ActionType.TIMEOUT, priority, -1), actionTurnId, tileId);
                // 버튼 삭제
                DeleteButtons();
                buttonsDeleted = true;
            }

            yield return null;  // 매 프레임 대기
        }
    }

    private GameObject additionalChoicesContainer;
    public GameObject backButtonPrefab;
    private GameObject backButton;
    // 추가 선택지를 보여주는 함수
    private void ShowAdditionalChoices(int playerIndex, ActionType actionType, List<ActionPriorityInfo> choices, int actionTurnId, int tileId)
    {
        Debug.Log($"[ShowAdditionalChoices] Displaying additional choices about {actionType.ToString()}.");

        // 기존 버튼 비활성화
        DisableButtons();

        // `Main Canvas` 찾기
        Canvas mainCanvas = GameObject.Find("Main Canvas").GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("[ShowAdditionalChoices] Main Canvas not found.");
            return;
        }

        // 추가 선택지를 감싸는 컨테이너 생성
        additionalChoicesContainer = new GameObject("AdditionalChoicesContainer", typeof(RectTransform));
        additionalChoicesContainer.transform.SetParent(mainCanvas.transform, false);

        RectTransform containerRect = additionalChoicesContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(600, 300); // 기본 크기
        containerRect.anchorMin = new Vector2(0.5f, 0.5f); // 중앙 정렬
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.localPosition = new Vector3(0, 200, 0); // PlayerHaipai 위에 위치

        // 배경 생성
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(additionalChoicesContainer.transform, false);

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.sizeDelta = new Vector2(600, 300); // 기본 크기
        backgroundRect.anchorMin = new Vector2(0, 0);
        backgroundRect.anchorMax = new Vector2(1, 1);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        background.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f); // 반투명 검정

        // 뒤로가기 버튼 생성
        if (backButtonPrefab != null)
        {
            backButton = Instantiate(backButtonPrefab, additionalChoicesContainer.transform);
            RectTransform backButtonRect = backButton.GetComponent<RectTransform>();
            backButtonRect.anchorMin = new Vector2(1, 1); // 우상단 정렬
            backButtonRect.anchorMax = new Vector2(1, 1);
            backButtonRect.pivot = new Vector2(1, 1);
            backButtonRect.sizeDelta = new Vector2(50, 50); // 크기
            backButtonRect.anchoredPosition = new Vector2(-10, -10); // 우상단 여백

            backButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("[ShowAdditionalChoices] Back button clicked.");
                DestroyAdditionalChoices(); // 추가 선택지 삭제
                EnableButtons(); // 기존 버튼 활성화
            });
        }

        float tileWidth = 50;
        float tileHeight = 75;
        // 선택지 버튼 생성
        float buttonWidth;
        float buttonHeight = tileHeight;
        if (actionType == ActionType.CHII)
        {
            buttonWidth = tileWidth * 3;
        }
        else // kan
        {
            buttonWidth = tileWidth * 4;
        }
        float spacing = 10; // 버튼 간격

        int index = -1;
        choices.Sort();
        foreach (ActionPriorityInfo choice in choices)
        {
            if (choice.Type != actionType)
            {
                continue;
            }

            // 로컬 변수로 복사
            ActionPriorityInfo localChoice = choice;

            // 버튼 생성
            GameObject choiceButton = new GameObject($"ChoiceButton_{localChoice.Type}_{index++}", typeof(RectTransform), typeof(Button), typeof(Image));
            choiceButton.transform.SetParent(additionalChoicesContainer.transform, false);
            RectTransform choiceButtonRect = choiceButton.GetComponent<RectTransform>();
            choiceButtonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            // 버튼 위치 계산
            float xOffset = (index % 3) * (buttonWidth + spacing) - ((choices.Count - 1) % 3) * (buttonWidth + spacing) / 2;
            float yOffset = -((index / 3) * (buttonHeight + spacing));
            choiceButtonRect.anchoredPosition = new Vector2(xOffset, yOffset);

            // CHII와 KAN의 경우 타일 묶음을 생성
            GameObject tileGroup = new GameObject("TileGroup", typeof(RectTransform));
            tileGroup.transform.SetParent(choiceButton.transform, false);
            RectTransform tileGroupRect = tileGroup.GetComponent<RectTransform>();
            tileGroupRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            tileGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
            tileGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
            tileGroupRect.pivot = new Vector2(0.5f, 0.5f);
            tileGroupRect.localPosition = Vector3.zero;

            if (localChoice.Type == ActionType.CHII)
            {
                // CHII의 경우 TileId, TileId+1, TileId+2
                for (int i = 0; i < 3; i++)
                {
                    GameObject tile = Instantiate(TilePrefabArray[localChoice.TileId + i], tileGroup.transform);
                    RectTransform tileRect = tile.GetComponent<RectTransform>();
                    tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);
                    tileRect.anchoredPosition = new Vector2(i * tileWidth, 0); // 타일 간격

                    // TileEvent의 isDraggable 설정
                    TileEvent tileEvent = tile.GetComponent<TileEvent>();
                    if (tileEvent != null)
                    {
                        tileEvent.SetUndraggable();
                    }

                    // RaycastTarget 비활성화
                    Image tileImage = tile.GetComponent<Image>();
                    if (tileImage != null)
                    {
                        tileImage.raycastTarget = false;
                    }
                }
            }
            else if (localChoice.Type == ActionType.KAN)
            {
                tileGroupRect.sizeDelta = new Vector2(buttonWidth / 3 * 4, buttonHeight);
                // KAN의 경우 TileId 4개
                for (int i = 0; i < 4; i++)
                {
                    GameObject tile = Instantiate(TilePrefabArray[localChoice.TileId], tileGroup.transform);
                    RectTransform tileRect = tile.GetComponent<RectTransform>();
                    tileRect.sizeDelta = new Vector2(tileWidth, tileHeight);
                    tileRect.anchoredPosition = new Vector2(i * tileWidth, 0); // 타일 간격

                    // TileEvent의 isDraggable 설정
                    TileEvent tileEvent = tile.GetComponent<TileEvent>();
                    if (tileEvent != null)
                    {
                        tileEvent.SetUndraggable();
                    }

                    // RaycastTarget 비활성화
                    Image tileImage = tile.GetComponent<Image>();
                    if (tileImage != null)
                    {
                        tileImage.raycastTarget = false;
                    }
                }
            }

            // 버튼 클릭 이벤트
            choiceButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"[ShowAdditionalChoices] {localChoice.Type} choice {TileDictionary.NumToString[localChoice.TileId]} clicked.");
                remainingTime = -1;
                DeleteButtons(); // 추가 선택지 삭제
                CmdReturnActionDecision(playerIndex, localChoice, actionTurnId, tileId); // 선택 결과 전송
            });
        }

        Debug.Log("[ShowAdditionalChoices] Additional choices displayed.");
    }

    private void DestroyAdditionalChoices()
    {
        if (additionalChoicesContainer != null)
        {
            Destroy(additionalChoicesContainer);
            additionalChoicesContainer = null;
        }

        if (backButton != null)
        {
            Destroy(backButton);
            backButton = null;
        }

        Debug.Log("[DestroyAdditionalChoices] Additional choices and background destroyed.");
    }




    private bool IsTargetDiscardTileRunning = false; // TargetDiscardTile 실행 상태를 나타내는 플래그
    private bool IsTargetReplacementTileRunning = false;



    private bool EndFlag_FlowerReplacement = false;

    [Command]
    public void CmdSetFlowerReplacementFlagEnd()
    {
        EndFlag_FlowerReplacement = true;
    }

    [Server]
    public void SetFlowerReplacementFlagFalse()
    {
        EndFlag_FlowerReplacement = false;
    }
    [Server]
    public bool IsFlowerReplacementComplete()
    {
        return EndFlag_FlowerReplacement;
    }

    public IEnumerator HandleFlowerReplacementCoroutine(int tileId, int playerIndex, bool IsTsumoTile, Action onComplete)
    {
        Debug.Log($"[HandleFlowerReplacementCoroutine] Started. tileId: {tileId}, playerIndex: {playerIndex}, IsTsumoTile: {IsTsumoTile}");

        // tileId가 34인지 확인
        if (tileId != 34)
        {
            Debug.LogError($"[HandleFlowerReplacementCoroutine] Invalid tileId {tileId}. It is not 34 (flower tile).");
            onComplete?.Invoke();
            yield break;
        }

        GameObject FlowerField = null;
        GameObject haipaiPrefab = null;

        // FlowerField 및 haipaiPrefab 결정
        if (playerIndex == PlayerIndex)
        {
            FlowerField = PlayerFlowerField;
            haipaiPrefab = PlayerHaipai;
            Debug.Log($"[HandleFlowerReplacementCoroutine] Player's FlowerField: {FlowerField?.name}, Player's Haipai: {haipaiPrefab?.name}");
        }
        else
        {
            int relativeIndex = GetRelativeIndex(playerIndex);
            Debug.Log($"[HandleFlowerReplacementCoroutine] Get relative index {relativeIndex} for playerIndex: {playerIndex}");

            switch (relativeIndex)
            {
                case 0:
                    FlowerField = EnemyFlowerFieldShimo;
                    haipaiPrefab = EnemyHaipaiShimo;
                    break;
                case 1:
                    FlowerField = EnemyFlowerFieldToi;
                    haipaiPrefab = EnemyHaipaiToi;
                    break;
                case 2:
                    FlowerField = EnemyFlowerFieldKami;
                    haipaiPrefab = EnemyHaipaiKami;
                    break;
                default:
                    Debug.LogError($"[HandleFlowerReplacementCoroutine] Invalid relative index: {relativeIndex}");
                    onComplete?.Invoke();
                    yield break;
            }

            Debug.Log($"[HandleFlowerReplacementCoroutine] Enemy's FlowerField: {FlowerField?.name}, Enemy's Haipai: {haipaiPrefab?.name}");
        }

        // FlowerField가 null인 경우 처리 중단
        if (FlowerField == null)
        {
            Debug.LogError("[HandleFlowerReplacementCoroutine] FlowerField is null. Cannot proceed.");
            onComplete?.Invoke();
            yield break;
        }

        // haipaiPrefab 처리
        if (haipaiPrefab != null)
        {
            TileGrid haipaiTileGrid = haipaiPrefab.GetComponent<TileGrid>();
            if (haipaiTileGrid != null)
            {
                if (playerIndex != PlayerIndex)
                {
                    Debug.Log($"[HandleFlowerReplacementCoroutine] Showing Tedashi for playerIndex: {playerIndex}, IsTsumoTile: {!IsTsumoTile}");
                    haipaiTileGrid.ShowTedashi(!IsTsumoTile);
                }
                else
                {
                    Debug.Log($"[HandleFlowerReplacementCoroutine] Destroying tileId: {tileId} in player's Haipai.");
                    haipaiTileGrid.DestoryByTileId(tileId);
                }
            }
            else
            {
                Debug.LogError($"[HandleFlowerReplacementCoroutine] TileGrid component not found on haipaiPrefab: {haipaiPrefab.name}");
            }
        }
        else
        {
            Debug.LogWarning("[HandleFlowerReplacementCoroutine] HaipaiPrefab is null. Skipping Tedashi or Destroy.");
        }

        // FlowerField의 활성화 상태 확인 및 Count 업데이트
        TMP_Text countText = FlowerField.transform.Find("Count").GetComponent<TMP_Text>();
        if (countText == null)
        {
            Debug.LogError($"[HandleFlowerReplacementCoroutine] Count TMP_Text not found in {FlowerField.name}");
            onComplete?.Invoke();
            yield break;
        }

        if (!FlowerField.activeSelf)
        {
            Debug.Log($"[HandleFlowerReplacementCoroutine] Activating FlowerField: {FlowerField.name} and initializing count to ×1.");
            FlowerField.SetActive(true);
            countText.text = "×1";
        }
        else
        {
            string currentText = countText.text.Replace("×", "").Trim();
            Debug.Log($"[HandleFlowerReplacementCoroutine] Current Count Text in {FlowerField.name}: {currentText}");

            if (int.TryParse(currentText, out int currentCount))
            {
                currentCount++;
                if (currentCount > 8)
                {
                    Debug.LogWarning($"[HandleFlowerReplacementCoroutine] Count exceeds maximum value (8) in {FlowerField.name}. Clamping to 8.");
                    currentCount = 8;
                }
                countText.text = $"×{currentCount}";
                Debug.Log($"[HandleFlowerReplacementCoroutine] Updated Count Text in {FlowerField.name}: {countText.text}");
            }
            else
            {
                Debug.LogError($"[HandleFlowerReplacementCoroutine] Failed to parse Count text in {FlowerField.name}: {currentText}");
            }
        }

        Debug.Log($"[HandleFlowerReplacementCoroutine] Flower replacement handled successfully for PlayerIndex {playerIndex}. FlowerField: {FlowerField.name}, Final Count: {countText.text}");
        onComplete?.Invoke();
    }



    [TargetRpc]
    public void TargetFlowerReplacement(NetworkConnection target, int tileId, int playerIndex, bool IsTsumoTile)
    {
        Debug.Log($"[TargetFlowerReplacement] Started. Target: {target}, tileId: {tileId}, playerIndex: {playerIndex}, IsTsumoTile: {IsTsumoTile}");

        // 현재 씬에서 모든 PlayerManager 객체를 가져옵니다.
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        Debug.Log($"[TargetFlowerReplacement] Found {allPlayerManagers.Length} PlayerManager instances in the scene.");

        // 모든 PlayerManager 객체를 순회합니다.
        foreach (var playerManager in allPlayerManagers)
        {
            Debug.Log($"[TargetFlowerReplacement] Checking PlayerManager. PlayerIndex: {playerManager.PlayerIndex}, isOwned: {playerManager.isOwned}");

            // 현재 로컬 플레이어가 소유한 PlayerManager인지 확인합니다.
            if (playerManager.isOwned)
            {
                Debug.Log($"[TargetFlowerReplacement] Found owned PlayerManager. PlayerIndex: {playerManager.PlayerIndex}");

                // 타일 교체 작업이 진행 중임을 표시하는 플래그 설정
                playerManager.IsTargetReplacementTileRunning = true;

                // 작업 완료 여부를 확인하기 위한 플래그 초기화
                bool isComplete = false;

                // 코루틴 실행
                Debug.Log($"[TargetFlowerReplacement] Starting HandleFlowerReplacementCoroutine for PlayerIndex: {playerManager.PlayerIndex}");
                playerManager.StartCoroutine(playerManager.HandleFlowerReplacementCoroutine(tileId, playerIndex, IsTsumoTile, () =>
                {
                    Debug.Log($"[TargetFlowerReplacement] HandleFlowerReplacementCoroutine completed for tileId: {tileId}, playerIndex: {playerIndex}");
                    isComplete = true;

                    // 작업 완료 플래그 해제
                    playerManager.IsTargetReplacementTileRunning = false;

                    // 서버에 작업 완료 보고
                    Debug.Log($"[TargetFlowerReplacement] Reporting completion to server for PlayerIndex: {playerManager.PlayerIndex}");
                    playerManager.CmdSetFlowerReplacementFlagEnd();
                }));

                // 작업 완료될 때까지 대기하는 코루틴 실행
                Debug.Log($"[TargetFlowerReplacement] Starting WaitUntilComplete for PlayerIndex: {playerManager.PlayerIndex}");
                playerManager.StartCoroutine(WaitUntilComplete(() => isComplete));

                Debug.Log($"[TargetFlowerReplacement] Exiting loop after handling PlayerIndex: {playerManager.PlayerIndex}");
                return;
            }
        }

        Debug.Log("[TargetFlowerReplacement] No owned PlayerManager found. Exiting.");
    }


    [TargetRpc]
    public void TargetDiscardTile(NetworkConnection target, int tileId, int playerIndex, bool IsTsumoTile)
    {
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        foreach (var playerManager in allPlayerManagers)
        {
            if (playerManager.isOwned)
            {
                Debug.Log($"Executing TargetRpc on owned PlayerManager. PlayerIndex: {playerManager.PlayerIndex}");
                // TargetDiscardTile 시작
                playerManager.IsTargetDiscardTileRunning = true;

                // 완료 여부를 확인하기 위한 플래그
                bool isComplete = false;

                // 코루틴 실행 및 완료 시 플래그 설정
                playerManager.StartCoroutine(playerManager.HandleTileDiscardCoroutine(tileId, playerIndex, IsTsumoTile, () =>
                {
                    Debug.Log($"HandleTileDiscardCoroutine for tileId {tileId} and playerIndex {playerIndex} completed.");
                    isComplete = true;
                    playerManager.IsTargetDiscardTileRunning = false;
                }));

                // 대기 루프
                playerManager.StartCoroutine(WaitUntilComplete(() => isComplete));
                return;
            }
        }
    }

    // 대기 루프를 구현한 코루틴
    private IEnumerator WaitUntilComplete(Func<bool> completionCheck)
    {
        yield return new WaitUntil(completionCheck);
        Debug.Log("TargetDiscardTile operation completed.");
    }



    // 상대적인 인덱스를 계산 (Shimo, Kami, Toi 순서)
    public int GetRelativeIndex(int otherPlayerIndex)
    {
        for (int i = 0; i < EnemyIndexMap.Length; i++)
        {
            if (EnemyIndexMap[i] == otherPlayerIndex)
            {
                return i; // Shimo (0), Toi (1), Kami (2) 순서로 반환
            }
        }
        return -1; // 예상치 못한 경우
    }


    [TargetRpc]
    public void TargetSetEnemyIndexMap(NetworkConnection target, int[] playerIndices, int startIndex)
    {

        // 다음 인덱스를 계산하고 EnemyIndexMap에 저장
        for (int i = 0; i < EnemyIndexMap.Length; i++)
        {
            EnemyIndexMap[i] = playerIndices[(startIndex + i + 1) % playerIndices.Length];
        }

        Debug.Log($"EnemyIndexMap updated for Player position {startIndex}: {string.Join(", ", EnemyIndexMap)}");
    }


    private void emptyGrid(GameObject gameObject)
    {
        TileGrid tileGrid = gameObject.GetComponent<TileGrid>();
        if (tileGrid == null)
        {
            Debug.LogError($"TileGrid component not found on {gameObject.name}.");
            return;
        }
        Debug.Log($"Empty Grid Function on {gameObject.name}");
        tileGrid.EmptyAll();
    }


    public override void OnStartServer()
    {
        base.OnStartServer();

        // ServerManager 찾기
        if (serverManager == null)
        {
            serverManager = UnityEngine.Object.FindAnyObjectByType<ServerManager>();

            if (serverManager == null)
            {
                Debug.LogError("ServerManager not found in the scene.");
                return;
            }

            Debug.Log("ServerManager assigned to PlayerManager.");
        }

        // GameObject 초기화
        PlayerHaipai = GameObject.Find("PlayerHaipai");
        EnemyHaipaiToi = GameObject.Find("EnemyHaipaiToi");
        EnemyHaipaiKami = GameObject.Find("EnemyHaipaiKami");
        EnemyHaipaiShimo = GameObject.Find("EnemyHaipaiShimo");

        PlayerKawa = GameObject.Find("PlayerKawa");
        EnemyKawaToi = GameObject.Find("EnemyKawaToi");
        EnemyKawaKami = GameObject.Find("EnemyKawaKami");
        EnemyKawaShimo = GameObject.Find("EnemyKawaShimo");
        EnemyKawaList = new GameObject[3];
        EnemyKawaList[0] = EnemyKawaShimo;
        EnemyKawaList[1] = EnemyKawaToi;
        EnemyKawaList[2] = EnemyKawaKami;
        EnemyHaipaiList = new GameObject[3];
        EnemyHaipaiList[0] = EnemyHaipaiShimo;
        EnemyHaipaiList[1] = EnemyHaipaiToi;
        EnemyHaipaiList[2] = EnemyHaipaiKami;

        PlayerFuroField = GameObject.Find("PlayerFuroField");

        // ServerManager에 플레이어 등록
        serverManager.IncrementPlayerCount();
    }


    private IEnumerator WaitForTargetDiscardTile()
    {
        while (IsTargetDiscardTileRunning)
        {
            Debug.Log("TargetDiscardTile is still running. Waiting...");
            yield return null; // 다음 프레임까지 대기
        }

        Debug.Log("TargetDiscardTile has completed. Proceeding with emptyGrid...");
        emptyGrid(EnemyKawaToi);
        emptyGrid(PlayerKawa);
        emptyGrid(PlayerHaipai);
        emptyGrid(EnemyHaipaiKami);
        emptyGrid(EnemyHaipaiShimo);
        emptyGrid(EnemyHaipaiToi);
        emptyGrid(EnemyKawaKami);
        emptyGrid(EnemyKawaShimo);
    }


    public void InitializeRoundState()
    {
        var gameStatusUI = GameStatusUI.GetComponent<GameStatusUI>();
        gameStatusUI.IsUpdated = false;
        Debug.Log($"InitializeRoundState, Player Index: {PlayerIndex}, SeatWind: {playerStatus.SeatWind}, RoundWind: {playerStatus.RoundWind}");

        // TargetDiscardTile 실행 중인지 확인
        StartCoroutine(WaitForTargetDiscardTile());

        remainingTime = -1;

        PlayerHandTiles.Clear();
        PlayerCallBlocksList.Clear();
        for (int i = 0; i < EnemyCallBlocksList.Length; i++)
        {
            if (EnemyCallBlocksList[i] == null)
            {
                EnemyCallBlocksList[i] = new List<int>();
            }
            else
            {
                EnemyCallBlocksList[i].Clear();
            }
        }

        for (int i = 0; i < EnemyHandTilesCount.Length; i++)
        {
            EnemyHandTilesCount[i] = 13;
        }

        PlayerWinningCondition = new WinningCondition();

        PlayerKawaTiles.Clear();

        for (int i = 0; i < EnemyKawaTiles.Length; i++)
        {
            if (EnemyKawaTiles[i] == null)
            {
                EnemyKawaTiles[i] = new List<int>();
            }
            else
            {
                EnemyKawaTiles[i].Clear();
            }
        }
        if (GameStatusUI != null)
        {
            if (gameStatusUI != null)
            {
                gameStatusUI.Initialize();
            }
        }
        DestoryAllChildrenOfFuroFields();
        DisableFlowerFields();
        CmdSetInitializeFlagEnd();
    }

    [Command]
    public void CmdSetInitializeFlagEnd()
    {
        EndFlag_InitializePlayer = true;
    }

    private bool EndFlag_InitializePlayer;
    [Server]
    public void SetInitializeFlagFalse()
    {
        EndFlag_InitializePlayer = false;
    }
    [Server]
    public bool IsInitializationComplete()
    {
        return EndFlag_InitializePlayer;
    }
    [Server]
    public void InitializePlayerOnClient(Wind seatWind, Wind roundWind)
    {
        Debug.Log($"InitializePlayerOnClient called for PlayerManager[{PlayerIndex}]. SeatWind: {seatWind}, RoundWind: {roundWind}");

        // playerStatus 생성자 사용
        playerStatus = new PlayerStatus(playerStatus.CurrentScore, seatWind, roundWind);
        Debug.Log($"PlayerManager[{PlayerIndex}]: PlayerStatus initialized. SeatWind: {seatWind}, RoundWind: {roundWind}, IsPlayerTurn: {playerStatus.IsPlayerTurn}, CurrentScore: {playerStatus.CurrentScore}");

        var networkIdentity = GetComponent<NetworkIdentity>();
        if (networkIdentity == null)
        {
            Debug.LogError($"PlayerManager[{PlayerIndex}]: NetworkIdentity is null.");
            return;
        }

        // TargetRpc 호출
        TargetInitializePlayer(networkIdentity.connectionToClient, seatWind, roundWind);
        Debug.Log($"PlayerManager[{PlayerIndex}]: TargetInitializePlayer called.");
    }


    [TargetRpc]
    public void TargetInitializePlayer(NetworkConnection target, Wind seatWind, Wind roundWind)
    {
        Debug.Log($"TargetInitializePlayer called for PlayerManager[{PlayerIndex}]. SeatWind: {seatWind}, RoundWind: {roundWind}");
        InitializeRoundState();
        Debug.Log($"PlayerManager[{PlayerIndex}]: PlayerStatus set on client.");
    }




    public bool CheckIfPlayerTurn()
    {
        return playerStatus.IsPlayerTurn;
    }
}