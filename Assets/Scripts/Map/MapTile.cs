using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
    private TextMeshProUGUI tileLongName;
    [SerializeField]
    private Transform centerContainer;
    [SerializeField]
    private GameObject pooledTileResource;
    [SerializeField]
    private Transform resourceParent;
    [SerializeField]
    private MeshCollider meshCollider;
    [SerializeField]
    private Transform stackContainer;
    [SerializeField]
    private FactionCoinVisuals coinVisuals;

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
    public Transform StackContaienr
    {
        get { return stackContainer; }
    }

    private List<StackManager> localStacks = new List<StackManager>();
    public List<StackManager> LocalStacks
    {
        get { return localStacks; }
    }


    public void InitializePrefab(ProvinceData provinceData, Mesh msh, Material mat, Bounds bounds)
    {
        gameObject.name = provinceData.Data.TileTag;
        float minDiameter = Mathf.Min(bounds.size.x, bounds.size.z);
        MeshBounds = bounds;

        //tileTagUI.fontSize = 10f;
        tileLongName.text = provinceData.Data.TileName;
        tileLongName.fontSize = minDiameter / 15f;

        tileTagUI.text = provinceData.Data.TileTag;
        tileTagUI.rectTransform.Translate(new Vector3(0, -minDiameter / 10f, 0));

        //tileTagUI.gameObject.transform.localScale = new Vector3(minDiameter / 10f, minDiameter / 10f, minDiameter / 10f);

        meshFilter.mesh = msh;
        meshRenderer.material = mat;
        meshCollider.sharedMesh = msh;
        TileName = provinceData.Data.TileTag;

        if (provinceData.Data.Stress != 0)
        {
            InitializeResources(Color.white, provinceData.Data.Stress);
        }
        if (provinceData.Data.OSR != 0)
        {
            InitializeResources(Color.yellow, provinceData.Data.OSR);
        }
        if (provinceData.Data.Iron != 0)
        {
            InitializeResources(Color.blue, provinceData.Data.Iron);
        }
        if (provinceData.Data.Oil != 0)
        {
            InitializeResources(Color.red, provinceData.Data.Oil);
        }

    }
    public void AddLocalStack(StackManager newStack)
    {
        if (localStacks.Contains(newStack))
        {
            return;
        }
        localStacks.Add(newStack);
    }
    public void RemoveLocalStack(StackManager oldStack)
    {
        if (localStacks.Contains(oldStack))
        {
            localStacks.Remove(oldStack);
        }
    }

    List<FactionCoinVisuals> factionCoins = new List<FactionCoinVisuals>();
    public void AddFactionCoins(FactionData factionData)
    {
        if(localStacks.Count == 0)
        {
            return;
        }
        var coinVisual = Instantiate(coinVisuals, stackContainer);

        factionCoins.Add(coinVisual);
        RepositionFactionCoins();

        coinVisual.ModifyCoinVisuals(factionData, localStacks.ToArray());
    }

    private void RepositionFactionCoins()
    {
        float radius = 5f;
        switch (factionCoins.Count)
        {
            case 0:
                return;

            case 1:
                factionCoins[0].transform.localPosition = Vector3.zero;
                break;
            case >1:
                float angle = 360f / factionCoins.Count;
                for(int i = 0; i < factionCoins.Count; i++)
                {
                    Vector3 newPosition = Vector3.zero;
                    newPosition.x = radius * Mathf.Cos(Mathf.Deg2Rad * angle * i);
                    newPosition.z = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(newPosition.x, 2));
                    factionCoins[0].transform.localPosition = newPosition;
                }
                break;
        }
    }

    public void InitializeResources(Color backgroundColor, int index)
    {
        GameObject input = Instantiate(pooledTileResource, resourceParent);
        TileResourceUI tileResource = input.GetComponent<TileResourceUI>();
        tileResource.SetVisuals(backgroundColor, index);
        input.SetActive(true);
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
