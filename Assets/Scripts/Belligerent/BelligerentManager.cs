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

    void InitializeFactions(MapMeshGenerator.MeshGenerationData data)
    {
        BelligerentData belligerentData = new BelligerentData();
        belligerentData.LoadFromFile(factionPath);

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
                tempManager.UpdateVisuals(VectorToColor(currentFaction.Color), currentFaction.StackArray[i]);
            }
        }
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
