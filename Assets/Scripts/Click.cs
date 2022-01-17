using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Clicked " + gameObject.name);
    }
}
