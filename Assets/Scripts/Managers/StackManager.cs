using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StackManager : MonoBehaviour
{

    [SerializeField]
    private GameObject landVisual;
    [SerializeField]
    private Transform visualParent;
    [SerializeField]
    private TextMeshProUGUI stackID;
    [SerializeField]
    private BoxCollider stackCollider;

    private string stackLongTag;

    public string StackLongTag
    {
        get { return stackLongTag; }
    }

    private StackData localData;
    public StackData LocalData
    {
        get { return localData; }
    }

    public string ownerID;
    public string OwnerID
    {
        get { return ownerID; }
    }

    private GameObject[] stackVisuals;

    public void UpdateVisuals(Color32 factionColor)
    {
        int stackTotal = localData.GetStackTotal();
        stackVisuals = new GameObject[stackTotal+1];
        int counter = 0;
        for(int i = 0; i < localData.YellowTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.yellow);
        }

        for (int i = 0; i < localData.BlueTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.blue);
        }

        for (int i = 0; i < localData.GreenTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.green);
        }

        for (int i = 0; i < localData.RedTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.red);
        }

        TroopClassLoop(counter, factionColor);
        stackID.SetText(localData.TroopNumberID);
        stackID.gameObject.transform.Translate(0, counter * 0.25f + 0.125f, 0, Space.World);

        UpdateCollider(stackTotal + 1) ;
    }

    private void UpdateCollider(int stackHeight)
    {
        float tileHeight = landVisual.transform.localScale.y * stackHeight;
        float halfHeight = tileHeight / 2;
        stackCollider.size = new Vector3(landVisual.transform.localScale.x, tileHeight, landVisual.transform.localScale.z);
        stackCollider.center = new Vector3(0, halfHeight, 0);
    }

    private void TroopClassLoop(int overallCounter, Color troopColor)
    {
        GameObject tempVisual = Instantiate(landVisual, visualParent);
        tempVisual.transform.localPosition = Vector3.zero;
        tempVisual.transform.Translate(0, 0.25f * overallCounter + 0.125f, 0);
        tempVisual.GetComponent<Renderer>().material.color = troopColor;
        stackVisuals[overallCounter] = tempVisual;
    }

    public void InitializeStack(FactionData faction, int stackDataIndex)
    {
        ownerID = faction.ID;
        localData = faction.StackArray[stackDataIndex];
        stackLongTag = localData.TroopLongTag;

        UpdateVisuals(faction.VectorToColor());
    }

}
