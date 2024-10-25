using UnityEngine;

public class BoardTile : MonoBehaviour
{
    public SpriteRenderer pieceSpriteRenderer;
    public SpriteRenderer tileSpriteRenderer;
    public SpriteRenderer moveIndicatorSpriteRenderer;
    public GameController gameController;
    public int index;
    public Color OriginalColor;

    private int currentYRotation;

    private void FixedUpdate()
    {
        if(index==-1)
        {
            return;
        }
        if(index>=1000 && gameController.gamestate!=GameState.Promoting)
        {
            Destroy(gameObject);
            return;
        }
        currentYRotation += 4;
        if(!gameController.ShouldSpin(index))
        {
            currentYRotation = 0;
        }
        pieceSpriteRenderer.transform.rotation = gameController.GetRotation(index, currentYRotation);
        pieceSpriteRenderer.sprite = gameController.GetSprite(index, currentYRotation);
        pieceSpriteRenderer.enabled = true;
        pieceSpriteRenderer.color=gameController.GetSpriteColor(index, currentYRotation);
        tileSpriteRenderer.enabled = index < 1000;
        moveIndicatorSpriteRenderer.enabled = index < 1000 && gameController.IsPossibleMove(index);
        if (gameController.board.kingInDanger && index<1000 && gameController.board[index].type==PieceType.King && gameController.board[index].color==gameController.Turn)
        {
            tileSpriteRenderer.color = Color.red/1.5f;
        }
        else
        {
            tileSpriteRenderer.color = OriginalColor;
            if (gameController.board.lastMove.Contains(index) && !(gameController.board.lastMove.currTile==0 && gameController.board.lastMove.prevTile == 0))
            {
                if(OriginalColor == GameController.whiteTile)
                {
                    tileSpriteRenderer.color = GameController.whiteTileNewlyMoved;
                }
                else if(OriginalColor == GameController.blackTile)
                {
                    tileSpriteRenderer.color=GameController.blackTileNewlyMoved;
                }
            }            
        }
    }

    void OnMouseDown()
    {
        gameController.Click(index);
    }
}
