// Assets/Scripts/Game/GameManager.cs
using System.Collections.Generic;
using MCRGame.Common; // AbsoluteSeat, Round, GameTile 등

namespace MCRGame.Game
{
    /// <summary>
    /// 게임 로직 전반을 담당합니다.
    /// 플레이어 목록, 내 자리, 현재 라운드/턴, 내 손패를 보관하고 초기화합니다.
    /// </summary>
    public class GameManager
    {
        public List<Player> Players { get; private set; }
        public AbsoluteSeat MySeat { get; private set; }
        public AbsoluteSeat CurrentTurnSeat { get; private set; }
        public Round CurrentRound { get; private set; }
        public GameHand Hand { get; private set; }


    }

}