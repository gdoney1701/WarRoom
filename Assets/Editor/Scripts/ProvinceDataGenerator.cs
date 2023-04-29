using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProvinceDataGenerator : EditorWindow
{
    [System.Serializable]
    public class TileData
    {
        public Vector3Int TileColor = Vector3Int.zero;
        public string TileTag = "Z100";
    }
    [System.Serializable]
    public class MapData
    {
        public List<TileData> TileList = new List<TileData>() { new TileData() };
    }

    private MapData currentData = new MapData();
    private Vector2 scrollView = Vector2.zero;
    private string fileName = "newTileData";

    [MenuItem("Tools/Generate Tile Color Data")]
    public static ProvinceDataGenerator GetWindow()
    {
        return GetWindow<ProvinceDataGenerator>("Tile Color Data");
    }

    public static ProvinceDataGenerator OpenWindow()
    {
        var window = GetWindow();
        window.Show();
        return window;
    }

    private void OnGUI()
    {
        this.minSize = new Vector2(500, 300);
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        GUILayout.BeginHorizontal();
        fileName = EditorGUILayout.TextField("File Name: ", fileName);
        if (GUILayout.Button("Load"))
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                LoadFromFile(fileName);
            }
        }
        EditorGUI.BeginDisabledGroup(currentData == null);
        if (GUILayout.Button("Save"))
        {
            SaveToFile(fileName);
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        scrollView = GUILayout.BeginScrollView(scrollView);
        for(int i = 0; i < currentData.TileList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            currentData.TileList[i].TileTag = EditorGUILayout.TextField(currentData.TileList[i].TileTag);
            currentData.TileList[i].TileColor = 
                ColorToVector(EditorGUILayout.ColorField(
                    VectorToColor(currentData.TileList[i].TileColor))
                );
            if (GUILayout.Button("-", new GUIStyle("minibutton")))
            {
                RemoveTile(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("+", new GUIStyle("minibutton")))
        {
            AddTile();
        }
        if (GUILayout.Button("-", new GUIStyle("minibutton")))
        {
            RemoveTile(currentData.TileList.Count - 1);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void AddTile()
    {
        currentData.TileList.Add(new TileData());
        Repaint();
    }
    private void RemoveTile(int removePoint)
    {
        currentData.TileList.RemoveAt(removePoint);
        Repaint();
    }

    private Color32 VectorToColor(Vector3Int entry)
    {
        return new Color32((byte)entry.x, (byte)entry.y, (byte)entry.z, 255);
    }
    private Vector3Int ColorToVector(Color32 entry)
    {
        return new Vector3Int(entry.r, entry.g, entry.b);
    }

    private void SaveToFile(string inputName)
    {
        string data = JsonUtility.ToJson(currentData);
        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "MapData", inputName);
        System.IO.File.WriteAllText(path, data);
    }
    private MapData LoadFromFile(string inputName)
    {
        MapData result = new MapData();

        string path = string.Format("{0}/{1}/{2}.json", Application.streamingAssetsPath, "MapData", inputName);
        try
        {
            string jsonString = System.IO.File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(jsonString, result);
        }
        catch
        {
            Debug.LogError("Failed to find a file to load");
        }

        return result;
    }
}
