using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FactionLayoutEditor : EditorWindow
{

    private string fileName = "newLayout";

    private Dictionary<Vector3Int, string> loadedMapData = new Dictionary<Vector3Int, string>();
    private Texture2D mapTexture;

    private const float kZoomMin = 0.1f;
    private const float kZoomMax = 10.0f;

    private readonly Rect _zoomArea = new Rect(20.0f, 30.0f, 450f, 450f);
    private float _zoom = 1.0f;
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
            }

        }
    }

    private string selectedTag = "Z100";


    void GetTagFromColor()
    {
        Vector3Int convertedColor = new Vector3Int(selectedColor.r, selectedColor.g, selectedColor.b);
        Debug.Log(convertedColor);
        if (LoadedMapData.ContainsKey(convertedColor))
        {
            selectedTag = LoadedMapData[convertedColor];
            Repaint();
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

    [MenuItem("Tools/Faction Layout")]
    public static FactionLayoutEditor GetWindow()
    {
        return GetWindow<FactionLayoutEditor>("Faction Layout");
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
        fileName = EditorGUILayout.TextField("File Name: ", fileName);
        if(GUILayout.Button("Load Map Data"))
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                ImportMapData();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        DrawZoomArea();
        DrawNonZoomArea();
    }

    private void HandleEvents()
    {
        if(Event.current.type == EventType.ScrollWheel)
        {
            Vector2 screenCoordsMousePos = Event.current.mousePosition;
            Vector2 delta = Event.current.delta;
            Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y / 30.0f;
            float oldZoom = _zoom;
            _zoom += zoomDelta;
            _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);

            //Event.current.Use();
            Repaint();
        }

        if(Event.current.type == EventType.MouseDrag &&
            (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
            Event.current.button == 2)
        {
            Vector2 delta = Event.current.delta;
            delta /= _zoom;
            _zoomCoordsOrigin -= delta;

            //Event.current.Use();
            Repaint();
        }
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector2 clickPos = Event.current.mousePosition;
            if(IsInsideRect(clickPos, _zoomArea))
            {
                Debug.Log(string.Format("Click Position: {0}, Pixel Position {1}",clickPos, GetTextureColor(clickPos, _zoomArea)));
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

    private Vector2 GetTextureColor(Vector2 mousePos, Rect inputRect)
    {
        Vector2 adjustedPos = new Vector2(
            (mousePos.x - inputRect.x)/inputRect.width * mapTexture.width, 
            (mousePos.y - inputRect.y)/inputRect.height * mapTexture.height);


        return adjustedPos;
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
        GUILayout.BeginArea(new Rect(_zoomArea.x, _zoomArea.y + _zoomArea.height + 10f, _zoomArea.width, Screen.height - _zoomArea.height - _zoomArea.y - 10f));
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

        GUILayout.BeginArea(new Rect(_zoomArea.x + _zoomArea.width + 10.0f, _zoomArea.y, Screen.width - _zoomArea.width - 20.0f - _zoomArea.x, Screen.height));
        GUILayout.BeginVertical();
        if (GUILayout.Button("Fucking Nothing"))
        {
            Debug.Log("It Does Nothing");
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void ImportMapData()
    {
        MapColorData loadedData = new MapColorData();
        loadedData.LoadFromFile(fileName);

        if(loadedData.TileList.Count != 0)
        {
            LoadedMapData.Clear();
            for(int i = 0; i < loadedData.TileList.Count; i++)
            {
                if (!LoadedMapData.ContainsKey(loadedData.TileList[i].TileColor))
                {
                    LoadedMapData.Add(loadedData.TileList[i].TileColor, loadedData.TileList[i].TileTag);
                }
            }

            Debug.LogWarning(LoadedMapData.Count);

            mapTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(loadedData.MapTexturePath);
        }
        else
        {
            if(EditorUtility.DisplayDialog("Invalid Map Data File", "Please Re-enter the File Name String", "OK"))
            {
                fileName = string.Empty;
            }
        }
        Repaint();
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
