using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ArmyLayoutEditor : EditorWindow
{

    private ArmyInfo activeData = new ArmyInfo();
    public ArmyInfo ActiveData
    {
        get { return activeData; }
        set { activeData = value; }
    }
    private string fileName = "newLayout";

    private Vector2 scrollView = Vector2.zero;
    private MapTile selectedTile;

    [MenuItem("Tools/Army Layout")]
    public static ArmyLayoutEditor GetWindow()
    {
        return GetWindow<ArmyLayoutEditor>("Army Layout");
    }

    public static ArmyLayoutEditor OpenWindow()
    {
        var window = GetWindow();
        window.Show();
        return window;
    }

    private void OnGUI()
    {
        this.minSize = new Vector2(500, 300);
        GUILayout.BeginVertical(new GUIStyle("GroupBox"));
        scrollView = GUILayout.BeginScrollView(scrollView);
        GUILayout.BeginHorizontal();
        fileName = EditorGUILayout.TextField("File Name: ", fileName);
        if (GUILayout.Button("Load"))
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                ActiveData.LoadFromFile(fileName);
            }
        }
        EditorGUI.BeginDisabledGroup(ActiveData == null);
        if (GUILayout.Button("Save"))
        {
            ActiveData.SaveToFile(fileName);
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical(new GUIStyle("GroupBox")); 
        ActiveData.FactionName = (ArmyInfoStatic.Faction)EditorGUILayout.EnumPopup("Faction Name: ", ActiveData.FactionName);
        if(GUILayout.Button("Add New Stack to Selected Tile"))
        {
            AddNewStack();
        }
        
        for(int i = 0; i < ActiveData.FactionStacks.Length; i++)
        {
            StackInfo stack = ActiveData.FactionStacks[i];
            GUILayout.BeginVertical(new GUIStyle("GroupBox"));
            GUILayout.BeginHorizontal();
            stack.TroopID = EditorGUILayout.TextField("Troop ID: ", stack.TroopID);
            if (GUILayout.Button("Generate"))
            {
                stack.GenerateStackID(ActiveData.FactionName);
            }
            GUILayout.EndHorizontal();
            stack.LocationCode = EditorGUILayout.TextField("Location: ", stack.LocationCode);

            stack.StackZone = (ArmyInfoStatic.CombatZone)EditorGUILayout.EnumPopup("Combat Zone: ", stack.StackZone);
            GUILayout.Space(10);
            stack.RedTroopCount = EditorGUILayout.IntField("Red Troops: ", stack.RedTroopCount);
            stack.GreenTroopCount = EditorGUILayout.IntField("Green Troops: ", stack.GreenTroopCount);
            stack.BlueTroopCount = EditorGUILayout.IntField("Blue Troops: ", stack.BlueTroopCount);
            stack.YellowTroopCount = EditorGUILayout.IntField("Yellow Troops: ", stack.YellowTroopCount);
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        if (selectedTile != null)
        {
            GUILayout.Label(selectedTile.TileName);
        }
        else
        {
            GUILayout.Label("No Selected Tile");
        }

        GUILayout.EndVertical();

    }

    private void OnSelectionChange()
    {
        try
        {
            selectedTile = Selection.activeGameObject.GetComponentInParent<MapTile>();
            Repaint();
        }
        catch
        {
            selectedTile = null;
            Repaint();
        }
    }

    private void AddNewStack()
    {
        ActiveData.ResizeFactionArray(ActiveData.FactionStacks.Length + 1);
        ActiveData.FactionStacks[ActiveData.FactionStacks.Length - 1].LocationCode = selectedTile.TileName;
        Repaint();
    }

}
