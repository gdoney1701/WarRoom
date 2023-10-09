using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float farMapDist;
    [SerializeField]
    private float closeMapDist;
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
    //private float zOffset;
    private int columnWidth;
    private Camera mainCamera;
    private float neutralMapDist;
    private bool canIssueOrder = false;
    private string playerFaction;
    bool useHorizontalScroll = false;

    public delegate void SendOrder(MapTile mapTile);
    public static event SendOrder sendOrder;

    private float scrollInput = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        neutralMapDist = (farMapDist + closeMapDist) / 2;
        tileOffset = new Vector3(0, neutralMapDist, 0);
        zoom = neutralMapDist;
    }

    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += RegisterMapObjects;
        PopulateButtons.onGameReady += AllowInput;
        TurnManager.updatePhase += UpdateControls;
        mainCamera = Camera.main;
    }
    public Vector3 tileOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (!readyToMove)
        {
            return;
        }
        UpdateZoom();
        UpdatePosition();
        MoveTiles();
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
            tileOffset += new Vector3(pos.x, 0, pos.y) * panSpeed * (tileOffset.y/farMapDist)/** (mainCamera.transform.localPosition.y/maxCameraHeight)*/;
            mousePos = Input.mousePosition;
        }
    }

    private void MoveTiles()
    {
        if (useHorizontalScroll)
        {
            for (int i = 0; i < mapColumns.Length; i++)
            {
                float xOffset = mapColumns[i].transform.localPosition.x + tileOffset.x;
                if (xOffset > maxDistance)
                {
                    xOffset -= maxDistance;
                }
                else if (xOffset < 0)
                {
                    xOffset += maxDistance;
                }
                mapColumns[i].transform.localPosition = new Vector3(xOffset, tileOffset.y, tileOffset.z);
            }
        }
        else
        {
            //mapColumns[0].transform.Translate(tileOffset,Space.Self);
            mapColumns[0].transform.position = tileOffset;
        }
    }
    public float zoom;
    private float zoomVelocity = 0f;
    private float smoothTime = 0.25f;
    Ray mouseRay = new Ray();

    private void UpdateZoom()
    {
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        //if(scrollInput == 0)
        //{
        //    mouseRay = mainCamera.ScreenPointToRay(
        //        new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
        //}

        zoom += scrollInput * zoomMultiplier * Time.deltaTime;
        zoom = Mathf.Clamp(zoom, farMapDist, closeMapDist);
        tileOffset.y = Mathf.SmoothDamp(tileOffset.y, zoom, ref zoomVelocity, smoothTime);

        //if (!mainCamera.transform.localPosition.Equals(newPosition))
        //{
        //    mainCamera.transform.localPosition = newPosition;
        //}
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
