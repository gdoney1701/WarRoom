using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    [SerializeField]
    Transform rotateComponent;
    [SerializeField]
    float rotationSpeed = 90f;
    [SerializeField]
    GameObject disableGroup;

    bool spin = false;

    private void OnEnable()
    {
        spin = true;
        MapMeshGenerator.MapMeshGenerator.onMapLoad += DisableLoadingScreen;
    }

    private void OnDisable()
    {
        spin = false;
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= DisableLoadingScreen;
    }


    void Update()
    {
        if (spin)
        {
            rotateComponent.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime), Space.Self);
        }
    }

    void DisableLoadingScreen(MapMeshGenerator.MeshGenerationData data, SaveData saveData)
    {
        disableGroup.SetActive(false);
        spin = false;
    }
}
