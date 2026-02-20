namespace Chess.Model.Rule
{
    using Chess.Model.Command;
    using Chess.Model.Data;
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Visitor;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    // Chess960 rulebook - identical to standard chess except for the starting position.
    public class Chess960Rulebook : IRulebook
    {
        private readonly CheckRule checkRule;
        private readonly EndRule endRule;
        private readonly MovementRule movementRule;
        private readonly int? fixedSpId;
        private readonly Random random;

        // Random starting position chosen each game.
        public Chess960Rulebook() : this(null, new Random()) { }

        // Fixed starting position by SP-ID. SP-ID 518 is the standard chess layout.
        public Chess960Rulebook(int spId) : this((int?)spId, new Random())
        {
            Validation.InRange(spId, 0, 959, nameof(spId));
        }

        // Used internally and in tests to allow a seeded random for reproducible results.
        public Chess960Rulebook(int? spId, Random rng)
        {
            this.fixedSpId = spId;
            this.random = rng;

            var threatAnalyzer = new ThreatAnalyzer();
            var castlingRule = new CastlingRule(threatAnalyzer);
            var enPassantRule = new EnPassantRule();
            var promotionRule = new PromotionRule();

            this.checkRule = new CheckRule(threatAnalyzer);
            this.movementRule = new MovementRule(castlingRule, enPassantRule, promotionRule, threatAnalyzer);
            this.endRule = new EndRule(this.checkRule, this.movementRule);
        }

        // Creates a game with a Chess960 back rank. Pawns are always in their standard rows.
        public ChessGame CreateGame()
        {
            var spId = this.fixedSpId ?? this.random.Next(0, 960);
            var backRank = Chess960BoardSetup.GetBackRankArrangement(spId);

            IEnumerable<PlacedPiece> makeBackRank(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    col => new PlacedPiece(new Position(row, col), MakePiece(backRank[col], color))
                );

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color)
            {
                var pieces = makeBackRank(baseRow, color).Union(makePawns(pawnRow, color));
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White);
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }

        public Status GetStatus(ChessGame game)
        {
            return this.endRule.GetStatus(game);
        }

        public IEnumerable<Update> GetUpdates(ChessGame game, Position position)
        {
            var piece = game.Board.GetPiece(position, game.ActivePlayer.Color);
            var updates = piece.Map(
                p =>
                {
                    var moves = this.movementRule.GetCommands(game, p);
                    var turnEnds = moves.Select(c => new SequenceCommand(c, EndTurnCommand.Instance));
                    var records = turnEnds.Select
                    (
                        c => new SequenceCommand(c, new SetLastUpdateCommand(new Update(game, c)))
                    );
                    var futures = records.Select(c => c.Execute(game).Map(g => new Update(g, c)));
                    return futures.FilterMaybes().Where
                    (
                        e => !this.checkRule.Check(e.Game, e.Game.PassivePlayer)
                    );
                }
            );

            return updates.GetOrElse(Enumerable.Empty<Update>());
        }

        private static ChessPiece MakePiece(string name, Color color)
        {
            switch (name)
            {
                case "King": return new King(color);
                case "Queen": return new Queen(color);
                case "Rook": return new Rook(color);
                case "Bishop": return new Bishop(color);
                case "Knight": return new Knight(color);
                default: throw new ArgumentException($"Unexpected piece name: {name}");
            }
        }
    }
}