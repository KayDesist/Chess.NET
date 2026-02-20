namespace Chess.Model.Rule
{
    using Chess.Model.Data;

    // Converts a SP-ID(starting point) into a valid Chess960 back rank arrangement.
    public static class Chess960BoardSetup
    {
        // Returns an 8-element array of piece names for the given SP-ID.
        public static string[] GetBackRankArrangement(int spId)
        {
            Validation.InRange(spId, 0, 959, nameof(spId));
            return Decode(spId);
        }

        private static string[] Decode(int spId)
        {
            var rank = new string[8];

            //light-squared bishop on an odd column (1, 3, 5, or 7)
            rank[(spId % 4) * 2 + 1] = "Bishop";
            spId /= 4;

            //dark-squared bishop on an even column (0, 2, 4, or 6)
            rank[(spId % 4) * 2] = "Bishop";
            spId /= 4;

            //queen in the nth empty square
            PlaceInEmptySquare(rank, spId % 6, "Queen");
            spId /= 6;

            //knights using a lookup for all 10 ways to choose 2 from 5 remaining squares
            int[] first = { 0, 0, 0, 0, 1, 1, 1, 2, 2, 3 };
            int[] second = { 1, 2, 3, 4, 2, 3, 4, 3, 4, 4 };
            PlaceInEmptySquare(rank, first[spId], "Knight");
            PlaceInEmptySquare(rank, second[spId] - 1, "Knight");

            //last 3 squares are always Rook-King-Rook so the king sits between both rooks
            var remaining = new[] { "Rook", "King", "Rook" };
            var i = 0;
            for (int col = 0; col < 8; col++)
            {
                if (rank[col] == null)
                {
                    rank[col] = remaining[i++];
                }
            }

            return rank;
        }

        // Places a piece in the nth empty square of the rank, counting from the left.
        private static void PlaceInEmptySquare(string[] rank, int nth, string piece)
        {
            int count = 0;
            for (int col = 0; col < rank.Length; col++)
            {
                if (rank[col] == null)
                {
                    if (count == nth)
                    {
                        rank[col] = piece;
                        return;
                    }

                    count++;
                }
            }
        }
    }
}