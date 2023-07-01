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
    private float maxZoomSpeed = 20f;
    [SerializeField]
    private LayerMask clickMask;

    private GameObject[] mapColumns;
    private bool readyToMove = false;
    private float maxDistance;
    private float zOffset;
    private int columnWidth;
    private Camera mainCamera;
    private float neutralHeight;

    private void Awake()
    {
        mainCamera = Camera.main;
        neutralHeight = (maxCameraHeight + minCameraHeight) / 2;
        playerInput.enabled = false;
    }

    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += RegisterMapObjects;
        PopulateButtons.onGameReady += AllowInput;
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= RegisterMapObjects;
        PopulateButtons.onGameReady -= AllowInput;
    }

    void RegisterMapObjects(MapMeshGenerator.MeshGenerationData data, SaveData loadedSave)
    {
        mapColumns = data.columnArray;
        maxDistance = data.imageScale.x;
        columnWidth = (int)maxDistance / mapColumns.Length;
        readyToMove = true;
        zOffset = 0;
    }

    void AllowInput()
    {
        playerInput.enabled = true;
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {
        if (readyToMove)
        {
            Vector2 input = context.ReadValue<Vector2>();
            mainCamera.ScreenToWorldPoint(input);

            zOffset += input.y;
            for (int i = 0; i < mapColumns.Length; i++)
            {         
                float xOffset = mapColumns[i].transform.localPosition.x + input.x;
                if(xOffset > maxDistance)
                {
                    xOffset -= maxDistance;
                }
                else if(xOffset < 0)
                {
                    xOffset += maxDistance;
                }
                mapColumns[i].transform.localPosition = new Vector3(xOffset, 0, zOffset);
               
            }
        }
    }


    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            SelectionManager.Instance.DeselectAll();
            SelectionManager.Instance.DeselectTile();

            RaycastHit hit;
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100f, clickMask))
            {

                if (hit.collider.TryGetComponent(out MapTile mapTile))
                {
                    SelectionManager.Instance.SelectTile(mapTile);
                }
                else if(hit.collider.TryGetComponent(out StackManager stackManager))
                {
                    SelectionManager.Instance.SelectUnits(stackManager);
                }
            }
        }

    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        Vector2 scrollInput = context.ReadValue<Vector2>();
        if(scrollInput.y != 0)
        {
            float heightRatio = Mathf.Abs(neutralHeight - mainCamera.transform.position.y) / (maxCameraHeight - neutralHeight);
            float scrollSpeed = Mathf.SmoothStep(maxZoomSpeed, maxZoomSpeed / 5, heightRatio) * Time.deltaTime;

            if(scrollSpeed * -scrollInput.y + mainCamera.transform.position.y > maxCameraHeight ||
                scrollSpeed * -scrollInput.y + mainCamera.transform.position.y < minCameraHeight)
            {
                return;
            }

            mainCamera.transform.Translate(new Vector3(0, -scrollInput.y * scrollSpeed, 0), Space.World);
        }

    }
}
