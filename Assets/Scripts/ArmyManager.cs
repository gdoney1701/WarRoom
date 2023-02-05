using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    private List<StackManager> factionStack = new List<StackManager>();
    [SerializeField]
    private string armyInfoPath;
    [SerializeField]
    private GameObject stackManagerPrefab;

    private ArmyInfo armyInfo = new ArmyInfo();

    private void OnEnable()
    {
        armyInfo.LoadFromFile(armyInfoPath);
        foreach(StackInfo stack in armyInfo.FactionStacks)
        {
            GameObject tempStack = Instantiate(stackManagerPrefab);
            tempStack.transform.SetParent(transform);
            StackManager tempManager = tempStack.GetComponent<StackManager>();
            tempManager.UpdateVisuals(armyInfo.FactionName, stack);
            factionStack.Add(tempManager);
        }
    }

}
