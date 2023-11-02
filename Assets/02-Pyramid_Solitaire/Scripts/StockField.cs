using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockField : MonoBehaviour
{
    private void OnMouseDown()
    {
        Pyramid.S.WasteToStock();
    }
}
