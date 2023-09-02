using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapColorData
{

    public List<TileData> TileList = new List<TileData>() { new TileData() };
    public string MapTexturePath = string.Empty;
    public bool horizontalLooping = false;
    public void SaveToFile(string inputName)
    {
        string data = JsonUtility.ToJson(this);
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "MapData", inputName);
        System.IO.File.WriteAllText(path, data);
    }
    public MapColorData LoadFromFile(string inputName)
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

[System.Serializable]
public class TileData
{
    public Vector3Int TileColor = Vector3Int.zero;
    public string TileTag = "Z100";
    public string TileName = "Caprica City";

    public int Stress = 1;
    public int Iron = 1;
    public int Oil = 1;
    public int OSR = 1;
}
public class ProvinceData
{
    public ProvinceData(Color32 provinceColor, EdgeVertex[] inputVertices, TileData tileData)
    {
        Data = tileData;
        ProvinceColor = provinceColor;
        EdgeVertices = inputVertices;
        VertexOrder = new List<int>();
        MaxPoint = Vector2.zero;
        MinPoint = Vector2.zero;

        CollapseEdgeVertex();
    }
    public TileData Data { get; set; }
    public Color32 ProvinceColor { get; }
    public EdgeVertex[] EdgeVertices { get; set; }
    public List<int> VertexOrder { get; set; } //Indices of EdgePixels in clockwise rotation from the top left pixel
    public string[] NeighborTags { get; set; }
    public Vector2 MaxPoint { get; set; }
    public Vector2 MinPoint { get; set; }
    public Vector2[] VertexPoints { get; set; }

    public void CollapseEdgeVertex()
    {
        VertexPoints = new Vector2[EdgeVertices.Length];
        for(int i = 0; i < EdgeVertices.Length; i++)
        {
            VertexPoints[i] = EdgeVertices[i].Pos;
        }
    }
}
public struct EdgeVertex
{
    public Vector2 Pos;
    public Color32[] EdgeColors;

    public EdgeVertex(Vector2 inPos, Color32[] inEdgeCols)
    {
        Pos = inPos;
        EdgeColors = inEdgeCols;
    }
}
