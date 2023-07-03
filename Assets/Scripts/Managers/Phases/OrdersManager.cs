using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdersManager
{
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

    public OrdersManager(int newOrderTotal)
    {
        OrderAmount = newOrderTotal;
        orders = new OrderPairs[OrderAmount];
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
