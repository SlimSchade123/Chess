

using Chess.Common.Generators;
using System.Text.RegularExpressions;

namespace Chess960Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Chess960RanksMatchOnEitherSide()
        {
            var chess960FEN = Chess960Generator.GenerateChess960FEN();
            var ranks = chess960FEN.Split('/');

            var whiteRank = ranks[0];
            var blackRank = ranks[7];

            Assert.That(whiteRank, Is.EqualTo(blackRank.ToLower()));
        }

        [Test]
        public void Chess960BishopsAreOnOppositeColors()
        {
            var chess960FEN = Chess960Generator.GenerateChess960FEN();
            var ranks = chess960FEN.Split('/');
            var blackRank = ranks[0];

            var bishop1Position = blackRank.IndexOf('b');
            var bishop2Position = blackRank.LastIndexOf('b');

            var bishop1Color = bishop1Position % 2 == 0 ? "light" : "dark";
            var bishop2Color = bishop2Position % 2 == 0 ? "light" : "dark";

            Assert.That(bishop1Color, Is.Not.EqualTo(bishop2Color));  
        }

        [Test]
        public void Chess960RooksAreOnEiterSideOfKing()
        {
            var chess960FEN = Chess960Generator.GenerateChess960FEN();
            var ranks = chess960FEN.Split('/');
            var blackRank = ranks[0];

            var kingPosition = blackRank.IndexOf('k');
            var rook1Position = blackRank.IndexOf('r');
            var rook2Position = blackRank.LastIndexOf('r');

            var rook1OnLeftOfKing = rook1Position < kingPosition;
            var rook2OnRightOfKing = rook2Position > kingPosition;

            var actual = true;
            var expected = rook1OnLeftOfKing && rook2OnRightOfKing;

            Assert.That(actual, Is.EqualTo(expected));


        }

        [Test]
        public void Chess960StartIsNotStandard()
        {
            var chess960FEN = Chess960Generator.GenerateChess960FEN();
            var standardFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
            Assert.That(chess960FEN, Is.Not.EqualTo(standardFEN));
        }


        [Test]
        public void Chess960GeneratesValidFEN()
        {
            
            var chess960FEN = Chess960Generator.GenerateChess960FEN();
            var ranks = chess960FEN.Split('/');
            var blackRank = ranks[0];
            var backRankPieces = "rnbqkbnr";

            bool validBackRank = false;
            

            // Check if all pieces in the black rank are valid and appear the correct number of times
            foreach (char piece in blackRank)
            {
                if (!backRankPieces.Contains(piece))
                {
                    validBackRank = false;
                    break;
                }
                validBackRank = true;
                var regex = new Regex(Regex.Escape(piece.ToString()), RegexOptions.IgnoreCase);
                backRankPieces = regex.Replace(backRankPieces, piece.ToString(), 1);
            }


            bool has8Ranks = ranks.Length == 8;
            bool finalVerdict = validBackRank && has8Ranks; 

            // All we need to check is if there is 8 ranks and if 1 of the ranks have the correct amount of pieces in the back rank.
            Assert.That(finalVerdict, Is.True);
        }

        [Test]
        public void Chess960MultipleIterationsAreNotIdentical()
        {
            var chess960Boards = new HashSet<string>(); //Hashmaps do not allow dupes so its a good way to tell unique board states
            for (int i = 0; i < 100; i++)
            {
                chess960Boards.Add(Chess960Generator.GenerateChess960FEN());
            }

            //Using Probability, we can expect to have around 90 unique boards after 100 iterations. This is because there are 960 possible board configurations, and the probability of generating a duplicate increases as we generate more boards. After 100 iterations, we can expect to have around 90 unique boards.
            // Stack Overflow discussion on this formula: https://math.stackexchange.com/questions/5775/how-many-bins-do-random-numbers-fill
            Assert.That(chess960Boards.Count, Is.GreaterThanOrEqualTo(90));
        }
        [Test]
        public void Chess960MultipleIterationsAreNotStandard()
        {
            var standardFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
            var chess960Boards = new HashSet<string>();

            for (int i = 0; i < 100; i++)
            {
                chess960Boards.Add(Chess960Generator.GenerateChess960FEN());
                
            }

            bool noneAreStandard = !chess960Boards.Contains(standardFEN);
            Assert.That(noneAreStandard, Is.True);

        }
    }
}