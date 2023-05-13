using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    private List<StackManager> factionStack = new List<StackManager>();
    [SerializeField]
    private string armyInfoPath;
    [SerializeField]
    private GameObject stackPrefab;

    private ArmyInfo armyInfo = new ArmyInfo();

    private void OnEnable()
    {
        armyInfo.LoadFromFile(armyInfoPath);
        MapMeshGenerator.MapMeshGenerator.onMapLoad += SpawnInitialStacks;
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= SpawnInitialStacks;
    }

    void SpawnInitialStacks(MapMeshGenerator.MeshGenerationData data)
    {
        for(int i = 0; i < armyInfo.FactionStacks.Length; i++)
        {
            GameObject tempStack = Instantiate(stackPrefab);
            MapTile assignedTile = data.mapTiles[armyInfo.FactionStacks[i].TileTag];
            tempStack.name = armyInfo.FactionStacks[i].TroopID;
            tempStack.transform.SetParent(assignedTile.CenterContainer);
            tempStack.transform.localPosition = Vector3.zero;
            StackManager tempManager = tempStack.GetComponent<StackManager>();
            tempManager.UpdateVisuals(armyInfo.FactionName, armyInfo.FactionStacks[i]);
            factionStack.Add(tempManager);
        }
    }

}
