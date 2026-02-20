namespace Chess.Tests
{
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Rule;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class Chess960Tests
    {
        // Helper: get all pieces on a row sorted left to right
        private static List<PlacedPiece> PiecesOnRow(Board board, int row)
            => board.Where(p => p.Position.Row == row)
                    .OrderBy(p => p.Position.Column)
                    .ToList();

        // --- Traditional setup tests ---

        [Test]
        public void Standard_WhiteBackRank_IsCorrectOrder()
        {
            var game = new StandardRulebook().CreateGame();
            var row0 = PiecesOnRow(game.Board, 0);

            Assert.That(row0.Count, Is.EqualTo(8));
            Assert.That(row0[0].Piece, Is.InstanceOf<Rook>());
            Assert.That(row0[1].Piece, Is.InstanceOf<Knight>());
            Assert.That(row0[2].Piece, Is.InstanceOf<Bishop>());
            Assert.That(row0[3].Piece, Is.InstanceOf<Queen>());
            Assert.That(row0[4].Piece, Is.InstanceOf<King>());
            Assert.That(row0[5].Piece, Is.InstanceOf<Bishop>());
            Assert.That(row0[6].Piece, Is.InstanceOf<Knight>());
            Assert.That(row0[7].Piece, Is.InstanceOf<Rook>());
        }

        [Test]
        public void Standard_BothSidesHave16Pieces()
        {
            var game = new StandardRulebook().CreateGame();

            Assert.That(game.Board.GetPieces(Color.White).Count(), Is.EqualTo(16));
            Assert.That(game.Board.GetPieces(Color.Black).Count(), Is.EqualTo(16));
        }

        [Test]
        public void Standard_PawnRowsAreCorrect()
        {
            var game = new StandardRulebook().CreateGame();

            var whitePawns = PiecesOnRow(game.Board, 1);
            var blackPawns = PiecesOnRow(game.Board, 6);

            Assert.That(whitePawns.Count, Is.EqualTo(8));
            Assert.That(blackPawns.Count, Is.EqualTo(8));
            Assert.That(whitePawns.All(p => p.Piece is Pawn && p.Piece.Color == Color.White), Is.True);
            Assert.That(blackPawns.All(p => p.Piece is Pawn && p.Piece.Color == Color.Black), Is.True);
        }

        // --- Chess960 rule tests ---

        [Test]
        public void Chess960_BishopsOnOppositeColors_AllPositions()
        {
            for (int spId = 0; spId < 960; spId++)
            {
                var arr = Chess960BoardSetup.GetBackRankArrangement(spId);
                var bishopCols = arr.Select((n, c) => (n, c)).Where(t => t.n == "Bishop").Select(t => t.c).ToArray();

                Assert.That(bishopCols.Length, Is.EqualTo(2), $"SP-ID {spId}: expected 2 bishops");
                Assert.That(bishopCols.Any(c => c % 2 == 0) && bishopCols.Any(c => c % 2 != 0), Is.True,
                    $"SP-ID {spId}: bishops must be on opposite colored squares");
            }
        }

        [Test]
        public void Chess960_KingBetweenRooks_AllPositions()
        {
            for (int spId = 0; spId < 960; spId++)
            {
                var arr = Chess960BoardSetup.GetBackRankArrangement(spId);
                int kingCol = Array.IndexOf(arr, "King");
                var rookCols = arr.Select((n, c) => (n, c)).Where(t => t.n == "Rook").Select(t => t.c).ToArray();

                int left = Math.Min(rookCols[0], rookCols[1]);
                int right = Math.Max(rookCols[0], rookCols[1]);

                Assert.That(kingCol > left && kingCol < right, Is.True,
                    $"SP-ID {spId}: king must be between rooks");
            }
        }

        [Test]
        public void Chess960_BackRankHasCorrectPieces()
        {
            foreach (int spId in new[] { 0, 100, 333, 518, 750, 959 })
            {
                var arr = Chess960BoardSetup.GetBackRankArrangement(spId);

                Assert.That(arr.Count(p => p == "King"), Is.EqualTo(1), $"SP-ID {spId}");
                Assert.That(arr.Count(p => p == "Queen"), Is.EqualTo(1), $"SP-ID {spId}");
                Assert.That(arr.Count(p => p == "Rook"), Is.EqualTo(2), $"SP-ID {spId}");
                Assert.That(arr.Count(p => p == "Bishop"), Is.EqualTo(2), $"SP-ID {spId}");
                Assert.That(arr.Count(p => p == "Knight"), Is.EqualTo(2), $"SP-ID {spId}");
            }
        }

        [Test]
        public void Chess960_SpId518_MatchesTraditionalSetup()
        {
            var c960 = new Chess960Rulebook(518).CreateGame();
            var trad = new StandardRulebook().CreateGame();

            var c960Types = PiecesOnRow(c960.Board, 0).Select(p => p.Piece.GetType()).ToArray();
            var tradTypes = PiecesOnRow(trad.Board, 0).Select(p => p.Piece.GetType()).ToArray();

            Assert.That(c960Types, Is.EqualTo(tradTypes));
        }

        [Test]
        public void Chess960_GameHasCorrectPieceCounts()
        {
            var game = new Chess960Rulebook(null, new Random(42)).CreateGame();

            Assert.That(game.Board.GetPieces(Color.White).Count(), Is.EqualTo(16));
            Assert.That(game.Board.GetPieces(Color.Black).Count(), Is.EqualTo(16));
            Assert.That(PiecesOnRow(game.Board, 1).All(p => p.Piece is Pawn), Is.True);
            Assert.That(PiecesOnRow(game.Board, 6).All(p => p.Piece is Pawn), Is.True);
        }

        // --- Determinism and validation tests ---

        [Test]
        public void Chess960_SameSpIdGivesSameResult()
        {
            var first = Chess960BoardSetup.GetBackRankArrangement(77);
            var second = Chess960BoardSetup.GetBackRankArrangement(77);

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void Chess960_InvalidSpIdThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Chess960BoardSetup.GetBackRankArrangement(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Chess960BoardSetup.GetBackRankArrangement(960));
        }
    }
}