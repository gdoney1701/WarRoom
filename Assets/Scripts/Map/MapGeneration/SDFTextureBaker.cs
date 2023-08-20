using UnityEngine;

public class SDFTextureBaker: MonoBehaviour
{
    [SerializeField]
    Vector2[] meshPoints;
    [SerializeField]
    Material testMAT;
    [SerializeField]
    int textureSize = 32;
    [SerializeField]
    int edgeSoftner = 10;
    [SerializeField]
    Vector2 xOffset = Vector2.zero;
    [SerializeField]
    Vector2 yOffset = Vector2.zero;

    private void Awake()
    {
        SDFTesting();
    }

    public void SDFTesting()
    {
        Vector3 offset = transform.position;
        Vector2[] vertices2D = new Vector2[meshPoints.Length];
        Vector3[] vertices = new Vector3[meshPoints.Length];

        Vector2 minPoint = meshPoints[0];
        Vector2 maxPoint = meshPoints[0];

        for(int i = 0; i < meshPoints.Length; i++)
        {
            vertices2D[i] = new Vector2(meshPoints[i].x + offset.x, meshPoints[i].y + offset.z);
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);

            minPoint = Vector2.Min(minPoint, vertices2D[i]);
            maxPoint = Vector2.Max(maxPoint, vertices2D[i]);
        }

        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        Mesh msh = new Mesh
        {
            name = "New Mesh"
        };

        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();
        msh.RecalculateTangents();

        msh.uv = GenerateUVs(vertices2D, msh.bounds);

        GameObject newTile = new GameObject();
        newTile.name = string.Format("{0}_Tile", gameObject.name);

        var mR = newTile.AddComponent<MeshRenderer>();
        var mF = newTile.AddComponent<MeshFilter>();

        mF.mesh = msh;
        mR.material = testMAT;

        Texture2D sdf = CreateRuntimeSDF(vertices2D, msh.bounds);

        mR.material.SetTexture("_BaseTexture", sdf);
    }

    Vector2[] GenerateUVs(Vector2[] vertices2D, Bounds bounds)
    {
        Vector2[] uvs = new Vector2[vertices2D.Length];

        for (int i = 0; i < vertices2D.Length; i++)
        {
            uvs[i] = new Vector2((vertices2D[i].x - bounds.min.x) / bounds.size.x, (vertices2D[i].y - bounds.min.z) / bounds.size.z);
            //uvs[i] = new Vector2(vertices2D[i].x / uvScale, vertices2D[i].y / uvScale);
            //uvs[i] = new Vector2((vertices2D[i].x - minPoint.x) / uvScale, (vertices2D[i].y - minPoint.y) / uvScale);
        }

        return uvs;
    }

    public Texture2D CreateRuntimeSDF(Vector2[] points, Bounds meshBounds)
    {
        Texture2D result = new Texture2D(textureSize, textureSize);

        //float uvScale = Mathf.Max(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);

        for(int y = 0; y < textureSize; y++)
        {
            for(int x = 0; x < textureSize; x++)
            {
                Vector2 worldSpace = ConvertUVToPos(new Vector2(x, y), meshBounds);
                float signedDistance = SDFHelperMethods.SignedDistance(points, worldSpace) / edgeSoftner;

                float newColor = signedDistance > 0 ? signedDistance : 0;
                //Debug.Log(string.Format("New SDF Value = {0}", newColor));
                result.SetPixel(x, y, new Color(newColor, newColor, newColor));
                
                //Debug.Log(string.Format("UV: ({0},{1}) with world pos ({2},{3}), uvScale {4} has SDF of {5}", x, y, worldSpace.x, worldSpace.y, uvScale, signedDistance));
            }
        }

        Debug.Log(string.Format("Mesh Bounds: ({0}, {1}), Minimum = ({2}, {3})", meshBounds.size.x, meshBounds.size.z, meshBounds.min.x, meshBounds.min.z));

        result.Apply();

        //Debug.Log(string.Format("Min Point: {0}, Max Point: {1}", minPoint, maxPoint));

        return result;
    }

    Vector2 ConvertUVToPos(Vector2 uvInput, Bounds bounds)
    {
        float xMod = uvInput.x > (textureSize / 2) ? uvInput.x + xOffset.x : uvInput.x + xOffset.y;
        float yMod = uvInput.y > (textureSize / 2) ? uvInput.y + yOffset.x : uvInput.y + yOffset.y;
        Vector2 result = new Vector2(xMod / textureSize * bounds.size.x + bounds.min.x, yMod / textureSize * bounds.size.z + bounds.min.z);
        return result;
    }

}
