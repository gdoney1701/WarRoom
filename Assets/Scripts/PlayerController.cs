using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float maxCameraHeight;
    [SerializeField]
    private float minCameraHeight;
    [SerializeField]
    private LayerMask selectMask;
    [SerializeField]
    private LayerMask orderMask;
    [SerializeField]
    private LayerMask backgroundMask;
    [SerializeField]
    private float moveModifier = 0.5f;
    [SerializeField]
    private float zoomMultiplier = 4f;

    private GameObject[] mapColumns;
    private bool readyToMove = false;
    private float maxDistance;
    private float zOffset;
    private int columnWidth;
    private Camera mainCamera;
    private float neutralHeight;
    private bool canIssueOrder = false;
    private string playerFaction;
    bool useHorizontalScroll = false;

    public delegate void SendOrder(MapTile mapTile);
    public static event SendOrder sendOrder;

    private float scrollInput = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        neutralHeight = (maxCameraHeight + minCameraHeight) / 2;
        mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, neutralHeight, mainCamera.transform.localPosition.z);
        zoom = mainCamera.transform.localPosition.y;
    }

    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += RegisterMapObjects;
        PopulateButtons.onGameReady += AllowInput;
        TurnManager.updatePhase += UpdateControls;
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        UpdatePosition();
        UpdateZoom();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SelectionManager.Instance.DeselectAll();
            SelectionManager.Instance.DeselectTile();

            RaycastHit hit;
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100f, selectMask))
            {

                if (hit.collider.TryGetComponent(out MapTile mapTile))
                {
                    SelectionManager.Instance.SelectTile(mapTile);
                }
                else if (hit.collider.TryGetComponent(out StackManager stackManager) & stackManager.OwnerID == playerFaction)
                {
                    SelectionManager.Instance.SelectUnits(stackManager);
                }
            }
            return;
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (canIssueOrder && SelectionManager.Instance.SelectedUnits.Count > 0
                && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100f, orderMask) &&
                    hit.collider.TryGetComponent(out MapTile foundTile))
                {
                    sendOrder?.Invoke(foundTile);
                }
            }
        }
    }

    private float zoom;
    private float zoomVelocity = 0f;
    private float smoothTime = 0.25f;
    [SerializeField]
    private float panSpeed = 10f;
    //private Vector3 mousePosition = Vector3.zero;

    private void UpdatePosition()
    {
        if (Input.GetMouseButtonDown(2))
        {
            panning = true;
            mousePos = Input.mousePosition;
            return;
        }
        if (Input.GetMouseButtonUp(2))
        {
            panning = false;
            return;
        }
        if (panning)
        {
            Vector3 pos = mainCamera.ScreenToViewportPoint(Input.mousePosition-mousePos);
            Debug.Log(string.Format("Mouse Pos {0}", pos));
            Vector3 move = new Vector3(pos.x, 0, pos.y) *panSpeed * (mainCamera.transform.localPosition.y/maxCameraHeight);
            MoveTiles(move);
            mousePos = Input.mousePosition;
        }
    }

    private void MoveTiles(Vector3 input)
    {
        zOffset += input.z;
        if (useHorizontalScroll)
        {
            for (int i = 0; i < mapColumns.Length; i++)
            {
                float xOffset = mapColumns[i].transform.localPosition.x + input.x;
                if (xOffset > maxDistance)
                {
                    xOffset -= maxDistance;
                }
                else if (xOffset < 0)
                {
                    xOffset += maxDistance;
                }
                mapColumns[i].transform.localPosition = new Vector3(xOffset, 0, zOffset);
            }
        }
        else
        {
            //Debug.Log(input);
            mapColumns[0].transform.Translate(input,Space.Self);

            //float xOffset = mapColumns[0].transform.localPosition.x + input.x;
            //mapColumns[0].transform.localPosition = new Vector3(xOffset, 0, zOffset);
        }
    }
    private void UpdateZoom()
    {
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        zoom -= scrollInput * zoomMultiplier * Time.deltaTime;
        zoom = Mathf.Clamp(zoom, minCameraHeight, maxCameraHeight);
        Vector3 newPosition = mainCamera.transform.localPosition;
        newPosition.y = Mathf.SmoothDamp(newPosition.y, zoom, ref zoomVelocity, smoothTime);

        if (!mainCamera.transform.localPosition.Equals(newPosition))
        {
            mainCamera.transform.localPosition = newPosition;

            //Vector3 screenCoordsMousePos = new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane);
            //worldPosTarget = mainCamera.ScreenToWorldPoint(screenCoordsMousePos);

            //var ray = mainCamera.ScreenPointToRay(screenCoordsMousePos);
            //if (Physics.Raycast(ray, out RaycastHit hit, 100f, backgroundMask))
            //{
            //    if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit cameraHit, backgroundMask))
            //    {
            //        Vector2 cameraPos = new Vector2(cameraHit.point.x, cameraHit.point.z);
            //        Vector2 mousePos = new Vector2(hit.point.x, hit.point.y);

                    


            //        Vector2 distance = new Vector2(hit.point.x - cameraHit.point.x, hit.point.z - cameraHit.point.z) * Time.deltaTime;
            //        Debug.Log(distance);
            //        MoveTiles(distance);

            //    }
            //}
        }
    }

    private void OnDisable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad -= RegisterMapObjects;
        PopulateButtons.onGameReady -= AllowInput;
        TurnManager.updatePhase -= UpdateControls;
    }

    void UpdateControls(TurnPhase turnPhase)
    {
        if(turnPhase == TurnPhase.Order)
        {
            canIssueOrder = true;
        }
        else
        {
            canIssueOrder = false;
        }
    }

    void RegisterMapObjects(MapMeshGenerator.MeshGenerationData data, SaveData loadedSave)
    {
        mapColumns = data.columnArray;
        useHorizontalScroll = loadedSave.loadedMapData.horizontalLooping;
        maxDistance = data.imageScale.x;
        columnWidth = (int)maxDistance / mapColumns.Length;
        zOffset = 0;
    }

    void AllowInput(FactionData factionData)
    {
        readyToMove = true;
        playerFaction = factionData.ID;
    }

    private Vector3 mousePos = Vector3.zero;
    private bool panning = false;
    

    private Vector3 GetRayDistance(Vector2 pos)
    {
        float height = mainCamera.transform.localPosition.y / Mathf.Cos(mainCamera.transform.localEulerAngles.x);
        return mainCamera.ScreenPointToRay(new Vector3(pos.x, pos.y, mainCamera.nearClipPlane)).GetPoint(height);
    }
}
