using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    private float maxCameraHeight;
    [SerializeField]
    private float minCameraHeight;
    [SerializeField]
    private float moveSpeed;

    private GameObject[] mapColumns;
    private bool readyToMove = false;
    private float maxDistance;

    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += RegisterMapObjects;
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= RegisterMapObjects;
    }

    void RegisterMapObjects(MapMeshGenerator.MeshGenerationData data)
    {
        mapColumns = data.columnArray;
        maxDistance = data.imageTexture.width;
        readyToMove = true;
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>().normalized);
        if (readyToMove)
        {
            Vector2 input = context.ReadValue<Vector2>().normalized * moveSpeed * Time.deltaTime;
            for(int i = 0; i < mapColumns.Length; i++)
            {
                Transform mapTransform = mapColumns[i].transform;
                mapTransform.Translate(new Vector3(input.x, 0, 0), Space.Self);
                if(mapTransform.position.x < 0)
                {
                    float offset = mapTransform.position.x;
                    mapTransform.position = new Vector3(maxDistance + offset, 0, mapTransform.localPosition.z);
                }
                else if(mapTransform.localPosition.x > maxDistance)
                {
                    float offset = mapTransform.localPosition.x - maxDistance;
                    mapTransform.localPosition = new Vector3(offset, 0, mapTransform.localPosition.z);
                }

                //mapTransform.Translate(new Vector3(0, 0, input.y), Space.Self);
            }
        }
    }


    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Clicked");
        }

    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>().normalized);
    }
}
