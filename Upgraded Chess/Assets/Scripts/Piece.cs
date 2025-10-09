public class Piece
{
    public ColorOfPiece color;
    public PieceType type;
    public bool moved;
    public bool poweredUp;
    public bool egg;


    public Piece(ColorOfPiece color, PieceType type, bool moved=false, bool poweredUp=false, bool egg=false)
    {
        this.color = color;
        this.type = type;
        this.moved = moved;
        this.poweredUp = poweredUp;
        this.egg = egg;
    }

    public Piece(Piece piece)
    {
        color = piece.color;
        type = piece.type;
        moved = piece.moved;
        poweredUp = piece.poweredUp;
        egg = piece.egg;
    }

    public bool Empty()
    {
        return type == PieceType.None;
    }

    public bool CanBeMovedThrough()
    {
        return Empty() || type == PieceType.TemporaryQueen;
    }

    public float Value()
    {
        float currentValue;
        switch(type)
        {
            case PieceType.Pawn:
                currentValue = 1;
                break;
            case PieceType.Rook:
                currentValue = 5;
                break;
            case PieceType.Knight:
                currentValue = 3;
                break;
            case PieceType.Bishop:
                currentValue = 3;
                break;
            case PieceType.Queen:
                currentValue = 9;
                break;
            case PieceType.Miner:
                currentValue = 2;
                break;
            case PieceType.King:
                currentValue = 10;
                break;
            case PieceType.Dodo:
                currentValue = 9;
                break;
            default:
                currentValue = 0;
                break;
        }

        return poweredUp ? currentValue * 1.5f : currentValue;
    }

    public void PowerUp()
    {
        poweredUp = true;
    }

}
