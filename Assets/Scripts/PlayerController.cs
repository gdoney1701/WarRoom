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
        zoom = new Vector3(0, neutralMapDist, 0);
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
            zoom.x = tileOffset.x;
            zoom.z = tileOffset.z;
            mousePos = Input.mousePosition;
        }
    }

    private void MoveTiles()
    {
        if (useHorizontalScroll)
        {
            for (int i = 0; i < mapColumns.Length; i++)
            {
                float xOffset = mapColumns[i].transform.position.x + tileOffset.x;
                if (xOffset > maxDistance)
                {
                    xOffset -= maxDistance;
                }
                else if (xOffset < 0)
                {
                    xOffset += maxDistance;
                }
                mapColumns[i].transform.position = new Vector3(xOffset, tileOffset.y, tileOffset.z);
            }
        }
        else
        {
            //mapColumns[0].transform.Translate(tileOffset,Space.Self);
            mapColumns[0].transform.position = tileOffset;
        }
    }
    public Vector3 zoom = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private float smoothTime = 0.25f;
    Vector3 zoomPosTarget = Vector3.zero;

    private void UpdateZoom()
    {
        float newScroll = Input.GetAxis("Mouse ScrollWheel");
        if (newScroll > scrollInput)
        {
            Ray mouseRay = mainCamera.ScreenPointToRay(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
            Ray centerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            zoomPosTarget = mouseRay.GetPoint(farMapDist + zoom.y) - centerRay.GetPoint(farMapDist + zoom.y);

            Debug.DrawRay(centerRay.origin, centerRay.direction, Color.red, 10f);
            Debug.DrawRay(mouseRay.origin, mouseRay.direction, Color.blue, 10f);
        }
        scrollInput = newScroll;
        float speed = scrollInput * zoomMultiplier * Time.deltaTime;

        float targetY = Mathf.Clamp(zoom.y + speed, farMapDist, closeMapDist);

        if(!Mathf.Approximately(targetY, zoom.y))
        {
            zoom = scrollInput > 0 ? 
                new Vector3(zoomPosTarget.x, targetY, zoomPosTarget.z) : 
                new Vector3(zoom.x, targetY, zoom.z);
        }
        Vector3 result = Vector3.SmoothDamp(tileOffset, zoom, ref zoomVelocity, smoothTime);

        if (tileOffset != result) tileOffset = result;
        Debug.Log(string.Format("Target {0}, Zoom {1}, TileOffset{2}, Result{3}", zoomPosTarget, zoom, tileOffset, result));
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
