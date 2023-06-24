using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //A turn consists of 5 phases:
    //    1 - Order creation phase
    //    2 - Order Execution phase
    //    3 - Combat phase
    //    4 - Territory Resolution
    //    5 - Refit and Redeploy <- not going to be part of current goal

    private MapTile selectedTile;

    private void OnEnable()
    {
        PlayerController.onTileSelect += SelectTile;
    }

    private void OnDisable()
    {
        PlayerController.onTileSelect -= SelectTile;
    }

    void SelectTile(MapTile data)
    {
        if(selectedTile != null)
        {
            selectedTile.SetSelectedVisuals(false);
        }

        selectedTile = data;
        selectedTile.SetSelectedVisuals(true);
    }
}
