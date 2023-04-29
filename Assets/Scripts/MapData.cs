using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    [System.Serializable]
    public class TileData
    {
        public Vector3Int TileColor = Vector3Int.zero;
        public string TileTag = "Z100";
    }

    public List<TileData> TileList = new List<TileData>() { new TileData() };

    public void SaveToFile(string inputName)
    {
        string data = JsonUtility.ToJson(this);
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "MapData", inputName);
        System.IO.File.WriteAllText(path, data);
    }
    public MapData LoadFromFile(string inputName)
    {
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "MapData", inputName);
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
}
