using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BelligerentData
{
    public FactionData[] WarParticipants = new FactionData[] { new FactionData() };

    public void SaveToFile(string inputName)
    {
        GenerateLongID();

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

    public void GenerateLongID()
    {
        for (int i = 0; i < WarParticipants.Length; i++)
        {
            for (int j = 0; j < WarParticipants[i].StackArray.Length; j++)
            {
                WarParticipants[i].StackArray[j].GenerateLongID(WarParticipants[i].ID);
            }
        }
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
    public string IconPath = string.Empty;

    public TileData[] TileControl = new TileData[] { new TileData()};
    public StackData[] StackArray = new StackData[] { new StackData("hre") };

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
    public StackType StackZone = StackType.Land;

    public int RedTroopCount;
    public int BlueTroopCount;
    public int GreenTroopCount;
    public int YellowTroopCount;

    public string TileTag;
    public string TroopLongTag;
    public string TroopNumberID;

    public StackData(string inputID)
    {
        StackZone = StackType.Land;
        RedTroopCount = 0;
        BlueTroopCount = 0;
        GreenTroopCount = 0;
        YellowTroopCount = 0;

        TileTag = "Z1";
        GenerateDefaultID(inputID);
    }

    public int GetStackTotal()
    {
        return RedTroopCount + BlueTroopCount + YellowTroopCount + GreenTroopCount;
    }

    public void GenerateDefaultID(string factionID)
    {
        TroopLongTag = string.Format("{0}_{1}_{2}", factionID, ((int)StackZone), 57);
        TroopNumberID = string.Format("{0}", 57);
    }
    public void GenerateLongID(string factionID)
    {
        TroopLongTag = string.Format("{0}_{1}_{2}", factionID, ((int)StackZone), TroopNumberID);
    }

    public enum StackType
    {
        Air = 0,
        Land = 1,
        Ocean = 2
    }
}
