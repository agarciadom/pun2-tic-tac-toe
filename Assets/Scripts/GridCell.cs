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

    void OnMouseDown()
    {
        gameManager.CellClicked(this);
    }

}
