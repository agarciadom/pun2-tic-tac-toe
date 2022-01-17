using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public enum MarkType { EMPTY, X, O }

    public int Row, Column;

    private static MarkType turn = MarkType.O;

    private const string xMarkerName = "XMarker";
    private const string oMarkerName = "OMarker";
    private GameObject oMarker, xMarker;

    void Awake()
    {
        oMarker = transform.Find(xMarkerName).gameObject;
        xMarker = transform.Find(oMarkerName).gameObject;
    }

    public void SetMark(MarkType m)
    {
        oMarker.SetActive(m == MarkType.O);
        xMarker.SetActive(m == MarkType.X);
    }

    void OnMouseDown()
    {
        Debug.Log($"Clicked on row {Row}, column {Column}");
        SetMark(turn);

        // change turn
        turn = turn == MarkType.O ? MarkType.X : MarkType.O;
    }

}
