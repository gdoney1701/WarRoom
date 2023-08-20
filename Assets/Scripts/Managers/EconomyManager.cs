using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager
{
    private static EconomyManager _instance;

    public static EconomyManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new EconomyManager();
            }
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }

    private EconomyManager() { }

    private int oilReserve = 0;
    public int OilReserve
    {
        get { return oilReserve; }
    }

    private int ironReserve = 0;
    public int IronReserve
    {
        get { return ironReserve; }
    }
    private int osrReserve = 0;
    public int OsrReserve
    {
        get { return osrReserve; }
    }
}
