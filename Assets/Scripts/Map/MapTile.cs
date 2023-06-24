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
    [SerializeField]
    private LineRenderer borderRenderer;
    [SerializeField]
    private MeshCollider meshCollider;

    private Color32 ownColor;
    private Color32[] neighborColors;

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


    public void InitializePrefab(ProvinceData provinceData, Mesh msh, Material mat, Vector3 center, Vector2 inMax, Vector2 inMin)
    {
        
        centerContainer.localPosition = center * centerContainer.localScale.x;
        gameObject.name = provinceData.Tag;

        float minDiameter = Mathf.Min(inMax.x - inMin.x, inMax.y - inMin.y);
        tileTagUI.text = provinceData.Tag;
        tileTagUI.gameObject.transform.localScale = new Vector3(minDiameter / 10f, minDiameter / 10f, minDiameter / 10f);

        meshFilter.mesh = msh;
        meshRenderer.material = mat;
        meshCollider.sharedMesh = msh;

        TileName = provinceData.Tag;

        borderRenderer.positionCount = provinceData.EdgeVertices.Length;
        for(int i = 0; i < provinceData.EdgeVertices.Length; i++)
        {
            borderRenderer.SetPosition(i, provinceData.EdgeVertices[i].Pos);
        }
    }

    public void SetSelectedVisuals(bool isSelected)
    {
        if (isSelected)
        {
            borderRenderer.material.SetFloat("_Selected", 1);
        }
        else
        {
            borderRenderer.material.SetFloat("_Selected", 0);
        }

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
}
