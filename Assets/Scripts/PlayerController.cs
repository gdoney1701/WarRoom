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

    private GameObject[] mapColumns;
    private bool readyToMove = false;
    private float maxDistance;
    private Camera mainCamera;
    private float neutralMapDist;
    private bool canIssueOrder = false;
    private string playerFaction;
    bool useHorizontalScroll = false;
    private float columnWidth = 0f;

    public delegate void SendOrder(MapTile mapTile);
    public static event SendOrder sendOrder;

    private void Awake()
    {
        mainCamera = Camera.main;
        neutralMapDist = (farMapDist + closeMapDist) / 2;
        tileOffset = new Vector3(0, neutralMapDist, 0);
        //mouseWorldPosStart = GetPerspectivePos(new Vector3(Screen.width/2, Screen.height/2, 0));
    }

    private void OnEnable()
    {
        MapMeshGenerator.MapMeshGenerator.onMapLoad += RegisterMapObjects;
        PopulateButtons.onGameReady += AllowInput;
        TurnManager.updatePhase += UpdateControls;
        mainCamera = Camera.main;
        mouseWorldPosStart = GetPerspectivePos();
    }
    public Vector3 tileOffset = Vector3.zero;

    private void LateUpdate()
    {
        Vector3 tempOffset = tileOffset;
        if (!readyToMove)
        {
            return;
        }
        UpdateZoomPosition(Input.GetAxis("Mouse ScrollWheel"));
        UpdatePosition();

        if (tileOffset != tempOffset)
        {
            MoveTiles();
        }

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
            Vector3 pos = Vector2.ClampMagnitude(
                new Vector2((Input.mousePosition.x - mousePos.x) / mainCamera.pixelWidth, 
                (Input.mousePosition.y - mousePos.y) / mainCamera.pixelHeight), 1f);

            tileOffset += new Vector3(pos.x, 0, pos.y) * panSpeed * (mapColumns[0].transform.localPosition.y/farMapDist);
            mousePos = Input.mousePosition;
        }
    }

    private Vector3 mouseWorldPosStart = Vector3.zero;
    [SerializeField]
    private float zoomSpeed = 10f;
    [SerializeField]
    private float zoomMin = 0.1f;
    [SerializeField]
    private float zoomMax = 10.0f;
    private void UpdateZoomPosition(float mouseScroll)
    {
        if(mouseScroll == 0)
        {
            return;
        }

        mouseWorldPosStart = GetPerspectivePos();
        mainCamera.transform.position = new Vector3(0, Mathf.Clamp(mainCamera.transform.position.y - mouseScroll * zoomSpeed, zoomMin, zoomMax), 0);
        Vector3 testCase = GetPerspectivePos();
        Vector3 mouseWorldPosDiff = mouseWorldPosStart - testCase;

        tileOffset -= new Vector3(mouseWorldPosDiff.x, 0, mouseWorldPosDiff.z) ;
    }

    private Vector3 GetPerspectivePos()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(mainCamera.transform.forward, 0.0f);
        float dist;
        plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);
    }

    private void MoveTiles()
    {
        if (useHorizontalScroll)
        {
            for (int i = 0; i < mapColumns.Length; i++)
            {
                float xOffset = tileOffset.x + (columnWidth * i);
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
            mapColumns[0].transform.position = tileOffset;
            //mapColumns[0].transform.localPosition = new Vector3(0, zoomPos, 0);
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
        columnWidth = mapColumns.Length > 1 ? mapColumns[1].transform.position.x - mapColumns[0].transform.position.x : 0;
        foreach (GameObject column in mapColumns)
        {
            column.transform.Translate(new Vector3(0, neutralMapDist, 0), Space.Self);
        }
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
