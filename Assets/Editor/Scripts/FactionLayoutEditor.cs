using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FactionLayoutEditor : EditorWindow
{

    private string mapDataName = "newLayout";
    private string belligerentDataName = "newBelligerents";

    private Dictionary<Vector3Int, string> loadedMapData = new Dictionary<Vector3Int, string>();
    private Texture2D mapTexture;
    private MapColorData mapColorData = new MapColorData();
    private BelligerentData belligerentData = new BelligerentData();
    private string[] BelligerentNames = new string[0];

    private const float kZoomMin = 1.1f;
    private const float kZoomMax = 10.0f;

    private readonly Rect _zoomArea = new Rect(20.0f, 30.0f, 450f, 450f);
    private float _zoom = 1.1f;
    private Vector2 _zoomCoordsOrigin = Vector2.zero;
    private Color32 selectedColor = new Color32(255, 255, 255, 255);
    private Color32 SelectedColor
    {
        get { return selectedColor; }
        set
        {
            if (!value.Equals(selectedColor))
            {
                selectedColor = value;
                if (!value.Equals(new Color32(255, 255, 255, 255)))
                {
                    GetTagFromColor();
                }
                if(toolbarInt == 2)
                {
                    if(armyToolbarInt == 0)
                    {
                        AddSelectedOccupation();
                    }
                }
            }
        }
    }

    private string selectedTag = "Z100";

    private int toolbarInt = 0;
    private int armyToolbarInt = 0;
    private string[] toolbarStrings = { "Tag", "Faction", "Army" };
    private string[] armyToolbarStrings = { "Occupation", "Stacks" };

    private Vector2 tileScrollView = Vector2.zero;
    private Vector2 occupationScroll = Vector2.zero;
    private Vector2 stackScroll = Vector2.zero;

    private int factionIndex = 0;

    void AddSelectedOccupation()
    {
        FactionData currentData = belligerentData.WarParticipants[factionIndex];

        TileData newData = new TileData { TileColor = ColorToVector(selectedColor), TileTag = selectedTag };

        if(currentData.TileControl.Length > 0)
        {
            for(int i = 0; i < currentData.TileControl.Length; i++)
            {
                if (currentData.TileControl[i].Equals(newData))
                {
                    return;
                }
            }
            currentData.IncreaseTileArray(newData);
        }

        Repaint();
    }

    void GetTagFromColor()
    {
        Vector3Int convertedColor = ColorToVector(selectedColor);
        if (LoadedMapData.ContainsKey(convertedColor))
        {
            selectedTag = LoadedMapData[convertedColor];
            Repaint();
        }
        else
        {
            foreach(KeyValuePair<Vector3Int, string> keyValue in LoadedMapData)
            {
                Vector3Int vectorColor = ColorToVector(selectedColor);

                if( Mathf.Abs(keyValue.Key.x - vectorColor.x) <= 2 &&
                    Mathf.Abs(keyValue.Key.y - vectorColor.y) <= 2 &&
                    Mathf.Abs(keyValue.Key.z - vectorColor.z) <= 2)
                {
                    selectedColor = VectorToColor(keyValue.Key);
                    selectedTag = LoadedMapData[ColorToVector(selectedColor)];
                    Repaint();
                    break;
                }
            }
        }
    }
    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
    }

    public Dictionary<Vector3Int, string> LoadedMapData
    {
        get { return loadedMapData; }
        set { loadedMapData = value; }
    }

    [MenuItem("Tools/Tile and Faction Editor")]
    public static FactionLayoutEditor GetWindow()
    {
        return GetWindow<FactionLayoutEditor>("Tile and Faction Editor");
    }

    public static FactionLayoutEditor OpenWindow()
    {
        var window = GetWindow();
        window.Show();
        return window;
    }

    private void OnGUI()
    {
        HandleEvents();
        this.minSize = new Vector2(1000, 500);
        GUILayout.BeginVertical();
        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();

        GUILayout.Space(20f);
        EditorGUILayout.LabelField("Map Data File:", new GUIStyle("BoldLabel"), GUILayout.Width(100f));
        mapDataName = EditorGUILayout.TextField(mapDataName, GUILayout.Width(150f));
        if(GUILayout.Button("Load"))
        {
            if (!string.IsNullOrEmpty(mapDataName))
            {
                ImportMapData();
            }
        }
        if (GUILayout.Button("New"))
        {
            if (!string.IsNullOrEmpty(mapDataName))
            {
                ImportMapData(true);
            }
        }

        GUILayout.Space(20f);
        EditorGUILayout.LabelField("Belligerent Data File:", new GUIStyle("BoldLabel"), GUILayout.Width(150f));
        belligerentDataName = EditorGUILayout.TextField(belligerentDataName, GUILayout.Width(150f));
        if (GUILayout.Button("Load"))
        {
            if (!string.IsNullOrEmpty(belligerentDataName))
            {
                ImportBelligerentData();
            }
        }
        if (GUILayout.Button("New"))
        {
            if (!string.IsNullOrEmpty(belligerentDataName))
            {
                ImportBelligerentData(true);
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        DrawZoomArea();
        DrawNonZoomArea();
    }

    private void HandleEvents()
    {
        if(Event.current.type == EventType.ScrollWheel && IsInsideRect(Event.current.mousePosition, _zoomArea))
        {
            Vector2 screenCoordsMousePos = Event.current.mousePosition;
            Vector2 delta = Event.current.delta;
            Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y / 30.0f;
            float oldZoom = _zoom;
            _zoom += zoomDelta;
            _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);
            Repaint();
        }

        if(Event.current.type == EventType.MouseDrag &&
            (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
            Event.current.button == 2)
        {
            if(IsInsideRect(Event.current.mousePosition, _zoomArea))
            {
                Vector2 delta = Event.current.delta;
                delta /= _zoom;
                _zoomCoordsOrigin -= delta;

                Repaint();
            }
        }
    }

    private bool IsInsideRect(Vector2 mousePos, Rect inputRect)
    {
        if(mousePos.x >= inputRect.x && mousePos.x <= inputRect.x + inputRect.width)
        {
            if (mousePos.y >= inputRect.y && mousePos.y <= inputRect.y + inputRect.height)
            {
                return true;
            }
        }
        return false;
    }

    private void DrawZoomArea()
    {
        EditorZoomArea.Begin(_zoom, _zoomArea);

        GUILayout.BeginArea(new Rect(10.0f - _zoomCoordsOrigin.x, 10.0f - _zoomCoordsOrigin.y, 500.0f, 500.0f));
        if (mapTexture != null)
        {
            EditorGUI.DrawPreviewTexture(new Rect(0, 0, 400, 400), mapTexture);
        }
        GUILayout.EndArea();
        EditorZoomArea.End();
    }

    private void DrawNonZoomArea()
    {
        float smallBoxHeight = Screen.height - _zoomArea.height - _zoomArea.y - 10f;
        GUILayout.BeginArea(new Rect(_zoomArea.x, _zoomArea.y + _zoomArea.height + 10f, _zoomArea.width, smallBoxHeight));
        EditorGUILayout.BeginHorizontal();

        GUIStyle text = new GUIStyle("In BigTitle");
        text.fontSize = 15;
        GUI.backgroundColor = Color.black;
        EditorGUILayout.LabelField(selectedTag, text, GUILayout.Height(30f), GUILayout.Width(40f));
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
        SelectedColor = EditorGUILayout.ColorField(SelectedColor, GUILayout.Height(30f));

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(_zoomArea.x + _zoomArea.width + 10.0f, _zoomArea.y, Screen.width - _zoomArea.width - 20.0f - _zoomArea.x, _zoomArea.height + smallBoxHeight - 30f));
        if (loadedMapData.Count == 0)
        {
            EditorGUILayout.LabelField("No Loaded Map Data", new GUIStyle("IN BigTitle"), GUILayout.Height(30));
        }
        else
        {
            GUILayout.BeginVertical();
            toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);
            switch (toolbarInt)
            {
                case 0:
                    DrawTagTab();
                    break;
                case 1:
                    DrawFactionTab();
                    break;
                case 2:
                    DrawArmyTab();
                    break;

            }
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
    }

    private void DrawTagTab()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        GUILayout.BeginHorizontal();

        mapTexture = (Texture2D)EditorGUILayout.ObjectField("Map Texture", mapTexture, typeof(Texture2D), false);

        GUILayout.EndHorizontal();
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        tileScrollView = GUILayout.BeginScrollView(tileScrollView);
        for (int i = 0; i < mapColorData.TileList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            mapColorData.TileList[i].TileTag = EditorGUILayout.TextField(mapColorData.TileList[i].TileTag, GUILayout.Width(50f));
            mapColorData.TileList[i].TileColor =
                ColorToVector(EditorGUILayout.ColorField(
                    VectorToColor(mapColorData.TileList[i].TileColor))
                );
            if (GUILayout.Button("-", new GUIStyle("minibutton"), GUILayout.Width(40f)))
            {
                RemoveTile(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Save Map Data"))
        {
            mapColorData.MapTexturePath = AssetDatabase.GetAssetPath(mapTexture);
            mapColorData.SaveToFile(mapDataName);
            if (!string.IsNullOrEmpty(mapDataName))
            {
                ImportMapData();
            }
            Repaint();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", new GUIStyle("minibutton")))
        {
            AddTile();
        }
        if (GUILayout.Button("-", new GUIStyle("minibutton")))
        {
            RemoveTile(mapColorData.TileList.Count - 1);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawFactionTab()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        factionIndex = EditorGUILayout.Popup(factionIndex, BelligerentNames);

        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        FactionData selectedFaction = belligerentData.WarParticipants[factionIndex];

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Long Name: ", GUILayout.Width(80f));
        selectedFaction.LongName = EditorGUILayout.TextField(selectedFaction.LongName);
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Color: ", GUILayout.Width(80f));
        selectedFaction.Color = ColorToVector(EditorGUILayout.ColorField(VectorToColor(selectedFaction.Color)));
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID: ", GUILayout.Width(80f));
        selectedFaction.ID = EditorGUILayout.TextField(selectedFaction.ID);
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Alliance ID: ", GUILayout.Width(80f));
        selectedFaction.AllianceID = EditorGUILayout.IntField(selectedFaction.AllianceID);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save"))
        {
            belligerentData.SaveToFile(belligerentDataName);
            RefreshBelligerentPopup();
            if (!string.IsNullOrEmpty(belligerentDataName))
            {
                ImportMapData();
            }
            Repaint();
        }
        if(GUILayout.Button("New Entry"))
        {
            belligerentData.IncreaseArray();
            RefreshBelligerentPopup();
            if (!string.IsNullOrEmpty(belligerentDataName))
            {
                ImportMapData();
            }
            Repaint();
        }
        if(GUILayout.Button("Remove Current"))
        {
            belligerentData.DecreaseArray(factionIndex);
            factionIndex = 0;
            RefreshBelligerentPopup();
            Repaint();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawArmyTab()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        factionIndex = EditorGUILayout.Popup(factionIndex, BelligerentNames);

        armyToolbarInt = GUILayout.Toolbar(armyToolbarInt, armyToolbarStrings);

        switch (armyToolbarInt)
        {
            case 0:
                DrawOccupation();
                break;
            case 1:
                DrawStacks();
                break;

        }

        GUILayout.EndVertical();
    }

    private void DrawOccupation()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        occupationScroll = GUILayout.BeginScrollView(occupationScroll);
        FactionData currentFaction = belligerentData.WarParticipants[factionIndex];
        for(int i = 0; i < currentFaction.TileControl.Length; i++)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(currentFaction.TileControl[i].TileTag, new GUIStyle("In BigTitle"), GUILayout.Height(25f), GUILayout.Width(40f));

            EditorGUILayout.ColorField(VectorToColor(currentFaction.TileControl[i].TileColor), GUILayout.Height(25f));

            if (GUILayout.Button("-", GUILayout.Height(25f), GUILayout.Width(25f)))
            {
                currentFaction.DecreaseTileArray(i);
                Repaint();
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(70f)))
        {
            belligerentData.SaveToFile(belligerentDataName);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void DrawStacks()
    {

    }

    private void RefreshBelligerentPopup()
    {
        if (belligerentData.WarParticipants.Length != 0)
        {
            BelligerentNames = new string[belligerentData.WarParticipants.Length];
            for (int i = 0; i < BelligerentNames.Length; i++)
            {
                string newName = belligerentData.WarParticipants[i].LongName;
                if(belligerentData.WarParticipants[i].LongName == new FactionData().LongName)
                {
                    newName = string.Format("{0} ({1})", newName, i);
                }
                BelligerentNames[i] = newName;
            }
        }
    }

    private void ImportBelligerentData(bool newData = false)
    {
        belligerentData = new BelligerentData();
        if (!newData)
        {
            belligerentData.LoadFromFile(belligerentDataName);
        }


        if (belligerentData.WarParticipants.Length != 0)
        {
            RefreshBelligerentPopup();
        }
        else
        {
            if (EditorUtility.DisplayDialog("Invalid Belligerent Data File", "Please Re-enter the File Name String", "OK"))
            {
                belligerentDataName = string.Empty;
            }
        }
    }

    private void ImportMapData(bool newData = false)
    {
        mapColorData = new MapColorData();
        if (!newData)
        {
            mapColorData.LoadFromFile(mapDataName);
        }

        mapTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(mapColorData.MapTexturePath);

        if (mapColorData.TileList.Count != 0)
        {
            LoadedMapData.Clear();
            for(int i = 0; i < mapColorData.TileList.Count; i++)
            {
                if (!LoadedMapData.ContainsKey(mapColorData.TileList[i].TileColor))
                {
                    LoadedMapData.Add(mapColorData.TileList[i].TileColor, mapColorData.TileList[i].TileTag);
                }
            }

            mapTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(mapColorData.MapTexturePath);
        }
        else
        {
            if(EditorUtility.DisplayDialog("Invalid Map Data File", "Please Re-enter the File Name String", "OK"))
            {
                mapDataName = string.Empty;
            }
        }
        Repaint();
    }
    private void AddTile()
    {
        mapColorData.TileList.Add(new TileData());
    }
    private void RemoveTile(int removePoint)
    {
        mapColorData.TileList.RemoveAt(removePoint);
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

public class EditorZoomArea
{

    private const float kEditorWindowTabHeight = 21.0f;
    private static Matrix4x4 _prevGuiMatrix;

    public static Rect Begin(float zoomScale, Rect screenCoords)
    {
        GUI.EndGroup();

        Rect clippedArea = screenCoords.ScaleSizeBy(1.0f / zoomScale,
            screenCoords.TopLeft());
        clippedArea.y += kEditorWindowTabHeight;
        GUI.BeginGroup(clippedArea);

        _prevGuiMatrix = GUI.matrix;
        Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
        GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

        return clippedArea;

    }
    public static void End()
    {
        GUI.matrix = _prevGuiMatrix;
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, Screen.width, Screen.height));
    }
}

public static class RectExtensions
{
    public static Vector2 TopLeft(this Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }
    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale.x;
        result.xMax *= scale.x;
        result.yMin *= scale.y;
        result.yMax *= scale.y;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;

        return result;

    }
}
