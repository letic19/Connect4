using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public int columns = 7;
    public int rows = 6;

    public GameObject cellPrefab;
    public GameObject redPiecePrefab;
    public GameObject yellowPiecePrefab;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI turnText;
    public TCPConnection tcpConnection;

    private bool isRedTurn = true;
    private bool gameEnded = false;

    private int[,] board;

    void Start()
    {
        board = new int[columns, rows];

        CreateBoard();
        UpdateTurnText();
    }

    void CreateBoard()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 position = new Vector2(
                 x - (columns / 2f) + 0.5f,
                 y - (rows / 2f) + 0.5f
);

                Instantiate(cellPrefab, position, Quaternion.identity, transform);
            }
        }
    }

    void Update()
    {
        if (gameEnded)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int column = Mathf.RoundToInt(mousePosition.x + (columns / 2f) - 0.5f);

            LocalMove(column);
        }
    }

    public void LocalMove(int column)
    {
        DropPiece(column);

        if (tcpConnection != null)
        {
            tcpConnection.SendMessageToOther("MOVE:" + column);
        }
    }

    public void DropPiece(int column)
    {
        if (column < 0 || column >= columns)
            return;

        bool piecePlaced = false;

        for (int y = 0; y < rows; y++)
        {
            if (board[column, y] == 0)
            {
                board[column, y] = isRedTurn ? 1 : 2;

                Vector2 position = new Vector2(
                    column - (columns / 2f) + 0.5f,
                    y - (rows / 2f) + 0.5f
                );

                GameObject piecePrefab = isRedTurn ? redPiecePrefab : yellowPiecePrefab;

                Instantiate(piecePrefab, position, Quaternion.identity);

                if (CheckWin(column, y))
                {
                    winnerText.gameObject.SetActive(true);

                    winnerText.text = (isRedTurn ? "Vermelho" : "Amarelo") + " venceu!";
                    gameEnded = true;
                }
                else
                {
                    isRedTurn = !isRedTurn;
                    UpdateTurnText();
                }

                piecePlaced = true;

                break;
            }
        }

        if (!piecePlaced)
        {
            Debug.Log("Coluna cheia!");
        }
    }

    bool CheckWin(int column, int row)
    {
        int currentPlayer = board[column, row];

        return CheckDirection(column, row, 1, 0, currentPlayer) || // Horizontal
               CheckDirection(column, row, 0, 1, currentPlayer) || // Vertical
               CheckDirection(column, row, 1, 1, currentPlayer) || // Diagonal \
               CheckDirection(column, row, 1, -1, currentPlayer);  // Diagonal /
    }

    bool CheckDirection(int column, int row, int dirX, int dirY, int player)
    {
        int count = 1;

        count += CountPieces(column, row, dirX, dirY, player);
        count += CountPieces(column, row, -dirX, -dirY, player);

        return count >= 4;
    }

    int CountPieces(int startX, int startY, int dirX, int dirY, int player)
    {
        int count = 0;

        int x = startX + dirX;
        int y = startY + dirY;

        while (
            x >= 0 && x < columns &&
            y >= 0 && y < rows &&
            board[x, y] == player
        )
        {
            count++;

            x += dirX;
            y += dirY;
        }

        return count;
    }

    void UpdateTurnText()
    {
        turnText.text = isRedTurn ? "Vez do Vermelho" : "Vez do Amarelo";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    
}
