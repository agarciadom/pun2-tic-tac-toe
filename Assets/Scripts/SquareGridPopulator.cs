using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class SquareGridPopulator : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    public int Size = 3;
    public float CellWidth, CellHeight, XSpacing, YSpacing;

    void Start()
    {
        CreateGrid();
    }

    void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void CreateGrid()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        if (prefab != null)
        {
            float totalWidth = Size * CellWidth + (Size - 1) * XSpacing;
            float totalHeight = Size * CellHeight + (Size - 1) * YSpacing;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            gridLayout.constraintCount = Size;

            for (int i = 0; i < Size * Size; i++)
            {
                GameObject newObject = Instantiate(prefab);
                newObject.transform.SetParent(gameObject.transform);

                GridCell cell = newObject.GetComponent<GridCell>();
                if (cell != null)
                {
                    cell.Row = i / Size;
                    cell.Column = i % Size;
                }
            }
        }
    }
}
