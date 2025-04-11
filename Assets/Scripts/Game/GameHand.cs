using System;
using System.Collections.Generic;
using System.Linq;
using MCRGame.Common; // GameTile, GameAction, GameActionType, RelativeSeat, WinningConditions, CallBlockData 등이 정의되어 있음
using UnityEngine;
using MCRGame.UI;

namespace MCRGame.Game
{
    public class GameHand
    {
        // 각 타일의 개수를 저장하는 역할
        public Dictionary<GameTile, int> Tiles { get; private set; }
        // 호출(Call) 블록 데이터 목록 (내부 논리용: UI 오브젝트가 아닌 데이터만 관리)
        public List<CallBlockData> CallBlockData { get; private set; }
        // 마지막으로 뽑은(또는 점수 계산 시 사용) 타일 (tsumo tile)
        public GameTile? TsumoTile { get; private set; }

        public const int FULL_HAND_SIZE = 14;

        // FLOWER_TILES: 모든 꽃 타일을 카운트 형태로 저장 (각 타일은 1로 카운트)
        public static readonly Dictionary<GameTile, int> FLOWER_TILES = GetFlowerTilesCounter();

        private static Dictionary<GameTile, int> GetFlowerTilesCounter()
        {
            var dict = new Dictionary<GameTile, int>();
            // GameTileExtensions.FlowerTiles() 는 꽃 타일들을 IEnumerable<GameTile>로 반환한다고 가정
            foreach (var tile in GameTileExtensions.FlowerTiles())
            {
                if (!dict.ContainsKey(tile))
                {
                    dict[tile] = 1;
                }
            }
            return dict;
        }

        public GameHand()
        {
            Tiles = new Dictionary<GameTile, int>();
            CallBlockData = new List<CallBlockData>();
            TsumoTile = null;
        }

        // 주어진 타일 목록으로 GameHand를 생성합니다.
        public static GameHand CreateFromTiles(List<GameTile> tiles)
        {
            var hand = new GameHand();
            foreach (var tile in tiles)
            {
                if (hand.Tiles.ContainsKey(tile))
                    hand.Tiles[tile]++;
                else
                    hand.Tiles[tile] = 1;
            }
            return hand;
        }

        // 꽃 타일이 하나라도 있으면 true 반환
        public bool HasFlower
        {
            get
            {
                foreach (var kv in FLOWER_TILES)
                {
                    if (Tiles.ContainsKey(kv.Key) && Tiles[kv.Key] > 0)
                        return true;
                }
                return false;
            }
        }

        // 꽃 타일이 있다면, TsumoTile이 꽃이면 해당 타일을 버리고,
        // 그렇지 않으면 손에 있는 꽃 타일 중 하나를 버립니다.
        public void ApplyFlower()
        {
            if (!HasFlower)
                throw new InvalidOperationException("Cannot apply flower: hand doesn't have flower tile");

            if (TsumoTile.HasValue && GameTileExtensions.IsFlower(TsumoTile.Value))
            {
                ApplyDiscard(TsumoTile.Value);
            }
            else
            {
                foreach (var flowerTile in GameTileExtensions.FlowerTiles())
                {
                    if (Tiles.ContainsKey(flowerTile) && Tiles[flowerTile] > 0)
                    {
                        ApplyDiscard(flowerTile);
                        break;
                    }
                }
            }
        }

        // 손 크기: 보유한 타일 수 + 호출 블록 수 * 3 (호출은 3장 조합으로 간주)
        public int HandSize
        {
            get
            {
                int total = Tiles.Values.Sum();
                total += CallBlockData.Count * 3;
                return total;
            }
        }

        // tsumo(자기 차례에 뽑은 타일) 적용: 손의 크기가 가득 찼으면 예외 발생
        public GameTile ApplyTsumo(GameTile tile)
        {
            if (HandSize >= FULL_HAND_SIZE)
                throw new InvalidOperationException("Cannot apply tsumo: hand is already full.");
            if (Tiles.ContainsKey(tile))
                Tiles[tile]++;
            else
                Tiles[tile] = 1;
            TsumoTile = tile;
            return tile;
        }

        // 오른쪽(정렬 시 마지막) 타일을 반환합니다.
        public GameTile? GetRightmostTile()
        {
            if (TsumoTile.HasValue)
                return TsumoTile.Value;
            if (Tiles.Count == 0)
                return null;
            var sorted = Tiles.Keys.ToList();
            sorted.Sort();
            return sorted.Last();
        }

        // 타일 버리기: 해당 타일이 있으면 1장 제거하고, TsumoTile은 초기화합니다.
        public void ApplyDiscard(GameTile tile)
        {
            if (!Tiles.ContainsKey(tile) || Tiles[tile] <= 0)
                throw new InvalidOperationException($"Cannot apply discard: hand doesn't have tile {tile}");
            RemoveTiles(tile, 1);
            TsumoTile = null;
        }

        // 호출(Call) 블록 적용: 단순히 내부 데이터에 등록합니다.
        public void ApplyCall(CallBlockData cbData)
        {
            switch (cbData.Type)
            {
                case CallBlockType.CHII:
                    ApplyChii(cbData);
                    break;
                case CallBlockType.PUNG:
                    ApplyPung(cbData);
                    break;
                case CallBlockType.AN_KONG:
                    ApplyAnKong(cbData);
                    break;
                case CallBlockType.DAIMIN_KONG:
                    ApplyDaiminKong(cbData);
                    break;
                case CallBlockType.SHOMIN_KONG:
                    ApplyShominKong(cbData);
                    break;
            }
        }

        // 내부적으로 타일 개수를 감소시킵니다.
        private void RemoveTiles(GameTile tile, int count)
        {
            if (Tiles.ContainsKey(tile))
            {
                Tiles[tile] -= count;
                if (Tiles[tile] <= 0)
                    Tiles.Remove(tile);
            }
        }

        // 호출 액션 적용 함수들: 내부 데이터에 적용 후 CallBlockDatas에 등록
        public void ApplyChii(CallBlockData cbData)
        {
            var deleteTileList = new List<GameTile>();
            for (int index = 0; index < 3; index++)
            {
                if (index != cbData.SourceTileIndex)
                {
                    int rawVal = (int)cbData.FirstTile + index;
                    if (Enum.IsDefined(typeof(GameTile), rawVal))
                    {
                        deleteTileList.Add((GameTile)rawVal);
                    }
                }
            }
            foreach (var tile in deleteTileList)
            {
                if (!Tiles.ContainsKey(tile) || Tiles[tile] <= 0)
                {
                    throw new InvalidOperationException("Cannot apply chii: not enough valid tiles to chii");
                }
            }
            foreach (var tile in deleteTileList)
            {
                RemoveTiles(tile, 1);
            }
            CallBlockData.Add(cbData);
        }

        public void ApplyPung(CallBlockData cbData)
        {
            if (!Tiles.ContainsKey(cbData.FirstTile) || Tiles[cbData.FirstTile] < 2)
            {
                throw new InvalidOperationException("Cannot apply pung: not enough valid tiles to pung");
            }
            RemoveTiles(cbData.FirstTile, 2);
            CallBlockData.Add(cbData);
        }

        public void ApplyAnKong(CallBlockData cbData)
        {
            if (!Tiles.ContainsKey(cbData.FirstTile) || Tiles[cbData.FirstTile] < 4)
            {
                throw new InvalidOperationException("Cannot apply ankong: not enough valid tiles to ankong");
            }
            RemoveTiles(cbData.FirstTile, 4);
            CallBlockData.Add(cbData);
            TsumoTile = null;
        }

        public void ApplyDaiminKong(CallBlockData cbData)
        {
            if (!Tiles.ContainsKey(cbData.FirstTile) || Tiles[cbData.FirstTile] < 3)
            {
                throw new InvalidOperationException("Cannot apply daiminkong: not enough valid tiles to daiminkong");
            }
            RemoveTiles(cbData.FirstTile, 3);
            CallBlockData.Add(cbData);
        }

        public void ApplyShominKong(CallBlockData cbData)
        {
            if (!Tiles.ContainsKey(cbData.FirstTile))
            {
                throw new InvalidOperationException("Cannot apply shominkong: not enough valid tiles to shominkong");
            }
            CallBlockData targetData = null;
            foreach (var data in CallBlockData)
            {
                if (data.Type == CallBlockType.PUNG && data.FirstTile.Equals(cbData.FirstTile))
                {
                    targetData = data;
                    break;
                }
            }
            if (targetData == null)
            {
                throw new InvalidOperationException("Cannot apply shominkong: hand doesn't have valid pung block");
            }
            RemoveTiles(cbData.FirstTile, 1);
            targetData.Type = CallBlockType.SHOMIN_KONG;
            TsumoTile = null;
            CallBlockData.Add(targetData);
        }

        public List<GameAction> GetPossibleChiiGameActions(RelativeSeat priority, WinningConditions winningCondition)
        {
            var result = new List<GameAction>();
            if (!winningCondition.WinningTile.HasValue)
                throw new InvalidOperationException("[GameHand.GetPossibleChiiGameActions] tile is null");
            GameTile tile = winningCondition.WinningTile.Value;
            if (winningCondition.IsLastTileInTheGame || !tile.IsNumber())
                return result;
            for (int delta = -2; delta <= 0; delta++)
            {
                var chiiTileList = new List<GameTile>();
                bool valid = true;
                for (int index = 0; index < 3; index++)
                {
                    int rawTile = (int)tile + delta + index;
                    if (Enum.IsDefined(typeof(GameTile), rawTile))
                    {
                        var newTile = (GameTile)rawTile;
                        if (newTile.ToString()[0] != tile.ToString()[0])
                        {
                            valid = false;
                            break;
                        }
                        if (!Tiles.ContainsKey(newTile))
                        {
                            valid = false;
                            break;
                        }
                        if (newTile.Equals(tile))
                        {
                            valid = false;
                            break;
                        }
                        chiiTileList.Add(newTile);
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid && chiiTileList.Count == 2)
                {
                    result.Add(new GameAction
                    {
                        Type = GameActionType.CHII,
                        SeatPriority = priority,
                        Tile = (GameTile)((int)tile + delta)
                    });
                }
            }
            return result;
        }

        public List<GameAction> GetPossiblePonGameActions(RelativeSeat priority, WinningConditions winningCondition)
        {
            var result = new List<GameAction>();
            if (!winningCondition.WinningTile.HasValue)
                throw new InvalidOperationException("[GameHand.GetPossiblePonGameActions] tile is null");
            GameTile tile = winningCondition.WinningTile.Value;
            if (Tiles.ContainsKey(tile) &&
                Tiles[tile] >= 2 &&
                !winningCondition.IsLastTileInTheGame &&
                winningCondition.IsDiscarded)
            {
                result.Add(new GameAction { Type = GameActionType.PON, SeatPriority = priority, Tile = tile });
            }
            return result;
        }

        public List<GameAction> GetPossibleKanGameActions(RelativeSeat priority, WinningConditions winningCondition)
        {
            var result = new List<GameAction>();
            if (!winningCondition.WinningTile.HasValue)
                throw new InvalidOperationException("[GameHand.GetPossibleKanGameActions] tile is null");
            GameTile tile = winningCondition.WinningTile.Value;
            if (winningCondition.IsLastTileInTheGame)
                return result;
            if (winningCondition.IsDiscarded)
            {
                if (Tiles.ContainsKey(tile) && Tiles[tile] >= 3)
                {
                    result.Add(new GameAction { Type = GameActionType.KAN, SeatPriority = priority, Tile = tile });
                }
            }
            else
            {
                foreach (var kv in Tiles)
                {
                    if (kv.Value == 4 || (kv.Value == 3 && kv.Key.Equals(tile)))
                    {
                        result.Add(new GameAction { Type = GameActionType.KAN, SeatPriority = priority, Tile = kv.Key });
                    }
                }
                foreach (var cb in CallBlockData)
                {
                    if (cb.Type == CallBlockType.PUNG &&
                        (cb.FirstTile.Equals(tile) || Tiles.ContainsKey(cb.FirstTile)))
                    {
                        result.Add(new GameAction { Type = GameActionType.KAN, SeatPriority = priority, Tile = cb.FirstTile });
                    }
                }
            }
            return result;
        }
    }
}
