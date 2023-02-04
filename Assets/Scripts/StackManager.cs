using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackManager : MonoBehaviour
{

    [SerializeField]
    private GameObject landVisual;

    private StackInfo stackInfo;
    private GameObject[] stackVisuals;

    private string stackID;


    private void Awake()
    {
        UpdateVisuals();
    }
    private void UpdateVisuals()
    {
        int stackTotal = stackInfo.GetStackTotal();
        stackVisuals = new GameObject[stackTotal];


    }
}
