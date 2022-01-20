using System;
using UnityEngine;
using UnityEngine.Events;

public class GridCell : MonoBehaviour
{
    private const string xMarkerName = "XMarker";
    private const string oMarkerName = "OMarker";

    public UnityEvent<GridCell> Clicked;
    public int Row, Column;

    private GameObject oMarker, xMarker;
    private GameManager.MarkType _mark = GameManager.MarkType.EMPTY;

    public GameManager.MarkType Mark
    {
        get
        {
            return _mark;
        }
        set
        {
            _mark = value;
            oMarker.SetActive(value == GameManager.MarkType.O);
            xMarker.SetActive(value == GameManager.MarkType.X);
        }
    }

    void Awake()
    {
        oMarker = transform.Find(xMarkerName).gameObject;
        xMarker = transform.Find(oMarkerName).gameObject;
    }

    public void OnMouseDown()
    {
        Clicked.Invoke(this);
    }
}
