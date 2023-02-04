using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StackInfo
{

    public ArmyInfoStatic.CombatZone StackZone = ArmyInfoStatic.CombatZone.Land;


    public int RedTroopCount = 0;
    public int BlueTroopCount = 0;
    public int GreenTroopCount = 0;
    public int YellowTroopCount = 0;


    public string LocationCode = "Z1";
    public string TroopID = "us_l_57";

    public int GetStackTotal()
    {
        return RedTroopCount + BlueTroopCount + YellowTroopCount + GreenTroopCount;
    }
    public void GenerateStackID(ArmyInfoStatic.Faction inputFaction)
    {
        string factionID = ArmyInfoStatic.ConvertFactionToInfo(inputFaction).factionID;
        TroopID = string.Format("{0}_{1}_{2}", factionID, "l", Random.Range(10, 100));
    }
}
