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
    public LineRenderer borderRenderer;

    private Color32 ownColor;
    private Color32[] neighborColors;

    private MapTile[] neighbors;

    public MapTile[] Neighbors
    {
        get { return neighbors; }
        set { neighbors = value; }
    }


    public void InitializePrefab(ProvinceData provinceData, Mesh msh, Material mat, Vector3 center)
    {
        centerContainer.position = center;
        gameObject.name = provinceData.Tag;
        tileTagUI.text = provinceData.Tag;
        meshFilter.mesh = msh;
        meshRenderer.material = mat;

        TileName = provinceData.Tag;

        borderRenderer.positionCount = provinceData.EdgeVertices.Length;
        for(int i = 0; i < provinceData.EdgeVertices.Length; i++)
        {
            borderRenderer.SetPosition(i, provinceData.EdgeVertices[i].Pos);
        }
    }

}
