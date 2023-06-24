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

    private string stackLongTag;

    public string StackLongTag
    {
        get { return stackLongTag; }
    }

    private GameObject[] stackVisuals;

    public void UpdateVisuals(Color32 factionColor, StackData inputStack)
    {
        int stackTotal = inputStack.GetStackTotal();
        stackVisuals = new GameObject[stackTotal+1];
        int counter = 0;
        for(int i = 0; i < inputStack.YellowTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.yellow);
        }

        for (int i = 0; i < inputStack.BlueTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.blue);
        }

        for (int i = 0; i < inputStack.GreenTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.green);
        }

        for (int i = 0; i < inputStack.RedTroopCount; i++, counter++)
        {
            TroopClassLoop(counter, Color.red);
        }

        TroopClassLoop(counter, factionColor);
        stackID.SetText(inputStack.TroopNumberID);
        stackID.gameObject.transform.Translate(0, counter * 0.25f + 0.125f, 0, Space.World);
    }

    private void TroopClassLoop(int overallCounter, Color troopColor)
    {
        GameObject tempVisual = Instantiate(landVisual, visualParent);
        tempVisual.transform.localPosition = Vector3.zero;
        tempVisual.transform.Translate(0, 0.25f * overallCounter + 0.125f, 0);
        tempVisual.GetComponent<Renderer>().material.color = troopColor;
        stackVisuals[overallCounter] = tempVisual;
    }


}
