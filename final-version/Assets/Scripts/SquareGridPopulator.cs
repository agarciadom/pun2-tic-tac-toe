using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class SquareGridPopulator : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    public float CellWidth, CellHeight, XSpacing, YSpacing;
    public GridCell[] Cells { get; private set; }
    public UnityEvent<GridCell> createdCell;

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        if (prefab != null)
        {
            int n = GameManager.Size;
            float totalWidth = n * CellWidth + (n - 1) * XSpacing;
            float totalHeight = n * CellHeight + (n - 1) * YSpacing;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            gridLayout.constraintCount = n;

            this.Cells = new GridCell[n * n];
            for (int i = 0; i < Cells.Length; i++)
            {
                GameObject newObject = Instantiate(prefab);
                newObject.transform.SetParent(gameObject.transform);

                GridCell cell = newObject.GetComponent<GridCell>();
                if (cell != null)
                {
                    cell.Row = i / n;
                    cell.Column = i % n;
                }
                Cells[i] = cell;

                createdCell.Invoke(cell);
            }
        }
    }

}
