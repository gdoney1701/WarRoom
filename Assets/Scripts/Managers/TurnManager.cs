using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField]
    PhaseOrderUI orderPhase;
    //A turn consists of 5 phases:
    //    1 - Order creation phase
    //    2 - Order Execution phase
    //    3 - Combat phase
    //    4 - Territory Resolution
    //    5 - Refit and Redeploy <- not going to be part of current goal

    private void OnEnable()
    {
        PopulateButtons.onGameReady += StartGame;
        PlayerController.sendOrder += AcceptOrder;
    }

    private void OnDisable()
    {
        PopulateButtons.onGameReady -= StartGame;
        PlayerController.sendOrder -= AcceptOrder;
    }

    private TurnPhase currentPhase = TurnPhase.Order;
    public TurnPhase CurrentPhase
    {
        get { return currentPhase; }
        set
        {
            if(value != currentPhase)
            {
                currentPhase = value;
                updatePhase?.Invoke(currentPhase);
                InitializeTurn();
            }
        }
    }
    public int currentTurn = 0;

    public delegate void UpdateTurnPhase(TurnPhase phase);
    public static event UpdateTurnPhase updatePhase;

    public void StartGame(FactionData faction)
    {
        //TODO --- Serialize the last turn order, for now we'll always default to the Order phase of turn 0

        updatePhase?.Invoke(currentPhase);
        InitializeTurn();
    }

    public void InitializeTurn()
    {
        switch (currentPhase)
        {
            case TurnPhase.Order:
                StartOrder();
                break;
            case TurnPhase.Move:
                //Add Move Phase;
                break;
            case TurnPhase.Combat:
                //Add Combat Phase
                break;
            case TurnPhase.Resolution:
                //Add Resolution Phase;
                break;
            case TurnPhase.Refit:
                //Add Refit Phase
                break;
        }
    }

    private void StartOrder()
    {
        int orderAmount = 6;
        OrdersManager.Instance.InitializeOrderArray(orderAmount);
        orderPhase.InitializeOrders(orderAmount);
    }

    private void AcceptOrder(MapTile mapTile)
    {
        StackManager selectedStack = SelectionManager.Instance.SelectedUnits[0];
        if(OrdersManager.Instance.OrdersContainStack(selectedStack, out int presentAt))
        {
            if(selectedStack.CurrentTileTag == mapTile.TileName)
            {
                selectedStack.OnClearMove();
                orderPhase.ClearEntry(presentAt);
                OrdersManager.Instance.Orders[presentAt].ClearOrderPair();
                return;
            }
            else
            {
                selectedStack.OnSetMove(mapTile.CenterContainer.position);
                orderPhase.ReviseOrder(presentAt, selectedStack.ShortTag, mapTile.TileName);
                OrdersManager.Instance.ChangeOrders(presentAt, selectedStack, mapTile);
                return;
            }

        }
        selectedStack.OnSetMove(mapTile.CenterContainer.position);
        int newOrderIndex = orderPhase.SetNewOrder(
            selectedStack.ShortTag, mapTile.TileName
            );
        if(newOrderIndex != int.MinValue)
        {
            OrdersManager.Instance.ChangeOrders(newOrderIndex, selectedStack, mapTile);
        }

    }
}

public enum TurnPhase
{
    Order = 0,
    Move = 1,
    Combat = 2,
    Resolution = 3,
    Refit = 4
}
