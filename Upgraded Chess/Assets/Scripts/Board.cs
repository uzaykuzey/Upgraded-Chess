using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public Piece[] tiles;
    public float whitePowerUp;
    public float blackPowerUp;
    public ColorOfPiece turn;
    public Move lastMove;
    public bool kingInDanger;
    public GameController controller;
    public bool firstMovesMade;
    public float dodosByWhite;
    public float dodosByBlack;

    public MinerSituation[] minerSituations;

    public static Vector2[] knightMoves = new Vector2[]
    {
        new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2, 1), new Vector2(-2, -1),
        new Vector2(1, 2), new Vector2(1, -2), new Vector2(-1, 2), new Vector2(-1, -2)
    };



    public Piece this[int index]
    {
        get
        {
            return tiles[index];
        }
        set
        {
            tiles[index]=value;
        }
    }

    public Piece this[Vector2 vector]
    {
        get
        {
            return tiles[GetIndexFromVector(vector)];
        }
        set
        {
            tiles[GetIndexFromVector(vector)] = value;
        }
    }

    public Board()
    {
        tiles = new Piece[70];
        minerSituations = new MinerSituation[] { MinerSituation.NotUpgraded, MinerSituation.NotUpgraded };
    }

    public Board(Board board)
    {
        tiles = new Piece[board.tiles.Length];
        for(int i=0;i<tiles.Length;i++)
        {
            tiles[i] = new Piece(board[i]);
        }
        whitePowerUp = board.whitePowerUp;
        blackPowerUp = board.blackPowerUp;
        turn = board.turn;
        lastMove = new Move(board.lastMove);
        firstMovesMade = board.firstMovesMade;
        minerSituations = new MinerSituation[] { board.minerSituations[(int) ColorOfPiece.White], board.minerSituations[(int) ColorOfPiece.Black] };
        dodosByWhite = board.dodosByWhite;
        dodosByBlack = board.dodosByBlack;
        controller = null;
    }

    public Vector2 GetVectorFromIndex(int index)
    {
        return new Vector2(index % 8, index / 8);
    }

    public int GetIndexFromVector(Vector2 vector)
    {
        return Mathf.RoundToInt(vector.x) + Mathf.RoundToInt(vector.y) * 8;
    }

    public bool LegalVector(Vector2 vector2)
    {
        return vector2.x >= 0 && vector2.y >= 0 && vector2.x < 8 && vector2.y < 8;
    }

    public void MakeMove(Move move)
    {
        Piece capturedPiece;
        if (move.currTile==66+(int)turn)
        {
            capturedPiece = GameController.emptyPiece;
            if(turn==ColorOfPiece.White)
            {
                whitePowerUp -= tiles[move.prevTile].Value();
                tiles[move.prevTile].PowerUp();
            }
            else
            {
                blackPowerUp -= tiles[move.prevTile].Value();
                tiles[move.prevTile].PowerUp();
            }

            if (tiles[move.prevTile].type == PieceType.Miner)
            {
                minerSituations[(int)turn] = MinerSituation.UpgradedAlive;
            }
        }
        else if (move.prevTile == 68 + (int)turn)
        {
            float dodoIncrease = this[move.prevTile].poweredUp ? 1 : 0.6f;
            if (turn == ColorOfPiece.White)
            {
                whitePowerUp -= EggThresholdCalculator(turn);
                dodosByWhite += dodoIncrease;
            }
            else
            {
                blackPowerUp -= EggThresholdCalculator(turn);
                dodosByBlack += dodoIncrease;
            }
            tiles[move.currTile] = new Piece(turn, PieceType.Pawn, false, tiles[move.prevTile].poweredUp, true);
            capturedPiece = GameController.emptyPiece;
        }
        else
        {
            if (tiles[move.currTile].color == tiles[move.prevTile].color && tiles[move.currTile].type == PieceType.King && tiles[move.prevTile].type==(PieceType.Rook) && tiles[move.prevTile].poweredUp)
            {
                capturedPiece = GameController.emptyPiece;
                Piece temp = tiles[move.currTile];
                tiles[move.currTile] = new Piece(tiles[move.prevTile]);
                tiles[move.prevTile] = new Piece(temp);
                tiles[move.currTile].moved = true;
                tiles[move.prevTile].moved = true;
            }
            else
            {
                capturedPiece = tiles[move.currTile];
                tiles[move.currTile] = new Piece(tiles[move.prevTile]);
                tiles[move.currTile].moved = true;
                tiles[move.prevTile] = GameController.emptyPiece;

                //castle
                if (tiles[move.currTile].type == PieceType.King && Mathf.Abs(move.currTile - move.prevTile) == 2 && move.currTile / 8 == move.prevTile / 8)
                {
                    if (move.currTile % 8 == 2)
                    {
                        int rookSource = (move.currTile / 8) * 8;
                        tiles[move.currTile + 1] = new Piece(tiles[rookSource]);
                        tiles[move.currTile + 1].moved = true;
                        tiles[rookSource] = GameController.emptyPiece;
                    }
                    else
                    {
                        int rookSource = (move.currTile / 8) * 8 + 7;
                        tiles[move.currTile - 1] = new Piece(tiles[rookSource]);
                        tiles[move.currTile - 1].moved = true;
                        tiles[rookSource] = GameController.emptyPiece;
                    }
                }
            }



            //en passant
            if (!tiles[move.currTile].poweredUp && tiles[move.currTile].type == PieceType.Pawn && move.currTile % 8 != move.prevTile % 8 && capturedPiece.Empty())
            {
                capturedPiece = this[new Vector2(move.currTile % 8, move.prevTile / 8)];
                this[new Vector2(move.currTile % 8, move.prevTile / 8)] = GameController.emptyPiece;
            }
            if (tiles[move.currTile].poweredUp && tiles[move.currTile].type == PieceType.Pawn && move.currTile % 8 != move.prevTile % 8)
            {
                Piece enPassanted = this[new Vector2(move.currTile % 8, move.prevTile / 8)];
                if(enPassanted.color!= tiles[move.currTile].color)
                {
                    if (turn == ColorOfPiece.Black)
                    {
                        whitePowerUp += enPassanted.Value() * 0.75f;
                        blackPowerUp += enPassanted.Value() * 0.25f;
                    }
                    else
                    {
                        blackPowerUp += enPassanted.Value() * 0.75f;
                        whitePowerUp += enPassanted.Value() * 0.25f;
                    }
                    this[new Vector2(move.currTile % 8, move.prevTile / 8)] = GameController.emptyPiece;
                }
            }
        }
        
        for(int i=0;i<tiles.Length; i++)
        {
            Piece piece = tiles[i];
            if(piece.color==turn && piece.type==PieceType.TemporaryQueen)
            {
                tiles[i] = GameController.emptyPiece;
            }
        }

        if (minerSituations[(int)turn] == MinerSituation.UpgradedAlive)
        {
            bool minerAlive = false;
            for (int i = 0; i < tiles.Length; i++)
            {
                Piece piece = tiles[i];
                if (piece.color == turn && piece.type == PieceType.Miner)
                {
                    minerAlive = true;
                    break;
                }
            }
            if(!minerAlive)
            {
                minerSituations[(int)turn] = MinerSituation.UpgradedDead;
            }
        }
        else if (minerSituations[(int)turn] == MinerSituation.UpgradedDead)
        {
            minerSituations[(int)turn] = MinerSituation.UpgradedAlive;
            this[64 + (int)turn] = new Piece(turn, PieceType.Miner, false, true, false);
        }


        if (tiles[move.currTile].type==PieceType.Queen && tiles[move.currTile].poweredUp && move.prevTile<64)
        {
            Piece piece = new Piece(turn, PieceType.TemporaryQueen, true);
            tiles[move.prevTile]=piece;
        }


        turn = (1 - turn);
        lastMove = new Move(move);
        kingInDanger = IsKingInDangerCurrently(turn);
        if(controller!=null && !CanMakeAMove(turn))
        {
            if(kingInDanger)
            {
                controller.gamestate = turn == ColorOfPiece.White ? GameState.BlackWin: GameState.WhiteWin;
            }
            else
            {
                controller.gamestate = GameState.Draw;
            }
        }

        if (turn == ColorOfPiece.White)
        {
            whitePowerUp += capturedPiece.Value() * 0.75f;
            blackPowerUp += capturedPiece.Value() * 0.25f;
            if(capturedPiece.color==ColorOfPiece.Black)
            {
                whitePowerUp -= capturedPiece.Value() * 0.75f;
                blackPowerUp += capturedPiece.Value() * 0.75f;
            }
            blackPowerUp += 0.1f;
            if(kingInDanger)
            {
                blackPowerUp += 0.1f;
                whitePowerUp += 0.3f;
            }
            firstMovesMade = true;
        }
        else
        {
            blackPowerUp += capturedPiece.Value() * 0.75f;
            whitePowerUp += capturedPiece.Value() * 0.25f;
            if (capturedPiece.color == ColorOfPiece.White)
            {
                blackPowerUp -= capturedPiece.Value() * 0.75f;
                whitePowerUp += capturedPiece.Value() * 0.75f;
            }
            whitePowerUp += 0.1f;

            if (kingInDanger)
            {
                blackPowerUp += 0.3f;
                whitePowerUp += 0.1f;
            }
        }
    }

    public float AverageValue(Piece p1, Piece p2)
    {
        return (p1.Value()+ p2.Value()) / 2f;
    }

    public List<int> GetPossibleMoves(Piece piece, int index, bool eliminateMovesThatHangsTheKing=true, bool includeTakeableTilesToo=false)
    {
        List<int> result = new();
        Vector2 position=GetVectorFromIndex(index);
        Vector2 positionForTesting;
        Vector2 moveDirection;
        switch (piece.type)
        {
            case PieceType.Pawn:
                moveDirection = new Vector2(0, piece.color == ColorOfPiece.White ? -1: 1);
                positionForTesting = position + moveDirection;
                if (LegalVector(positionForTesting) && this[positionForTesting].CanBeMovedThrough())
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                    positionForTesting = position + moveDirection * 2;
                    if (LegalVector(positionForTesting) && position.y == (piece.color == ColorOfPiece.White ? 6 : 1) && this[positionForTesting].CanBeMovedThrough())
                    {
                        result.Add(GetIndexFromVector(positionForTesting));
                    }
                }
                positionForTesting = position + moveDirection + new Vector2(1, 0);
                if (LegalVector(positionForTesting) && !this[positionForTesting].Empty() && this[positionForTesting].color != piece.color)
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                }
                if(!piece.poweredUp && LegalVector(positionForTesting) && this[positionForTesting].Empty() && this[position+new Vector2(1,0)].type==PieceType.Pawn && lastMove.currTile==GetIndexFromVector(position + new Vector2(1, 0)) && position.y == (piece.color==ColorOfPiece.White ? 3: 4))
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                    if (includeTakeableTilesToo)
                    {
                        result.Add(GetIndexFromVector(position + new Vector2(1, 0)));
                    }
                }
                if(piece.poweredUp && LegalVector(positionForTesting) && lastMove.currTile == GetIndexFromVector(position + new Vector2(1, 0)) && tiles[lastMove.currTile].color!=piece.color && (this[positionForTesting].CanBeMovedThrough() || this[positionForTesting].color != piece.color))
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                    if(includeTakeableTilesToo)
                    {
                        result.Add(GetIndexFromVector(position + new Vector2(1, 0)));
                    }
                }
                positionForTesting = position + moveDirection + new Vector2(-1, 0);
                if (LegalVector(positionForTesting) && !this[positionForTesting].Empty() && this[positionForTesting].color != piece.color)
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                }
                if (!piece.poweredUp && LegalVector(positionForTesting) && this[positionForTesting].Empty() && this[position + new Vector2(-1, 0)].type == PieceType.Pawn && lastMove.currTile == GetIndexFromVector(position + new Vector2(-1, 0)) && position.y == (piece.color == ColorOfPiece.White ? 3 : 4))
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                    if (includeTakeableTilesToo)
                    {
                        result.Add(GetIndexFromVector(position + new Vector2(-1, 0)));
                    }
                }
                if (piece.poweredUp && LegalVector(positionForTesting) && lastMove.currTile == GetIndexFromVector(position + new Vector2(-1, 0)) && tiles[lastMove.currTile].color != piece.color && (this[positionForTesting].CanBeMovedThrough() || this[positionForTesting].color != piece.color))
                {
                    result.Add(GetIndexFromVector(positionForTesting));
                    if (includeTakeableTilesToo)
                    {
                        result.Add(GetIndexFromVector(position + new Vector2(-1, 0)));
                    }
                }
                break;
            case PieceType.Rook:
                for(int i=0;i<4;i++)
                {
                    moveDirection = new Vector2((1-i/2) * (i%2==0 ? 1: -1), i/2 * (i % 2 == 0 ? 1 : -1));
                    result.AddRange(GetInfiniteMovementFromVector(moveDirection, position, piece.color));
                }
                if (piece.poweredUp)
                {
                    result.Add(IndexOfKing(piece.color));
                }
                break;

            case PieceType.Knight:
                foreach(Vector2 knightMovement in knightMoves)
                {
                    positionForTesting = position + knightMovement;
                    if(LegalVector(positionForTesting) && (this[positionForTesting].CanBeMovedThrough() || this[positionForTesting].color!=piece.color))
                    {
                        result.Add(GetIndexFromVector(positionForTesting));
                        if(piece.poweredUp && this[positionForTesting].CanBeMovedThrough())
                        {
                            result.AddRange(GetPossibleMoves(new(piece){poweredUp = false}, GetIndexFromVector(positionForTesting), false));
                        }
                    }
                }
                break;

            case PieceType.Bishop:
                for (int i = 0; i < 4; i++)
                {
                    moveDirection = new Vector2((i/2)%2==0 ? 1: -1,i%2==0 ? 1: -1);
                    result.AddRange(GetInfiniteMovementFromVector(moveDirection, position, piece.color, piece.poweredUp));
                }
                break;

            case PieceType.Queen:
                for (int i = 0; i < 4; i++)//Bishop
                {
                    moveDirection = new Vector2((i / 2) % 2 == 0 ? 1 : -1, i % 2 == 0 ? 1 : -1);
                    result.AddRange(GetInfiniteMovementFromVector(moveDirection, position, piece.color));
                }
                for (int i = 0; i < 4; i++) //Rook
                {
                    moveDirection = new Vector2((1 - i / 2) * (i % 2 == 0 ? 1 : -1), i / 2 * (i % 2 == 0 ? 1 : -1));
                    result.AddRange(GetInfiniteMovementFromVector(moveDirection, position, piece.color));
                }
                break;

            case PieceType.King:
                for (int i = 0; i < 4; i++) //Diagonal
                {
                    moveDirection = new Vector2((i / 2) % 2 == 0 ? 1 : -1, i % 2 == 0 ? 1 : -1);
                    positionForTesting = moveDirection + position;
                    if(LegalVector(positionForTesting) && (this[positionForTesting].CanBeMovedThrough() || this[positionForTesting].color != piece.color))
                    {
                        result.Add(GetIndexFromVector(positionForTesting));
                    }
                }
                for (int i = 0; i < 4; i++) //Direct
                {
                    moveDirection = new Vector2((1 - i / 2) * (i % 2 == 0 ? 1 : -1), i / 2 * (i % 2 == 0 ? 1 : -1));
                    positionForTesting = moveDirection + position;
                    if (LegalVector(positionForTesting) && (this[positionForTesting].CanBeMovedThrough() || this[positionForTesting].color != piece.color))
                    {
                        result.Add(GetIndexFromVector(positionForTesting));
                    }
                }
                if(piece.poweredUp)
                {
                    result.AddRange(GetPossibleMoves(new Piece(piece.color, PieceType.Knight, true, false), index, false));
                }
                if(!piece.moved && !kingInDanger) //Castle
                {
                    if (!this[new Vector2(7 ,position.y)].moved && this[new Vector2(5, position.y)].Empty() && this[new Vector2(6, position.y)].Empty())
                    {
                        result.Add(GetIndexFromVector(new Vector2(6, position.y)));
                    }

                    if (!this[new Vector2(0, position.y)].moved && this[new Vector2(1, position.y)].Empty() && this[new Vector2(2, position.y)].Empty() && this[new Vector2(3, position.y)].Empty())
                    {
                        result.Add(GetIndexFromVector(new Vector2(2, position.y)));
                    }
                }
                int pawnIndex = piece.color == ColorOfPiece.White ? 52 : 12;
                if(!firstMovesMade)
                {
                    result.Add(pawnIndex);
                }
                break;

            case PieceType.Miner:
                if(index == 64 + (int) piece.color)
                {
                    int opponentKingPosition = IndexOfKing(1 - piece.color);
                    for (int i=0;i<64;i++)
                    {
                        if (this[i].CanBeMovedThrough())
                        {
                            if(GetPossibleMoves(piece, i, false).Contains(opponentKingPosition))
                            {
                                continue;
                            }
                            result.Add(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        moveDirection = new Vector2((1 - i / 2) * (i % 2 == 0 ? 1 : -1), i / 2 * (i % 2 == 0 ? 1 : -1));
                        positionForTesting = moveDirection * 2 + position;
                        if ((LegalVector(positionForTesting) && (this[positionForTesting].Empty() || this[positionForTesting].color != piece.color)))
                        {
                            result.Add(GetIndexFromVector(positionForTesting));
                        }
                    }
                    if(piece.poweredUp)
                    {
                        result.Add(64 + (int)piece.color);
                    }
                }

                break;
            case PieceType.TemporaryQueen:
                result.AddRange(GetPossibleMoves(new Piece(piece.color, PieceType.Queen, true, false), index, false));
                break;

            case PieceType.Dodo:
                if(turn==ColorOfPiece.White)
                {
                    if(whitePowerUp < EggThresholdCalculator(piece.color))
                    {
                        break;
                    }
                }
                else
                {
                    if (blackPowerUp < EggThresholdCalculator(piece.color))
                    {
                        break;
                    }
                }
                int offset = GetIndexFromVector(new(0, turn == ColorOfPiece.White ? 6 : 1));
                for (int i=0; i<8 ; i++)
                {
                    int currentPosition=i+offset;
                    if (!this[currentPosition].CanBeMovedThrough())
                    {
                        continue;
                    }
                    if(!GetPossibleMoves(new(piece.color, PieceType.Pawn), currentPosition, false).Contains(IndexOfKing(1-piece.color)))
                    {
                        result.Add(currentPosition);
                    }
                }
                break;
        }

        
        if(turn==ColorOfPiece.White)
        {
            if(whitePowerUp>=piece.Value() && piece.type != PieceType.TemporaryQueen && !piece.poweredUp)
            {
                result.Add(66);
            }
        }
        else
        {
            if (blackPowerUp >= piece.Value() && piece.type != PieceType.TemporaryQueen && !piece.poweredUp)
            {
                result.Add(67);
            }
        }


        if((piece.color==ColorOfPiece.White && whitePowerUp < piece.Value()) || (piece.color == ColorOfPiece.Black && blackPowerUp < piece.Value()) || piece.poweredUp)
        {
            while(result.Contains(66))
            {
                result.Remove(66);
            }
            while (result.Contains(67))
            {
                result.Remove(67);
            }
        }

        List<int> realResult;

        if (eliminateMovesThatHangsTheKing)
        {
            realResult = new List<int>();

            foreach (int i in result)
            {
                if (!DoesHangKing(new Move(index, i)))
                {
                    realResult.Add(i);
                }
            }
        }
        else
        {
            realResult = result;
        }
        


        return realResult;
    }

    public float CalculateAllThePieceValues(ColorOfPiece color)
    {
        float result = 0;
        foreach(Piece piece in tiles)
        {
            if(!piece.Empty() && piece.color==color)
            {
                result += piece.Value();
            }
        }
        return result;
    }

    public bool DoesHangKing(Move move)
    {
        Board hypoBoard = new Board(this);
        hypoBoard.MakeMove(move);

        return hypoBoard.IsKingInDangerCurrently(turn);
    }

    public int IndexOfKing(ColorOfPiece kingColor)
    {
        for(int i=0;i<tiles.Length;i++)
        {
            if (tiles[i].type == PieceType.King && tiles[i].color == kingColor)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsKingInDangerCurrently(ColorOfPiece KingColor)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            Piece piece = tiles[i];
            if (piece.Empty() || piece.color == KingColor)
            {
                continue;
            }
            List<int> currentMoves = GetPossibleMoves(piece, i, false, true);
            foreach (int j in currentMoves)
            {
                if (!this[j].Empty() && this[j].type == PieceType.King && this[j].color == KingColor)
                {
                    return true;
                }
            }
        }
        return false;
    }

    
    public List<int> GetInfiniteMovementFromVector(Vector2 direction, Vector2 position, ColorOfPiece color, bool poweredUpBishop=false)
    {
        List<int> result = new();
        Vector2 currentPosition = new Vector2(position.x, position.y) + direction;
        while (LegalVector(currentPosition) && this[currentPosition].CanBeMovedThrough())
        {
            result.Add(GetIndexFromVector(currentPosition));
            currentPosition += direction;
        }

        if (poweredUpBishop && !LegalVector(currentPosition))
        {
            if (currentPosition.x >= 8)
            {
                result.AddRange(GetInfiniteMovementFromVector(direction, currentPosition - new Vector2(8, 0) - direction, color));
            }
            else if (currentPosition.x < 0)
            {
                result.AddRange(GetInfiniteMovementFromVector(direction, currentPosition + new Vector2(8, 0) - direction, color));
            }
            else if (currentPosition.y >= 8)
            {
                result.AddRange(GetInfiniteMovementFromVector(direction, currentPosition - new Vector2(0, 8) - direction, color));
            }
            else if (currentPosition.y < 0)
            {
                result.AddRange(GetInfiniteMovementFromVector(direction, currentPosition + new Vector2(0, 8) - direction, color));
            }
        }

        if (LegalVector(currentPosition) && this[currentPosition].color!=color)
        {
            result.Add(GetIndexFromVector(currentPosition));
        }
        return result;
    }

    public float EggThresholdCalculator(ColorOfPiece color)
    {
        float dodoCount = color==ColorOfPiece.White ? dodosByWhite: dodosByBlack;

        if(dodoCount <= 4)
        {
            return 1;
        }
        if(dodoCount <= 10)
        {
            return 3;
        }
        return 7;

        /*float threshold = CalculateAllThePieceValues(color) - tiles[IndexOfKing(color)].Value() - tiles[68 + (int) color].Value();
        return 7.5f / (1f + 6.5f * Mathf.Exp(-(threshold-1f) / 3f)) - 0.1f;*/
    }

    public bool CanMakeAMove(ColorOfPiece turn)
    {
        for(int i=0;i<tiles.Length;i++)
        {
            Piece piece = tiles[i];
            if(piece.Empty() || piece.color!=turn)
            {
                continue;
            }
            if(GetPossibleMoves(piece, i).Count>0)
            {
                return true;
            }
        }
        return false;
    }
}