using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Shared
{


    public static class YakuDictionary
    {
        public static readonly Dictionary<int, string> dict = new Dictionary<int, string>
        {
            { -1, "Flower Point" },
            { 0, "Chicken Hand" },
            { 1, "Chained Seven Pairs" },
            { 2, "Thirteen Orphans" },
            { 3, "Big Four Winds" },
            { 4, "Big Three Dragons" },
            { 5, "Nine Gates" },
            { 6, "All Green" },
            { 7, "Four Quads" },
            { 8, "Four Concealed Pungs" },
            { 9, "All Terminals" },
            { 10, "Little Four Winds" },
            { 11, "Little Three Dragons" },
            { 12, "All Honors" },
            { 13, "Pure Terminal Chows" },
            { 14, "Quadruple Chow" },
            { 15, "Four Pure Shifted Pungs" },
            { 16, "Four Pure Shifted Chows" },
            { 17, "Three Kongs" },
            { 18, "All Terminals And Honors" },
            { 19, "Seven Pairs" },
            { 20, "Greater Honors And Knitted Tiles" },
            { 21, "All Even Pungs" },
            { 22, "Full Flush" },
            { 23, "Upper Tiles" },
            { 24, "Middle Tiles" },
            { 25, "Lower Tiles" },
            { 26, "Pure Triple Chow" },
            { 27, "Pure Shifted Pungs" },
            { 28, "Pure Straight" },
            { 29, "Three Suited Terminal Chows" },
            { 30, "Pure Shifted Chows" },
            { 31, "All Fives" },
            { 32, "Triple Pung" },
            { 33, "Three Concealed Pungs" },
            { 34, "Lesser Honors And Knitted Tiles" },
            { 35, "Knitted Straight" },
            { 36, "Upper Four" },
            { 37, "Lower Four" },
            { 38, "Big Three Winds" },
            { 39, "Last Tile Draw" },
            { 40, "Last Tile Claim" },
            { 41, "Out With Replacement Tile" },
            { 42, "Robbing The Kong" },
            { 43, "Mixed Straight" },
            { 44, "Mixed Triple Chow" },
            { 45, "Reversible Tiles" },
            { 46, "Mixed Shifted Pungs" },
            { 47, "Two Concealed Kongs" },
            { 48, "Melded Hand" },
            { 49, "Mixed Shifted Chows" },
            { 50, "All Pungs" },
            { 51, "Half Flush" },
            { 52, "All Types" },
            { 53, "Two Dragons" },
            { 54, "Fully Concealed Hand" },
            { 55, "Last Tile" },
            { 56, "Outside Hand" },
            { 57, "Two Melded Kongs" },
            { 58, "Concealed Hand" },
            { 59, "Dragon Pung" },
            { 60, "Prevalent Wind" },
            { 61, "Seat Wind" },
            { 62, "All Chows" },
            { 63, "Double Pung" },
            { 64, "Two Concealed Pungs" },
            { 65, "Tile Hog" },
            { 66, "Concealed Kong" },
            { 67, "All Simples" },
            { 68, "Pure Double Chow" },
            { 69, "Mixed Double Chow" },
            { 70, "Short Straight" },
            { 71, "Two Terminal Chows" },
            { 72, "Pung Of Terminals Or Honors" },
            { 73, "One Voided Suit" },
            { 74, "No Honors" },
            { 75, "Single Wait" },
            { 76, "Melded Kong" },
            { 77, "Self Drawn" },
            { 78, "Edge Wait" },
            { 79, "Closed Wait" }
        };
    }


    public static class TileDictionary
    {
        public static readonly Dictionary<int, string> NumToString = new Dictionary<int, string>();
        public static readonly Dictionary<string, int> StringToNum = new Dictionary<string, int>();

        static TileDictionary()
        {
            for (int i = 0; i < 34; i++)
            {
                string tileName = TileNumToString(i);
                NumToString[i] = tileName;
                StringToNum[tileName] = i;
            }
            NumToString[34] = "0f";
            StringToNum["0f"] = 34;
        }

        private static string TileNumToString(int tileNum)
        {
            string tileName = "";
            if (tileNum >= 0 && tileNum < 34)
            {
                tileName += (char)('1' + tileNum % 9);
                if (tileNum < 9) tileName += "m";
                else if (tileNum < 18) tileName += "p";
                else if (tileNum < 27) tileName += "s";
                else tileName += "z";
            }
            else if (tileNum == 34)
            {
                tileName = "0f";
            }
            return tileName;
        }
    }

    [System.Serializable]
    public struct PlayerStatus
    {
        public int CurrentScore;
        public Wind SeatWind;
        public Wind RoundWind;
        public bool IsPlayerTurn;

        public PlayerStatus(Wind seatWind, Wind roundWind)
        {
            CurrentScore = 0;
            SeatWind = seatWind;
            RoundWind = roundWind;
            IsPlayerTurn = (seatWind == Wind.EAST);
        }

        public PlayerStatus(int currentScore, Wind seatWind, Wind roundWind)
        {
            CurrentScore = currentScore;
            SeatWind = seatWind;
            RoundWind = roundWind;
            IsPlayerTurn = (seatWind == Wind.EAST);
        }

        public override string ToString()
        {
            return $"Score: {CurrentScore}, SeatWind: {SeatWind}, RoundWind: {RoundWind}, IsPlayerTurn: {IsPlayerTurn}";
        }
    }

    public enum KanType
    {
        ISNOTKAN = -1,
        ANKAN,
        SHOMINKAN,
        DAIMINKAN
    }

    public enum ActionType 
    { 
        TIMEOUT = -2,
        SKIP,
        HU,
        KAN,
        PON,
        CHII,
        FLOWER
    }

    public struct ActionPriorityInfo : IComparable<ActionPriorityInfo>
    {
        public ActionType Type;
        public int Priority;
        public int TileId;

        public ActionPriorityInfo(ActionType type, int priority, int tileId)
        {
            Type = type;
            Priority = priority;
            TileId = tileId;
        }

        public int CompareTo(ActionPriorityInfo other)
        {
            // Type 기준으로 우선 정렬
            int typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0)
            {
                return typeComparison;
            }
            int PriorityComparision = Priority.CompareTo(other.Priority);
            if (PriorityComparision != 0)
            {
                return PriorityComparision;
            }
            // Type이 같으면 Priority 기준으로 정렬
            return TileId.CompareTo(other.TileId);
        }
        public override string ToString()
        {
            return $"Type: {Type}, Priority: {Priority}, TileId: {TileId}";
        }
    }


    public enum BlockType
    {
        SEQUENCE,
        TRIPLET,
        QUAD,
        KNITTED,
        PAIR,
        SINGLETILE
    }

    public enum BlockSource
    {
        SELF,
        SHIMOCHA,
        TOIMEN,
        KAMICHA
    }

    public enum Wind
    {
        EAST = 27,
        SOUTH = 28,
        WEST = 29,
        NORTH = 30,
        END = 31
    }

    public enum TileType
    {
        MANZU = 0,
        PINZU = 1,
        SOUZU = 2,
        HONOR = 3,
        FLOWER = 4
    }

    public class Block
    {
        public BlockType Type;
        public int Tile;
        public BlockSource Source;
        public int SourceTileIndex;

        public Block() { }

        public Block(BlockType type, int tile)
        {
            Type = type;
            Tile = tile;
            Source = BlockSource.SELF;
            SourceTileIndex = 0;
        }

        public Block(BlockType type, int tile, BlockSource source, int sourceTileIndex = 0)
        {
            Type = type;
            Tile = tile;
            Source = source;
            SourceTileIndex = sourceTileIndex;
        }
    }

    public class WinningCondition
    {
        public int WinningTile;
        public bool IsDiscarded;
        public bool IsLastTileInTheGame;
        public bool IsLastTileOfItsKind;
        public bool IsReplacementTile;
        public bool IsRobbingTheKong;

        public WinningCondition() {
            WinningTile = -1;
            IsDiscarded = false;
            IsLastTileInTheGame = false;
            IsLastTileOfItsKind = false;
            IsReplacementTile = false;
            IsRobbingTheKong = false;
        }

        public WinningCondition(int winningTile, bool isDiscarded, bool isLastTileInTheGame, bool isLastTileOfItsKind, bool isReplacementTile, bool isRobbingTheKong)
        {
            WinningTile = winningTile;
            IsDiscarded = isDiscarded;
            IsLastTileInTheGame = isLastTileInTheGame;
            IsLastTileOfItsKind = isLastTileOfItsKind;
            IsReplacementTile = isReplacementTile;
            IsRobbingTheKong = isRobbingTheKong;
        }
    }

    [System.Serializable]
    public class Hand
    {
        public List<int> ClosedTiles { get; private set; }
        public List<int> OpenedTiles { get; private set; }
        public List<Block> CallBlocks { get; private set; }
        public int FlowerPoint { get; set; }
        public int WinningTile { get; set; }
        public List<KeyValuePair<int, int>> YakuScoreList { get; private set; }
        public int HighestScore { get; set; }
        public List<int> KeishikiTenpaiTiles { get; private set; }
        private int TilesLeftToDraw { get; set; }
        public Hand()
        {
            Initialize();
        }

        
        public Hand(Hand originalHand)
        {
            ClosedTiles = new List<int>(originalHand.ClosedTiles);
            OpenedTiles = new List<int>(originalHand.OpenedTiles);
            CallBlocks = originalHand.CallBlocks
                                     .Select(block => new Block(block.Type, block.Tile, block.Source, block.SourceTileIndex))
                                     .ToList();
            FlowerPoint = originalHand.FlowerPoint;
            WinningTile = originalHand.WinningTile;
            YakuScoreList = new List<KeyValuePair<int, int>>(originalHand.YakuScoreList);
            HighestScore = originalHand.HighestScore;
            KeishikiTenpaiTiles = new List<int>(originalHand.KeishikiTenpaiTiles);
        }


        private bool TileOutOfRange(int tile)
        {
            return tile < 0 || tile >= 35;
        }
        private void Initialize()
        {
            ClosedTiles = Enumerable.Repeat(0, 35).ToList(); // 초기값 0으로 35개 생성
            OpenedTiles = Enumerable.Repeat(0, 35).ToList(); // 초기값 0으로 35개 생성
            CallBlocks = new List<Block>();
            YakuScoreList = new List<KeyValuePair<int, int>>();
            KeishikiTenpaiTiles = new List<int>();
            FlowerPoint = 0;
            WinningTile = -1;
            HighestScore = 0;
            TilesLeftToDraw = 14;
        }

        public int GetRightmostTileId()
        {
            if (WinningTile != -1)
            {
                return WinningTile;
            }
            for (int i = 34; i >= 0; --i)
            {
                if (ClosedTiles[i] > 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public int ApplyAnKan(int sourceTileId, int startTileId, BlockSource blockSource)
        {
            if (TilesLeftToDraw != 0 || TileOutOfRange(sourceTileId) || TileOutOfRange(startTileId))
            {
                return -1;
            }

            OpenedTiles[startTileId] += 4;
            ClosedTiles[startTileId] -= 4;
            WinningTile = -1;
            CallBlocks.Add(new Block(BlockType.QUAD, startTileId, BlockSource.SELF, 0));
            TilesLeftToDraw += 1;

            Debug.Log("[ApplyAnKan]");
            PrintHand();
            PrintHandNames();
            return 0;
        }

        public int ApplyShominKan(int sourceTileId, int startTileId, BlockSource blockSource)
        {
            if (TilesLeftToDraw != 0 || TileOutOfRange(sourceTileId) || TileOutOfRange(startTileId))
            {
                return -1;
            }
            WinningTile = -1;
            ClosedTiles[startTileId] -= 1;
            OpenedTiles[startTileId] += 1;
            TilesLeftToDraw += 1;

            foreach (Block block in CallBlocks)
            {
                if (block != null)
                {
                    if (block.Type == BlockType.TRIPLET && block.Tile == startTileId)
                    {
                        block.Type = BlockType.QUAD;
                        Debug.Log("[ApplyShominKan]");
                        PrintHand();
                        PrintHandNames();
                        return 0;
                    }
                }
            }
            Debug.LogError("[ApplyShominKan] Failed.");
            return -1;
        }

        public int ApplyDaiminKan(int sourceTileId, int startTileId, BlockSource blockSource)
        {
            if (TilesLeftToDraw != 1 || TileOutOfRange(sourceTileId) || TileOutOfRange(startTileId))
            {
                return -1;
            }
            OpenedTiles[startTileId] += 4;
            ClosedTiles[startTileId] -= 3;
            CallBlocks.Add(new Block(BlockType.QUAD, startTileId, blockSource, 0));
            WinningTile = -1;
            Debug.Log("[ApplyDaiminKan]");
            PrintHand();
            PrintHandNames();
            return 0;
        }

        public int ApplyPon(int sourceTileId, int startTileId, BlockSource blockSource)
        {
            if (TilesLeftToDraw != 1 || TileOutOfRange(sourceTileId) || TileOutOfRange(startTileId))
            {
                return -1;
            }
            OpenedTiles[startTileId] += 3;
            ClosedTiles[startTileId] -= 2;
            CallBlocks.Add(new Block(BlockType.TRIPLET, startTileId, blockSource, 0));
            TilesLeftToDraw -= 1;
            WinningTile = -1;
            Debug.Log("[ApplyPon]");
            PrintHand();
            PrintHandNames();
            return 0;
        }

        public int ApplyChii(int sourceTileId, int startTileId)
        {
            if (TilesLeftToDraw != 1 || TileOutOfRange(sourceTileId) || TileOutOfRange(startTileId))
            {
                return -1;
            }
            int sourceTileIndex = 0;
            for (int i = 0; i < 3; ++i)
            {
                int targetTileId = startTileId + i;
                OpenedTiles[targetTileId] += 1;
                if (targetTileId == sourceTileId)
                {
                    sourceTileIndex = i;
                    continue;
                }
                ClosedTiles[targetTileId] -= 1;
            }
            CallBlocks.Add(new Block(BlockType.SEQUENCE, startTileId, BlockSource.KAMICHA, sourceTileIndex));
            TilesLeftToDraw -= 1;
            WinningTile = -1;
            Debug.Log("[ApplyChii]");
            PrintHand();
            PrintHandNames();
            return 0;
        }
        public int DrawFirstHand(List<int> hand)
        {
            Debug.Log($"[DrawFirstHand] Attempting to draw hand: {string.Join(", ", hand.Select(t => TileDictionary.NumToString[t]))}");

            if (hand == null || hand.Count != 13 || TilesLeftToDraw != 14)
            {
                Debug.LogError("[DrawFirstHand] Invalid hand or incorrect draw state.");
                return -1;
            }
            for (int i = 0; i < hand.Count; i++)
            {
                if (TileOutOfRange(hand[i]))
                {
                    Debug.LogError($"[DrawFirstHand] Tile {hand[i]} is out of range.");
                    return -1;
                }
                ClosedTiles[hand[i]] += 1;
                TilesLeftToDraw -= 1;
            }

            Debug.Log("[DrawFirstHand] Successfully drew first hand.");
            PrintHand();
            PrintHandNames();
            return 0;
        }

        public int DiscardOneTile(int tile)
        {
            Debug.Log($"[DiscardOneTile] Attempting to discard tile: {TileDictionary.NumToString[tile]}");

            if (TilesLeftToDraw != 0 || TileOutOfRange(tile) || ClosedTiles[tile] <= 0)
            {
                Debug.LogError("[DiscardOneTile] Invalid discard operation.");
                return -1;
            }
            ClosedTiles[tile] -= 1;
            TilesLeftToDraw += 1;
            WinningTile = -1;

            Debug.Log("[DiscardOneTile] Successfully discarded tile.");
            PrintHand();
            PrintHandNames();
            return 0;
        }

        public int TsumoOneTile(int tile)
        {
            Debug.Log($"[TsumoOneTile] Attempting to tsumo tile: {TileDictionary.NumToString[tile]}");

            if (TilesLeftToDraw != 1 || TileOutOfRange(tile))
            {
                PrintHandNames();
                Debug.LogError($"[TsumoOneTile] Tile Left to Draw: {TilesLeftToDraw}");
                Debug.LogError("[TsumoOneTile] Invalid tsumo operation.");
                return -1;
            }
            ClosedTiles[tile] += 1;
            TilesLeftToDraw -= 1;
            WinningTile = tile;

            Debug.Log("[TsumoOneTile] Successfully tsumoed tile.");
            PrintHand();
            PrintHandNames();
            return 0;
        }

        public void PrintHand()
        {
            var closedTilesOutput = string.Join(" ", ClosedTiles.Select((value, index) => index % 9 == 0 && index != 0 ? $"\n{value}" : value.ToString()));
            var openedTilesOutput = string.Join(" ", OpenedTiles.Select((value, index) => index % 9 == 0 && index != 0 ? $"\n{value}" : value.ToString()));
            Debug.Log($"[PrintHand] Closed Tiles:\n{closedTilesOutput}\n[PrintHand] Opened Tiles:\n{openedTilesOutput}\nFlower Points: {FlowerPoint}");
            PrintCallBlocks();
        }

        public void PrintHandNames()
        {
            var closedTilesNames = string.Join(" ", ClosedTiles.SelectMany((value, index) => Enumerable.Repeat(TileDictionary.NumToString[index], value)));
            var openedTilesNames = string.Join(" ", OpenedTiles.SelectMany((value, index) => Enumerable.Repeat(TileDictionary.NumToString[index], value)));
            var winningTileName = WinningTile >= 0 && WinningTile < TileDictionary.NumToString.Count ? TileDictionary.NumToString[WinningTile] : "None";

            Debug.Log($"[PrintHandNames] Closed Tiles: {closedTilesNames}\n[PrintHandNames] Opened Tiles: {openedTilesNames}\nFlower Points: {FlowerPoint}\nWinning Tile: {winningTileName}");
            Debug.Log($"Tiles Left to Draw: {TilesLeftToDraw}");
            PrintCallBlocks();
        }

        // CallBlocks 출력 메서드
        public void PrintCallBlocks()
        {
            if (CallBlocks == null || CallBlocks.Count == 0)
            {
                Debug.Log("CallBlocks가 비어있습니다.");
                return;
            }

            Debug.Log("CallBlocks 출력:");
            foreach (var block in CallBlocks)
            {
                Debug.Log(GetBlockString(block));
            }
        }

        // Block 정보를 문자열로 변환하는 헬퍼 함수
        private string GetBlockString(Block block)
        {
            string result = "";

            if (block.Source != BlockSource.SELF)
                result += "[";
            else if (block.Type == BlockType.QUAD)
                result += "{";

            switch (block.Type)
            {
                case BlockType.PAIR:
                    result += $"{TileDictionary.NumToString[block.Tile]}{TileDictionary.NumToString[block.Tile]}";
                    break;

                case BlockType.SEQUENCE:
                    for (int i = 0; i < 3; i++)
                    {
                        result += TileDictionary.NumToString[block.Tile + i];
                    }
                    break;

                case BlockType.TRIPLET:
                    for (int i = 0; i < 3; i++)
                    {
                        result += TileDictionary.NumToString[block.Tile];
                    }
                    break;

                case BlockType.QUAD:
                    for (int i = 0; i < 4; i++)
                    {
                        result += TileDictionary.NumToString[block.Tile];
                    }
                    break;

                case BlockType.SINGLETILE:
                    result += TileDictionary.NumToString[block.Tile];
                    break;

                case BlockType.KNITTED:
                    for (int i = 0; i < 3; i++)
                    {
                        result += TileDictionary.NumToString[block.Tile + i * 3];
                    }
                    break;

                default:
                    result += $"{block.Type} {TileDictionary.NumToString[block.Tile]} {block.Source}";
                    break;
            }

            if (block.Source != BlockSource.SELF)
                result += "]";
            else if (block.Type == BlockType.QUAD)
                result += "}";

            return result;
        }
    }
}
