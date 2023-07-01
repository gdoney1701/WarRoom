using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFTextureBaker: MonoBehaviour
{
    [SerializeField]
    Vector2[] meshPoints;
    [SerializeField]
    Material testMAT;
    [SerializeField]
    int textureSize = 32;

    private void Awake()
    {
        SDFTesting();
    }

    public void SDFTesting()
    {
        Vector2[] vertices2D = new Vector2[meshPoints.Length];
        Vector3[] vertices = new Vector3[meshPoints.Length];

        Vector2 minPoint = meshPoints[0];
        Vector2 maxPoint = meshPoints[0];

        for(int i = 0; i < meshPoints.Length; i++)
        {
            vertices2D[i] = meshPoints[i];
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

        msh.uv = GenerateUVs(vertices2D, minPoint, maxPoint);

        GameObject newTile = new GameObject();

        var mR = newTile.AddComponent<MeshRenderer>();
        var mF = newTile.AddComponent<MeshFilter>();

        mF.mesh = msh;
        mR.material = testMAT;

        Texture2D sdf = CreateRuntimeSDF(vertices2D, minPoint, maxPoint);

        mR.material.SetTexture("_BaseTexture", sdf);
    }

    Vector2[] GenerateUVs(Vector2[] vertices2D, Vector2 minPoint, Vector2 maxPoint)
    {
        Vector2[] uvs = new Vector2[vertices2D.Length];
        float uvScale = Mathf.Max(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);

        for (int i = 0; i < vertices2D.Length; i++)
        {
            uvs[i] = new Vector2(vertices2D[i].x / uvScale, vertices2D[i].y / uvScale);
            //uvs[i] = new Vector2((vertices2D[i].x - minPoint.x) / uvScale, (vertices2D[i].y - minPoint.y) / uvScale);
        }

        return uvs;
    }

    public Texture2D CreateRuntimeSDF(Vector2[] points, Vector2 minPoint, Vector2 maxPoint)
    {
        Texture2D result = new Texture2D(textureSize, textureSize);

        float uvScale = Mathf.Max(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);

        for(int y = 0; y < textureSize; y++)
        {
            for(int x = 0; x < textureSize; x++)
            {
                Vector2 worldSpace = ConvertUVToPos(new Vector2(x, y), uvScale/textureSize);
                float signedDistance = SignedDistance(points, worldSpace);

                float newColor = signedDistance > 0 ? signedDistance : 0;
                Debug.Log(string.Format("New SDF Value = {0}", newColor));
                result.SetPixel(x, y, new Color(newColor, newColor, newColor));
                
                Debug.Log(string.Format("UV: ({0},{1}) with world pos ({2},{3}), uvScale {4} has SDF of {5}", x, y, worldSpace.x, worldSpace.y, uvScale, signedDistance));
            }
        }

        result.Apply();

        return result;
    }

    Vector2 ConvertUVToPos(Vector2 input, float uvScale)
    {
        Vector2 result = new Vector2(input.x, input.y) * uvScale;
        return result;
    }

    float SignedDistance(Vector2[] vertices, Vector2 point)
    {
        bool inside = false;
        float minDistSq = float.MaxValue;

        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            Vector2 a = vertices[i];
            Vector2 b = vertices[j];

            if ((((a.y <= point.y) && (point.y < b.y)) ||
                ((b.y <= point.y) && (point.y < a.y))) &&
                (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x))
                inside = !inside;
            minDistSq = Mathf.Min(minDistSq, SegmentDistance(a, b, point));
        }

        return (inside ? 1 : -1) * Mathf.Sqrt(minDistSq);
    }
    float SegmentDistance(Vector2 a, Vector2 b, Vector2 p)
    {
        float x = a.x;
        float y = a.y;
        float dx = b.x - x;
        float dy = b.y - y;

        if (dx != 0 || dy != 0)
        {
            float t = ((p.x - x) * dx + (p.y - y) * dy) / (dx * dx + dy * dy);

            if (t > 1)
            {
                x = b.x;
                y = b.y;
            }
            else if (t > 0)
            {
                x += dx * t;
                y += dy * t;
            }
        }

        dx = p.x - x;
        dy = p.y - y;

        return dx * dx + dy * dy;
    }


}
