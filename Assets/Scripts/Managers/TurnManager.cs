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

    private void OnEnable()
    {
        PopulateButtons.onGameReady += InitializeTurn;
    }

    private void OnDisable()
    {
        PopulateButtons.onGameReady -= InitializeTurn;
    }
    public enum TurnPhase
    {
        Order = 0,
        Move = 1,
        Combat = 2,
        Resolution = 3,
        Refit = 4
    }

    public TurnPhase currentPhase = TurnPhase.Order;
    public int currentTurn = 0;

    public void InitializeTurn()
    {

    }

    private void StartOrder()
    {

    }
}
