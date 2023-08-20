using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdersManager
{
    private static OrdersManager _instance;
    public static OrdersManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new OrdersManager();
            }
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }
    

    private int OrderAmount = 6;

    public class OrderPairs
    {
        public StackManager Unit = null;
        public MapTile Destination = null;

        public void ClearOrderPair()
        {
            Unit = null;
            Destination = null;
        }
    }

    private OrderPairs[] orders = new OrderPairs[0];
    public OrderPairs[] Orders
    {
        get { return orders; }
    }

    public void InitializeOrderArray(int orderAmount)
    {
        OrderAmount = orderAmount;
        orders = new OrderPairs[orderAmount];
    }
    public void ChangeOrders(int index, StackManager stack, MapTile tile)
    {
        Orders[index] = new OrderPairs() { Destination = tile, Unit = stack };
    }

    public bool OrdersContainStack(StackManager compare, out int index)
    {
        for (int i = 0; i < orders.Length; i++)
        {
            if(orders[i] != null)
            {
                if (orders[i].Unit == compare)
                {
                    index = i;
                    return true;
                }
            }

        }
        index = int.MinValue;
        return false;
    }
}
