using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PieceType
{
    None,
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King,
    Miner,
    TemporaryQueen,
    Dodo
}

public enum ColorOfPiece
{
    White,
    Black
}

public enum GameState
{
    Playing,
    WhiteWin,
    BlackWin,
    Draw,
    Promoting
}

public enum PromotingTo
{
    Miner=1005,
    Knight = 1001,
    Bishop = 1002,
    Rook = 1000,
    Queen = 1003
}



public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject baseTile;
    [SerializeField] private Sprite[] whiteSprites;
    [SerializeField] private Sprite[] blackSprites;
    [SerializeField] private Sprite[] eggs;
    [SerializeField] private SpriteRenderer blackBox;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI[] powerUpTexts;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject helpCanvas;
    [SerializeField] private Button helpButton;

    public static Piece emptyPiece = new(ColorOfPiece.White, PieceType.None, true);
    public static Color whiteTile = new(238 / 255f, 238 / 255f, 210 / 255f);
    public static Color blackTile = new(118 / 255f, 150 / 255f, 86 / 255f);
    public static Color whitePowerUp = new(1, 0.6666666f, 0.9353692f);
    public static Color blackPowerUp = new(0.4622642f, 0.1337368f, 0.4004237f);
    public static Color whiteTileNewlyMoved = new(230 / 255f, 223 / 255f, 117 / 255f);
    public static Color blackTileNewlyMoved = new(164 / 255f, 171 / 255f, 55 / 255f);

    private List<int> possibleMoves=null;
    private int possibleMovesSource=-1;
    public GameState gamestate=GameState.Playing;
    public float scale;
    private int promotionPlace;
    private List<Transform> allTransforms;
    private bool quitting;
    private float timeFromLastPressingR;



    public ColorOfPiece Turn
    { 
        get 
        {
            return board.turn;    
        } 
        set
        {
            board.turn = (ColorOfPiece)(Math.Abs((int)value)%2);
        }
    }

    public Board board;

    void Start()
    {
        SetupBoard();
        mainCanvas.SetActive(true);
        helpCanvas.SetActive(false);
        helpButton.onClick.AddListener(() =>
        {
            mainCanvas.SetActive(false);
            helpCanvas.SetActive(true);
        });
    }

    public Sprite GetSprite(int index)
    {
        if (index >= 1000)
        {
            return Turn == ColorOfPiece.White ? whiteSprites[index - 998] : blackSprites[index - 998];
        }
        return GetSprite(board[index]);
    }

    public Sprite GetSprite(Piece piece)
    {
        if(piece.egg)
        {
            return eggs[piece.poweredUp ? 0: (int) piece.color];
        }
        return piece.color == 0 || piece.poweredUp ? whiteSprites[(int) piece.type] : blackSprites[(int) piece.type];
    }

    public Color GetSpriteColor(int index)
    {
        if (index >= 1000)
        {
            return Color.white;
        }
        return GetSpriteColor(board[index]);
    }

    public Color GetSpriteColor(Piece piece)
    {
        if(piece.type==PieceType.TemporaryQueen)
        {
            return new Color(1, 1, 1, Mathf.Sin(Mathf.PI * Time.time)*0.3f + 0.6f);
        }
        return !piece.poweredUp ? Color.white : piece.color==ColorOfPiece.White ? whitePowerUp: blackPowerUp;
    }

    public void Update()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            scale *= -1;
            foreach (Transform transform in allTransforms)
            {
                transform.position = new Vector3(-transform.position.x, -transform.position.y, transform.position.z);
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            if(quitting)
            {
                if(Time.time-timeFromLastPressingR>1f)
                {
                    SceneManager.LoadScene("Chess");
                }
            }
            else
            {
                timeFromLastPressingR=Time.time;
                quitting = true;
            }
            
        }
        else
        {
            quitting = false;
        }

        if(Input.GetKey(KeyCode.Escape))
        {
            mainCanvas.SetActive(true);
            helpCanvas.SetActive(false);
        }
    }

    public void SetupBoard()
    {
        board = new Board();
        allTransforms = new();
        board.controller = this;
        scale=baseTile.transform.lossyScale.x;
        Vector3 currentPosition = new(-3.5f * scale, 3.5f * scale, 0);
        for(int i=0;i<8;i++)
        {
            for(int j=0;j<8;j++)
            {
                GameObject o = Instantiate(baseTile);
                o.transform.position = currentPosition;
                Color color = ((i % 2) + (j % 2)) % 2 == 0 ? whiteTile : blackTile;
                o.GetComponent<SpriteRenderer>().color = color;
                BoardTile tile = o.GetComponent<BoardTile>();
                int currentIndex = j + i * 8;
                
                if(i%7 == 0)
                {
                    board[currentIndex] = new Piece(i == 0 ? ColorOfPiece.Black : ColorOfPiece.White, j%7==0 ? PieceType.Rook: j%5==1 ? PieceType.Knight: j%3==2 ? PieceType.Bishop: j%2==1 ? PieceType.Queen: PieceType.King);
                }
                else if(i%5==1)
                {
                    board[currentIndex] = new Piece(i == 1 ? ColorOfPiece.Black : ColorOfPiece.White, PieceType.Pawn);
                }
                else
                {
                    board[currentIndex] = emptyPiece;
                }
                tile.index=currentIndex;
                tile.gameController = this;
                tile.OriginalColor = color;
                allTransforms.Add(o.transform);
                currentPosition += new Vector3(scale, 0, 0);
            }
            currentPosition = new Vector3(-3.5f * scale, currentPosition.y - scale, 0);
        }

        {
            GameObject o = Instantiate(baseTile);
            o.transform.position = new(-5.5f * scale, 0, 0);
            Color color = whiteTile;
            o.GetComponent<SpriteRenderer>().color = color;
            BoardTile tile = o.GetComponent<BoardTile>();
            board[64] = new Piece(ColorOfPiece.White, PieceType.Miner);
            tile.index = 64;
            tile.gameController = this;
            tile.OriginalColor = color;
            allTransforms.Add(o.transform);
        }

        {
            GameObject o = Instantiate(baseTile);
            o.transform.position = new(5.5f * scale, 0, 0);
            Color color = blackTile;
            o.GetComponent<SpriteRenderer>().color = color;
            BoardTile tile = o.GetComponent<BoardTile>();
            board[65] = new Piece(ColorOfPiece.Black, PieceType.Miner);
            tile.index = 65;
            tile.gameController = this;
            tile.OriginalColor = color;
            allTransforms.Add(o.transform);
        }

        {
            GameObject o = Instantiate(baseTile);
            o.transform.position = new(-5.5f * scale, 3 * scale, 0);
            Color color = whitePowerUp;
            o.GetComponent<SpriteRenderer>().color = color;
            BoardTile tile = o.GetComponent<BoardTile>();
            board[66] = emptyPiece;
            tile.index = 66;
            tile.gameController = this;
            tile.OriginalColor = color;
            allTransforms.Add(o.transform);
        }

        {
            GameObject o = Instantiate(baseTile);
            o.transform.position = new(5.5f * scale, -3 * scale, 0);
            Color color = blackPowerUp;
            o.GetComponent<SpriteRenderer>().color = color;
            BoardTile tile = o.GetComponent<BoardTile>();
            board[67] = emptyPiece;
            tile.index = 67;
            tile.gameController = this;
            tile.OriginalColor = color;
            allTransforms.Add(o.transform);
        }

        {
            GameObject o = Instantiate(baseTile);
            o.transform.position = new(-5.5f * scale, -3 * scale, 0);
            Color color = whiteTile;
            o.GetComponent<SpriteRenderer>().color = color;
            BoardTile tile = o.GetComponent<BoardTile>();
            board[68] = new Piece(ColorOfPiece.White, PieceType.Dodo);
            tile.index = 68;
            tile.gameController = this;
            tile.OriginalColor = color;
            allTransforms.Add(o.transform);
        }

        {
            GameObject o = Instantiate(baseTile);
            o.transform.position = new(5.5f * scale, 3 * scale, 0);
            Color color = blackTile;
            o.GetComponent<SpriteRenderer>().color = color;
            BoardTile tile = o.GetComponent<BoardTile>();
            board[69] = new Piece(ColorOfPiece.Black, PieceType.Dodo);
            tile.index = 69;
            tile.gameController = this;
            tile.OriginalColor = color;
            allTransforms.Add(o.transform);
        }

    }

    void FixedUpdate()
    {
        switch(gamestate)
        {
            case GameState.Draw:
                blackBox.enabled = true;
                text.text = "Draw";
                break;
            case GameState.BlackWin:
                blackBox.enabled = true;
                text.text = "Black Wins!";
                break;
            case GameState.WhiteWin:
                blackBox.enabled = true;
                text.text = "White Wins!";
                break;
            default:
                blackBox.enabled = false;
                text.text = "";
                break;
        }

        if(Mathf.Sign(scale) == 1)
        {
            powerUpTexts[0].color = whitePowerUp;
            powerUpTexts[1].color = blackPowerUp;
            powerUpTexts[0].text = Mathf.Floor(board.whitePowerUp * 10) / 10f + "";
            powerUpTexts[1].text = Mathf.Floor(board.blackPowerUp * 10) / 10f + "";
        }
        else
        {
            powerUpTexts[0].color = blackPowerUp;
            powerUpTexts[1].color = whitePowerUp;
            powerUpTexts[0].text = Mathf.Floor(board.blackPowerUp * 10) / 10f + "";
            powerUpTexts[1].text = Mathf.Floor(board.whitePowerUp * 10) / 10f + "";
        }
    }

    public void Click(int index)
    {
        if(gamestate==GameState.Promoting && index>=1000)
        {
            board.tiles[possibleMovesSource].type = (PieceType)(index - 998);
            if(board.tiles[possibleMovesSource].poweredUp)
            {
                board.tiles[possibleMovesSource].poweredUp = false;
                if(Turn==ColorOfPiece.White)
                {
                    board.whitePowerUp++;
                }
                else
                {
                    board.blackPowerUp++;
                }
            }
            board.tiles[possibleMovesSource].egg = false;
            board.MakeMove(new Move(possibleMovesSource, promotionPlace));
            possibleMoves = null;
            possibleMovesSource = -1;
            gamestate = GameState.Playing;
            return;
        }
        if(gamestate!=GameState.Playing)
        {
            return;
        }
        if (!board[index].Empty() && board[index].color==Turn && !(board[index].type == PieceType.King && possibleMovesSource != -1 && board[possibleMovesSource].type==PieceType.Rook && board[possibleMovesSource].poweredUp) && !(possibleMovesSource != -1 &&board[possibleMovesSource].type == PieceType.King && board[index].type == PieceType.Pawn))
        {
            possibleMoves = board.GetPossibleMoves(board[index], index);
            possibleMovesSource = index;
        }
        else
        {
            if(possibleMoves!=null && possibleMoves.Contains(index))
            {
                if(board[possibleMovesSource].type == PieceType.Pawn && (index / 8) % 7==0)
                {
                    gamestate = GameState.Promoting;
                    promotionPlace = index;
                    Vector3 currentPosition = new(-5f * scale + (index%8) * scale, 3.5f * scale * ((index / 8)==0 ? 1: -1), -0.3f);
                    foreach (PromotingTo promotion in Enum.GetValues(typeof(PromotingTo)))
                    {
                        GameObject o = Instantiate(baseTile);
                        o.transform.position = currentPosition;
                        BoardTile tile = o.GetComponent<BoardTile>();
                        tile.index = (int) promotion;
                        tile.gameController = this;
                        tile.OriginalColor = Color.white;
                        currentPosition += new Vector3(scale,0,0);
                    }
                    return;
                }
                board.MakeMove(new Move(possibleMovesSource, index));
            }
            possibleMoves = null;
            possibleMovesSource = -1;
        }
    }

    public bool IsPossibleMove(int index)
    {
        if (possibleMoves == null)
        {
            return false;
        }

        return possibleMoves.Contains(index);
    }


}

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
                currentValue = 4;
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

public class Board
{
    public Piece[] tiles;
    public float whitePowerUp;
    public float blackPowerUp;
    public ColorOfPiece turn;
    public Move lastMove;
    public bool kingInDanger;
    public GameController controller;

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
        }
        else if (move.prevTile == 68 + (int)turn)
        {
            if (turn == ColorOfPiece.White)
            {
                whitePowerUp -= EggThresholdCalculator(turn);
            }
            else
            {
                blackPowerUp -= EggThresholdCalculator(turn);
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
                if(piece.poweredUp && LegalVector(positionForTesting) && lastMove.currTile == GetIndexFromVector(position + new Vector2(1, 0)) && tiles[lastMove.currTile].color!=piece.color)
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
                if (piece.poweredUp && LegalVector(positionForTesting) && lastMove.currTile == GetIndexFromVector(position + new Vector2(-1, 0)) && tiles[lastMove.currTile].color != piece.color)
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
                if(!piece.moved && tiles[pawnIndex].type == PieceType.Pawn && !tiles[pawnIndex].egg)
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
                    if(piece.poweredUp)
                    {
                        result.AddRange(GetPossibleMoves(new Piece(1-piece.color, PieceType.Miner, true, false), opponentKingPosition, false, false));
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        moveDirection = new Vector2((1 - i / 2) * (i % 2 == 0 ? 1 : -1), i / 2 * (i % 2 == 0 ? 1 : -1));
                        positionForTesting = moveDirection*2 + position;
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
        return 1;
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