using UnityEngine;
using TMPro;

public class MapTile : MonoBehaviour
{
    public string TileName = "A1";

    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private TextMeshProUGUI tileTagUI;
    [SerializeField]
    private Transform centerContainer;
    //[SerializeField]
    //private LineRenderer borderRenderer;
    [SerializeField]
    private MeshCollider meshCollider;

    private Color32 ownColor;
    private Color32[] neighborColors;
    private Bounds meshBounds;

    public Bounds MeshBounds
    {
        get { return meshBounds; }
        set
        {
            meshBounds = value;
        }
    }

    private MapTile[] neighbors;

    public MapTile[] Neighbors
    {
        get { return neighbors; }
        set { neighbors = value; }
    }

    public Transform CenterContainer
    {
        get { return centerContainer; }
    }


    public void InitializePrefab(ProvinceData provinceData, Mesh msh, Material mat, Bounds bounds)
    {
        gameObject.name = provinceData.Tag;
        float minDiameter = Mathf.Min(bounds.size.x, bounds.size.z);
        MeshBounds = bounds;

        tileTagUI.text = provinceData.Tag;
        tileTagUI.gameObject.transform.localScale = new Vector3(minDiameter / 10f, minDiameter / 10f, minDiameter / 10f);

        meshFilter.mesh = msh;
        meshRenderer.material = mat;
        meshCollider.sharedMesh = msh;
        TileName = provinceData.Tag;
    }

    public void InitializeSDFValues(Vector3 center, Texture2D sdf)
    {
        centerContainer.localPosition = center * centerContainer.localScale.x;
        meshRenderer.material.SetTexture("_BorderTex", sdf);

    }

    public void OnSelect()
    {
        meshRenderer.material.SetFloat("_IsSelected", 1);
    }

    public void OnDeselect()
    {
        meshRenderer.material.SetFloat("_IsSelected", 0);
    }

    public void SetOccupationVisuals(Color32 backgroundColor, Color32 secondaryColor)
    {
        meshRenderer.material.SetColor("_BaseColor", backgroundColor);
        if (!backgroundColor.Equals(secondaryColor))
        {
            meshRenderer.material.SetColor("_SecondaryColor", secondaryColor);
        }
        else
        {
            meshRenderer.material.SetColor("_SecondaryColor", backgroundColor);
        }
    }

    public void OnOrderTarget()
    {

    }
}
