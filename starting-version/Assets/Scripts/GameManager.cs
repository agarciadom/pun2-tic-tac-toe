using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/**
 * Holds the current state of the game, and updates the display.
 * 
 * Detects the winning condition.
 */
public class GameManager : MonoBehaviour
{
    #region Constants

    // Event: the remote player has clicked on a cell at (row, column)
    public const int EVENT_MOVE = 1;

    // Possible turns and states of a cell
    public enum MarkType { EMPTY, X, O, TIE }

    // Number of columns and rows of the grid
    public const int Size = 3;

    #endregion

    #region Inspector-based configuration

    // Text label we will use to display state
    public TextMeshProUGUI turnText;

    #endregion

    #region Shared game state

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
    private MarkType _winner;
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
                    string winnerName = value.ToString();
                    turnText.text = $"Winner: {winnerName}! - SPACE to reset, ESC to quit";
                    break;

                case MarkType.TIE:
                    turnText.text = $"Tied! - SPACE to reset, ESC to quit";
                    break;
            }
        }
    }

    // Access to cells
    private List<GridCell> cells = new List<GridCell>();

    #endregion

    #region Private game state

    #endregion

    #region Initialisation and input handling

    void Start()
    {
        Winner = MarkType.EMPTY;
        Turn = MarkType.O;
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

    #endregion

    #region Game event handling

    public void OnCellCreated(GridCell cell)
    {
        cells.Add(cell);
        cell.Clicked.AddListener(OnCellClicked);
    }

    public void OnCellClicked(GridCell cell)
    {
        if (Winner != MarkType.EMPTY)
        {
            // Game has finished, do nothing
            return;
        }

        // Really change the cell
        CellPlayed(cell);
    }

    private void CellPlayed(GridCell cell)
    {
        if (cell.Mark == MarkType.EMPTY)
        {
            cell.Mark = Turn;
            DetectVictoryConditionAround(cell.Row, cell.Column);
            Turn = Turn == MarkType.O ? MarkType.X : MarkType.O;
        }
    }

    #endregion

    #region Photon event handling and synchronisation

    #endregion

    #region Cell accessors

    private int GetCellIndex(GridCell cell)
    {
        return GetCellIndex(cell.Row, cell.Column);
    }

    private int GetCellIndex(int row, int column)
    {
        return row * Size + column;
    }

    #endregion

    #region Victory detection

    private void DetectVictoryConditionAround(int row, int column)
    {
        if (DetectVictoryConditionByRow(row, column)
            || DetectVictoryConditionByColumn(column, column)
            || DetectVictoryConditionByMajorDiagonal(row, column)
            || DetectVictoryConditionByMinorDiagonal(row, column))
        {
            int i = GetCellIndex(row, column);
            Winner = cells[i].Mark;
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
            if (cells[GetCellIndex(j, Size - 1 - j)].Mark != cells[i].Mark)
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
            if (cells[GetCellIndex(j, j)].Mark != cells[i].Mark)
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
            if (cells[GetCellIndex(iRow, column)].Mark != cells[i].Mark)
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
            if (cells[GetCellIndex(row, iCol)].Mark != cells[i].Mark)
            {
                return false;
            }
        }
        return true;
    }

    private bool DetectTie()
    {
        for (int j = 0; j < cells.Count; ++j)
        {
            if (cells[j].Mark == MarkType.EMPTY)
            {
                return false;
            }
        }
        return true;
    }

    #endregion
}
