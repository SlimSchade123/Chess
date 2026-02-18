namespace Chess.Common.Generators
{
    using System;
    using System.Linq;

    public static class Chess960Generator
    {
        public static string GenerateChess960FEN()
        {
            string pieces = "RNBQKBNR";
            string backRank;
            Random rng = new();

            do
            {
                backRank = new string(pieces.OrderBy(x => rng.Next()).ToArray());

                int bishop1 = backRank.IndexOf('B');
                int bishop2 = backRank.LastIndexOf('B');
                int king = backRank.IndexOf('K');
                int rook1 = backRank.IndexOf('R');
                int rook2 = backRank.LastIndexOf('R');

                bool bishopsOk = (bishop1 % 2) != (bishop2 % 2);
                bool kingOk = king > rook1 && king < rook2;

                if (bishopsOk && kingOk)
                {
                    break;
                }
            }
            while (true);

            return backRank.ToLower() + "/pppppppp/8/8/8/8/PPPPPPPP/" + backRank;
        }
    }
}
