public struct Move
{
    public int prevTile;
    public int currTile;

    public Move(int prevTile, int currTile)
    {
        this.prevTile = prevTile;
        this.currTile = currTile;
    }

    public Move(Move move)
    {
        this.prevTile = move.prevTile;
        this.currTile = move.currTile;
    }

    public readonly bool Contains(int index)
    {
        return prevTile == index || currTile == index;
    }
}
