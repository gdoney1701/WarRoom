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

        //foreach(StackInfo stack in armyInfo.FactionStacks)
        //{
        //    GameObject tempStack = Instantiate(stackManagerPrefab);
        //    tempStack.transform.SetParent(transform);
        //    StackManager tempManager = tempStack.GetComponent<StackManager>();
        //    tempManager.UpdateVisuals(armyInfo.FactionName, stack);
        //    factionStack.Add(tempManager);
        //}
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= SpawnInitialStacks;
    }

    void SpawnInitialStacks(Dictionary<string, MapTile> inDict)
    {
        for(int i = 0; i < armyInfo.FactionStacks.Length; i++)
        {
            GameObject tempStack = Instantiate(stackPrefab);
            MapTile assignedTile = inDict[armyInfo.FactionStacks[i].TileTag];
            tempStack.name = armyInfo.FactionStacks[i].TroopID;
            tempStack.transform.SetParent(assignedTile.CenterContainer);
            tempStack.transform.localPosition = Vector3.zero;
            StackManager tempManager = tempStack.GetComponent<StackManager>();
            tempManager.UpdateVisuals(armyInfo.FactionName, armyInfo.FactionStacks[i]);
            factionStack.Add(tempManager);
        }
    }

}
