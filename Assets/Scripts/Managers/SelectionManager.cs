using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager
{
    private static SelectionManager _instance;
    public static SelectionManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new SelectionManager();
            }
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }

    private SelectionManager() { }

    private List<StackManager> selectedUnits = new List<StackManager>();
    public List<StackManager> AvailableUnits = new List<StackManager>();
    private MapTile selectedTile = null;

    public List<StackManager> SelectedUnits
    {
        get { return selectedUnits; }
    }

    public MapTile SelectedTile
    {
        get { return selectedTile; }
    }

    public void SelectUnits(StackManager unit)
    {
        selectedUnits.Add(unit);
        unit.OnSelect();
    }

    public void DeselectUnit(StackManager unit)
    {
        unit.OnDeselect();
        selectedUnits.Remove(unit);
    }

    public void DeselectAll()
    {
        foreach(StackManager unit in selectedUnits)
        {
            unit.OnDeselect();
        }
        selectedUnits.Clear();
    }

    public void SelectTile(MapTile tile)
    {
        selectedTile = tile;
        selectedTile.OnSelect();
    }
    public void DeselectTile()
    {
        if(selectedTile != null)
        {
            selectedTile.OnDeselect();
        }
        selectedTile = null;
    }
}
