using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProvinceDataGenerator : EditorWindow
{
    //[System.Serializable]
    //public class TileData
    //{
    //    public Vector3Int TileColor = Vector3Int.zero;
    //    public string TileTag = "Z100";
    //}
    //[System.Serializable]
    //public class MapData
    //{
    //    public List<TileData> TileList = new List<TileData>() { new TileData() };
    //}

    private MapData currentData = new MapData();

    public MapData CurrentData
    {
        get { return currentData; }
        set { currentData = value; }
    }
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
                CurrentData.LoadFromFile(fileName);
                //Repaint();
            }
        }
        EditorGUI.BeginDisabledGroup(CurrentData == null);
        if (GUILayout.Button("Save"))
        {
            CurrentData.SaveToFile(fileName);
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        scrollView = GUILayout.BeginScrollView(scrollView);
        for(int i = 0; i < CurrentData.TileList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            CurrentData.TileList[i].TileTag = EditorGUILayout.TextField(CurrentData.TileList[i].TileTag);
            CurrentData.TileList[i].TileColor = 
                ColorToVector(EditorGUILayout.ColorField(
                    VectorToColor(CurrentData.TileList[i].TileColor))
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
            RemoveTile(CurrentData.TileList.Count - 1);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void AddTile()
    {
        CurrentData.TileList.Add(new MapData.TileData());
        Repaint();
    }
    private void RemoveTile(int removePoint)
    {
        CurrentData.TileList.RemoveAt(removePoint);
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


}
