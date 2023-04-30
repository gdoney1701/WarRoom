using UnityEngine;
using TMPro;

public class MapTileInfo : MonoBehaviour
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


    public void InitializePrefab(ProvinceData provinceData, Mesh msh, Material mat, Vector3 center)
    {
        centerContainer.position = center;
        gameObject.name = provinceData.Tag;
        tileTagUI.text = provinceData.Tag;
        meshFilter.mesh = msh;
        meshRenderer.material = mat;

        TileName = provinceData.Tag;
    } 

}
