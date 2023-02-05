using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmyInfo
{
    public ArmyInfoStatic.Faction FactionName = ArmyInfoStatic.Faction.UnitedStates;
    public StackInfo[] FactionStacks = new StackInfo[1] { new StackInfo() };

    private void LoadOverwrite(string jsonString)
    {
        JsonUtility.FromJsonOverwrite(jsonString, this);
    }

    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }

    public void SaveToFile(string inputName)
    {
        string data = SaveToString();
        string path = string.Format("{0}/{1}.json", Application.streamingAssetsPath, inputName);
        System.IO.File.WriteAllText(path, data);

    }

    public ArmyInfo LoadFromFile(string inputName)
    {
        string path = string.Format("{0}/{1}.json", Application.streamingAssetsPath, inputName);
        try
        {
            string jsonString = System.IO.File.ReadAllText(path);
            LoadOverwrite(jsonString);
        }
        catch
        {
            Debug.LogError("Failed to find a file to load");
        }

        return this;
    }

    public void ResizeFactionArray(int newSize)
    {
        StackInfo[] old = FactionStacks;
        FactionStacks = new StackInfo[newSize];

        for(int i =0; i < FactionStacks.Length; i++)
        {
            if(i < old.Length)
            {
                FactionStacks[i] = old[i];
            }
            else
            {
                FactionStacks[i] = new StackInfo();
                FactionStacks[i].GenerateStackID(FactionName);
            }

        }

    }
}
