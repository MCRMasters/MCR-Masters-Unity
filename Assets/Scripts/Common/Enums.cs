

using System;
using System.Collections.Generic;

namespace MCRGame.Common
{
    public enum Round
    {
        E1 = 0,
        E2 = 1,
        E3 = 2,
        E4 = 3,
        S1 = 4,
        S2 = 5,
        S3 = 6,
        S4 = 7,
        W1 = 8,
        W2 = 9,
        W3 = 10,
        W4 = 11,
        N1 = 12,
        N2 = 13,
        N3 = 14,
        N4 = 15,
        END = 16
    }

    public static class RoundExtensions
    {
        public static int Number(this Round round)
        {
            if (round == Round.END)
                throw new System.InvalidOperationException("game finished");
            string name = round.ToString();
            return int.Parse(name.Substring(1));
        }

        public static string Wind(this Round round)
        {
            if (round == Round.END)
                throw new System.InvalidOperationException("game finished");
            string name = round.ToString();
            return name.Substring(0, 1);
        }

        public static Round NextRound(this Round round)
        {
            if (round == Round.END)
                throw new System.InvalidOperationException("game finished");
            return (Round)((int)round + 1);
        }
    }

    public enum AbsoluteSeat
    {
        EAST = 0,
        SOUTH = 1,
        WEST = 2,
        NORTH = 3
    }

    public static class AbsoluteSeatExtensions
    {
        public static AbsoluteSeat NextSeat(this AbsoluteSeat seat)
        {
            return (AbsoluteSeat)(((int)seat + 1) % 4);
        }

        public static AbsoluteSeat NextSeatAfterAction(this AbsoluteSeat seat, Action action)
        {
            return (AbsoluteSeat)(((int)seat + (int)action.SeatPriority) % 4);
        }
    }

    public enum RelativeSeat
    {
        SELF = 0,
        SHIMO = 1,
        TOI = 2,
        KAMI = 3
    }

    public static class RelativeSeatExtensions
    {
        public static RelativeSeat CreateFromAbsoluteSeats(int currentSeat, int targetSeat)
        {
            return (RelativeSeat)(((targetSeat - currentSeat + 4) % 4));
        }
    }

    public enum GameTile
    {
        // Manzu tiles (M)
        M1 = 0,
        M2 = 1,
        M3 = 2,
        M4 = 3,
        M5 = 4,
        M6 = 5,
        M7 = 6,
        M8 = 7,
        M9 = 8,
        // Pinzu tiles (P)
        P1 = 9,
        P2 = 10,
        P3 = 11,
        P4 = 12,
        P5 = 13,
        P6 = 14,
        P7 = 15,
        P8 = 16,
        P9 = 17,
        // Souzu tiles (S)
        S1 = 18,
        S2 = 19,
        S3 = 20,
        S4 = 21,
        S5 = 22,
        S6 = 23,
        S7 = 24,
        S8 = 25,
        S9 = 26,
        // Honor tiles (Z)
        Z1 = 27,
        Z2 = 28,
        Z3 = 29,
        Z4 = 30,
        Z5 = 31,
        Z6 = 32,
        Z7 = 33,
        // Flower tiles (F)
        F0 = 34,
        F1 = 35,
        F2 = 36,
        F3 = 37,
        F4 = 38,
        F5 = 39,
        F6 = 40,
        F7 = 41
    }

    public static class GameTileExtensions
    {
        public static bool IsHonor(this GameTile tile)
        {
            return (int)tile >= 27 && (int)tile <= 33;
        }

        public static bool IsNumber(this GameTile tile)
        {
            return (int)tile >= 0 && (int)tile <= 26;
        }

        public static bool IsFlower(this GameTile tile)
        {
            return (int)tile >= 34 && (int)tile <= 41;
        }

        public static int Number(this GameTile tile)
        {
            return tile.IsNumber() ? ((int)tile % 9) + 1 : 0;
        }

        public static string Type(this GameTile tile)
        {
            return tile.ToString().Substring(0, 1);
        }

        public static IEnumerable<GameTile> AllTiles()
        {
            for (int i = (int)GameTile.M1; i <= (int)GameTile.F7; i++)
                yield return (GameTile)i;
        }

        public static IEnumerable<GameTile> NormalTiles()
        {
            for (int i = (int)GameTile.M1; i < (int)GameTile.F0; i++)
                yield return (GameTile)i;
        }

        public static IEnumerable<GameTile> FlowerTiles()
        {
            for (int i = (int)GameTile.F0; i <= (int)GameTile.F7; i++)
                yield return (GameTile)i;
        }

        public static string ToCustomString(this GameTile tile)
        {
            string original = tile.ToString();
            char[] chars = original.ToCharArray();
            Array.Reverse(chars);
            return new string(chars).ToLower();
        }
    }
}
