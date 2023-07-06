using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseOrderUI : MonoBehaviour
{
    [SerializeField]
    private GameObject disableGroup;
    [SerializeField]
    private GameObject pooledOrderUI;
    [SerializeField]
    private Transform orderContainer;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private InputField oilBidField;

    private int totalOilReservce = 0;
    private int oilBid = 0;
    public int OilBid
    {
        get { return oilBid; }
        set
        {
            if(value != oilBid)
            {
                if(value > totalOilReservce)
                {
                    oilBid = totalOilReservce;
                }
                else
                {
                    oilBid = value;
                }
            }
        }
    }

    public class OrderData
    {
        public OrderEntryUI orderEntry = null;
        public bool orderSet = false;
        public bool canBeOrder = false;
    }
    OrderData[] availableOrders = new OrderData[0];

    public void InitializeOrders(int orderSize)
    {
        disableGroup.SetActive(true);
        confirmButton.interactable = false;
        if(availableOrders.Length > 0)
        {
            foreach(OrderData entry in availableOrders)
            {
                if(entry.orderEntry != null)
                {
                    Destroy(entry.orderEntry.gameObject);
                }

            }
        }
        availableOrders = new OrderData[9];

        for(int i = 0; i < availableOrders.Length; i++)
        {
            GameObject newOrder = Instantiate(pooledOrderUI, orderContainer);
            OrderEntryUI orderEntry = newOrder.GetComponent<OrderEntryUI>();
            int index = i;
            orderEntry.ClearButton.onClick.AddListener(() => ClearEntry(index));
            orderEntry.SetInactive = i >= orderSize;
            newOrder.SetActive(true);

            availableOrders[i] = new OrderData
            {
                orderEntry = orderEntry,
                canBeOrder = i < orderSize ? true : false
            };
        }
    }

    public int SetNewOrder(string unitTag, string destTag)
    {
        OrderData result = null;
        int index = int.MinValue;
        for (int i = 0; i < availableOrders.Length; i++)
        {
            OrderData entry = availableOrders[i];
            if (entry.canBeOrder && !entry.orderSet)
            {
                result = entry;
                index = i;

                availableOrders[i].orderSet = true;
                break;
            }
        }
        if(result != null)
        {
            result.orderEntry.SetDestination = destTag;
            result.orderEntry.SetUnit = unitTag;
        }
        return index;
    }

    public void ClearEntry(int entryIndex)
    {
        availableOrders[entryIndex].orderEntry.ResetEntry();
        availableOrders[entryIndex].orderSet = false;
        OrdersManager.Instance.Orders[entryIndex].ClearOrderPair();
    }

    public void ReviseOrder(int index, string unitTag, string destTag)
    {
        OrderData result = availableOrders[index];
        result.orderEntry.SetDestination = destTag;
        result.orderEntry.SetUnit = unitTag;
    }
}
