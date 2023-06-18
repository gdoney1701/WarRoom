using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BelligerentData
{
    public FactionData[] WarParticipants = new FactionData[] { new FactionData() };

    public void SaveToFile(string inputName)
    {
        string data = JsonUtility.ToJson(this);
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "BelligerentData", inputName);
        System.IO.File.WriteAllText(path, data);
    }
    public BelligerentData LoadFromFile(string inputName)
    {
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "BelligerentData", inputName);
        try
        {
            string jsonString = System.IO.File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonString, this);

        }
        catch
        {
            Debug.LogError("Failed to find a file to load");
        }
        return this;
    }

    public void IncreaseArray()
    {
        FactionData[] newArray = new FactionData[WarParticipants.Length + 1];
        for(int i = 0; i < WarParticipants.Length; i++)
        {
            newArray[i] = WarParticipants[i];
        }
        newArray[newArray.Length - 1] = new FactionData();
        WarParticipants = newArray;
    }
    public void DecreaseArray(int index)
    {
        FactionData[] newArray = new FactionData[WarParticipants.Length - 1];
        for(int i = 0, j = 0; i< WarParticipants.Length; i++)
        {
            if(i != index)
            {
                newArray[j] = WarParticipants[i];
                j++;
            }
        }
        WarParticipants = newArray;
    }
}

[System.Serializable]
public class FactionData
{
    public string LongName = "Holy Roman Empire";
    public Vector3Int Color = Vector3Int.zero;
    public string ID = "hre";
    public int AllianceID = 0;

    public string[] TileControl = new string[] { "Z100" };
    public StackData[] StackArray = new StackData[] { new StackData() };
}

[System.Serializable]
public class StackData
{

    public ArmyInfoStatic.CombatZone StackZone = ArmyInfoStatic.CombatZone.Land;

    public int RedTroopCount = 0;
    public int BlueTroopCount = 0;
    public int GreenTroopCount = 0;
    public int YellowTroopCount = 0;


    public string TileTag = "Z1";
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
