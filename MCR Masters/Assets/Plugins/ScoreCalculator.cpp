

// dllmain.cpp : DLL 애플리케이션의 진입점을 정의합니다.
// #include "pch.h"
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <utility>
#include <map>
#include <cstring>
#include <cstdlib>
#include <fstream>
#include <mutex>
#include <stack>

std::mutex initMutex;
bool initialized = false;
// #pragma warning(disable : 4996) // C4996 에러를 무시

// #include <iostream>
// #include <vector>
// #include <string>
// #include <cstring>
// #include <algorithm>
// #include <utility>
// #include <map>

// #define DEBUG

// Define the YAKU_LIST using X Macros with Yaku name and YakuCheck function
#define YAKU_LIST                                                           \
    X(ChickenHand, YakuCheck_ChickenHand)                                   \
    X(ChainedSevenPairs, YakuCheck_ChainedSevenPairs)                       \
    X(ThirteenOrphans, YakuCheck_ThirteenOrphans)                           \
    X(BigFourWinds, YakuCheck_BigFourWinds)                                 \
    X(BigThreeDragons, YakuCheck_BigThreeDragons)                           \
    X(NineGates, YakuCheck_NineGates)                                       \
    X(AllGreen, YakuCheck_AllGreen)                                         \
    X(FourQuads, YakuCheck_FourQuads)                                       \
    X(FourConcealedPungs, YakuCheck_FourConcealedPungs)                     \
    X(AllTerminals, YakuCheck_AllTerminals)                                 \
    X(LittleFourWinds, YakuCheck_LittleFourWinds)                           \
    X(LittleThreeDragons, YakuCheck_LittleThreeDragons)                     \
    X(AllHonors, YakuCheck_AllHonors)                                       \
    X(PureTerminalChows, YakuCheck_PureTerminalChows)                       \
    X(QuadrupleChow, YakuCheck_QuadrupleChow)                               \
    X(FourPureShiftedPungs, YakuCheck_FourPureShiftedPungs)                 \
    X(FourPureShiftedChows, YakuCheck_FourPureShiftedChows)                 \
    X(ThreeKongs, YakuCheck_ThreeKongs)                                     \
    X(AllTerminalsAndHonors, YakuCheck_AllTerminalsAndHonors)               \
    X(SevenPairs, YakuCheck_SevenPairs)                                     \
    X(GreaterHonorsAndKnittedTiles, YakuCheck_GreaterHonorsAndKnittedTiles) \
    X(AllEvenPungs, YakuCheck_AllEvenPungs)                                 \
    X(FullFlush, YakuCheck_FullFlush)                                       \
    X(UpperTiles, YakuCheck_UpperTiles)                                     \
    X(MiddleTiles, YakuCheck_MiddleTiles)                                   \
    X(LowerTiles, YakuCheck_LowerTiles)                                     \
    X(PureTripleChow, YakuCheck_PureTripleChow)                             \
    X(PureShiftedPungs, YakuCheck_PureShiftedPungs)                         \
    X(PureStraight, YakuCheck_PureStraight)                                 \
    X(ThreeSuitedTerminalChows, YakuCheck_ThreeSuitedTerminalChows)         \
    X(PureShiftedChows, YakuCheck_PureShiftedChows)                         \
    X(AllFives, YakuCheck_AllFives)                                         \
    X(TriplePung, YakuCheck_TriplePung)                                     \
    X(ThreeConcealedPungs, YakuCheck_ThreeConcealedPungs)                   \
    X(LesserHonorsAndKnittedTiles, YakuCheck_LesserHonorsAndKnittedTiles)   \
    X(KnittedStraight, YakuCheck_KnittedStraight)                           \
    X(UpperFour, YakuCheck_UpperFour)                                       \
    X(LowerFour, YakuCheck_LowerFour)                                       \
    X(BigThreeWinds, YakuCheck_BigThreeWinds)                               \
    X(LastTileDraw, YakuCheck_LastTileDraw)                                 \
    X(LastTileClaim, YakuCheck_LastTileClaim)                               \
    X(OutWithReplacementTile, YakuCheck_OutWithReplacementTile)             \
    X(RobbingTheKong, YakuCheck_RobbingTheKong)                             \
    X(MixedStraight, YakuCheck_MixedStraight)                               \
    X(MixedTripleChow, YakuCheck_MixedTripleChow)                           \
    X(ReversibleTiles, YakuCheck_ReversibleTiles)                           \
    X(MixedShiftedPungs, YakuCheck_MixedShiftedPungs)                       \
    X(TwoConcealedKongs, YakuCheck_TwoConcealedKongs)                       \
    X(MeldedHand, YakuCheck_MeldedHand)                                     \
    X(MixedShiftedChows, YakuCheck_MixedShiftedChows)                       \
    X(AllPungs, YakuCheck_AllPungs)                                         \
    X(HalfFlush, YakuCheck_HalfFlush)                                       \
    X(AllTypes, YakuCheck_AllTypes)                                         \
    X(TwoDragons, YakuCheck_TwoDragons)                                     \
    X(FullyConcealedHand, YakuCheck_FullyConcealedHand)                     \
    X(LastTile, YakuCheck_LastTile)                                         \
    X(OutsideHand, YakuCheck_OutsideHand)                                   \
    X(TwoMeldedKongs, YakuCheck_TwoMeldedKongs)                             \
    X(ConcealedHand, YakuCheck_ConcealedHand)                               \
    X(DragonPung, YakuCheck_DragonPung)                                     \
    X(PrevalentWind, YakuCheck_PrevalentWind)                               \
    X(SeatWind, YakuCheck_SeatWind)                                         \
    X(AllChows, YakuCheck_AllChows)                                         \
    X(DoublePung, YakuCheck_DoublePung)                                     \
    X(TwoConcealedPungs, YakuCheck_TwoConcealedPungs)                       \
    X(TileHog, YakuCheck_TileHog)                                           \
    X(ConcealedKong, YakuCheck_ConcealedKong)                               \
    X(AllSimples, YakuCheck_AllSimples)                                     \
    X(PureDoubleChow, YakuCheck_PureDoubleChow)                             \
    X(MixedDoubleChow, YakuCheck_MixedDoubleChow)                           \
    X(ShortStraight, YakuCheck_ShortStraight)                               \
    X(TwoTerminalChows, YakuCheck_TwoTerminalChows)                         \
    X(PungOfTerminalsOrHonors, YakuCheck_PungOfTerminalsOrHonors)           \
    X(OneVoidedSuit, YakuCheck_OneVoidedSuit)                               \
    X(NoHonors, YakuCheck_NoHonors)                                         \
    X(SingleWait, YakuCheck_SingleWait)                                     \
    X(MeldedKong, YakuCheck_MeldedKong)                                     \
    X(SelfDrawn, YakuCheck_SelfDrawn)                                       \
    X(EdgeWait, YakuCheck_EdgeWait)                                         \
    X(ClosedWait, YakuCheck_ClosedWait)

// Define an enum for Yaku using the YAKU_LIST
enum Yaku
{
#define X(YakuName, YakuFunc) Yaku_##YakuName,
    YAKU_LIST
#undef X
        Yaku_Count // Total number of Yaku
};

enum Blocktype
{
    SEQUENCE,
    TRIPLET,
    QUAD,
    KNITTED,
    PAIR,
    SINGLETILE
};

enum Blocksource
{
    SELF,
    SHIMOCHA,
    TOIMEN,
    KAMICHA
};

enum Wind
{
    EAST = 27,
    SOUTH = 28,
    WEST = 29,
    NORTH = 30
};

enum TileType
{
    MANZU = 0,
    PINZU = 1,
    SOUZU = 2,
    HONOR = 3,
    FLOWER = 4
};

std::string tileNumToString(int tile_num);
int tileNameToNum(std::string tile_name);
// static std::vector<int> is_chain_yaku = {};
// static std::vector<std::vector<int>> chain_yaku_index = {};

static std::vector<int> is_chain_yaku = {};
static std::vector<std::vector<int>> chain_yaku_index = {};


class Block
{
public:
    Blocktype type;        // 0 sequence 1 triplet 2 quad 3 knitted 4 pair 5 one tile block(for special shape)
    int tile;              // first tile of block
    Blocksource source;    // 0 self 1 shimocha 2 toimen 3 kamicha
    int source_tile_index; // 0 defalut 0~3 open sequence

    Block();
    Block(Blocktype init_type, int init_tile);
    Block(Blocktype init_type, int init_tile, Blocksource init_source, int init_source_tile_index = 0);
};

class WinningCondition
{
public:
    int tile;
    bool is_discarded;
    bool is_last_tile_in_the_game;
    bool is_last_tile_of_its_kind;
    bool is_replacement_tile;
    bool is_robbing_the_kong;
    int count_winning_conditions;
    int seat_wind;
    int round_wind;

    WinningCondition();
    WinningCondition(int init_tile, bool init_is_discarded, bool init_is_last_tile_in_the_game, bool init_is_last_tile_of_its_kind, bool init_is_replacement_tile, bool init_is_robbing_the_kong, int init_count_winning_conditions, int init_seat_wind, int init_round_wind);
};

void PrintBlocks_detail(std::vector<Block> &blocks);
void PrintBlocks(std::vector<Block> &blocks);
void PrintBlock(Block &block);
// when tedashi, must check it's not from concealed kong
class Hand
{
public:
    std::vector<int> closed_tiles; // include ankong tiles
    std::vector<int> opened_tiles;
    std::vector<Block> call_blocks;
    // int flower_point;
    int winning_tile;
    // int seat_wind;
    // int round_wind;
    bool is_blocks_divided;
    std::vector<std::pair<int, int>> yaku_score_list;
    int highest_score;
    std::vector<int> keishiki_tenpai_tiles;

    Hand();
    Hand(std::vector<int> init_hand);
    Hand(std::vector<int> init_hand, int init_seat_wind, int init_round_wind);
    void Initialize_OpenedTiles(std::vector<Block> init_call_blocks);
    bool IsOpen();

    // 쯔모패가 오면 유효한 화료인지도 있어야함
    // 블럭 나누기
    int DivideBlocks(WinningCondition winning_condition);
    int DivideGeneralBlocks(WinningCondition winning_condition);
    int DivideKnittedInGeneralBlocks(WinningCondition winning_condition);
    int DivideSevenPairsBlocks(WinningCondition winning_condition);
    int DivideHonorsAndKnittedTilesBlocks(WinningCondition winning_condition);
    int DivdeThirteenOrphansBlocks(WinningCondition winning_condition);

    void GetKeishikiTenpaiTiles();
    void DivideBlocksForTenpaiTiles();

    void print_score();
    void print_hand()
    {
        std::cout << "[closed tiles]\n";
        for (int i = 0; i < closed_tiles.size(); ++i)
        {
            std::cout << closed_tiles[i] << ' ';
            if (i % 9 == 8)
                std::cout << '\n';
        }
        std::cout << '\n';
        std::cout << "[opened tiles]\n";
        for (int i = 0; i < opened_tiles.size(); ++i)
        {
            std::cout << opened_tiles[i] << ' ';
            if (i % 9 == 8)
                std::cout << '\n';
        }
        std::cout << '\n';
        // std::cout << "flower points: " << flower_point << std::endl;
    }
    void print_hand_name()
    {
        std::cout << "[closed tiles]\n";
        for (int i = 0; i < closed_tiles.size(); ++i)
        {
            for (int j = 0; j < closed_tiles[i]; ++j)
            {
                std::cout << tileNumToString(i) << ' ';
            }
        }
        std::cout << '\n';
        std::cout << "[opened tiles]\n";
        for (int i = 0; i < opened_tiles.size(); ++i)
        {
            for (int j = 0; j < opened_tiles[i]; ++j)
            {
                std::cout << tileNumToString(i) << ' ';
            }
        }
        std::cout << '\n';
        // std::cout << "flower points: " << flower_point << std::endl;
        std::cout << "winning tile: " << tileNumToString(winning_tile) << std::endl;
    }
};
void Hand::Initialize_OpenedTiles(std::vector<Block> init_call_blocks)
{
    opened_tiles = std::vector<int>(34, 0);
    call_blocks = init_call_blocks;

    for (const auto &block : call_blocks)
    {
        if (block.type == SEQUENCE)
        {
            opened_tiles[block.tile] += 1;
            opened_tiles[block.tile + 1] += 1;
            opened_tiles[block.tile + 2] += 1;
        }
        else if (block.type == TRIPLET)
        {
            opened_tiles[block.tile] += 3;
        }
        else if (block.type == QUAD)
        {
            opened_tiles[block.tile] += 4;
        }
    }
}

// Define the function pointer type for YakuCheck functions
typedef int (*YakuFunction)(int, std::vector<Block> &,
                            std::vector<int> &,
                            std::vector<int> &,
                            WinningCondition &);

std::string tileNumToString(int tile_num)
{
    std::string tile_name;
    if (tile_num >= 0 && tile_num < 34)
    {
        tile_name += (char)('1' + tile_num % 9);
        if (tile_num >= 0 && tile_num < 9)
        {
            tile_name += 'm';
        }
        else if (tile_num >= 9 && tile_num < 18)
        {
            tile_name += 'p';
        }
        else if (tile_num >= 18 && tile_num < 27)
        {
            tile_name += 's';
        }
        else
        {
            tile_name += 'z';
        }
    }
    else if (tile_num == 34)
    {
        tile_name = "0f";
    }
    return tile_name;
}

int tileNameToNum(std::string tile_name)
{
    int tile_num = 0;
    if (tile_name.size() != 2)
    {
        return -1;
    }
    if (tile_name[1] == 'f')
    {
        tile_num = 34;
    }
    else
    {
        tile_num += (int)(tile_name[0] - '1');
        if (tile_name[1] == 'p')
        {
            tile_num += 9;
        }
        else if (tile_name[1] == 's')
        {
            tile_num += 18;
        }
        else if (tile_name[1] == 'z')
        {
            tile_num += 27;
        }
    }

    return tile_num;
}

bool IsHonor(int tile)
{
    return (tile >= 27 && tile < 34);
}
bool IsNumber(int tile)
{
    return (tile >= 0 && tile < 27);
}
bool IsManzu(int tile)
{
    return (tile >= 0 && tile < 9);
}
bool IsPinzu(int tile)
{
    return (tile >= 9 && tile < 18);
}
bool IsSouzu(int tile)
{
    return (tile >= 18 && tile < 27);
}
bool IsWind(int tile)
{
    return (tile >= 27 && tile < 31);
}
bool IsDragon(int tile)
{
    return (tile >= 31 && tile < 34);
}

bool IsTerminal(int tile)
{
    if (tile >= 0 && tile < 27)
    {
        if (tile % 9 == 0 || tile % 9 == 8)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    return false;
}

bool IsOutside(int tile)
{
    if (tile >= 0 && tile < 27)
    {
        if (tile % 9 == 0 || tile % 9 == 8)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    else if (tile >= 27 && tile < 34)
    {
        return true;
    }
    return false;
}

bool HasOpenedBlocks(std::vector<Block> &blocks)
{
    for (Block &block : blocks)
    {
        if (block.source != SELF)
        {
            return true;
        }
    }
    return false;
}

// static std::vector<int> is_chain_yaku = {};
// static std::vector<std::vector<int>> chain_yaku_index = {};
static std::map<std::string, int> yaku_map = {};
static std::map<int, std::string> inverse_yaku_map = {};
static std::map<YakuFunction, std::string> YakuFunctionToName_Map = {};


// // Initialize the yaku_map using YAKU_LIST
// static std::map<std::string, int> yaku_map = {};

// // Initialize inverse_yaku_map using YAKU_LIST
// static std::map<int, std::string> inverse_yaku_map = {};

// // Map to associate YakuFunction pointers with Yaku names
// static std::map<YakuFunction, std::string> YakuFunctionToName_Map = {};

void InitializeChainYakuIndex();
void InitializeYakuFunctionToNameMap();
void InitializeInverseYakuMap();
void InitializeYakuMap();
void InitializeAll();
void InitializeAllOnce();

std::vector<int> BlocksToTiles(std::vector<Block> &blocks)
{
    std::vector<int> tiles(35, 0);
    for (Block &block : blocks)
    {
        if (block.type == SEQUENCE)
        {
            tiles[block.tile] += 1;
            tiles[block.tile + 1] += 1;
            tiles[block.tile + 2] += 1;
        }
        else if (block.type == TRIPLET)
        {
            tiles[block.tile] += 3;
        }
        else if (block.type == QUAD)
        {
            tiles[block.tile] += 4;
        }
        else if (block.type == KNITTED)
        {
            tiles[block.tile] += 1;
            tiles[block.tile + 3] += 1;
            tiles[block.tile + 6] += 1;
        }
        else if (block.type == PAIR)
        {
            tiles[block.tile] += 2;
        }
        else if (block.type == SINGLETILE)
        {
            tiles[block.tile] += 1;
        }
    }
    return tiles;
}

Block::Block()
{
    tile = source_tile_index = 0;
    type = SEQUENCE;
    source = SELF;
}

Block::Block(Blocktype init_type, int init_tile)
{
    type = init_type;
    tile = init_tile;
    source = SELF;
    source_tile_index = 0;
}

Block::Block(Blocktype init_type, int init_tile, Blocksource init_source, int init_source_tile_index)
{
    type = init_type;
    tile = init_tile;
    source = init_source;
    source_tile_index = init_source_tile_index;
}

WinningCondition::WinningCondition()
{
    tile = count_winning_conditions = 0;
    is_discarded = is_last_tile_in_the_game = is_last_tile_of_its_kind = is_replacement_tile = is_robbing_the_kong = false;
    seat_wind = round_wind = EAST;
}

WinningCondition::WinningCondition(int init_tile, bool init_is_discarded, bool init_is_last_tile_in_the_game, bool init_is_last_tile_of_its_kind, bool init_is_replacement_tile, bool init_is_robbing_the_kong, int init_count_winning_conditions, int init_seat_wind, int init_round_wind)
{
    tile = init_tile;
    is_discarded = init_is_discarded;
    is_last_tile_in_the_game = init_is_last_tile_in_the_game;
    is_last_tile_of_its_kind = init_is_last_tile_of_its_kind;
    is_replacement_tile = init_is_replacement_tile;
    is_robbing_the_kong = init_is_robbing_the_kong;
    count_winning_conditions = init_count_winning_conditions;
    seat_wind = init_seat_wind;
    round_wind = init_round_wind;
}

void PrintBlock(Block &block)
{
    if (block.source != SELF)
        std::cout << '[';
    else if (block.type == QUAD)
        std::cout << '{';
    if (block.type == PAIR)
    {
        for (int i = 0; i < 2; ++i)
            std::cout << tileNumToString(block.tile);
    }
    else if (block.type == SEQUENCE)
    {
        for (int i = 0; i < 3; ++i)
        {
            std::cout << tileNumToString(block.tile + i);
        }
    }
    else if (block.type == TRIPLET)
    {
        for (int i = 0; i < 3; ++i)
        {
            std::cout << tileNumToString(block.tile);
        }
    }
    else if (block.type == QUAD)
    {
        for (int i = 0; i < 4; ++i)
        {
            std::cout << tileNumToString(block.tile);
        }
        // std::cout << ' ';
    }
    else if (block.type == SINGLETILE)
    {
        std::cout << tileNumToString(block.tile);
    }
    else if (block.type == KNITTED)
    {
        for (int i = 0; i < 3; ++i)
        {
            std::cout << tileNumToString(block.tile + i * 3);
        }
    }
    else
    {
        std::cout << block.type << ' ' << tileNumToString(block.tile) << ' ' << block.source << std::endl;
    }
    if (block.source != SELF)
        std::cout << ']';
    else if (block.type == QUAD)
        std::cout << '}';
    std::cout << ' ';
}

void PrintBlocks_detail(std::vector<Block> &blocks)
{
    for (Block &block : blocks)
    {
        std::cout << block.type << ' ' << tileNumToString(block.tile) << ' ' << block.source << std::endl;
    }
}

void PrintBlocks(std::vector<Block> &blocks)
{
    for (Block &block : blocks)
    {
        PrintBlock(block);
    }
    std::cout << std::endl;
}

void Print_YakuScoreVector(std::vector<std::pair<int, int>> yaku_score_vector)
{
    for (auto p : yaku_score_vector)
    {
        std::cout << inverse_yaku_map[p.first] << ": " << p.second << '\n';
    }
    std::cout << std::endl;
}

// dummy function for indexing from 1
int YakuCheck_ChickenHand(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    return 0;
}

// impling lower yaku (ex. no honors, half flush, one voided suit, full flush, seven pairs, concealed hand, single wait)
// out of block chain
int YakuCheck_ChainedSevenPairs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 7)
    {
        return 0;
    }

    std::vector<int> count(9, 0);
    int color = -1;
    for (Block &block : blocks)
    {
        if (block.type != PAIR || !IsNumber(block.tile))
        {
            return 0;
        }
        if (color == -1)
        {
            color = block.tile / 9;
        }
        else
        {
            if (color != block.tile / 9)
            {
                return 0;
            }
        }
        count[block.tile % 9] += 1;
    }

    int streak = 0;

    for (int i = 0; i < 9; ++i)
    {
        if (count[i] == 0)
        {
            streak = 0;
        }
        else if (count[i] == 1)
        {
            streak += 1;
        }
        else
        {
            return 0;
        }
        if (streak == 7)
        {
            checked_yaku[yaku_map["YakuCheck_SingleWait"]] -= 1;
            checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
            checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] -= 1;
            checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 2;
            checked_yaku[yaku_map["YakuCheck_HalfFlush"]] -= 6;
            checked_yaku[yaku_map["YakuCheck_FullFlush"]] -= 24;
            checked_yaku[yaku_map["YakuCheck_SevenPairs"]] -= 24;
            checked_yaku[yaku_map["YakuCheck_ChainedSevenPairs"]] = 88;
            return 88;
        }
    }
    return 0;
}

// impling lower yaku (all terminals and honors, all types, concealed hand, single wait)
// out of block chain
int YakuCheck_ThirteenOrphans(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 13)
    {
        return 0;
    }

    checked_yaku[yaku_map["YakuCheck_SingleWait"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 2;
    checked_yaku[yaku_map["YakuCheck_AllTypes"]] -= 6;
    checked_yaku[yaku_map["YakuCheck_AllTerminalsAndHonors"]] -= 32;
    checked_yaku[yaku_map["YakuCheck_ThirteenOrphans"]] = 88;
    return 88;
}

// impling lower yaku (ex. pung of terminal~, big three winds, prevalent wind, seat wind, all pungs)
// out of block chain
int YakuCheck_BigFourWinds(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_wind = 0;
    int honor_count = 0;
    for (Block &block : blocks)
    {
        if (IsWind(block.tile) && (block.type == QUAD || block.type == TRIPLET))
        {
            if ((block.tile != winning_condition.seat_wind) && (block.tile != winning_condition.round_wind))
            {
                honor_count += 1;
            }
            count_wind += 1;
        }
    }
    if (count_wind == 4)
    {
        checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= honor_count;
        checked_yaku[yaku_map["YakuCheck_SeatWind"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_PrevalentWind"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
        checked_yaku[yaku_map["YakuCheck_BigThreeWinds"]] -= 12;
        checked_yaku[yaku_map["YakuCheck_BigFourWinds"]] = 88;
        return 88;
    }
    return 0;
}

// impling lower yaku (two dragon pungs, dragon pung)
// out of block chain
int YakuCheck_BigThreeDragons(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_dragon = 0;
    for (Block &block : blocks)
    {
        if (IsDragon(block.tile) && (block.type == QUAD || block.type == TRIPLET))
        {
            count_dragon += 1;
        }
    }
    if (count_dragon == 3)
    {
        checked_yaku[yaku_map["YakuCheck_DragonPung"]] -= 3;
        checked_yaku[yaku_map[__func__]] = 88;
        return 88;
    }
    return 0;
}

// impling lower yaku (about full flush, no honors, concealed hand) + pungofter~
// out of block chain
int YakuCheck_NineGates(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5 || winning_condition.count_winning_conditions != 9)
    {
        return 0;
    }

    std::vector<int> tile_count(35, 0);
    for (Block block : blocks)
    {
        if (block.source != SELF || !IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == PAIR)
        {
            tile_count[block.tile] += 2;
        }
        else if (block.type == TRIPLET)
        {
            tile_count[block.tile] += 3;
        }
        else if (block.type == SEQUENCE)
        {
            tile_count[block.tile] += 1;
            tile_count[block.tile + 1] += 1;
            tile_count[block.tile + 2] += 1;
        }
        else
        {
            return 0;
        }
    }
    for (int i = 0; i < 3; ++i)
    {
        int index = i * 9;
        bool check_flag = true;
        bool one_tile_flag = false;
        for (int j = index; j < index + 9; ++j)
        {
            if (j == index || j == index + 8)
            {
                tile_count[j] -= 3;
            }
            else
            {
                tile_count[j] -= 1;
            }
            if (tile_count[j] == 1)
            {
                if (one_tile_flag)
                {
                    check_flag = false;
                    break;
                }
                else
                {
                    one_tile_flag = true;
                }
            }
            else if (tile_count[j] != 0)
            {
                check_flag = false;
                break;
            }
        }
        if (check_flag)
        {
            checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= 1;
            checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 2;
            checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
            checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] -= 1;
            checked_yaku[yaku_map["YakuCheck_HalfFlush"]] -= 6;
            checked_yaku[yaku_map["YakuCheck_FullFlush"]] -= 24;
            checked_yaku[yaku_map["YakuCheck_SevenPairs"]] -= 24;
            checked_yaku[yaku_map["YakuCheck_NineGates"]] = 88;
            return 88;
        }
    }

    return 0;
}

// out of block chain
int YakuCheck_AllGreen(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    std::vector<int> green_tiles = {
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 1, 1, 1, 0, 1, 0, 1, 0,
        0, 0, 0, 0, 0, 1, 0};

    for (Block &block : blocks)
    {
        if (block.type == KNITTED)
        {
            return 0;
        }
        if (block.type == SEQUENCE)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (green_tiles[block.tile + i] == 0)
                {
                    return 0;
                }
            }
        }
        else
        {
            if (green_tiles[block.tile] == 0)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map[__func__]] = 88;
    return 88;
}

// impling lower yaku (ex. all pungs, all lower kong yakus: two melded/concealed kongs, concealed kong, melded kong)
// out of block chain
int YakuCheck_FourQuads(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int count_kongs = 0;
    for (Block &block : blocks)
    {
        if (block.type == QUAD)
        {
            count_kongs += 1;
        }
    }
    if (count_kongs == 4)
    {
        checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
        checked_yaku[yaku_map["YakuCheck_MeldedKong"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_ConcealedKong"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoMeldedKongs"]] -= 4;
        checked_yaku[yaku_map["YakuCheck_TwoConcealedKongs"]] -= 8;
        checked_yaku[yaku_map["YakuCheck_ThreeKongs"]] -= 32;
        checked_yaku[yaku_map["YakuCheck_FourQuads"]] = 88;
        return 88;
    }
    else
    {
        return 0;
    }
}

// impling lower yaku (ex. three/two concealed pungs, all pungs, concealed hand)
// out of block chain
int YakuCheck_FourConcealedPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_closed = 0;
    for (Block &block : blocks)
    {
        if ((block.type == TRIPLET || block.type == QUAD) && block.source == SELF)
        {
            if (!winning_condition.is_discarded || block.tile != winning_condition.tile)
            {
                count_closed += 1;
            }
        }
    }
    if (count_closed == 4)
    {
        checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoConcealedPungs"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
        checked_yaku[yaku_map["YakuCheck_ThreeConcealedPungs"]] -= 16;
        checked_yaku[yaku_map[__func__]] = 64;
        return 64;
    }
    else
    {
        return 0;
    }
}

// impling lower yaku (ex. no honors, all terminals and honors, outside hand, PungOfTerminalsOrHonors)
// out of block chain
int YakuCheck_AllTerminals(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!(IsTerminal(block.tile) && (block.type == TRIPLET || block.type == QUAD || block.type == PAIR)))
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_OutsideHand"]] -= 4;
    checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
    checked_yaku[yaku_map["YakuCheck_AllTerminalsAndHonors"]] -= 32;
    checked_yaku[yaku_map[__func__]] = 64;
    return 64;
}

int YakuCheck_LittleFourWinds(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_wind = 0;
    int count_wind_head = 0;
    for (Block &block : blocks)
    {
        if (IsWind(block.tile))
        {
            if (block.type == QUAD || block.type == TRIPLET)
            {
                count_wind += 1;
            }
            else if (block.type == PAIR)
            {
                count_wind_head += 1;
            }
        }
    }
    if (count_wind == 3 && count_wind_head == 1)
    {
        checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_BigThreeWinds"]] -= 12;
        checked_yaku[yaku_map[__func__]] = 64;
        return 64;
    }
    return 0;
}

int YakuCheck_LittleThreeDragons(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_dragon_pung = 0;
    int count_dragon_head = 0;
    for (Block &block : blocks)
    {
        if (IsDragon(block.tile))
        {
            if (block.type == QUAD || block.type == TRIPLET)
            {
                count_dragon_pung += 1;
            }
            else if (block.type == PAIR)
            {
                count_dragon_head += 1;
            }
        }
    }
    if (count_dragon_pung == 2 && count_dragon_head == 1)
    {
        checked_yaku[yaku_map["YakuCheck_DragonPung"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoDragons"]] -= 6;
        checked_yaku[yaku_map[__func__]] = 64;
        return 64;
    }
    return 0;
}

// impling lower yaku
// out of block chain
int YakuCheck_AllHonors(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!(IsHonor(block.tile) && (block.type == TRIPLET || block.type == QUAD || block.type == PAIR)))
        {
            return 0;
        }
    }

    checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_OutsideHand"]] -= 4;
    checked_yaku[yaku_map["YakuCheck_AllTerminalsAndHonors"]] -= 32;
    checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
    checked_yaku[yaku_map[__func__]] = 64;
    return 64;
}

// impling lower yaku (ex. all chows, full flush, pure double chow, two terminal chows)
// out of block chain
int YakuCheck_PureTerminalChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5)
    {
        return 0;
    }
    std::vector<int> count(3, 0);
    int color = -1;
    for (Block block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (color == -1)
        {
            color = block.tile / 9;
        }
        else
        {
            if (color != block.tile / 9)
            {
                return 0;
            }
        }
        if (block.type == SEQUENCE)
        {
            if (block.tile % 9 == 0)
            {
                count[0] += 1;
            }
            else if (block.tile % 9 == 6)
            {
                count[2] += 1;
            }
        }
        else if (block.type == PAIR)
        {
            if (block.tile % 9 == 4)
            {
                count[1] += 1;
            }
        }
    }

    if (count[0] == 2 && count[1] == 1 && count[2] == 2)
    {
        for (int &flag : used_block_flag)
        {
            flag = 1;
        }
        checked_yaku[yaku_map["YakuCheck_AllChows"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_HalfFlush"]] -= 12;
        checked_yaku[yaku_map["YakuCheck_FullFlush"]] -= 24;
        checked_yaku[yaku_map[__func__]] = 64;
        return 64;
    }
    return 0;
}

// impling lower yaku (ex. tile hog)
// in block chain
int YakuCheck_QuadrupleChow(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5)
    {
        return 0;
    }
    int color = -1;
    std::vector<int> sequence_count(7);
    for (Block &block : blocks)
    {
        if (block.type == PAIR)
        {
            continue;
        }
        if (block.type != SEQUENCE)
        {
            return 0;
        }
        if (color == -1)
        {
            color = block.tile / 9;
        }
        else
        {
            if (color != block.tile / 9)
            {
                return 0;
            }
        }
        sequence_count[block.tile % 9] += 1;
    }
    for (int i = 0; i < 7; ++i)
    {
        if (sequence_count[i] == 4)
        {
            for (int &flag : used_block_flag)
            {
                flag = 1;
            }
            checked_yaku[yaku_map["YakuCheck_TileHog"]] -= 2;
            checked_yaku[yaku_map[__func__]] = 48;
            return 48;
        }
    }
    return 0;
}
const int OkFlag_PungOfTerminelsOrHonor = 1557;
// impling lower yaku (ex. all pungs)
// in block chain
int YakuCheck_FourPureShiftedPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5)
    {
        return 0;
    }
    int color = -1;
    std::vector<int> pung_count(9);
    for (Block &block : blocks)
    {
        if (block.type == PAIR)
        {
            continue;
        }
        if (!(block.type == TRIPLET || block.type == QUAD) || !IsNumber(block.tile))
        {
            return 0;
        }
        if (color == -1)
        {
            color = block.tile / 9;
        }
        else
        {
            if (color != block.tile / 9)
            {
                return 0;
            }
        }
        pung_count[block.tile % 9] += 1;
    }
    int streak = 0;
    for (int i = 0; i < 9; ++i)
    {
        if (pung_count[i])
        {
            streak += 1;
        }
        else
        {
            streak = 0;
        }
        if (streak == 4)
        {
            for (int &flag : used_block_flag)
            {
                flag = OkFlag_PungOfTerminelsOrHonor;
            }

            checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
            checked_yaku[yaku_map[__func__]] = 48;
            return 48;
        }
    }
    return 0;
}

// in block chain, but check independently
int YakuCheck_FourPureShiftedChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5)
    {
        return 0;
    }
    int color = -1;
    std::vector<int> sequence_count(7);
    for (Block &block : blocks)
    {
        if (block.type == PAIR)
        {
            continue;
        }
        if (block.type != SEQUENCE)
        {
            return 0;
        }
        if (color == -1)
        {
            color = block.tile / 9;
        }
        else
        {
            if (color != block.tile / 9)
            {
                return 0;
            }
        }
        sequence_count[block.tile % 9] += 1;
    }
    int streak = 0;
    for (int i = 0; i < 7; ++i)
    {
        if (sequence_count[i])
        {
            streak += 1;
        }
        else
        {
            streak = 0;
        }
        if (streak == 4)
        {
            for (int &flag : used_block_flag)
            {
                flag = 1;
            }
            checked_yaku[yaku_map[__func__]] = 32;
            return 32;
        }
    }
    if (sequence_count[0] && sequence_count[2] && sequence_count[4] && sequence_count[6])
    {
        for (int &flag : used_block_flag)
        {
            flag = 1;
        }
        checked_yaku[yaku_map[__func__]] = 32;
        return 32;
    }
    return 0;
}

// implied by Higher yaku (ex. four kongs)
// impling lower yaku (ex. all lower kong yakus: two melded/concealed kongs, concealed kong, melded kong)
// out of block chain
int YakuCheck_ThreeKongs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int count_kongs = 0;
    for (Block &block : blocks)
    {
        if (block.type == QUAD)
        {
            count_kongs += 1;
        }
    }
    if (count_kongs == 3)
    {

        checked_yaku[yaku_map["YakuCheck_MeldedKong"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_ConcealedKong"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoMeldedKongs"]] -= 4;
        checked_yaku[yaku_map["YakuCheck_TwoConcealedKongs"]] -= 8;
        checked_yaku[yaku_map["YakuCheck_ThreeKongs"]] = 32;
        return 32;
    }
    else
    {
        return 0;
    }
}

// implied by Higher yaku (ex. all terminals)
// impling lower yaku (ex. all pungs, outside hand, pung of terminal~)
// out of block chain
int YakuCheck_AllTerminalsAndHonors(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!(block.type == PAIR || block.type == TRIPLET || block.type == QUAD) || !IsOutside(block.tile))
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_OutsideHand"]] -= 4;
    checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
    checked_yaku[yaku_map["YakuCheck_AllTerminalsAndHonors"]] = 32;
    return 32;
}

// impling lower yaku (single wait, concealed hand)
// out of block chain
int YakuCheck_SevenPairs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 7)
    {
        return 0;
    }
    for (Block &block : blocks)
    {
        if (block.type != PAIR)
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_SingleWait"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_SevenPairs"]] = 24;
    return 24;
}

// impling lower yaku (ex. all types, single wait, Concealed Hand, Lesser~)
// out of block chain
int YakuCheck_GreaterHonorsAndKnittedTiles(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 14)
    {
        return 0;
    }
    std::vector<std::vector<int>> table(3, std::vector<int>(3, 0));
    std::vector<int> honor_count(8, 0);
    for (Block &block : blocks)
    {
        if (block.type != SINGLETILE)
        {
            return 0;
        }
        if (IsNumber(block.tile))
        {
            table[block.tile / 9][block.tile % 3] += 1;
        }
        else if (IsHonor(block.tile))
        {
            honor_count[block.tile - 27] += 1;
        }
    }
    for (int i = 0; i < 7; ++i)
    {
        if (honor_count[i] != 1)
        {
            return 0;
        }
    }
    for (int i = 0; i < 3; ++i)
    {
        std::vector<int> zero_count(2, 0);
        for (int j = 0; j < 3; ++j)
        {
            if (table[i][j] == 0)
            {
                zero_count[0] += 1;
            }
            if (table[j][i] == 0)
            {
                zero_count[1] += 1;
            }
        }
        if (zero_count[0] < 2 || zero_count[1] < 2)
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_LesserHonorsAndKnittedTiles"]] -= 12;
    checked_yaku[yaku_map["YakuCheck_GreaterHonorsAndKnittedTiles"]] = 24;
    return 24;
}

// impling lower yaku (ex. all pungs, all simples, no honors)
// out of block chain
int YakuCheck_AllEvenPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5)
    {
        return 0;
    }
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile) || !(block.type == QUAD || block.type == TRIPLET || block.type == PAIR) || block.tile % 9 % 2 != 1)
        {
            return 0;
        }
    }

    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_AllSimples"]] -= 2;
    checked_yaku[yaku_map["YakuCheck_AllPungs"]] -= 6;
    checked_yaku[yaku_map[__func__]] = 24;
    return 24;
}

// impling lower yaku (ex. no honors, half flush, one voided suit)
// out of block chain
int YakuCheck_FullFlush(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int number_type = -1;
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (number_type == -1)
        {
            number_type = block.tile / 9;
        }
        else
        {
            if (number_type != block.tile / 9)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_HalfFlush"]] -= 12;
    checked_yaku[yaku_map["YakuCheck_FullFlush"]] = 24;
    return 24;
}

// impling lower yaku (ex. no honors, upper four)
// out of block chain
int YakuCheck_UpperTiles(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == KNITTED)
        {
            return 0;
        }
        else if (block.type == SEQUENCE)
        {
            if (block.tile % 9 != 6)
            {
                return 0;
            }
        }
        else if (block.type == PAIR || block.type == TRIPLET || block.type == QUAD)
        {
            if (block.tile % 9 < 6)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_UpperFour"]] -= 12;
    checked_yaku[yaku_map[__func__]] = 24;
    return 24;
}

// impling lower yaku (ex. no honors, all simples)
// out of block chain
int YakuCheck_MiddleTiles(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == KNITTED)
        {
            return 0;
        }
        else if (block.type == SEQUENCE)
        {
            if (block.tile % 9 != 3)
            {
                return 0;
            }
        }
        else if (block.type == PAIR || block.type == TRIPLET || block.type == QUAD)
        {
            if (block.tile % 9 < 3 || block.tile % 9 >= 6)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_AllSimples"]] -= 2;
    checked_yaku[yaku_map[__func__]] = 24;
    return 24;
}

// impling lower yaku (ex. no honors, lower four)
// out of block chain
int YakuCheck_LowerTiles(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == KNITTED)
        {
            return 0;
        }
        else if (block.type == SEQUENCE)
        {
            if (block.tile % 9 != 0)
            {
                return 0;
            }
        }
        else if (block.type == PAIR || block.type == TRIPLET || block.type == QUAD)
        {
            if (block.tile % 9 >= 3)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_LowerFour"]] -= 12;
    checked_yaku[yaku_map[__func__]] = 24;
    return 24;
}

// implied by Higher yaku (ex. Quadruple Chow)
// in block chain
int YakuCheck_PureTripleChow(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }

    std::vector<int> valid_block_index;
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile == blocks[index].tile))
        {
            valid_block_index.push_back(i);
        }
    }
    if ((int)valid_block_index.size() == 2)
    {
        if (used_block_flag[valid_block_index[0]] < 0 && used_block_flag[valid_block_index[1]] < 0)
        {
            return 0;
        }
        if (used_block_flag[valid_block_index[0]] < 0)
        {
            used_block_flag[valid_block_index[0]] = 1;
        }
        else
        {
            used_block_flag[valid_block_index[0]] = -yaku_map[__func__];
        }
        if (used_block_flag[valid_block_index[1]] < 0)
        {
            used_block_flag[valid_block_index[1]] = 1;
        }
        else
        {
            used_block_flag[valid_block_index[1]] = -yaku_map[__func__];
        }
        used_block_flag[index] = -yaku_map[__func__];
        checked_yaku[yaku_map[__func__]] = 24;
        return 24;
    }
    return 0;
}

// implied by Higher yaku (ex. Four Pure Shifted Pungs)
// in block chain
int YakuCheck_PureShiftedPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == QUAD || blocks[index].type == TRIPLET))
    {
        return 0;
    }
    const int bias = 2;
    const int table_length = bias * 2 + 1;
    std::vector<int> table(table_length, -1);
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (!IsNumber(blocks[i].tile))
        {
            continue;
        }
        if ((blocks[i].type == QUAD || blocks[i].type == TRIPLET) && (blocks[i].tile / 9 == blocks[index].tile / 9) && abs(blocks[i].tile % 9 - blocks[index].tile % 9) <= bias)
        {
            if (table[blocks[i].tile % 9 - blocks[index].tile % 9 + bias] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile % 9 - blocks[index].tile % 9 + bias] = i;
            }
        }
    }
    for (int i = bias; i < table_length - 1; ++i)
    {
        table[i] = table[i + 1];
        table[i] = table[i + 1];
    }
    for (int i = 0; i < table_length - 2; ++i)
    {
        if (table[i] != -1 && table[i + 1] != -1)
        {
            if (!(used_block_flag[table[i]] < 0 && used_block_flag[table[i + 1]] < 0))
            {
                if (used_block_flag[table[i]] < 0)
                {
                    used_block_flag[table[i]] = 1;
                }
                else
                {
                    used_block_flag[table[i]] = -yaku_map[__func__];
                }
                if (used_block_flag[table[i + 1]] < 0)
                {
                    used_block_flag[table[i + 1]] = 1;
                }
                else
                {
                    used_block_flag[table[i + 1]] = -yaku_map[__func__];
                }
                used_block_flag[index] = -yaku_map[__func__];
                checked_yaku[yaku_map[__func__]] = 24;
                return 24;
            }
        }
    }
    return 0;
}

// in block chain
int YakuCheck_PureStraight(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE) || blocks[index].tile % 3 != 0)
    {
        return 0;
    }
    const int table_length = 3;
    std::vector<int> table(table_length, -1);
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 == blocks[index].tile / 9) && (blocks[i].tile != blocks[index].tile) && (blocks[i].tile % 3 == 0))
        {
            if (table[blocks[i].tile % 9 / 3] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile % 9 / 3] = i;
            }
        }
    }
    std::vector<int> table_index(2, -1);
    for (int i = 0; i < table_length; ++i)
    {
        if (table[i] == -1 || table[i] == index)
        {
            continue;
        }
        if (table_index[0] == -1)
        {
            table_index[0] = i;
        }
        else
        {
            table_index[1] = i;
        }
    }

    if (table_index[0] != -1 && table_index[1] != -1)
    {
        if (used_block_flag[table[table_index[0]]] < 0 && used_block_flag[table[table_index[1]]] < 0)
        {
            return 0;
        }
        else
        {
            used_block_flag[table[table_index[0]]] = -yaku_map[__func__];
        }
        if (used_block_flag[table[table_index[1]]] < 0)
        {
            used_block_flag[table[table_index[1]]] = 1;
        }
        else
        {
            used_block_flag[table[table_index[1]]] = -yaku_map[__func__];
        }
        used_block_flag[index] = -yaku_map[__func__];
        checked_yaku[yaku_map[__func__]] = 16;
        return 16;
    }
    return 0;
}

// impling lower yaku (all chows, two terminal chows, two mixed chows)
// out of block chain
int YakuCheck_ThreeSuitedTerminalChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((int)blocks.size() != 5)
    {
        return 0;
    }
    std::vector<std::vector<int>> table(3, std::vector<int>(3, 0));
    for (Block block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == SEQUENCE)
        {
            if (block.tile % 9 == 0)
            {
                table[block.tile / 9][0] += 1;
            }
            else if (block.tile % 9 == 6)
            {
                table[block.tile / 9][2] += 1;
            }
        }
        else if (block.type == PAIR)
        {
            if (block.tile % 9 == 4)
            {
                table[block.tile / 9][1] += 1;
            }
        }
    }
    std::vector<int> count(2, 0);
    for (int i = 0; i < 3; ++i)
    {
        if (table[i][0] == 1 && table[i][1] == 0 && table[i][2] == 1)
        {
            count[0] += 1;
        }
        else if (table[i][0] == 0 && table[i][1] == 1 && table[i][2] == 0)
        {
            count[1] += 1;
        }
    }
    if (count[0] == 2 && count[1] == 1)
    {
        for (int &flag : used_block_flag)
        {
            flag = 1;
        }
        checked_yaku[yaku_map["YakuCheck_MixedDoubleChow"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_TwoTerminalChows"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_AllChows"]] -= 2;
        checked_yaku[yaku_map[__func__]] = 16;
    }
    return 16;
}

// implied by Higher yaku (ex. ;Four Pure Shifted Chows)
// in block chain
int YakuCheck_PureShiftedChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }
    const int bias = 4;
    const int table_length = bias * 2 + 1;
    int color = -1;
    std::vector<int> table(table_length, -1);
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 == blocks[index].tile / 9) && abs(blocks[i].tile % 9 - blocks[index].tile % 9) <= bias)
        {
            if (table[blocks[i].tile % 9 - blocks[index].tile % 9 + bias] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile % 9 - blocks[index].tile % 9 + bias] = i;
            }
        }
    }
    for (int i = bias; i < table_length - 1; ++i)
    {
        table[i] = table[i + 1];
        table[i] = table[i + 1];
    }
    for (int i = bias - 2; i < bias + 1; ++i)
    {
        if (table[i] != -1 && table[i + 1] != -1)
        {
            if (!(used_block_flag[table[i]] < 0 && used_block_flag[table[i + 1]] < 0))
            {
                if (used_block_flag[table[i]] < 0)
                {
                    used_block_flag[table[i]] = 1;
                }
                else
                {
                    used_block_flag[table[i]] = -yaku_map[__func__];
                }
                if (used_block_flag[table[i + 1]] < 0)
                {
                    used_block_flag[table[i + 1]] = 1;
                }
                else
                {
                    used_block_flag[table[i + 1]] = -yaku_map[__func__];
                }
                used_block_flag[index] = -yaku_map[__func__];

                checked_yaku[yaku_map["YakuCheck_PureShiftedChows"]] = 16;
                return 16;
            }
        }
    }
    for (int i = bias; i < table_length - 1; ++i)
    {
        table[i] = table[i + 1];
        table[i] = table[i + 1];
    }
    for (int i = 0; i < table_length - 2; i += 2)
    {
        if ((i < table_length - 3) && (table[i] != -1 && table[i + 2] != -1))
        {
            if (!(used_block_flag[table[i]] < 0 && used_block_flag[table[i + 2]] < 0))
            {
                if (used_block_flag[table[i]] < 0)
                {
                    used_block_flag[table[i]] = 1;
                }
                else
                {
                    used_block_flag[table[i]] = -yaku_map[__func__];
                }
                if (used_block_flag[table[i + 2]] < 0)
                {
                    used_block_flag[table[i + 2]] = 1;
                }
                else
                {
                    used_block_flag[table[i + 2]] = -yaku_map[__func__];
                }
                used_block_flag[index] = -yaku_map[__func__];

                checked_yaku[yaku_map["YakuCheck_PureShiftedChows"]] = 16;
                return 16;
            }
        }
    }
    return 0;
}

// impling lower yaku (ex. all simples, no honors)
// out of block chain
int YakuCheck_AllFives(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == SEQUENCE)
        {
            // 345 ~ 567 -> 234~456
            if (block.tile % 9 < 2 || block.tile % 9 > 4)
            {
                return 0;
            }
        }
        else if (block.type == TRIPLET || block.type == QUAD || block.type == PAIR)
        {
            if (block.tile % 9 != 4)
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_AllSimples"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_AllFives"]] = 16;
    return 16;
}

// impling lower yaku (ex. double pung)
// in block chain
int YakuCheck_TriplePung(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == TRIPLET || blocks[index].type == QUAD))
    {
        return 0;
    }
    std::vector<int> table(3, -1);
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (!IsNumber(blocks[i].tile))
        {
            continue;
        }
        if ((blocks[i].type == TRIPLET || blocks[i].type == QUAD) && (blocks[i].tile / 9 != blocks[index].tile / 9) && blocks[i].tile % 9 == blocks[index].tile % 9)
        {
            if (table[blocks[i].tile / 9] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile / 9] = i;
            }
        }
    }
    std::vector<int> number_type(2, -1);
    for (int i = 0; i < 3; ++i)
    {
        if (i == blocks[index].tile / 9)
        {
            continue;
        }
        if (number_type[0] == -1)
        {
            number_type[0] = i;
        }
        else
        {
            number_type[1] = i;
        }
    }
    if (table[number_type[0]] != -1 && table[number_type[1]] != -1)
    {
        if (used_block_flag[table[number_type[0]]] < 0 && used_block_flag[table[number_type[1]]] < 0)
        {
            return 0;
        }
        used_block_flag[index] = 1;
        used_block_flag[table[number_type[0]]] = 1;
        used_block_flag[table[number_type[1]]] = 1;
        checked_yaku[yaku_map["YakuCheck_DoublePung"]] = -1;
        checked_yaku[yaku_map[__func__]] = 16;
        return 16;
    }
    return 0;
}

// need be added //
// implied by Higher yaku (FourConcealedPungs)
// impling lower yaku (ex. TwoConcealedPungs)
// out of block chain
int YakuCheck_ThreeConcealedPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_closed = 0;
    for (Block &block : blocks)
    {
        if ((block.type == TRIPLET || block.type == QUAD) && block.source == SELF)
        {
            if (block.tile != winning_condition.tile)
            {
                count_closed += 1;
            }
        }
    }
    if (count_closed == 3)
    {
        checked_yaku[yaku_map["YakuCheck_TwoConcealedPungs"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_ThreeConcealedPungs"]] = 16;
        return 16;
    }
    else
    {
        return 0;
    }
}

// implied by Higher yaku (ex. Greater~)
// impling lower yaku (ex. Concealed Hand)
// out of block chain
int YakuCheck_LesserHonorsAndKnittedTiles(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 14)
    {
        return 0;
    }
    std::vector<std::vector<int>> table(3, std::vector<int>(3, 0));
    std::vector<int> honor_count(8, 0);
    for (Block &block : blocks)
    {
        if (block.type != SINGLETILE)
        {
            return 0;
        }
        if (IsNumber(block.tile))
        {
            table[block.tile / 9][block.tile % 3] += 1;
        }
        else if (IsHonor(block.tile))
        {
            honor_count[block.tile - 27] += 1;
        }
    }
    for (int i = 0; i < 7; ++i)
    {
        if (honor_count[i] > 1)
        {
            return 0;
        }
    }
    for (int i = 0; i < 3; ++i)
    {
        std::vector<int> zero_count(2, 0);
        for (int j = 0; j < 3; ++j)
        {
            if (table[i][j] == 0)
            {
                zero_count[0] += 1;
            }
            if (table[j][i] == 0)
            {
                zero_count[1] += 1;
            }
        }
        if (zero_count[0] < 2 || zero_count[1] < 2)
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_LesserHonorsAndKnittedTiles"]] = 12;
    return 12;
}

// can imply sigle wait
// out of block chain
int YakuCheck_KnittedStraight(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() == 14)
    {
        std::vector<std::vector<int>> table(3, std::vector<int>(3, 0));
        std::vector<int> sum_number(3, 0);
        for (Block &block : blocks)
        {
            if (block.type != SINGLETILE)
            {
                return 0;
            }
            if (IsNumber(block.tile))
            {
                table[block.tile / 9][block.tile % 3] += 1;
                sum_number[block.tile / 9] += block.tile;
            }
        }
        for (int i = 0; i < 3; ++i)
        {
            std::vector<int> zero_count(2, 0);
            for (int j = 0; j < 3; ++j)
            {
                if ((table[i][j] > 0 && table[i][j] != 3))
                {
                    return 0;
                }
                if (table[i][j] == 0)
                {
                    zero_count[0] += 1;
                }
                if (table[j][i] == 0)
                {
                    zero_count[1] += 1;
                }
            }
            if (zero_count[0] != 2 || zero_count[1] != 2)
            {
                return 0;
            }
        }
        checked_yaku[yaku_map["YakuCheck_KnittedStraight"]] = 12;
        return 12;
    }

    for (Block &block : blocks)
    {
        if (block.type == KNITTED)
        {
            checked_yaku[yaku_map["YakuCheck_KnittedStraight"]] = 12;
            return 12;
        }
    }
    return 0;
}

// implied by Higher yaku
// impling lower yaku (ex. no honors)
// out of block chain
int YakuCheck_UpperFour(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    // need to be added
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile) || block.type == KNITTED)
        {
            return 0;
        }
        if (block.type == SEQUENCE)
        {
            for (int i = 0; i < 3; ++i)
            {
                if ((block.tile + i) % 9 <= 4)
                {
                    return 0;
                }
            }
        }
        else
        {
            if (block.tile % 9 <= 4)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_UpperFour"]] = 12;
    return 12;
}

// implied by Higher yaku
// impling lower yaku (ex. no honors)
// out of block chain
int YakuCheck_LowerFour(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    // need to be added
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile) || block.type == KNITTED)
        {
            return 0;
        }
        if (block.type == SEQUENCE)
        {
            for (int i = 0; i < 3; ++i)
            {
                if ((block.tile + i) % 9 >= 4)
                {
                    return 0;
                }
            }
        }
        else
        {
            if (block.tile % 9 >= 4)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_LowerFour"]] = 12;
    return 12;
}

// implied by Higher yaku
// impling lower yaku (ex. pung of terminal~)
// out of block chain
int YakuCheck_BigThreeWinds(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    int count_wind = 0;
    for (Block &block : blocks)
    {
        if (IsWind(block.tile) && (block.type == QUAD || block.type == TRIPLET))
        {
            count_wind += 1;
        }
    }
    if (count_wind == 3)
    {
        checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_BigThreeWinds"]] = 12;
        return 12;
    }
    return 0;
}

// impling lower yaku (ex. Conceald hand)
// out of block chain
int YakuCheck_LastTileDraw(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_last_tile_in_the_game == 1 && !winning_condition.is_discarded)
    {
        checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_LastTileDraw"]] = 8;
        return 8;
    }
    else
    {
        return 0;
    }
}

// out of block chain
int YakuCheck_LastTileClaim(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_last_tile_in_the_game == 1 && winning_condition.is_discarded)
    {
        checked_yaku[yaku_map["YakuCheck_LastTileClaim"]] = 8;
        return 8;
    }
    else
    {
        return 0;
    }
}

// impling lower yaku (ex. Conceald hand)
// out of block chain
int YakuCheck_OutWithReplacementTile(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_replacement_tile == 1)
    {
        checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_OutWithReplacementTile"]] = 8;
        return 8;
    }
    else
    {
        return 0;
    }
}

// out of block chain
int YakuCheck_RobbingTheKong(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_robbing_the_kong == 1)
    {
        checked_yaku[yaku_map["YakuCheck_RobbingTheKong"]] = 8;
        return 8;
    }
    else
    {
        return 0;
    }
}

// in block chain
int YakuCheck_MixedStraight(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE) || blocks[index].tile % 3 != 0)
    {
        return 0;
    }
    std::vector<std::vector<int>> table(3, std::vector<int>(3, -1));
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 != blocks[index].tile / 9) && blocks[i].tile % 3 == 0)
        {
            if (table[blocks[i].tile / 9][blocks[i].tile % 9 / 3] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile / 9][blocks[i].tile % 9 / 3] = i;
            }
        }
    }
    std::vector<int> type(2, -1);
    for (int i = 0; i < 3; ++i)
    {
        if (i == blocks[index].tile / 9)
        {
            continue;
        }
        if (type[0] == -1)
        {
            type[0] = i;
        }
        else
        {
            type[1] = i;
        }
    }
    std::vector<int> start_num(2, -1);
    for (int i = 0; i < 3; ++i)
    {
        if (i == blocks[index].tile % 9 / 3)
        {
            continue;
        }
        if (start_num[0] == -1)
        {
            start_num[0] = i;
        }
        else
        {
            start_num[1] = i;
        }
    }
    for (int i = 0; i < 2; ++i)
    {
        if (table[type[i]][start_num[0]] != -1 && table[type[i ^ 1]][start_num[1]] != -1)
        {
            if (!(used_block_flag[table[type[i]][start_num[0]]] < 0 && used_block_flag[table[type[i ^ 1]][start_num[1]]] < 0))
            {
                if (used_block_flag[table[type[i]][start_num[0]]] < 0)
                {
                    used_block_flag[table[type[i]][start_num[0]]] = 1;
                }
                else
                {
                    used_block_flag[table[type[i]][start_num[0]]] = -yaku_map[__func__];
                }

                if (used_block_flag[table[type[i ^ 1]][start_num[1]]] < 0)
                {
                    used_block_flag[table[type[i ^ 1]][start_num[1]]] = 1;
                }
                else
                {
                    used_block_flag[table[type[i ^ 1]][start_num[1]]] = -yaku_map[__func__];
                }

                used_block_flag[index] = -yaku_map[__func__];
                checked_yaku[yaku_map["YakuCheck_MixedStraight"]] = 8;
                return 8;
            }
        }
    }
    return 0;
}

// in block chain
int YakuCheck_MixedTripleChow(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }
    std::vector<int> table(3, -1);
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 != blocks[index].tile / 9) && blocks[i].tile % 9 == blocks[index].tile % 9)
        {
            if (table[blocks[i].tile / 9] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile / 9] = i;
            }
        }
    }
    std::vector<int> type(2, -1);
    for (int i = 0; i < 3; ++i)
    {
        if (i == blocks[index].tile / 9)
        {
            continue;
        }
        if (type[0] == -1)
        {
            type[0] = i;
        }
        else
        {
            type[1] = i;
        }
    }
    if (table[type[0]] != -1 && table[type[1]] != -1)
    {
        if (!(used_block_flag[table[type[0]]] < 0 && used_block_flag[table[type[1]]] < 0))
        {
            if (used_block_flag[table[type[0]]] < 0)
            {
                used_block_flag[table[type[0]]] = 1;
            }
            else
            {
                used_block_flag[table[type[0]]] = -yaku_map[__func__];
            }

            if (used_block_flag[table[type[1]]] < 0)
            {
                used_block_flag[table[type[1]]] = 1;
            }
            else
            {
                used_block_flag[table[type[1]]] = -yaku_map[__func__];
            }
            used_block_flag[index] = -yaku_map[__func__];
            checked_yaku[yaku_map["YakuCheck_MixedTripleChow"]] = 8;
            return 8;
        }
    }
    return 0;
}

// impling lower yaku (ex. one voided suit)
// out of block chain
int YakuCheck_ReversibleTiles(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    std::vector<int> reversible_tiles = {
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        1, 1, 1, 1, 1, 0, 0, 1, 1,
        0, 1, 0, 1, 1, 1, 0, 1, 1,
        0, 0, 0, 0, 1, 0, 0};

    for (Block &block : blocks)
    {
        if (block.type == KNITTED)
        {
            return 0;
        }
        if (block.type == SEQUENCE)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (reversible_tiles[block.tile + i] == 0)
                {
                    return 0;
                }
            }
        }
        else
        {
            if (reversible_tiles[block.tile] == 0)
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_ReversibleTiles"]] = 8;
    return 8;
}

// 1234589p245689s5z
// in block chain
int YakuCheck_MixedShiftedPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!((blocks[index].type == TRIPLET || blocks[index].type == QUAD) && IsNumber(blocks[index].tile)))
    {
        return 0;
    }
    std::vector<std::vector<int>> table(3, std::vector<int>(5, -1));
    const int bias = 2;
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (!IsNumber(blocks[i].tile))
        {
            continue;
        }
        if ((blocks[i].type == TRIPLET || blocks[i].type == QUAD) && (blocks[i].tile / 9 != blocks[index].tile / 9) && abs(blocks[i].tile % 9 - blocks[index].tile % 9) <= 2)
        {
            if (table[blocks[i].tile / 9][blocks[i].tile % 9 - blocks[index].tile % 9 + bias] == -1 && used_block_flag[i] == 0)
            {
                table[blocks[i].tile / 9][blocks[i].tile % 9 - blocks[index].tile % 9 + bias] = i;
            }
        }
    }
    std::vector<int> type(2, -1);
    for (int i = 0; i < 3; ++i)
    {
        if (i == blocks[index].tile / 9)
        {
            continue;
        }
        if (type[0] == -1)
        {
            type[0] = i;
        }
        else
        {
            type[1] = i;
        }
    }
    for (int i = 3; i < 5; ++i)
    {
        table[type[0]][i - 1] = table[type[0]][i];
        table[type[1]][i - 1] = table[type[1]][i];
    }
    for (int i = 0; i < 2; ++i)
    {
        for (int j = 0; j < 3; ++j)
        {
            if (table[type[i]][j] != -1 && table[type[i ^ 1]][j + 1] != -1)
            {
                if (!(used_block_flag[table[type[i]][j]] < 0 && used_block_flag[table[type[i ^ 1]][j + 1]] < 0))
                {
                    if (used_block_flag[table[type[i]][j]] < 0)
                    {
                        used_block_flag[table[type[i]][j]] = 1;
                    }
                    else
                    {
                        used_block_flag[table[type[i]][j]] = -yaku_map[__func__];
                    }
                    if (used_block_flag[table[type[i ^ 1]][j + 1]] < 0)
                    {
                        used_block_flag[table[type[i ^ 1]][j + 1]] = 1;
                    }
                    else
                    {
                        used_block_flag[table[type[i ^ 1]][j + 1]] = -yaku_map[__func__];
                    }

                    used_block_flag[index] = -yaku_map[__func__];
                    checked_yaku[yaku_map["YakuCheck_MixedShiftedPungs"]] = 8;
                    return 8;
                }
            }
        }
    }
    return 0;
}

// implied by Higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_TwoConcealedKongs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int count_closed_kong = 0;
    for (Block &block : blocks)
    {
        if (block.type == QUAD && block.source == SELF)
        {
            count_closed_kong += 1;
        }
    }
    if (count_closed_kong == 2)
    {
        checked_yaku[yaku_map["YakuCheck_TwoMeldedKongs"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_ConcealedKong"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoConcealedKongs"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoConcealedKongs"]] = 8;
        return 8;
    }
    else
    {
        return 0;
    }
}

// impling lower yaku (ex. single wait)
// out of block chain
int YakuCheck_MeldedHand(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks.size() == 5))
    {
        return 0;
    }
    for (Block &block : blocks)
    {
        if (!(block.type == PAIR) && block.source == SELF)
        {
            return 0;
        }
    }
    if (!winning_condition.is_discarded)
    {
        return 0;
    }
    checked_yaku[yaku_map["YakuCheck_SingleWait"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_MeldedHand"]] = 6;
    return 6;
}

// in block chain
int YakuCheck_MixedShiftedChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }
    std::vector<std::vector<int>> table(3, std::vector<int>(5, -1));
    const int bias = 2;
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 != blocks[index].tile / 9) && abs(blocks[i].tile % 9 - blocks[index].tile % 9) <= 2)
        {
            if (table[blocks[i].tile / 9][blocks[i].tile % 9 - blocks[index].tile % 9 + bias] == -1 || used_block_flag[i] == 0)
            {
                table[blocks[i].tile / 9][blocks[i].tile % 9 - blocks[index].tile % 9 + bias] = i;
            }
        }
    }
    std::vector<int> type(2, -1);
    for (int i = 0; i < 3; ++i)
    {
        if (i == blocks[index].tile / 9)
        {
            continue;
        }
        if (type[0] == -1)
        {
            type[0] = i;
        }
        else
        {
            type[1] = i;
        }
    }
    for (int i = 3; i < 5; ++i)
    {
        table[type[0]][i - 1] = table[type[0]][i];
        table[type[1]][i - 1] = table[type[1]][i];
    }
    for (int i = 0; i < 2; ++i)
    {
        for (int j = 0; j < 3; ++j)
        {
            if (table[type[i]][j] != -1 && table[type[i ^ 1]][j + 1] != -1)
            {
                if (!(used_block_flag[table[type[i]][j]] < 0 && used_block_flag[table[type[i ^ 1]][j + 1]] < 0))
                {
                    if (used_block_flag[table[type[i]][j]] < 0)
                    {
                        used_block_flag[table[type[i]][j]] = 1;
                    }
                    else
                    {
                        used_block_flag[table[type[i]][j]] = -yaku_map[__func__];
                    }
                    if (used_block_flag[table[type[i ^ 1]][j + 1]] < 0)
                    {
                        used_block_flag[table[type[i ^ 1]][j + 1]] = 1;
                    }
                    else
                    {
                        used_block_flag[table[type[i ^ 1]][j + 1]] = -yaku_map[__func__];
                    }

                    used_block_flag[index] = -yaku_map[__func__];
                    checked_yaku[yaku_map["YakuCheck_MixedShiftedChows"]] = 6;
                    return 6;
                }
            }
        }
    }
    return 0;
}

// implied by Higher yaku(ex. All Terminals)
// out of block chain
int YakuCheck_AllPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks.size() == 5))
    {
        return 0;
    }
    for (Block &block : blocks)
    {
        if (!(block.type == TRIPLET || block.type == QUAD || block.type == PAIR))
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_AllPungs"]] = 6;
    return 6;
}

// implied by Higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_HalfFlush(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    std::vector<int> type_check_array(3, 0);

    for (Block &block : blocks)
    {
        if (IsNumber(block.tile))
        {
            type_check_array[block.tile / 9] = 1;
        }
    }
    int type_count = 0;
    for (int i = 0; i < 3; ++i)
    {
        type_count += type_check_array[i];
    }
    if (type_count == 1)
    {
        checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_HalfFlush"]] = 6;
        return 6;
    }
    else
    {
        return 0;
    }
}

// implied by Higher yaku
// out of block chain
int YakuCheck_AllTypes(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks.size() == 5 || blocks.size() == 7))
    {
        return 0;
    }
    std::vector<int> type_check_array(5, 0);
    for (Block &block : blocks)
    {
        if (IsManzu(block.tile))
        {
            type_check_array[0] = 1;
        }
        else if (IsSouzu(block.tile))
        {
            type_check_array[1] = 1;
        }
        else if (IsPinzu(block.tile))
        {
            type_check_array[2] = 1;
        }
        else if (IsWind(block.tile))
        {
            type_check_array[3] = 1;
        }
        else if (IsDragon(block.tile))
        {
            type_check_array[4] = 1;
        }
    }
    int type_count = 0;
    for (int i = 0; i < 5; ++i)
    {
        type_count += type_check_array[i];
    }
    if (type_count == 5)
    {
        checked_yaku[yaku_map["YakuCheck_AllTypes"]] = 6;
        return 6;
    }
    else
    {
        return 0;
    }
}

// implied by Higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_TwoDragons(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int count_dragons = 0;
    for (Block &block : blocks)
    {
        if ((block.type == QUAD || block.type == TRIPLET) && IsDragon(block.tile))
        {
            count_dragons += 1;
        }
    }
    if (count_dragons == 2)
    {
        checked_yaku[yaku_map["YakuCheck_DragonPung"]] -= 2;
        checked_yaku[yaku_map["YakuCheck_TwoDragons"]] = 6;
        return 6;
    }
    return 0;
}

// impling lower yaku
// out of block chain
int YakuCheck_FullyConcealedHand(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_discarded == false && HasOpenedBlocks(blocks) == false)
    {
        checked_yaku[yaku_map["YakuCheck_SelfDrawn"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_FullyConcealedHand"]] = 4;
        return 4;
    }
    return 0;
}

// out of block chain
int YakuCheck_LastTile(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_last_tile_of_its_kind == true)
    {
        checked_yaku[yaku_map["YakuCheck_LastTile"]] = 4;
        return 4;
    }
    return 0;
}

// implied by Higher yaku
// out of block chain
int YakuCheck_OutsideHand(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (block.type == SEQUENCE)
        {
            if (!(block.tile % 9 == 0 || block.tile % 9 == 6))
            {
                return 0;
            }
        }
        else if (block.type == TRIPLET || block.type == QUAD || block.type == PAIR)
        {
            if (!IsOutside(block.tile))
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_OutsideHand"]] = 4;
    return 4;
}

// implied by Higher yaku (ex. two conceald kongs)
// impling lower yaku
// out of block chain
int YakuCheck_TwoMeldedKongs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int count_melded_quad = 0;
    int count_quad = 0;
    for (Block &block : blocks)
    {
        if (block.type == QUAD)
        {
            if (block.source != SELF)
            {
                count_melded_quad += 1;
            }
            count_quad += 1;
        }
    }
    if (count_quad == 2)
    {
        checked_yaku[yaku_map["YakuCheck_MeldedKong"]] -= count_melded_quad;
        checked_yaku[yaku_map["YakuCheck_TwoMeldedKongs"]] = 4;
        return 4;
    }
    else
    {
        return 0;
    }
}

// out of block chain
int YakuCheck_ConcealedHand(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.is_discarded == true && HasOpenedBlocks(blocks) == false)
    {
        checked_yaku[yaku_map["YakuCheck_ConcealedHand"]] = 2;
        return 2;
    }
    return 0;
}

// implied by Higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_DragonPung(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        Block &block = blocks[i];
        if ((block.type == QUAD || block.type == TRIPLET) && IsDragon(block.tile))
        {
            used_block_flag[i] = 1;
            checked_yaku[yaku_map["YakuCheck_DragonPung"]] = 2;
            return 2;
        }
    }
    return 0;
}

// implied by Higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_PrevalentWind(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        Block &block = blocks[i];
        if ((block.type == QUAD || block.type == TRIPLET) && (block.tile == winning_condition.round_wind))
        {
            used_block_flag[i] = 1;
            checked_yaku[yaku_map["YakuCheck_PrevalentWind"]] = 2;
            return 2;
        }
    }
    return 0;
}

// implied by Higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_SeatWind(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        Block &block = blocks[i];
        if ((block.type == QUAD || block.type == TRIPLET) && (block.tile == winning_condition.seat_wind))
        {
            used_block_flag[i] = 1;
            checked_yaku[yaku_map["YakuCheck_SeatWind"]] = 2;
            return 2;
        }
    }
    return 0;
}

// implied by Higher yaku(ex. Terminal Chows)
// impling lower yaku
// out of block chain
int YakuCheck_AllChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (blocks.size() != 5)
    {
        return 0;
    }
    std::vector<int> type_count(6, 0);
    for (Block &block : blocks)
    {
        if (block.type != SEQUENCE && block.type != KNITTED && block.type != PAIR)
        {
            return 0;
        }
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        type_count[block.type]++;
    }
    if (type_count[PAIR] == 1 && (type_count[SEQUENCE] + type_count[KNITTED]) == 4)
    {
        checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
        checked_yaku[yaku_map["YakuCheck_AllChows"]] = 2;
        return 2;
    }
    else
    {
        return 0;
    }
}

// may be implied by higher yaku
// in block chain
int YakuCheck_DoublePung(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!((blocks[index].type == TRIPLET || blocks[index].type == QUAD) && IsNumber(blocks[index].tile)))
    {
        return 0;
    }
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (!IsNumber(blocks[i].tile))
        {
            continue;
        }
        if ((blocks[i].type == TRIPLET || blocks[i].type == QUAD) && (blocks[i].tile / 9 != blocks[index].tile / 9) && (blocks[i].tile % 9 - blocks[index].tile % 9 == 0))
        {
            if (used_block_flag[i] < 0)
            {
                used_block_flag[i] = 1;
            }
            else
            {
                used_block_flag[i] = -yaku_map[__func__];
            }
            used_block_flag[index] = -yaku_map[__func__];

            checked_yaku[yaku_map["YakuCheck_DoublePung"]] += 2;
            return 2;
        }
    }
    return 0;
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_TwoConcealedPungs(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int count_closed = 0;

    for (Block &block : blocks)
    {
        if (block.source == SELF && (block.type == QUAD || block.type == TRIPLET))
        {
            if (block.tile != winning_condition.tile)
            {
                count_closed += 1;
            }
        }
    }
    if (count_closed == 2)
    {
        checked_yaku[yaku_map["YakuCheck_TwoConcealedPungs"]] = 2;
        return 2;
    }
    else
    {
        return 0;
    }
}

// can't combined with seven pairs + (all terminals  or  all greens)
// can't count with concealed kong
// CAN count multiple times
// out of block chain
int YakuCheck_TileHog(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if ((checked_yaku[yaku_map["YakuCheck_AllGreen"]] || checked_yaku[yaku_map["YakuCheck_AllTerminals"]]) && checked_yaku[yaku_map["YakuCheck_SevenPairs"]])
    {
        return 0;
    }
    std::vector<int> tiles = BlocksToTiles(blocks);
    int score = 0;
    for (int i = 0; i < 34; ++i)
    {
        if (tiles[i] == 4)
        {
            score += 2;
        }
    }
    for (Block &block : blocks)
    {
        if (block.type == QUAD)
        {
            score -= 2;
        }
    }
    checked_yaku[yaku_map["YakuCheck_TileHog"]] = score;
    return score;
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_ConcealedKong(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (block.source == SELF && block.type == QUAD)
        {
            checked_yaku[yaku_map["YakuCheck_ConcealedKong"]] = 2;
            return 2;
        }
    }
    return 0;
}

// may be implied by higher yaku
// impling lower yaku
// out of block chain
int YakuCheck_AllSimples(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
        if (block.type == KNITTED)
        {
            return 0;
        }
        else if (block.type == SEQUENCE)
        {
            if ((block.tile % 9 == 0 || block.tile % 9 == 6))
            {
                return 0;
            }
        }
        else
        {
            if ((block.tile % 9 == 0 || block.tile % 9 == 8))
            {
                return 0;
            }
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] -= 1;
    checked_yaku[yaku_map["YakuCheck_AllSimples"]] = 2;
    return 2;
}

// may be implied by higher yaku
// in block chain
int YakuCheck_PureDoubleChow(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 == blocks[index].tile / 9) && (blocks[i].tile % 9 - blocks[index].tile % 9 == 0))
        {
            if (used_block_flag[i] < 0)
            {
                used_block_flag[i] = 1;
            }
            else
            {
                used_block_flag[i] = -yaku_map[__func__];
            }
            used_block_flag[index] = -yaku_map[__func__];

            checked_yaku[yaku_map["YakuCheck_PureDoubleChow"]] += 1;
            return 1;
        }
    }
    return 0;
}

// may be implied by higher yaku
// in block chain
int YakuCheck_MixedDoubleChow(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 != blocks[index].tile / 9) && (blocks[i].tile % 9 - blocks[index].tile % 9 == 0))
        {
            if (used_block_flag[i] < 0)
            {
                used_block_flag[i] = 1;
            }
            else
            {
                used_block_flag[i] = -yaku_map[__func__];
            }
            used_block_flag[index] = -yaku_map[__func__];

            checked_yaku[yaku_map["YakuCheck_MixedDoubleChow"]] += 1;
            return 1;
        }
    }
    return 0;
}

// may be implied by higher yaku
// in block chain
int YakuCheck_ShortStraight(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE))
    {
        return 0;
    }
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && (blocks[i].tile / 9 == blocks[index].tile / 9) && abs(blocks[i].tile % 9 - blocks[index].tile % 9) == 3)
        {
            if (used_block_flag[i] < 0)
            {
                used_block_flag[i] = 1;
            }
            else
            {
                used_block_flag[i] = -yaku_map[__func__];
            }
            used_block_flag[index] = -yaku_map[__func__];

            checked_yaku[yaku_map["YakuCheck_ShortStraight"]] += 1;
            return 1;
        }
    }
    return 0;
}

// may be implied by higher yaku
// in block chain
int YakuCheck_TwoTerminalChows(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (!(blocks[index].type == SEQUENCE && (blocks[index].tile % 9 == 0 || blocks[index].tile % 9 == 6)))
    {
        return 0;
    }
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        if (i == index || used_block_flag[i] == 1 || used_block_flag[i] == -yaku_map[__func__])
        {
            continue;
        }
        if (blocks[i].type == SEQUENCE && blocks[i].tile / 9 == blocks[index].tile / 9 && blocks[i].tile % 9 + blocks[index].tile % 9 == 6)
        {
            if (used_block_flag[i] < 0)
            {
                used_block_flag[i] = 1;
            }
            else
            {
                used_block_flag[i] = -yaku_map[__func__];
            }
            used_block_flag[index] = -yaku_map[__func__];

            checked_yaku[yaku_map["YakuCheck_TwoTerminalChows"]] += 1;
            return 1;
        }
    }
    return 0;
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_PungOfTerminalsOrHonors(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    int score = 0;
    for (int i = 0; i < (int)blocks.size(); ++i)
    {
        Block &block = blocks[i];
        if (used_block_flag[i] && used_block_flag[i] != OkFlag_PungOfTerminelsOrHonor)
        {
            continue;
        }
        if ((block.type == QUAD || block.type == TRIPLET) && IsOutside(block.tile) &&
            !IsDragon(block.tile) &&
            !(IsWind(block.tile) && ((block.tile == winning_condition.seat_wind) || (block.tile == winning_condition.round_wind))))
        {
            score += 1;
        }
    }
    checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]] = score;
    return checked_yaku[yaku_map["YakuCheck_PungOfTerminalsOrHonors"]];
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_OneVoidedSuit(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    std::vector<int> type_count(3, 0);

    for (Block &block : blocks)
    {
        if (IsNumber(block.tile))
        {
            if (block.type == KNITTED)
            {
                type_count[0] = 1;
                type_count[1] = 1;
                type_count[2] = 1;
            }
            else
            {
                type_count[block.tile / 9] = 1;
            }
        }
    }
    if ((type_count[0] + type_count[1] + type_count[2]) == 2)
    {
        checked_yaku[yaku_map["YakuCheck_OneVoidedSuit"]] = 1;
        return 1;
    }
    else
    {
        return 0;
    }
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_NoHonors(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (!IsNumber(block.tile))
        {
            return 0;
        }
    }
    checked_yaku[yaku_map["YakuCheck_NoHonors"]] = 1;
    return 1;
}

// implied by Higher yaku (ex. melded hand)
// out of block chain
int YakuCheck_SingleWait(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (winning_condition.count_winning_conditions == 1)
    {
        checked_yaku[yaku_map["YakuCheck_SingleWait"]] = 1;
        return 1;
    }
    else
    {
        return 0;
    }
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_MeldedKong(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    for (Block &block : blocks)
    {
        if (block.source != SELF && block.type == QUAD)
        {
            checked_yaku[yaku_map["YakuCheck_MeldedKong"]] = 1;
            return 1;
        }
    }
    return 0;
}

// may be implied by higher yaku
// out of block chain
int YakuCheck_SelfDrawn(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    if (HasOpenedBlocks(blocks) == true && winning_condition.is_discarded == false)
    {
        checked_yaku[yaku_map["YakuCheck_SelfDrawn"]] = 1;
        return 1;
    }
    else
    {
        return 0;
    }
}

// dummy function
int YakuCheck_EdgeWait(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    return 0;
}

int YakuCheck_ClosedWait(int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)
{
    return 0;
}

// Flower tile can checked in final scoring phase

// Initialize the YakuCheck array using YAKU_LIST
YakuFunction YakuCheck_Array[Yaku_Count] = {
#define X(YakuName, YakuFunc) YakuFunc,
    YAKU_LIST
#undef X
};

Hand::Hand()
{
    closed_tiles = std::vector<int>(35, 0);
    opened_tiles = std::vector<int>(35, 0);
    // flower_point = 0;
    winning_tile = -1;
    is_blocks_divided = false;
    highest_score = 0;
}

Hand::Hand(std::vector<int> init_hand)
{
    closed_tiles = std::vector<int>(35, 0);
    opened_tiles = std::vector<int>(35, 0);
    // flower_point = 0;
    winning_tile = -1;
    for (int tile : init_hand)
    {
        if (tile < 0 || tile >= 35)
        {
            throw std::invalid_argument{"tile number is invalid"};
        }
        closed_tiles[tile] += 1;
    }
    is_blocks_divided = false;
    highest_score = 0;
}

Hand::Hand(std::vector<int> init_hand, int init_seat_wind, int init_round_wind)
{
    closed_tiles = std::vector<int>(35, 0);
    opened_tiles = std::vector<int>(35, 0);
    // flower_point = 0;
    winning_tile = -1;
    for (int tile : init_hand)
    {
        if (tile < 0 || tile >= 35)
        {
            throw std::invalid_argument{"tile number is invalid"};
        }
        closed_tiles[tile] += 1;
    }
    is_blocks_divided = false;
    highest_score = 0;
}

bool Hand::IsOpen()
{
    int open_flag = false;
    for (Block &block : call_blocks)
    {
        if (block.source != SELF)
        {
            open_flag = true;
            break;
        }
    }
    return open_flag;
}

// Function to convert tile name to number (0-33)
int Sub_TileNameToNum(const std::string &tile)
{
    if (tile.length() != 2)
        return -1;
    char num = tile[0];
    char suit = tile[1];
    if (num < '1' || num > '9')
        return -1;

    if (suit == 'm')
    {
        return (num - '1');
    }
    else if (suit == 'p')
    {
        return 9 + (num - '1');
    }
    else if (suit == 's')
    {
        return 18 + (num - '1');
    }
    else if (suit == 'z')
    {
        return 27 + (num - '1');
    }
    return -1;
}

// Function to convert hand string to pair of tile count array and blocks array
std::pair<std::vector<int>, std::vector<Block>> handString_To_Pair_Of_ConcealedTileArray_And_CallBlockArray(const std::string &str)
{
    std::vector<int> tiles(35, 0);
    std::vector<Block> blocks;
    std::string tmp_tiles = "";
    int mode = 0; // 0: closed, 1: KAMICHA, 2: SELF_QUAD

    for (size_t i = 0; i < str.size(); ++i)
    {
        char c = str[i];
        if (c == '[')
        {
            mode = 1; // KAMICHA
            tmp_tiles = "";
            continue;
        }
        else if (c == '{')
        {
            mode = 2; // SELF_QUAD
            tmp_tiles = "";
            continue;
        }
        else if (c == ']' || c == '}')
        {
            if (tmp_tiles.empty())
            {
                mode = 0;
                continue;
            }
            char suit = tmp_tiles.back();
            std::string digits = tmp_tiles.substr(0, tmp_tiles.size() - 1);
            bool is_triplet = true;
            for (size_t j = 1; j < digits.size(); ++j)
            {
                if (digits[j] != digits[j - 1])
                {
                    is_triplet = false;
                    break;
                }
            }
            Blocktype type;
            if (digits.size() == 3 && !is_triplet)
                type = SEQUENCE;
            else if (digits.size() == 3 && is_triplet)
                type = TRIPLET;
            else if (digits.size() == 4 && is_triplet)
                type = QUAD;
            else
            {
                mode = 0;
                continue;
            }
            std::string tile_str = "";
            tile_str += digits[0];
            tile_str += suit;
            int tile_num = Sub_TileNameToNum(tile_str);
            if (tile_num == -1)
            {
                mode = 0;
                continue;
            }
            Blocksource source;
            if (mode == 1)
            {
                source = KAMICHA;
            }
            else
            {
                source = SELF;
            }
            blocks.emplace_back(type, tile_num, source, 0);
            mode = 0;
            continue;
        }
        else if (isdigit(c))
        {
            tmp_tiles += c;
            continue;
        }
        else if (c == 'm' || c == 'p' || c == 's' || c == 'z')
        {
            tmp_tiles += c;
            if (mode == 0)
            {
                for (char x : tmp_tiles.substr(0, tmp_tiles.size() - 1))
                {
                    std::string t = "";
                    t += x;
                    t += c;
                    int tile_num = Sub_TileNameToNum(t);
                    if (tile_num != -1)
                        tiles[tile_num] += 1;
                }
                tmp_tiles = "";
            }
            continue;
        }
    }

    return {tiles, blocks};
}

std::vector<int> handStringToNumList(std::string str)
{
    std::vector<int> result;
    std::vector<char> tmp;
    for (auto s : str)
    {
        if (s >= 'a' && s <= 'z')
        {
            for (auto x : tmp)
            {
                std::string t;
                t += x;
                t += s;
                result.push_back(tileNameToNum(t));
            }
            tmp.clear();
        }
        else
        {
            tmp.push_back(s);
        }
    }
    return result;
}

// high yaku(ex. All Simples) is implying low yaku(ex. No Honors), we need index of yaku-function-pointer-array

// chicken hand is exception of all yaku when hand can be divided to 5-block

// (int index, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition)

void GetScore_General_Recursion(int recursion_depth, int score, std::vector<Block> &blocks, std::vector<int> &used_block_flag, std::vector<int> &checked_yaku, WinningCondition &winning_condition, int &total_score, std::vector<std::pair<int, int>> &yaku_score_vector)
{
    if (recursion_depth == (int)blocks.size())
    {
        if (total_score <= score)
        {
            total_score = score;
            yaku_score_vector.clear();
            for (int i = 0; i < Yaku_Count; ++i)
            {
                if (checked_yaku[i] > 0)
                {
                    yaku_score_vector.push_back(std::pair<int, int>(i, checked_yaku[i]));
                }
            }
        }
        return;
    }

    std::vector<int> backup_used_block_flag(used_block_flag);
    bool visited_next_step = false;
    for (int block_index = 0; block_index < (int)blocks.size(); ++block_index)
    {
        if (used_block_flag[block_index])
        {
            continue;
        }
        for (std::vector<int> &yaku_index_vector : chain_yaku_index)
        {
            const int yaku_index = yaku_index_vector[0];

            bool continue_flag = false;
            for (int impling_yaku_index : yaku_index_vector)
            {
                if (used_block_flag[block_index] == -impling_yaku_index)
                {
                    continue_flag = true;
                    break;
                }
            }
            if (continue_flag)
            {
                continue;
            }
            int backup_checked_yaku = checked_yaku[yaku_index];
            int delta_score = YakuCheck_Array[yaku_index](block_index, blocks, used_block_flag, checked_yaku, winning_condition);
            if (delta_score != 0)
            {
                visited_next_step = true;
                GetScore_General_Recursion(block_index + 1, score + delta_score, blocks, used_block_flag, checked_yaku, winning_condition, total_score, yaku_score_vector);

                used_block_flag = backup_used_block_flag;
                checked_yaku[yaku_index] = backup_checked_yaku;
            }
        }
    }
    if (!visited_next_step)
    {
        GetScore_General_Recursion(recursion_depth + 1, score, blocks, used_block_flag, checked_yaku, winning_condition, total_score, yaku_score_vector);
        used_block_flag = backup_used_block_flag;
    }
}

std::vector<std::pair<int, int>> GetScore_General(std::vector<Block> &blocks, WinningCondition &winning_condition)
{
    std::vector<std::pair<int, int>> yaku_score_vector;
    std::vector<int> used_block_flag((int)blocks.size(), 0);
    std::vector<int> checked_yaku(Yaku_Count, 0);
    

    int total_score = 0;
    // 0 is dummy
    for (int i = 1; i < Yaku_Count; ++i)
    {
        if (is_chain_yaku[i] || checked_yaku[i] != 0)
        {
            continue;
        }
        total_score += YakuCheck_Array[i](0, blocks, used_block_flag, checked_yaku, winning_condition);
    }
    GetScore_General_Recursion(0, total_score, blocks, used_block_flag, checked_yaku, winning_condition, total_score, yaku_score_vector);
    return yaku_score_vector;
}

/*
std::vector<std::pair<int, int>> GetScore_General(std::vector<Block> &blocks, WinningCondition &winning_condition)
{
    std::vector<int> used_block_flag(blocks.size(), 0);
    std::vector<int> checked_yaku(Yaku_Count, 0);
    std::vector<std::pair<int, int>> yaku_score_vector;

    int total_score = 0;
    // 0 is dummy
    for (int i = 1; i < Yaku_Count; ++i)
    {
        if (is_chain_yaku[i] || checked_yaku[i] != 0)
        {
            continue;
        }
        total_score += YakuCheck_Array[i](0, blocks, used_block_flag, checked_yaku, winning_condition);
    }
    GetScore_General_Recursion(0, total_score, blocks, used_block_flag, checked_yaku, winning_condition, total_score, yaku_score_vector);
    return yaku_score_vector;
}
*/

struct State
{
    std::vector<int> tiles;
    std::vector<Block> blocks;
    int index;
    int count_blocks;
    bool haspair;
    bool is_sequence_before;
    int before_index;
};

int DivideGeneralBlocksIterative(std::vector<int> &tiles, std::vector<Block> &blocks, WinningCondition &winning_condition, Hand *hand)
{
    std::stack<State> stack;

    // 초기 상태 푸시
    stack.push({tiles, blocks, 0, 0, false, false, -1});

    int success_count = 0;

    while (!stack.empty())
    {
        State current = stack.top();
        stack.pop();

        int index = current.index;

        // 1. 타일 개수가 0인 경우 건너뜀
        while (index < 34 && current.tiles[index] == 0)
        {
            ++index;
        }

        // 2. 종료 조건 (블록 수 5 이상 또는 인덱스 초과)
        if (index >= 34 || current.count_blocks >= 5)
        {
            bool has_tiles_left = false;
            for (int tile_count : current.tiles)
            {
                if (tile_count)
                {
                    has_tiles_left = true;
                    break;
                }
            }
            if (has_tiles_left)
                continue;

            std::vector<int> count_knitted(hand->closed_tiles.size());
            bool has_knitted_block = false;
            for (Block &block : current.blocks)
            {
                if (block.type == KNITTED)
                {
                    has_knitted_block = true;
                    count_knitted[block.tile] += 1;
                    count_knitted[block.tile + 3] += 1;
                    count_knitted[block.tile + 6] += 1;
                }
            }

            std::vector<std::pair<int, int>> yaku_score_vector = GetScore_General(current.blocks, winning_condition);
            if (!yaku_score_vector.empty())
            {
                int total_score = 0;
                std::vector<std::pair<int, int>> new_yaku_score_vector;

                for (auto &yaku_score_pair : yaku_score_vector)
                {
                    if (yaku_score_pair.first == yaku_map["YakuCheck_SingleWait"])
                    {
                        if (has_knitted_block && !count_knitted[winning_condition.tile])
                        {
                            total_score += yaku_score_pair.second;
                            new_yaku_score_vector.push_back(yaku_score_pair);
                        }
                        else
                        {
                            int wait_type = 0;
                            for (Block block : current.blocks)
                            {
                                if (block.source != SELF || block.type == QUAD)
                                    continue;
                                if (block.type == SEQUENCE)
                                {
                                    if ((block.tile % 9 == 0 && block.tile + 2 == winning_condition.tile) || (block.tile % 9 == 6 && block.tile == winning_condition.tile))
                                    {
                                        wait_type = 2;
                                        break;
                                    }
                                    if (block.tile + 1 == winning_condition.tile)
                                    {
                                        wait_type = 1;
                                        break;
                                    }
                                }
                                else if (block.type == PAIR && block.tile == winning_condition.tile)
                                {
                                    wait_type = 0;
                                }
                            }
                            total_score += yaku_score_pair.second;
                            new_yaku_score_vector.push_back(yaku_score_pair);
                        }
                    }
                    else
                    {
                        total_score += yaku_score_pair.second;
                        new_yaku_score_vector.push_back(yaku_score_pair);
                    }
                }

                if (hand->highest_score < total_score)
                {
                    hand->highest_score = total_score;
                    hand->yaku_score_list.clear();
                    hand->yaku_score_list = new_yaku_score_vector;
                }
                success_count++;
            }
            hand->is_blocks_divided = true;
            continue;
        }

        // 3. 트리플렛 시도
        if (current.tiles[index] >= 3 && (current.before_index != index || !current.is_sequence_before))
        {
            State next = current;
            next.tiles[index] -= 3;
            next.blocks.emplace_back(TRIPLET, index, SELF, 0);
            next.count_blocks++;
            stack.push(next);
        }

        // 4. 페어 시도
        if (current.tiles[index] >= 2 && !current.haspair && (current.before_index != index || !current.is_sequence_before))
        {
            State next = current;
            next.tiles[index] -= 2;
            next.blocks.emplace_back(PAIR, index, SELF, 0);
            next.count_blocks++;
            next.haspair = true;
            stack.push(next);
        }

        // 5. 시퀀스 시도
        if (index < 27 && (index % 9) <= 6 && current.tiles[index] >= 1 && current.tiles[index + 1] >= 1 && current.tiles[index + 2] >= 1)
        {
            State next = current;
            next.tiles[index] -= 1;
            next.tiles[index + 1] -= 1;
            next.tiles[index + 2] -= 1;
            next.blocks.emplace_back(SEQUENCE, index, SELF, 0);
            next.count_blocks++;
            next.is_sequence_before = true;
            next.before_index = index;
            stack.push(next);
        }
    }

    return success_count;
}

int DivideGeneralBlocksRecursion(std::vector<int> &tiles, std::vector<Block> &blocks, WinningCondition &winning_condition, int index, int count_blocks, bool haspair, bool is_sequence_before, int before_index, Hand *hand)
{
    // Skip tiles with zero count
    while (index < 34 && tiles[index] == 0)
    {
        ++index;
    }

    if (index >= 34 || count_blocks >= 5)
    {
        for (int tile_count : tiles)
        {
            if (tile_count)
            {
                return 0;
            }
        }
        std::vector<int> count_knitted(hand->closed_tiles.size());
        bool has_knitted_block = false;
        for (Block &block : blocks)
        {
            if (block.type == KNITTED)
            {
                has_knitted_block = true;
                count_knitted[block.tile] += 1;
                count_knitted[block.tile + 3] += 1;
                count_knitted[block.tile + 6] += 1;
            }
        }
        std::vector<std::pair<int, int>> yaku_score_vector = GetScore_General(blocks, winning_condition);
        if (!yaku_score_vector.empty())
        {
            int total_score = 0;
            std::vector<std::pair<int, int>> new_yaku_score_vector;
            for (auto &yaku_score_pair : yaku_score_vector)
            {
                if (yaku_score_pair.first == yaku_map["YakuCheck_SingleWait"])
                {
                    if (has_knitted_block)
                    {
                        if (!count_knitted[winning_condition.tile])
                        {
                            total_score += yaku_score_pair.second;
                            new_yaku_score_vector.push_back(yaku_score_pair);
                        }
                    }
                    else
                    {
                        int wait_type = 0;
                        for (Block block : blocks)
                        {
                            if (block.source != SELF || block.type == QUAD)
                            {
                                continue;
                            }
                            if (block.type == SEQUENCE)
                            {
                                if ((block.tile % 9 == 0 && block.tile + 2 == winning_condition.tile) || (block.tile % 9 == 6 && block.tile == winning_condition.tile))
                                {
                                    wait_type = 2;
                                    break;
                                }
                                if (block.tile + 1 == winning_condition.tile)
                                {
                                    wait_type = 1;
                                    break;
                                }
                            }
                            else if (block.type == PAIR)
                            {
                                if (block.tile == winning_condition.tile)
                                {
                                    wait_type = 0;
                                }
                            }
                        }
                        if (wait_type == 0)
                        {
                            total_score += yaku_score_pair.second;
                            new_yaku_score_vector.push_back(yaku_score_pair);
                        }
                        else if (wait_type == 1)
                        {
                            total_score += 1;
                            new_yaku_score_vector.push_back(std::pair<int, int>(yaku_map["YakuCheck_ClosedWait"], 1));
                        }
                        else
                        {
                            total_score += 1;
                            new_yaku_score_vector.push_back(std::pair<int, int>(yaku_map["YakuCheck_EdgeWait"], 1));
                        }
                    }
                }
                else
                {
                    total_score += yaku_score_pair.second;
                    new_yaku_score_vector.push_back(yaku_score_pair);
                }
            }
            if (hand->highest_score < total_score)
            {
                hand->highest_score = total_score;
                hand->yaku_score_list.clear();
                hand->yaku_score_list = new_yaku_score_vector;
            }
            return 1;
        }
        hand->is_blocks_divided = true;
        return 0;
    }

    int success_count = 0;

    // Attempt Triplet
    if (tiles[index] >= 3 && (before_index != index || (before_index == index && is_sequence_before == false)))
    {
        tiles[index] -= 3;
        blocks.emplace_back(TRIPLET, index, SELF, 0);
        success_count += DivideGeneralBlocksRecursion(tiles, blocks, winning_condition, index, count_blocks + 1, haspair, false, index, hand);
        tiles[index] += 3;
        blocks.pop_back();
    }

    // Attempt Pair
    if (tiles[index] >= 2 && !haspair && (before_index != index || (before_index == index && is_sequence_before == false)))
    {
        tiles[index] -= 2;
        blocks.emplace_back(PAIR, index, SELF, 0);
        success_count += DivideGeneralBlocksRecursion(tiles, blocks, winning_condition, index, count_blocks + 1, true, false, index, hand);
        tiles[index] += 2;
        blocks.pop_back();
    }

    // Attempt Sequence
    if (index < 27 && (index % 9) <= 6 && tiles[index] >= 1 && tiles[index + 1] >= 1 && tiles[index + 2] >= 1)
    {
        tiles[index] -= 1;
        tiles[index + 1] -= 1;
        tiles[index + 2] -= 1;
        blocks.emplace_back(SEQUENCE, index, SELF, 0);
        success_count += DivideGeneralBlocksRecursion(tiles, blocks, winning_condition, index, count_blocks + 1, haspair, true, index, hand);
        tiles[index] += 1;
        tiles[index + 1] += 1;
        tiles[index + 2] += 1;
        blocks.pop_back();
    }

    return success_count;
}

int Hand::DivideGeneralBlocks(WinningCondition winning_condition)
{
    std::vector<int> tiles(closed_tiles);

    std::vector<Block> divided_blocks(call_blocks);
    return DivideGeneralBlocksRecursion(tiles, divided_blocks, winning_condition, 0, (int)divided_blocks.size(), false, false, -1, this);
    // int DivideGeneralBlocksIterative(std::vector<int> &tiles, std::vector<Block> &blocks, WinningCondition &winning_condition, Hand *hand)
    //return DivideGeneralBlocksIterative(tiles, divided_blocks, winning_condition, this);
}

int Hand::DivideKnittedInGeneralBlocks(WinningCondition winning_condition)
{
    std::vector<int> tiles(closed_tiles);
    std::vector<Block> divided_blocks(call_blocks);
    std::vector<std::vector<int>> knitted_case = {
        {0, 3, 6, 19, 22, 25, 11, 14, 17},
        {0, 3, 6, 10, 13, 16, 20, 23, 26},
        {18, 21, 24, 1, 4, 7, 11, 14, 17},
        {18, 21, 24, 10, 13, 16, 2, 5, 8},
        {9, 12, 15, 1, 4, 7, 20, 23, 26},
        {9, 12, 15, 19, 22, 25, 2, 5, 8}};

    for (std::vector<int> &one_case : knitted_case)
    {
        bool has_knitted_blocks = true;

        for (int tile_index : one_case)
        {
            if (tiles[tile_index] < 1)
            {
                has_knitted_blocks = false;
                break;
            }
        }
        if (has_knitted_blocks)
        {
            for (int i = 0; i < 9; ++i)
            {
                if (i % 3 == 0)
                {
                    divided_blocks.push_back(Block(KNITTED, one_case[i], SELF, 0));
                }
                tiles[one_case[i]] -= 1;
            }
            return DivideGeneralBlocksRecursion(tiles, divided_blocks, winning_condition, 0, (int)divided_blocks.size(), false, false, -1, this);
            //return DivideGeneralBlocksIterative(tiles, divided_blocks, winning_condition, this);
        }
    }
    return 0;
}

int Hand::DivideSevenPairsBlocks(WinningCondition winning_condition)
{
    if (call_blocks.size())
    {
        return 0;
    }
    std::vector<int> tiles(closed_tiles);
    std::vector<Block> divided_blocks;

    int count_blocks = 0;
    for (int index = 0; index < 34; ++index)
    {
        int &tile_count = tiles[index];
        if (tile_count && tile_count % 2 != 0)
        {
            return 0;
        }
        for (int i = 0; i < tile_count / 2; ++i)
        {
            divided_blocks.emplace_back(PAIR, index, SELF, 0);
            count_blocks += 1;
        }
    }
    if (count_blocks == 7)
    {
        auto yaku_score_vector = GetScore_General(divided_blocks, winning_condition);
        if (!yaku_score_vector.empty())
        {
            int total_score = 0;
            for (auto &yaku_score_pair : yaku_score_vector)
            {
                total_score += yaku_score_pair.second;
            }
            if (this->highest_score < total_score)
            {
                this->highest_score = total_score;
                this->yaku_score_list.clear();
                this->yaku_score_list = yaku_score_vector;
            }
            return 1;
        }
    }
    return 0;
}
int Hand::DivideHonorsAndKnittedTilesBlocks(WinningCondition winning_condition)
{
    if (call_blocks.size())
    {
        return 0;
    }
    std::vector<int> tiles(closed_tiles);
    std::vector<Block> divided_blocks;

    int count_blocks = 0;

    std::vector<std::vector<int>> table(3, std::vector<int>(3, 0));

    for (int index = 0; index < 34; ++index)
    {
        int &tile_count = tiles[index];
        if (tile_count && tile_count > 1)
        {
            return 0;
        }
        if (tile_count)
        {
            divided_blocks.emplace_back(SINGLETILE, index, SELF, 0);
            count_blocks += 1;
            if (IsNumber(index))
            {
                table[index / 9][index % 3] += 1;
            }
        }
    }
    for (int i = 0; i < 3; ++i)
    {
        std::vector<int> zero_count(2, 0);
        for (int j = 0; j < 3; ++j)
        {
            if (table[i][j] == 0)
            {
                zero_count[0] += 1;
            }
            if (table[j][i] == 0)
            {
                zero_count[1] += 1;
            }
        }
        if (zero_count[0] < 2 || zero_count[1] < 2)
        {
            return 0;
        }
    }
    if (count_blocks == 14)
    {
        auto yaku_score_vector = GetScore_General(divided_blocks, winning_condition);

        if (!yaku_score_vector.empty())
        {
            int total_score = 0;
            for (auto &yaku_score_pair : yaku_score_vector)
            {
                total_score += yaku_score_pair.second;
            }
            //Print_YakuScoreVector(yaku_score_vector);
            if (this->highest_score < total_score)
            {
                this->highest_score = total_score;
                this->yaku_score_list.clear();
                this->yaku_score_list = yaku_score_vector;
            }
            return 1;
        }
    }
    return 0;
}

int Hand::DivdeThirteenOrphansBlocks(WinningCondition winning_condition)
{
    if (call_blocks.size())
    {
        return 0;
    }
    std::vector<int> tiles(closed_tiles);
    std::vector<Block> divided_blocks;
    bool has_pair = false;
    int count_blocks = 0;

    for (int index = 0; index < 34; ++index)
    {
        int &tile_count = tiles[index];
        if (tile_count > 0 && !IsOutside(index))
        {
            return 0;
        }
        if (!has_pair)
        {
            if (tile_count == 2)
            {
                has_pair = true;
                divided_blocks.emplace_back(PAIR, index, SELF, 0);
                count_blocks += 1;
                continue;
            }
        }
        if (tile_count)
        {
            divided_blocks.emplace_back(SINGLETILE, index, SELF, 0);
            count_blocks += 1;
        }
    }
    if (has_pair && count_blocks == 13)
    {
        auto yaku_score_vector = GetScore_General(divided_blocks, winning_condition);
        if (!yaku_score_vector.empty())
        {
            int total_score = 0;
            for (auto &yaku_score_pair : yaku_score_vector)
            {
                total_score += yaku_score_pair.second;
            }
            if (this->highest_score < total_score)
            {
                this->highest_score = total_score;
                this->yaku_score_list.clear();
                this->yaku_score_list = yaku_score_vector;
            }
            return 1;
        }
    }
    return 0;
}

int Hand::DivideBlocks(WinningCondition winning_condition)
{
    this->is_blocks_divided = false;
    int success_count = 0;
    this->highest_score = 0;
    this->yaku_score_list.clear();

    if (this->closed_tiles[34] > 0)
    {
        return 0;
    }
    success_count += DivdeThirteenOrphansBlocks(winning_condition);
    success_count += DivideHonorsAndKnittedTilesBlocks(winning_condition);
    success_count += DivideSevenPairsBlocks(winning_condition);
    success_count += DivideKnittedInGeneralBlocks(winning_condition);
    if (success_count)
    {
        this->is_blocks_divided = true;
    }

    success_count += DivideGeneralBlocks(winning_condition);

    if (success_count == 0 && this->is_blocks_divided == true && winning_condition.count_winning_conditions > 1)
    {
        this->highest_score = 8;
        this->yaku_score_list.clear();
        this->yaku_score_list.push_back(std::pair<int, int>(0, 8));
    }
    return success_count;
}

void DivdeThirteenOrphansBlocksForTenpaiTiles(Hand *hand)
{
    if (hand->call_blocks.size())
    {
        return;
    }
    std::vector<int> tiles(hand->closed_tiles);
    std::vector<Block> divided_blocks;
    bool has_pair = false;
    int count_blocks = 0;

    for (int index = 0; index < 34; ++index)
    {
        int &tile_count = tiles[index];
        if (!has_pair)
        {
            if (tile_count == 2)
            {
                has_pair = true;
                divided_blocks.emplace_back(PAIR, index, SELF, 0);
                count_blocks += 1;
                continue;
            }
        }
        if (tile_count && (!IsOutside(index) || tile_count > 1))
        {
            return;
        }
        if (tile_count)
        {
            divided_blocks.emplace_back(SINGLETILE, index, SELF, 0);
            count_blocks += 1;
        }
    }
    if (has_pair && count_blocks == 13)
    {
        hand->is_blocks_divided = true;
    }
}

void DivideHonorsAndKnittedTilesBlocksForTenpaiTiles(Hand *hand)
{
    if (hand->call_blocks.size())
    {
        return;
    }
    std::vector<int> tiles(hand->closed_tiles);
    std::vector<Block> divided_blocks;

    int count_blocks = 0;

    std::vector<std::vector<int>> table(3, std::vector<int>(3, 0));

    for (int index = 0; index < 34; ++index)
    {
        int &tile_count = tiles[index];
        if (tile_count && tile_count > 1)
        {
            return;
        }
        if (tile_count)
        {
            divided_blocks.emplace_back(SINGLETILE, index, SELF, 0);
            count_blocks += 1;
            if (IsNumber(index))
            {
                table[index / 9][index % 3] += 1;
            }
        }
    }
    for (int i = 0; i < 3; ++i)
    {
        std::vector<int> zero_count(2, 0);
        for (int j = 0; j < 3; ++j)
        {
            if (table[i][j] == 0)
            {
                zero_count[0] += 1;
            }
            if (table[j][i] == 0)
            {
                zero_count[1] += 1;
            }
        }
        if (zero_count[0] < 2 || zero_count[1] < 2)
        {
            return;
        }
    }
    if (count_blocks == 14)
    {
        hand->is_blocks_divided = 1;
    }
}

void DivideSevenPairsBlocksForTenpaiTiles(Hand *hand)
{
    if (hand->call_blocks.size())
    {
        return;
    }
    std::vector<int> tiles(hand->closed_tiles);
    std::vector<Block> divided_blocks;

    int count_blocks = 0;
    for (int index = 0; index < 34; ++index)
    {
        int &tile_count = tiles[index];
        if (tile_count && tile_count % 2 != 0)
        {
            return;
        }
        for (int i = 0; i < tile_count / 2; ++i)
        {
            divided_blocks.emplace_back(PAIR, index, SELF, 0);
            count_blocks += 1;
        }
    }
    if (count_blocks == 7)
    {
        hand->is_blocks_divided = true;
    }
}

void DivideGeneralBlocksForTenpaiTile_Recursion(std::vector<int> &tiles, std::vector<Block> &blocks, int index, int count_blocks, bool haspair, bool is_sequence_before, int before_index, Hand *hand)
{
    // Skip tiles with zero count
    while (index < 34 && tiles[index] == 0)
    {
        ++index;
    }

    if (index >= 34 || count_blocks >= 5)
    {
        for (int tile_count : tiles)
        {
            if (tile_count)
            {
                return;
            }
        }
        hand->is_blocks_divided = true;
        return;
    }

    // Attempt Triplet
    if (tiles[index] >= 3 && (before_index != index || (before_index == index && is_sequence_before == false)))
    {
        tiles[index] -= 3;
        blocks.emplace_back(TRIPLET, index, SELF, 0);
        DivideGeneralBlocksForTenpaiTile_Recursion(tiles, blocks, index, count_blocks + 1, haspair, false, index, hand);
        tiles[index] += 3;
        blocks.pop_back();
    }

    // Attempt Pair
    if (tiles[index] >= 2 && !haspair && (before_index != index || (before_index == index && is_sequence_before == false)))
    {
        tiles[index] -= 2;
        blocks.emplace_back(PAIR, index, SELF, 0);
        DivideGeneralBlocksForTenpaiTile_Recursion(tiles, blocks, index, count_blocks + 1, true, false, index, hand);
        tiles[index] += 2;
        blocks.pop_back();
    }

    // Attempt Sequence
    if (index < 27 && (index % 9) <= 6 && tiles[index] >= 1 && tiles[index + 1] >= 1 && tiles[index + 2] >= 1)
    {
        tiles[index] -= 1;
        tiles[index + 1] -= 1;
        tiles[index + 2] -= 1;
        blocks.emplace_back(SEQUENCE, index, SELF, 0);
        DivideGeneralBlocksForTenpaiTile_Recursion(tiles, blocks, index, count_blocks + 1, haspair, true, index, hand);
        tiles[index] += 1;
        tiles[index + 1] += 1;
        tiles[index + 2] += 1;
        blocks.pop_back();
    }
}

void DivideGeneralBlocksForTenpaiTiles(Hand *hand)
{
    std::vector<int> tiles(hand->closed_tiles);

    std::vector<Block> divided_blocks(hand->call_blocks);
    DivideGeneralBlocksForTenpaiTile_Recursion(tiles, divided_blocks, 0, (int)divided_blocks.size(), false, false, -1, hand);
}

void DivideKnittedInGeneralBlocksForTenpaiTiles(Hand *hand)
{
    std::vector<int> tiles(hand->closed_tiles);
    std::vector<Block> divided_blocks(hand->call_blocks);
    std::vector<std::vector<int>> knitted_case = {
        {0, 3, 6, 19, 22, 25, 11, 14, 17},
        {0, 3, 6, 10, 13, 16, 20, 23, 26},
        {18, 21, 24, 1, 4, 7, 11, 14, 17},
        {18, 21, 24, 10, 13, 16, 2, 5, 8},
        {9, 12, 15, 1, 4, 7, 20, 23, 26},
        {9, 12, 15, 19, 22, 25, 2, 5, 8}};

    for (std::vector<int> &one_case : knitted_case)
    {
        bool has_knitted_blocks = true;

        for (int tile_index : one_case)
        {
            if (tiles[tile_index] < 1)
            {
                has_knitted_blocks = false;
                break;
            }
        }
        if (has_knitted_blocks)
        {
            for (int i = 0; i < 9; ++i)
            {
                if (i % 3 == 0)
                {
                    divided_blocks.push_back(Block(KNITTED, one_case[i], SELF, 0));
                }
                tiles[one_case[i]] -= 1;
            }
            DivideGeneralBlocksForTenpaiTile_Recursion(tiles, divided_blocks, 0, (int)divided_blocks.size(), false, false, -1, hand);
        }
    }
    return;
}

void Hand::DivideBlocksForTenpaiTiles()
{
    this->is_blocks_divided = false;
    DivdeThirteenOrphansBlocksForTenpaiTiles(this);
    DivideHonorsAndKnittedTilesBlocksForTenpaiTiles(this);
    DivideSevenPairsBlocksForTenpaiTiles(this);
    DivideKnittedInGeneralBlocksForTenpaiTiles(this);
    DivideGeneralBlocksForTenpaiTiles(this);
    ;
}
void Hand::GetKeishikiTenpaiTiles()
{
    this->keishiki_tenpai_tiles.clear();
    if (this->closed_tiles[34] > 0)
    {
        return;
    }
    for (int winning_tile = 0; winning_tile < 34; ++winning_tile)
    {
        this->closed_tiles[winning_tile] += 1;
        DivideBlocksForTenpaiTiles();
        if (this->is_blocks_divided == true)
        {
            keishiki_tenpai_tiles.push_back(winning_tile);
        }
        this->closed_tiles[winning_tile] -= 1;
    }
}

void InitializeYakuMap()
{
#define X(YakuName, YakuFunc) yaku_map["YakuCheck_" #YakuName] = Yaku_##YakuName;
    YAKU_LIST
#undef X
}

void InitializeInverseYakuMap()
{
#define X(YakuName, YakuFunc) inverse_yaku_map[Yaku_##YakuName] = "YakuCheck_" #YakuName;
    YAKU_LIST
#undef X
}

// Function to initialize the function-to-name map
void InitializeYakuFunctionToNameMap()
{
#define X(YakuName, YakuFunc) YakuFunctionToName_Map[YakuFunc] = #YakuName;
    YAKU_LIST
#undef X
}

void InitializeChainYakuIndex()
{
    is_chain_yaku = std::vector<int>(Yaku_Count, 0);

    chain_yaku_index = {
        {yaku_map["YakuCheck_PureTripleChow"]},
        {yaku_map["YakuCheck_PureShiftedPungs"]},
        {yaku_map["YakuCheck_PureStraight"]},
        {yaku_map["YakuCheck_PureShiftedChows"]},
        {yaku_map["YakuCheck_TriplePung"]},
        {yaku_map["YakuCheck_MixedStraight"]},
        {yaku_map["YakuCheck_MixedTripleChow"]},
        {yaku_map["YakuCheck_MixedShiftedPungs"]},
        {yaku_map["YakuCheck_MixedShiftedChows"]},
        {yaku_map["YakuCheck_DoublePung"], yaku_map["YakuCheck_TriplePung"]},
        {yaku_map["YakuCheck_PureDoubleChow"], yaku_map["YakuCheck_PureTripleChow"]},
        {yaku_map["YakuCheck_MixedDoubleChow"], yaku_map["YakuCheck_MixedTripleChow"]},
        {yaku_map["YakuCheck_ShortStraight"], yaku_map["YakuCheck_PureStraight"]},
        {yaku_map["YakuCheck_TwoTerminalChows"], yaku_map["YakuCheck_PureStraight"]}};
    // chain_yaku_index = {
    //     {yaku_map["YakuCheck_PureTripleChow"], yaku_map["YakuCheck_PureDoubleChow"]},
    //     {yaku_map["YakuCheck_PureShiftedPungs"]},
    //     {yaku_map["YakuCheck_PureStraight"], yaku_map["YakuCheck_ShortStraight"], yaku_map["YakuCheck_TwoTerminalChows"]},
    //     {yaku_map["YakuCheck_PureShiftedChows"]},
    //     {yaku_map["YakuCheck_TriplePung"], yaku_map["YakuCheck_DoublePung"]},
    //     {yaku_map["YakuCheck_MixedStraight"]},
    //     {yaku_map["YakuCheck_MixedTripleChow"], yaku_map["YakuCheck_MixedDoubleChow"]},
    //     {yaku_map["YakuCheck_MixedShiftedPungs"]},
    //     {yaku_map["YakuCheck_MixedShiftedChows"]},
    //     {yaku_map["YakuCheck_DoublePung"], yaku_map["YakuCheck_TriplePung"]},
    //     {yaku_map["YakuCheck_PureDoubleChow"], yaku_map["YakuCheck_PureTripleChow"]},
    //     {yaku_map["YakuCheck_MixedDoubleChow"], yaku_map["YakuCheck_MixedTripleChow"]},
    //     {yaku_map["YakuCheck_ShortStraight"], yaku_map["YakuCheck_PureStraight"]},
    //     {yaku_map["YakuCheck_TwoTerminalChows"], yaku_map["YakuCheck_TwoTerminalChows"]}};
    for (std::vector<int> &index_vector : chain_yaku_index)
    {
        is_chain_yaku[index_vector[0]] = 1;
    }
}

void InitializeAll()
{
    yaku_map.clear();
    inverse_yaku_map.clear();
    YakuFunctionToName_Map.clear();
    InitializeYakuMap();
    InitializeInverseYakuMap();
    InitializeYakuFunctionToNameMap();
    InitializeChainYakuIndex();
}

// BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
// {
//     switch (ul_reason_for_call)
//     {
//     case DLL_PROCESS_ATTACH:
//         InitializeAllOnce();
//         break;
//     case DLL_PROCESS_DETACH:
//         // 필요 시 자원 해제 로직 추가
//         break;
//     }
//     return TRUE;
// }

void Hand::print_score()
{
    std::cout << "highest score: " << highest_score << std::endl;
    for (auto yaku_score : yaku_score_list)
    {
        std::cout << inverse_yaku_map[yaku_score.first] << ": " << yaku_score.second << std::endl;
    }
}

struct YakuScoreData
{
    int YakuId;
    int Score;
};

struct YakuScoreDataArray
{
    YakuScoreData *data;
    int count;
    YakuScoreDataArray()
    {
        data = nullptr;
        count = 0;
    }
};

struct TileScoreData
{
    int tile;       // 타일 번호
    int tsumoScore; // 쯔모 점수
    int ronScore;   // 론 점수
};

struct TileScoreDataArray
{
    TileScoreData *data; // 데이터 배열
    int count;           // 배열 크기
    TileScoreDataArray()
    {
        data = nullptr;
        count = 0;
    }
};

// #pragma pack(push, 1) // Pack을 1로 설정하여 필드의 메모리 정렬을 C#과 동일하게 맞춤
struct BlockData
{
    int Type;            // BlockType (enum -> int)
    int Tile;            // 타일 ID
    int Source;          // BlockSource (enum -> int)
    int SourceTileIndex; // 출처 타일의 인덱스
};
// #pragma pack(pop)
//
// #pragma pack(push, 1)
struct WinningConditionData
{
    int WinningTile;         // Winning 타일 ID
    int IsDiscarded;         // 버려진 타일 여부
    int IsLastTileInTheGame; // 마지막 타일 여부
    int IsLastTileOfItsKind; // 해당 종류의 마지막 타일 여부
    int IsReplacementTile;   // 대체 타일 여부
    int IsRobbingTheKong;    // Robbing the Kong 여부
    int RoundWind;
    int SeatWind;
};
// #pragma pack(pop)
//
// #pragma pack(push, 1)
struct HandData
{
    int ClosedTiles[35];      // 숨겨진 손패 (고정 크기 배열)
    int OpenedTiles[35];      // 드러난 손패 (고정 크기 배열)
    int WinningTile;          // Winning 타일 ID
    BlockData CallBlocks[14]; // 블록 데이터 (최대 14개)
    int CallBlockCount;       // 실제 CallBlocks의 개수
};
// #pragma pack(pop)
/*
// 함수 선언
extern "C" YakuScoreDataArray GetHuYakuScoreData(HandData handInput, WinningConditionData conditionInput);
extern "C" TileScoreDataArray GetTenpaiTileScoreData(HandData handInput, WinningConditionData conditionInput);
extern "C" void FreeTileScoreData(TileScoreData *data);
extern "C" void FreeYakuScoreData(YakuScoreData *data);

extern "C" YakuScoreDataArray GetHuYakuScoreData(HandData handInput, WinningConditionData conditionInput)
{
    // InitializeAll();
    InitializeAllOnce();
    YakuScoreDataArray resultArray = YakuScoreDataArray();
    Hand hand = Hand();
    if (handInput.ClosedTiles[34] != 0)
    {
        return resultArray;
    }
    for (int i = 0; i < 34; ++i)
    {
        hand.closed_tiles[i] = handInput.ClosedTiles[i];
        hand.opened_tiles[i] = handInput.OpenedTiles[i];
    }
    for (int i = 0; i < handInput.CallBlockCount; ++i)
    {
        BlockData &block = handInput.CallBlocks[i];
        hand.call_blocks.push_back(Block((Blocktype)block.Type, block.Tile, (Blocksource)block.Source, block.SourceTileIndex));
    }
    WinningCondition winningCondition = WinningCondition();
    winningCondition.tile = conditionInput.WinningTile;
    if (winningCondition.tile < 0 || winningCondition.tile >= 34)
    {
        return resultArray;
    }
    winningCondition.round_wind = conditionInput.RoundWind;
    winningCondition.seat_wind = conditionInput.SeatWind;
    winningCondition.is_discarded = conditionInput.IsDiscarded;
    winningCondition.is_last_tile_in_the_game = conditionInput.IsLastTileInTheGame;
    winningCondition.is_last_tile_of_its_kind = conditionInput.IsLastTileOfItsKind;
    winningCondition.is_replacement_tile = conditionInput.IsReplacementTile;
    winningCondition.is_robbing_the_kong = conditionInput.IsRobbingTheKong;

    int tileCount = handInput.CallBlockCount * 3;
    for (int i = 0; i < 34; ++i)
    {
        if (hand.closed_tiles[i] >= 0)
        {
            tileCount += hand.closed_tiles[i];
        }
        else
        {
            return resultArray;
        }
    }
    if (tileCount != 14)
    {
        return resultArray;
    }
    hand.closed_tiles[winningCondition.tile] -= 1;
    if (hand.closed_tiles[winningCondition.tile] < 0)
    {
        return resultArray;
    }
    hand.GetKeishikiTenpaiTiles();

    if (hand.keishiki_tenpai_tiles.empty())
    {
        return resultArray;
    }
    hand.closed_tiles[winningCondition.tile] += 1;
    winningCondition.count_winning_conditions = (int)hand.keishiki_tenpai_tiles.size();
    hand.DivideBlocks(winningCondition);
    // debug code
    // return resultArray;
    //
    resultArray.count = (int)hand.yaku_score_list.size();
    resultArray.data = new YakuScoreData[resultArray.count];
    if (!resultArray.data)
    {
        resultArray.count = 0;
        return resultArray;
    }
    for (int i = 0; i < resultArray.count; ++i)
    {
        resultArray.data[i].YakuId = hand.yaku_score_list[i].first;
        resultArray.data[i].Score = hand.yaku_score_list[i].second;
    }
    return resultArray;
}

extern "C" TileScoreDataArray GetTenpaiTileScoreData(HandData handInput, WinningConditionData conditionInput)
{
    // InitializeAll();
    InitializeAllOnce();
    TileScoreDataArray resultArray = TileScoreDataArray();
    Hand hand = Hand();
    if (handInput.ClosedTiles[34] != 0)
    {
        return resultArray;
    }
    for (int i = 0; i < 34; ++i)
    {
        hand.closed_tiles[i] = handInput.ClosedTiles[i];
        hand.opened_tiles[i] = handInput.OpenedTiles[i];
    }
    for (int i = 0; i < handInput.CallBlockCount; ++i)
    {
        BlockData &block = handInput.CallBlocks[i];
        hand.call_blocks.push_back(Block((Blocktype)block.Type, block.Tile, (Blocksource)block.Source, block.SourceTileIndex));
    }
    WinningCondition winningCondition = WinningCondition();
    winningCondition.tile = conditionInput.WinningTile;
    if (winningCondition.tile != -1)
    {
        return resultArray;
    }
    winningCondition.round_wind = conditionInput.RoundWind;
    winningCondition.seat_wind = conditionInput.SeatWind;
    winningCondition.is_discarded = conditionInput.IsDiscarded;
    winningCondition.is_last_tile_in_the_game = conditionInput.IsLastTileInTheGame;
    winningCondition.is_last_tile_of_its_kind = conditionInput.IsLastTileOfItsKind;
    winningCondition.is_replacement_tile = conditionInput.IsReplacementTile;
    winningCondition.is_robbing_the_kong = conditionInput.IsRobbingTheKong;

    int tileCount = handInput.CallBlockCount * 3;
    for (int i = 0; i < 34; ++i)
    {
        if (hand.closed_tiles[i] >= 0)
        {
            tileCount += hand.closed_tiles[i];
        }
        else
        {
            return resultArray;
        }
    }
    if (tileCount != 13)
    {
        return resultArray;
    }
    if (hand.closed_tiles[winningCondition.tile] < 0)
    {
        return resultArray;
    }
    hand.GetKeishikiTenpaiTiles();

    if (hand.keishiki_tenpai_tiles.empty())
    {
        return resultArray;
    }

    winningCondition.count_winning_conditions = (int)hand.keishiki_tenpai_tiles.size();
    resultArray.count = (int)hand.keishiki_tenpai_tiles.size();
    resultArray.data = new TileScoreData[resultArray.count];
    if (!resultArray.data)
    {
        resultArray.count = 0;
        return resultArray;
    }
    for (int i = 0; i < resultArray.count; ++i)
    {
        int winning_tile = hand.keishiki_tenpai_tiles[i];
        hand.closed_tiles[winning_tile] += 1;
        winningCondition.tile = winning_tile;
        resultArray.data[i].tile = winning_tile;
        winningCondition.is_discarded = false;
        hand.DivideBlocks(winningCondition);
        resultArray.data[i].tsumoScore = hand.highest_score;
        winningCondition.is_discarded = true;
        hand.DivideBlocks(winningCondition);
        resultArray.data[i].ronScore = hand.highest_score;
        hand.closed_tiles[winning_tile] -= 1;
    }
    return resultArray;
}

extern "C" void FreeTileScoreData(TileScoreData *data)
{
    if (data != nullptr)
    {
        delete[] data;
        std::cout << "[DEBUG] FreeTileScoreData called." << std::endl;
    }
}

extern "C" void FreeYakuScoreData(YakuScoreData *data)
{
    if (data != nullptr)
    {
        delete[] data;
        std::cout << "[DEBUG] FreeYakuScoreData called." << std::endl;
    }
}


extern "C" void InitLibrary()
{
    std::cout << "[DEBUG] InitLibrary() called from Unity." << std::endl;
    InitializeAllOnce();
}
*/

void InitializeAllOnce()
{
    //std::cout << "[DEBUG] InitializeAllOnce() called." << std::endl;
    std::lock_guard<std::mutex> lock(initMutex);
    if (!initialized)
    {
        //std::cout << "[DEBUG] Initializing components..." << std::endl;
        InitializeAll();
        initialized = true;
        //std::cout << "[DEBUG] Initialization complete." << std::endl;
    }
    else
    {
        //std::cout << "[DEBUG] Already initialized." << std::endl;
    }
}


extern "C" {
    // InitLibrary 연결
    void DataTransfer_ScoreCalculatorInterop_InitLibrary() {
        //std::cout << "[DEBUG] InitLibrary() called from Unity via IL2CPP." << std::endl;
        InitializeAllOnce();
    }

    // 후 역 데이터 반환
    YakuScoreDataArray DataTransfer_ScoreCalculatorInterop_GetHuYakuScoreData(HandData handInput, WinningConditionData conditionInput) {
        //std::cout << "[DEBUG] GetHuYakuScoreData called." << std::endl;
        YakuScoreDataArray resultArray = YakuScoreDataArray();
        Hand hand = Hand();
        if (handInput.ClosedTiles[34] != 0)
        {
            return resultArray;
        }
        for (int i = 0; i < 34; ++i)
        {
            hand.closed_tiles[i] = handInput.ClosedTiles[i];
            hand.opened_tiles[i] = handInput.OpenedTiles[i];
        }
        for (int i = 0; i < handInput.CallBlockCount; ++i)
        {
            BlockData& block = handInput.CallBlocks[i];
            hand.call_blocks.push_back(Block((Blocktype)block.Type, block.Tile, (Blocksource)block.Source, block.SourceTileIndex));
        }
        WinningCondition winningCondition = WinningCondition();
        winningCondition.tile = conditionInput.WinningTile;
        if (winningCondition.tile < 0 || winningCondition.tile >= 34)
        {
            return resultArray;
        }
        winningCondition.round_wind = conditionInput.RoundWind;
        winningCondition.seat_wind = conditionInput.SeatWind;
        winningCondition.is_discarded = conditionInput.IsDiscarded;
        winningCondition.is_last_tile_in_the_game = conditionInput.IsLastTileInTheGame;
        winningCondition.is_last_tile_of_its_kind = conditionInput.IsLastTileOfItsKind;
        winningCondition.is_replacement_tile = conditionInput.IsReplacementTile;
        winningCondition.is_robbing_the_kong = conditionInput.IsRobbingTheKong;

        int tileCount = handInput.CallBlockCount * 3;
        for (int i = 0; i < 34; ++i)
        {
            if (hand.closed_tiles[i] >= 0)
            {
                tileCount += hand.closed_tiles[i];
            }
            else
            {
                return resultArray;
            }
        }
        if (tileCount != 14)
        {
            return resultArray;
        }
        hand.closed_tiles[winningCondition.tile] -= 1;
        if (hand.closed_tiles[winningCondition.tile] < 0)
        {
            return resultArray;
        }
        hand.GetKeishikiTenpaiTiles();

        if (hand.keishiki_tenpai_tiles.empty())
        {
            return resultArray;
        }
        hand.closed_tiles[winningCondition.tile] += 1;
        winningCondition.count_winning_conditions = (int)hand.keishiki_tenpai_tiles.size();
        hand.DivideBlocks(winningCondition);
        // debug code
        // return resultArray;
        //
        resultArray.count = (int)hand.yaku_score_list.size();
        resultArray.data = new YakuScoreData[resultArray.count];
        if (!resultArray.data)
        {
            resultArray.count = 0;
            return resultArray;
        }
        for (int i = 0; i < resultArray.count; ++i)
        {
            resultArray.data[i].YakuId = hand.yaku_score_list[i].first;
            resultArray.data[i].Score = hand.yaku_score_list[i].second;
        }
        return resultArray;

    }

    // 점수 데이터 반환
    TileScoreDataArray DataTransfer_ScoreCalculatorInterop_GetTenpaiTileScoreData(HandData handInput, WinningConditionData conditionInput) {
        //std::cout << "[DEBUG] GetTenpaiTileScoreData called." << std::endl;

        TileScoreDataArray resultArray = TileScoreDataArray();
        Hand hand = Hand();
        if (handInput.ClosedTiles[34] != 0)
        {
            return resultArray;
        }
        for (int i = 0; i < 34; ++i)
        {
            hand.closed_tiles[i] = handInput.ClosedTiles[i];
            hand.opened_tiles[i] = handInput.OpenedTiles[i];
        }
        for (int i = 0; i < handInput.CallBlockCount; ++i)
        {
            BlockData& block = handInput.CallBlocks[i];
            hand.call_blocks.push_back(Block((Blocktype)block.Type, block.Tile, (Blocksource)block.Source, block.SourceTileIndex));
        }
        WinningCondition winningCondition = WinningCondition();
        winningCondition.tile = conditionInput.WinningTile;
        if (winningCondition.tile != -1)
        {
            return resultArray;
        }
        winningCondition.round_wind = conditionInput.RoundWind;
        winningCondition.seat_wind = conditionInput.SeatWind;
        winningCondition.is_discarded = conditionInput.IsDiscarded;
        winningCondition.is_last_tile_in_the_game = conditionInput.IsLastTileInTheGame;
        winningCondition.is_last_tile_of_its_kind = conditionInput.IsLastTileOfItsKind;
        winningCondition.is_replacement_tile = conditionInput.IsReplacementTile;
        winningCondition.is_robbing_the_kong = conditionInput.IsRobbingTheKong;

        int tileCount = handInput.CallBlockCount * 3;
        for (int i = 0; i < 34; ++i)
        {
            if (hand.closed_tiles[i] >= 0)
            {
                tileCount += hand.closed_tiles[i];
            }
            else
            {
                return resultArray;
            }
        }
        if (tileCount != 13)
        {
            return resultArray;
        }
        if (hand.closed_tiles[winningCondition.tile] < 0)
        {
            return resultArray;
        }
        hand.GetKeishikiTenpaiTiles();

        if (hand.keishiki_tenpai_tiles.empty())
        {
            return resultArray;
        }

        winningCondition.count_winning_conditions = (int)hand.keishiki_tenpai_tiles.size();
        resultArray.count = (int)hand.keishiki_tenpai_tiles.size();
        resultArray.data = new TileScoreData[resultArray.count];
        if (!resultArray.data)
        {
            resultArray.count = 0;
            return resultArray;
        }
        for (int i = 0; i < resultArray.count; ++i)
        {
            int winning_tile = hand.keishiki_tenpai_tiles[i];
            hand.closed_tiles[winning_tile] += 1;
            winningCondition.tile = winning_tile;
            resultArray.data[i].tile = winning_tile;
            winningCondition.is_discarded = false;
            hand.DivideBlocks(winningCondition);
            resultArray.data[i].tsumoScore = hand.highest_score;
            winningCondition.is_discarded = true;
            hand.DivideBlocks(winningCondition);
            resultArray.data[i].ronScore = hand.highest_score;
            hand.closed_tiles[winning_tile] -= 1;
        }
        return resultArray;
    }

    // 메모리 해제
    void DataTransfer_ScoreCalculatorInterop_FreeTileScoreData(TileScoreData* data) {
        if (data != nullptr) {
            delete[] data;
            //std::cout << "[DEBUG] FreeTileScoreData called." << std::endl;
        }
    }

    void DataTransfer_ScoreCalculatorInterop_FreeYakuScoreData(YakuScoreData* data) {
        if (data != nullptr) {
            delete[] data;
            //std::cout << "[DEBUG] FreeYakuScoreData called." << std::endl;
        }
    }
}

/*
// --- 결과 출력 함수 ---
void PrintYakuScoreData(const YakuScoreDataArray &yakuScores)
{
    std::cout << "==== Yaku Score Data ====\n";
    for (int i = 0; i < yakuScores.count; ++i)
    {
        std::cout << "Yaku ID: " << yakuScores.data[i].YakuId
                  << ", Score: " << yakuScores.data[i].Score << "\n";
    }
}

void PrintTenpaiTileScores(const TileScoreDataArray &tenpaiScores)
{
    std::cout << "==== Tenpai Tile Scores ====\n";
    for (int i = 0; i < tenpaiScores.count; ++i)
    {
        std::cout << "Tile: " << tenpaiScores.data[i].tile
                  << ", Tsumo Score: " << tenpaiScores.data[i].tsumoScore
                  << ", Ron Score: " << tenpaiScores.data[i].ronScore << "\n";
    }
}

int main()
{
    InitializeAll();

    std::string buf1[2];
    std::cout << "Enter two hands (space-separated):" << std::endl;
    std::cin >> buf1[0] >> buf1[1];

    auto p = handString_To_Pair_Of_ConcealedTileArray_And_CallBlockArray(buf1[0]);
    HandData handData = {};
    for (int i = 0; i < 34; ++i)
    {
        handData.ClosedTiles[i] = p.first[i];
    }
    for (size_t i = 0; i < p.second.size(); ++i)
    {
        handData.CallBlocks[i] = {p.second[i].type, p.second[i].tile, p.second[i].source, p.second[i].source_tile_index};
    }
    handData.CallBlockCount = static_cast<int>(p.second.size());

    WinningConditionData winningCondition = {};
    winningCondition.WinningTile = -1;
    handData.WinningTile = -1;
    TileScoreDataArray tenpaiScores = GetTenpaiTileScoreData(handData, winningCondition);
    PrintTenpaiTileScores(tenpaiScores);

    winningCondition.WinningTile = handData.WinningTile;

    auto p2 = handString_To_Pair_Of_ConcealedTileArray_And_CallBlockArray(buf1[1]);
    for (int i = 0; i < 34; ++i)
    {
        if (p2.first[i])
        {
            handData.WinningTile = i;
            winningCondition.WinningTile = i;
            handData.ClosedTiles[i]++;
            break;
        }
    }

    std::cout << "Enter discard flag (0: Tsumo, 1: Ron), round wind (0-3), seat wind (0-3):" << std::endl;
    std::cin >> winningCondition.IsDiscarded >> winningCondition.RoundWind >> winningCondition.SeatWind;

    YakuScoreDataArray yakuScores = GetHuYakuScoreData(handData, winningCondition);
    PrintYakuScoreData(yakuScores);

    // Free allocated memory
    FreeTileScoreData(tenpaiScores.data);
    FreeYakuScoreData(yakuScores.data);

    return 0;
}
*/