public struct Move
{
    public int prevTile;
    public int currTile;
    public PieceType promotion;

    public Move(int prevTile, int currTile, PieceType promotion = PieceType.None)
    {
        this.prevTile = prevTile;
        this.currTile = currTile;
        this.promotion = promotion;
    }

    public Move(Move move)
    {
        this.prevTile = move.prevTile;
        this.currTile = move.currTile;
        this.promotion = move.promotion;
    }

    public readonly bool Contains(int index)
    {
        return prevTile == index || currTile == index;
    }
}
