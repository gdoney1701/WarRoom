using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BelligerentData
{
    public FactionData[] WarParticipants = new FactionData[] { new FactionData() };

    public void SaveToFile(string inputName)
    {
        string data = JsonUtility.ToJson(this, true);
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
        Debug.Log(string.Format("Increaseing Array. Initial Lenght {0}. New Length {1}", WarParticipants.Length, newArray.Length));
        WarParticipants = newArray;
        Debug.Log(string.Format("Confirming Change. New Length {0}", WarParticipants.Length));
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

    public TileData[] TileControl = new TileData[] { new TileData()};
    public StackData[] StackArray = new StackData[] { new StackData() };

    public void IncreaseTileArray(TileData newEntry)
    {
        TileData[] newArray = new TileData[TileControl.Length + 1];
        for (int i = 0; i < TileControl.Length; i++)
        {
            newArray[i] = TileControl[i];
        }
        newArray[newArray.Length - 1] = newEntry;
        TileControl = newArray;
    }
    public void IncreaseStackArray(StackData newEntry)
    {
        StackData[] newArray = new StackData[StackArray.Length + 1];
        for (int i = 0; i < StackArray.Length; i++)
        {
            newArray[i] = StackArray[i];
        }
        newArray[newArray.Length - 1] = newEntry;
        StackArray = newArray;
    }
    public void DecreaseTileArray(int index)
    {
        TileData[] newArray = new TileData[TileControl.Length - 1];
        for (int i = 0, j = 0; i < TileControl.Length; i++)
        {
            if (i != index)
            {
                newArray[j] = TileControl[i];
                j++;
            }
        }
        TileControl = newArray;
    }
    public void DecreateStackArray(int index)
    {
        StackData[] newArray = new StackData[StackArray.Length - 1];
        for (int i = 0, j = 0; i < StackArray.Length; i++)
        {
            if (i != index)
            {
                newArray[j] = StackArray[i];
                j++;
            }
        }
        StackArray = newArray;
    }
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
