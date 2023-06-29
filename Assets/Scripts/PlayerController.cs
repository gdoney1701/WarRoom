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

    public delegate void OnTileSelect(MapTile selectedTile);
    public static event OnTileSelect onTileSelect;

    private MapTile selectedTile;
    public MapTile SelectedTile
    {
        get { return selectedTile; }
        set
        {
            if(selectedTile == null | value != selectedTile)
            {
                selectedTile = value;
                onTileSelect?.Invoke(value);
            }
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        neutralHeight = (maxCameraHeight + minCameraHeight) / 2;
    }



    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += RegisterMapObjects;
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= RegisterMapObjects;
    }

    void RegisterMapObjects(MapMeshGenerator.MeshGenerationData data, SaveData loadedSave)
    {
        mapColumns = data.columnArray;
        maxDistance = data.imageScale.x;
        columnWidth = (int)maxDistance / mapColumns.Length;
        Debug.Log(columnWidth);
        readyToMove = true;
        zOffset = 0;
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
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (context.performed)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, 1 << 6))
            {
                try
                {
                    SelectedTile = hit.collider.GetComponentInParent<MapTile>();
                    Debug.Log(selectedTile.TileName);
                }
                catch
                {
                    Debug.LogError("No Viable MapTile script");
                }
            }
            else
            {
                SelectedTile = null;
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
