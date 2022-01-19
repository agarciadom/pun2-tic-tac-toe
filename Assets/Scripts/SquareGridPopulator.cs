using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class SquareGridPopulator : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    public GameManager gameManager;
    public float CellWidth, CellHeight, XSpacing, YSpacing;

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

            for (int i = 0; i < n * n; i++)
            {
                GameObject newObject = Instantiate(prefab);
                newObject.transform.SetParent(gameObject.transform);

                GridCell cell = newObject.GetComponent<GridCell>();
                if (cell != null)
                {
                    cell.gameManager = gameManager;
                    cell.Row = i / n;
                    cell.Column = i % n;
                }
            }
        }
    }
}
