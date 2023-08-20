using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FactionLayoutEditor : EditorWindow
{

    //private string mapDataName = "EarthMap_01_Demo";
    //private string belligerentDataName = "EarthMap_01_Bell";
    private Dictionary<Vector3Int, string> loadedMapData = new Dictionary<Vector3Int, string>();
    private Texture2D mapTexture;
    //private MapColorData mapColorData = new MapColorData();
    //private BelligerentData belligerentData = new BelligerentData();
    private SaveData saveData = new SaveData();
    private string[] BelligerentNames = new string[0];

    private const float kZoomMin = 1.1f;
    private const float kZoomMax = 10.0f;

    private readonly Rect _zoomArea = new Rect(20.0f, 30.0f, 450f, 450f);
    private float _zoom = 1.1f;
    private Vector2 _zoomCoordsOrigin = Vector2.zero;
    private Color32 selectedColor = new Color32(255, 255, 255, 255);
    private Sprite factionIcon;
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
                    if(armyToolbarInt == 1)
                    {
                        AddSelectedStack();
                    }
                }
            }
        }
    }

    private string selectedTag = "Z100";

    private int toolbarInt = 0;
    private int armyToolbarInt = 0;
    private string[] toolbarStrings = { "Tag", "Belligerent", "Faction" };
    private string[] armyToolbarStrings = { "Occupation", "Stacks" };

    private string saveBelligerentPath = "EarthMap_01_Bell";
    private string saveDataPath = "SaveData_01";
    private string saveMapPath = "EarthMap_01_Demo";

    private Vector2 tileScrollView = Vector2.zero;
    private Vector2 occupationScroll = Vector2.zero;
    private Vector2 stackScroll = Vector2.zero;

    private int factionIndex = 0;
    private int FactionIndex
    {
        get { return factionIndex; }
        set
        {
            if(factionIndex != value)
            {
                factionIndex = value;
                LoadFactionIcon();
            }

        }
    }


    void AddSelectedStack()
    {
        FactionData currentData = saveData.saveBelligerents.WarParticipants[FactionIndex];

        StackData newStack = new StackData("57");
        newStack.TileTag = selectedTag;

        currentData.IncreaseStackArray(newStack);
        Repaint();
    }
    void AddSelectedOccupation()
    {
        FactionData currentData = saveData.saveBelligerents.WarParticipants[FactionIndex];

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
        EditorGUILayout.LabelField("Save Data File:", new GUIStyle("BoldLabel"), GUILayout.Width(100f));
        saveDataPath = EditorGUILayout.TextField(saveDataPath, GUILayout.Width(150f));
        if(GUILayout.Button("Load"))
        {
            if (!string.IsNullOrEmpty(saveDataPath))
            {
                //REPLACE saveData
                ImportSaveData();
            }
        }
        if (GUILayout.Button("New"))
        {
            if (!string.IsNullOrEmpty(saveDataPath))
            {
                //REPLACE saveData
                ImportSaveData(true);
            }
        }
        if (GUILayout.Button("Save"))
        {
            saveData.SaveToFile(saveDataPath);
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
                //case 3:
                //    DrawSaveTab();
                //    break;

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
        for (int i = 0; i < saveData.loadedMapData.TileList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            saveData.loadedMapData.TileList[i].TileTag = EditorGUILayout.TextField(saveData.loadedMapData.TileList[i].TileTag, GUILayout.Width(40f));
            saveData.loadedMapData.TileList[i].TileName = EditorGUILayout.TextField(saveData.loadedMapData.TileList[i].TileName, GUILayout.Width(85f));

            saveData.loadedMapData.TileList[i].TileColor = 
                ColorToVector(
                    EditorGUILayout.ColorField(
                        VectorToColor(saveData.loadedMapData.TileList[i].TileColor)
                        )
                    );

            saveData.loadedMapData.TileList[i].Stress = EditorGUILayout.IntField(saveData.loadedMapData.TileList[i].Stress, GUILayout.Width(30));
            GUI.backgroundColor = Color.yellow;
            saveData.loadedMapData.TileList[i].OSR = EditorGUILayout.IntField(saveData.loadedMapData.TileList[i].OSR, GUILayout.Width(30));
            GUI.backgroundColor = Color.blue;
            saveData.loadedMapData.TileList[i].Iron = EditorGUILayout.IntField(saveData.loadedMapData.TileList[i].Iron, GUILayout.Width(30));
            GUI.backgroundColor = Color.red;
            saveData.loadedMapData.TileList[i].Oil = EditorGUILayout.IntField(saveData.loadedMapData.TileList[i].Iron, GUILayout.Width(30));
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("-", new GUIStyle("minibutton"), GUILayout.Width(40f)))
            {
                RemoveTile(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", new GUIStyle("minibutton")))
        {
            AddTile();
        }
        if (GUILayout.Button("-", new GUIStyle("minibutton")))
        {
            RemoveTile(saveData.loadedMapData.TileList.Count - 1);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawFactionTab()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        FactionIndex = EditorGUILayout.Popup(FactionIndex, BelligerentNames);

        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        FactionData selectedFaction = saveData.saveBelligerents.WarParticipants[FactionIndex];

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

        factionIcon = (Sprite)EditorGUILayout.ObjectField("Faction Icon:", factionIcon, typeof(Sprite), false);

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("New Entry"))
        {
            saveData.saveBelligerents.IncreaseArray();
            RefreshBelligerentPopup();
            Repaint();
        }
        if(GUILayout.Button("Remove Current"))
        {
            saveData.saveBelligerents.DecreaseArray(FactionIndex);
            FactionIndex = 0;
            RefreshBelligerentPopup();
            Repaint();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawArmyTab()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        FactionIndex = EditorGUILayout.Popup(FactionIndex, BelligerentNames);

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
        FactionData currentFaction = saveData.saveBelligerents.WarParticipants[FactionIndex];
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
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void DrawStacks()
    {
        FactionData currentFaction = saveData.saveBelligerents.WarParticipants[FactionIndex];
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        stackScroll = GUILayout.BeginScrollView(stackScroll);

        for(int i = 0; i < currentFaction.StackArray.Length; i++)
        {
            StackData currentStack = currentFaction.StackArray[i];
            GUILayout.BeginHorizontal();

            currentStack.TileTag = EditorGUILayout.TextField(currentStack.TileTag, GUILayout.Width(50f));
            currentStack.StackZone = (StackData.StackType)EditorGUILayout.EnumPopup(currentStack.StackZone);
            currentStack.TroopNumberID = EditorGUILayout.TextField(currentStack.TroopNumberID, GUILayout.Width(50f));
            GUILayout.Space(10f);

            GUI.backgroundColor = Color.red;
            currentStack.RedTroopCount = EditorGUILayout.IntField(currentStack.RedTroopCount, GUILayout.Width(35f));
            GUI.backgroundColor = Color.green;
            currentStack.GreenTroopCount = EditorGUILayout.IntField(currentStack.GreenTroopCount, GUILayout.Width(35f));
            GUI.backgroundColor = Color.blue;
            currentStack.BlueTroopCount = EditorGUILayout.IntField(currentStack.BlueTroopCount, GUILayout.Width(35f));
            GUI.backgroundColor = Color.yellow;
            currentStack.YellowTroopCount = EditorGUILayout.IntField(currentStack.YellowTroopCount, GUILayout.Width(35f));

            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("-"))
            {
                currentFaction.DecreateStackArray(i);
                Repaint();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add"))
        {
            currentFaction.IncreaseStackArray(new StackData(currentFaction.ID));
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawSaveTab()
    {
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save File Path:", new GUIStyle("BoldLabel"));
        saveDataPath = EditorGUILayout.TextField(saveDataPath);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Belligerent Data:", new GUIStyle("BoldLabel"));
        saveBelligerentPath = EditorGUILayout.TextField(saveBelligerentPath);
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Map Data:", new GUIStyle("BoldLabel"));
        saveMapPath = EditorGUILayout.TextField(saveMapPath);
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save"))
        {
            saveData = new SaveData();
            saveData.saveName = saveMapPath;
            saveData.SwapLoadedBelligerentData(saveBelligerentPath);
            saveData.SwapLoadedMapData(saveMapPath);
            saveData.SaveToFile(saveDataPath);
        }

        if (GUILayout.Button("Load"))
        {
            saveData = new SaveData();
            saveData.LoadFromFile(saveDataPath);
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void RefreshBelligerentPopup()
    {
        if (saveData.saveBelligerents.WarParticipants.Length != 0)
        {
            BelligerentNames = new string[saveData.saveBelligerents.WarParticipants.Length];
            for (int i = 0; i < BelligerentNames.Length; i++)
            {
                string newName = saveData.saveBelligerents.WarParticipants[i].LongName;
                if(saveData.saveBelligerents.WarParticipants[i].LongName == new FactionData().LongName)
                {
                    newName = string.Format("{0} ({1})", newName, i);
                }
                BelligerentNames[i] = newName;
            }
        }
    }

    private void LoadFactionIcon()
    {
        string path = saveData.saveBelligerents.WarParticipants[FactionIndex].IconPath;
        if (string.IsNullOrEmpty(path))
        {
            factionIcon = null;
            return;
        }

        factionIcon = (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
        Repaint();
    }

    private void ImportSaveData(bool newData = false)
    {
        saveData = new SaveData();
        if (!newData)
        {
            saveData.LoadFromFile(saveDataPath);

        }

        mapTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(saveData.loadedMapData.MapTexturePath);

        if (saveData.loadedMapData.TileList.Count != 0)
        {
            LoadedMapData.Clear();
            for(int i = 0; i < saveData.loadedMapData.TileList.Count; i++)
            {
                if (!LoadedMapData.ContainsKey(saveData.loadedMapData.TileList[i].TileColor))
                {
                    LoadedMapData.Add(saveData.loadedMapData.TileList[i].TileColor, saveData.loadedMapData.TileList[i].TileTag);
                }
            }

            mapTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(saveData.loadedMapData.MapTexturePath);
        }

        if (saveData.saveBelligerents.WarParticipants.Length != 0)
        {
            RefreshBelligerentPopup();
            LoadFactionIcon();
        }
        Repaint();
    }
    private void AddTile()
    {
        saveData.loadedMapData.TileList.Add(new TileData());
    }
    private void RemoveTile(int removePoint)
    {
        saveData.loadedMapData.TileList.RemoveAt(removePoint);
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
