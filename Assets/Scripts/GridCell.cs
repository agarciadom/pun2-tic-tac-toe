using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    private const string xMarkerName = "XMarker";
    private const string oMarkerName = "OMarker";

    public int Row, Column;
    public GameManager gameManager;

    private GameObject oMarker, xMarker;

    void Awake()
    {
        oMarker = transform.Find(xMarkerName).gameObject;
        xMarker = transform.Find(oMarkerName).gameObject;
    }

    public void SetMark(GameManager.MarkType m)
    {
        oMarker.SetActive(m == GameManager.MarkType.O);
        xMarker.SetActive(m == GameManager.MarkType.X);
    }

    void OnMouseDown()
    {
        gameManager.CellClicked(this);
    }

}
