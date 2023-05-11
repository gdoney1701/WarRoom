using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArmyInfoStatic
{
    [System.Serializable]
    public enum CombatZone
    {
        Air = 0,
        Land = 1,
        Sea = 2
    }
    public enum Faction
    {
        UnitedStates = 0,
        Germany = 1,
        UnitedKingdom = 2,
        SovietUnion = 3,
        Italy = 4
        
    }

    public static FactionInfo factionUS = new FactionInfo
    {
        factionColor = new Color32(93, 159, 117, 255),
        factionID = "us",
        factionName = Faction.UnitedStates
    };

    public static FactionInfo factionGermany = new FactionInfo
    {
        factionColor = new Color32(134, 141, 151, 255),
        factionID = "ge",
        factionName = Faction.Germany
    };

    public static FactionInfo factionUK = new FactionInfo
    {
        factionColor = new Color32(160, 148, 114, 255),
        factionID = "uk",
        factionName = Faction.UnitedKingdom
    };

    public static FactionInfo factionSoviet = new FactionInfo
    {
        factionColor = new Color32(145, 102, 89, 255),
        factionID = "sv",
        factionName = Faction.SovietUnion
    };

    public static FactionInfo factionItaly = new FactionInfo
    {
        factionColor = new Color32(129, 176, 75, 255),
        factionID = "it",
        factionName = Faction.Italy
    };

    public static CombatZone ConvertIntToCombatZone(int input)
    {
        return (CombatZone)input;
    }

    public static FactionInfo ConvertFactionToInfo(Faction faction)
    {
        FactionInfo result = factionUS;
        switch ((int)faction)
        {
            case 0:
                result = factionUS;
                break;

            case 1:
                result = factionGermany;
                break;

            case 2:
                result = factionUK;
                break;

            case 3:
                result = factionSoviet;
                break;

            case 4:
                result = factionItaly;
                break;

        }
        return result;
    }

}
public class FactionInfo
{
    public Color factionColor = Color.white;
    public string factionID = "us";
    public ArmyInfoStatic.Faction factionName = ArmyInfoStatic.Faction.UnitedStates; 
}

