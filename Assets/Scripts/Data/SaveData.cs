using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public BelligerentData saveBelligerents = new BelligerentData();

    public MapColorData loadedMapData = new MapColorData();

    public string saveName = "Default";

    public void SaveToFile(string inputName)
    {
        saveBelligerents.GenerateLongID();

        string data = JsonUtility.ToJson(this, true);
        string path  = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "SaveData", inputName);
        File.WriteAllText(path, data);
    }

    public SaveData LoadFromFile(string inputName)
    {
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "SaveData", inputName);
        try
        {
            string jsonString = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonString, this);
        }
        catch
        {
            Debug.LogError(string.Format("Failed to find file at path {0}", path));
        }
        return this;
    }

    public void SwapLoadedMapData(string path)
    {
        loadedMapData = new MapColorData();
        loadedMapData.LoadFromFile(path);
    }

    public void SwapLoadedBelligerentData(string path)
    {
        saveBelligerents = new BelligerentData();
        saveBelligerents.LoadFromFile(path);
    }
}
