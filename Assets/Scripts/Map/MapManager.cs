using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += DoSomething;
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= DoSomething;
    }

    void DoSomething(MapMeshGenerator.MeshGenerationData data)
    {
        Debug.Log("Did Something");
    }

}
