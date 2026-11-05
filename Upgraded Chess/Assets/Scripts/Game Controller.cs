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

public enum MinerSituation
{
    NotUpgraded,
    UpgradedAlive,
    UpgradedDead/*1,
    UpgradedDead2*/
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
    [SerializeField] private Button backButton;
    [SerializeField] private Button restartButton;

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

    private Dictionary<PieceType, BoardTile> promotionSquares;

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
        backButton.onClick.AddListener(() =>
        {
            mainCanvas.SetActive(true);
            helpCanvas.SetActive(false);
        });
        restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Chess");
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

        //promotionSquares = new Dictionary<PieceType, BoardTile>();
        //foreach (var t in new PieceType[] { PieceType.Miner, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen })
        //{
        //    GameObject o = Instantiate(baseTile);
        //    o.transform.position = new(0, 0, 0);
        //    Color color = blackTile;
        //    o.GetComponent<SpriteRenderer>().enabled = false;
        //    BoardTile tile = o.GetComponent<BoardTile>();
        //    tile.index = 1000;
        //    tile.gameController = this;
        //    tile.OriginalColor = color;
        //    allTransforms.Add(o.transform);
        //    promotionSquares.Add(t, tile);
        //}
    }

    void FixedUpdate()
    {
        switch(gamestate)
        {
            case GameState.Draw:
                blackBox.enabled = true;
                restartButton.gameObject.SetActive(true);
                text.text = "Draw";
                break;
            case GameState.BlackWin:
                blackBox.enabled = true;
                restartButton.gameObject.SetActive(true);
                text.text = "Black Wins!";
                break;
            case GameState.WhiteWin:
                blackBox.enabled = true;
                restartButton.gameObject.SetActive(true);
                text.text = "White Wins!";
                break;
            default:
                blackBox.enabled = false;
                restartButton.gameObject.SetActive(false);
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
        if(gamestate==GameState.Promoting && Enum.IsDefined(typeof(PromotingTo), index))
        {
            gamestate = GameState.Playing;
            board.MakeMove(new Move(possibleMovesSource, promotionPlace, (PieceType)(index - 998)));
            possibleMoves = null;
            possibleMovesSource = -1;
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
            if(IsPossibleMove(index))
            {
                if(board.IsPromotion(new Move(possibleMovesSource, index)))
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
