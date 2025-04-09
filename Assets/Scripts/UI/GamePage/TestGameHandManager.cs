using UnityEngine;
using System;
using System.Linq;
using MCRGame.Common;   // GameTile, CallBlockData, WinningConditions, CallBlockType, RelativeSeat
using MCRGame.UI;       // GameHandManager, CallBlockField

public class TestGameHandManager : MonoBehaviour
{
    [SerializeField] private GameHandManager gameHandManager;

    void Start()
    {
        if (gameHandManager == null)
        {
            gameHandManager = FindAnyObjectByType<GameHandManager>();
            if (gameHandManager == null)
                Debug.LogError("GameHandManager를 찾을 수 없습니다.");
        }
    }

    void OnGUI()
    {
        // 한 칸 오른쪽으로 위치
        if (GUI.Button(new Rect(170, 10, 150, 30), "Add Tile"))
            OnAddTile();
        if (GUI.Button(new Rect(170, 50, 150, 30), "Test Call"))
            OnTestCall();
        if (GUI.Button(new Rect(170, 90, 150, 30), "Discard Tile"))
            OnDiscardTile();
    }

    // tsumo 테스트
    void OnAddTile()
    {
        if (gameHandManager == null) return;
        var normals = GameTileExtensions.NormalTiles().ToList();
        var tile = normals[UnityEngine.Random.Range(0, normals.Count)];
        gameHandManager.AddTsumo(tile);
        Debug.Log($"Add Tile: {tile} tsumo");
    }

    // 가능한 Call 찾아 ApplyCall
    void OnTestCall()
    {
        if (gameHandManager == null) return;
        var hand = gameHandManager.GameHand;
        if (hand == null) return;

        RelativeSeat priority = RelativeSeat.TOI;
        var condBase = new WinningConditions
        {
            IsDiscarded = true,
            IsLastTileInTheGame = false
        };

        // PUNG
        foreach (var kv in hand.Tiles)
        {
            condBase.WinningTile = kv.Key;
            var pon = hand.GetPossiblePonGameActions(priority, condBase);
            if (pon.Count > 0)
            {
                var tile = pon[0].Tile;
                var cb = new CallBlockData(CallBlockType.PUNG, tile, priority, 0);
                gameHandManager.ApplyCall(cb);
                Debug.Log($"Test Call: PUNG {tile}");
                return;
            }
        }

        // KAN
        foreach (var kv in hand.Tiles)
        {
            condBase.WinningTile = kv.Key;
            var kan = hand.GetPossibleKanGameActions(priority, condBase);
            if (kan.Count > 0)
            {
                var tile = kan[0].Tile;
                var cb = new CallBlockData(CallBlockType.DAIMIN_KONG, tile, priority, 0);
                gameHandManager.ApplyCall(cb);
                Debug.Log($"Test Call: KAN {tile}");
                return;
            }
        }

        // CHII
        foreach (var kv in hand.Tiles)
        {
            condBase.WinningTile = kv.Key;
            var chii = hand.GetPossibleChiiGameActions(priority, condBase);
            if (chii.Count > 0)
            {
                var tile = chii[0].Tile;
                var cb = new CallBlockData(CallBlockType.CHII, tile, priority, 1);
                gameHandManager.ApplyCall(cb);
                Debug.Log($"Test Call: CHII start {tile}");
                return;
            }
        }

        Debug.LogWarning("Test Call: 가능한 Call이 없습니다.");
    }

    // 폐기 테스트
    void OnDiscardTile()
    {
        if (gameHandManager == null) return;
        var tm = gameHandManager.GetComponentInChildren<TileManager>();
        if (tm != null)
        {
            gameHandManager.DiscardTile(tm);
            Debug.Log("Discard Tile: 요청");
        }
        else
        {
            Debug.LogError("폐기할 TileManager를 찾을 수 없습니다.");
        }
    }
}
