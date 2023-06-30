using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BelligerentManager : MonoBehaviour
{
    [SerializeField]
    private string factionPath;
    [SerializeField]
    private GameObject stackPrefab;

    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += InitializeFactions;
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= InitializeFactions;
    }

    void InitializeFactions(MapMeshGenerator.MeshGenerationData data, SaveData loadedSave)
    {
        BelligerentData belligerentData = loadedSave.saveBelligerents;

        for(int j = 0; j < belligerentData.WarParticipants.Length; j++)
        {
            FactionData currentFaction = belligerentData.WarParticipants[j];
            for(int i = 0; i < currentFaction.StackArray.Length; i++)
            {
                GameObject tempStack = Instantiate(stackPrefab);
                MapTile assignedTile = data.mapTiles[currentFaction.StackArray[i].TileTag];
                tempStack.name = currentFaction.StackArray[i].TroopLongTag;
                tempStack.transform.SetParent(assignedTile.CenterContainer);
                tempStack.transform.localPosition = Vector3.zero;

                StackManager tempManager = tempStack.GetComponent<StackManager>();
                tempManager.InitializeStack(currentFaction, i);
            }

            for(int i = 0; i < currentFaction.TileControl.Length; i++)
            {
                MapTile occupiedTile = data.mapTiles[currentFaction.TileControl[i].TileTag];
                Color32 convertedColor = currentFaction.VectorToColor();
                occupiedTile.SetOccupationVisuals(convertedColor, convertedColor);
            }
        }
    }


}
