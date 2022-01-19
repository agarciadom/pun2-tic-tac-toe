using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

/**
 * Holds the current state of the game, and updates the display.
 * 
 * Detects the winning condition.
 */
public class GameManager : MonoBehaviour, IPunObservable
{
    public enum MarkType { EMPTY, X, O, TIE }

    public TextMeshProUGUI turnText;

    // Number of columns and rows of the grid
    public const int Size = 3;

    // Current turn in the game
    private MarkType _turn;
    public MarkType Turn {
        get
        {
            return _turn;
        }
        private set {
            _turn = value;
            if (Winner == MarkType.EMPTY)
            {
                turnText.text = $"Turn: {value}";
            }
        }
    }

    // Winner of the game
    private MarkType _winner = MarkType.EMPTY;
    public MarkType Winner
    {
        get
        {
            return _winner;
        }
        private set
        {
            _winner = value;

            switch (value)
            {
                case MarkType.O:
                case MarkType.X:
                    turnText.text = $"Winner: {value}! - SPACE to reset, ESC to quit";
                    break;
                case MarkType.TIE:
                    turnText.text = $"Tied! - SPACE to reset, ESC to quit";
                    break;
            }
        }
    }

    // Cell states
    private MarkType[] cells;

    void OnEnable()
    {
        Turn = MarkType.O;

        cells = new MarkType[Size * Size];
        for (int i = 0; i < cells.Length; ++i)
        {
            cells[i] = MarkType.EMPTY;
        }
    }

    void Update()
    {
        if (Winner != MarkType.EMPTY && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void CellClicked(GridCell cell)
    {
        if (Winner != MarkType.EMPTY)
        {
            // Game has finished, do nothing
            return;
        }

        int i = GetCellIndex(cell);
        if (cells[i] == MarkType.EMPTY)
        {
            cells[i] = Turn;
            cell.SetMark(Turn);
            DetectVictoryConditionAround(cell.Row, cell.Column);

            Turn = Turn == MarkType.O ? MarkType.X : MarkType.O;
        }
    }

    private int GetCellIndex(GridCell cell)
    {
        return GetCellIndex(cell.Row, cell.Column);
    }

    private int GetCellIndex(int row, int column)
    {
        return row * Size + column;
    }

    private void DetectVictoryConditionAround(int row, int column)
    {
        if (DetectVictoryConditionByRow(row, column)
            || DetectVictoryConditionByColumn(column, column)
            || DetectVictoryConditionByMajorDiagonal(row, column)
            || DetectVictoryConditionByMinorDiagonal(row, column))
        {
            int i = GetCellIndex(row, column);
            Winner = cells[i];
        }
        else if (DetectTie())
        {
            Winner = MarkType.TIE;
        }
    }

    private bool DetectVictoryConditionByMinorDiagonal(int row, int column)
    {
        if (row != Size - 1 - column)
        {
            // Cell is not part of minor diagonal
            return false;
        }

        int i = GetCellIndex(row, column);
        for (int j = 0; j < Size; ++j)
        {
            if (cells[GetCellIndex(j, Size - 1 - j)] != cells[i])
            {
                return false;
            }
        }
        return true;
    }

    private bool DetectVictoryConditionByMajorDiagonal(int row, int column)
    {
        if (row != column)
        {
            // Cell is not part of major diagonal
            return false;
        }

        int i = GetCellIndex(row, column);
        for (int j = 0; j < Size; ++j)
        {
            if (cells[GetCellIndex(j, j)] != cells[i])
            {
                return false;
            }
        }
        return true;
    }

    private bool DetectVictoryConditionByColumn(int row, int column)
    {
        int i = GetCellIndex(row, column);
        for (int iRow = 0; iRow < Size; ++iRow)
        {
            if (cells[GetCellIndex(iRow, column)] != cells[i])
            {
                return false;
            }
        }
        return true;
    }

    private bool DetectVictoryConditionByRow(int row, int column)
    {
        int i = GetCellIndex(row, column);
        for (int iCol = 0; iCol < Size; ++iCol)
        {
            if (cells[GetCellIndex(row, iCol)] != cells[i])
            {
                return false;
            }
        }
        return true;
    }

    private bool DetectTie()
    {
        for (int j = 0; j < cells.Length; ++j)
        {
            if (cells[j] == MarkType.EMPTY)
            {
                return false;
            }
        }
        return true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.Turn);
            stream.SendNext(this.Winner);

            foreach (MarkType cell in cells)
            {
                stream.SendNext(cell);
            }
        }
        else
        {
            this.Turn = (MarkType) stream.ReceiveNext();
            this.Winner = (MarkType)stream.ReceiveNext();

            for (int i = 0; i < Size; i++)
            {
                cells[i] = (MarkType)stream.ReceiveNext();
            }
        }
    }
}
