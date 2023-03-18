using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMeshGenerator
{

    public class ProvinceData
    {
        public ProvinceData(Color32 provinceColor, string tag, Vector2Int[] edgePixels)
        {
            ProvinceColor = provinceColor;
            Tag = tag;
            EdgePixels = edgePixels;
            VertexOrder = new List<int>();
        }
        public Color32 ProvinceColor { get; }
        public string Tag { get; }
        public Vector2Int[] EdgePixels { get; set; }
        public List<int> VertexOrder { get; set; } //Indices of EdgePixels in clockwise rotation from the top left pixel
    }
    public class MeshGenerationData
    {
        //Continer class for input textures and materials. Could potentially be made redundant
        public Texture2D imageTexture;
        public Material faceMaterial;
        public Vector2[] vertexPositions = new Vector2[0];
        public Color32[] outlinePixels = new Color32[0];

        public Vector2 minCoords = Vector2.zero;
        public Vector2 maxCoords = Vector2.zero;
        public ProvinceData[] provinceList; //Master list of province data

    }
    public class MapMeshGenerator : MonoBehaviour
    {
        [SerializeField]
        private Texture2D inputTexture;
        [SerializeField]
        private Material faceMaterial;
        [SerializeField]
        private Color32 a1Color;
        [SerializeField]
        private Color32 a2Color;
        [SerializeField]
        private Color32 a3Color;
        [SerializeField]
        private GameObject fakeVertex;
        [SerializeField]
        private GameObject fakeContainer;

        Color32[] imagePixels = new Color32[0];
        Vector2Int imageScale = Vector2Int.zero;

        [ContextMenu("Test Generation")]
        public void GenerateMesh()
        {
            MeshGenerationData data = new MeshGenerationData();
            data.imageTexture = inputTexture;
            data.faceMaterial = faceMaterial;
            imageScale.x = data.imageTexture.width;
            imageScale.y = data.imageTexture.height;

            imagePixels = data.imageTexture.GetPixels32();

            //TODO: Make a json file where all province tags and colors are listed out manually
            //Read this input file instead of manually declaring them
            data.provinceList = new ProvinceData[]
            {
                new ProvinceData(a1Color, "A1", new Vector2Int[0]),
                new ProvinceData(a2Color, "A2", new Vector2Int[0]),
                new ProvinceData(a3Color, "A3", new Vector2Int[0])
            };

            FindVertexPixels(data);
            //FakeDraw(data);
            for (int i = 0; i < data.provinceList.Length; i++)
            {
                TriangulateVertices(data.provinceList[i]);
            }
        }

        //Iterate through the image and create Province data for each differently colored section
        public void FindVertexPixels(MeshGenerationData data)
        {
            Dictionary<Color32, List<Vector2Int>> provinceColors = new Dictionary<Color32, List<Vector2Int>>();
            for(int i = 0; i< imagePixels.Length; i++)
            {
                Color32 colorIterative = imagePixels[i];
                if (!provinceColors.ContainsKey(colorIterative))
                {
                    provinceColors.Add(colorIterative, new List<Vector2Int>());
                    provinceColors[colorIterative].AddRange(FindEdgeLoop(colorIterative, ConvertIndexToUV(i)));
                }
            }

            for (int i = 0; i < data.provinceList.Length; i++)
            {
                Color32 provCol = data.provinceList[i].ProvinceColor;
                if (provinceColors.ContainsKey(provCol))
                {
                    data.provinceList[i].EdgePixels = provinceColors[provCol].ToArray();
                }
            }
        }

        //Uses a modified Breadth First Search to find the adjacent edge tile 
        //Edge pixels are prioritized in N, E, S, W, NE, SE, SW, NW order
        private List<Vector2Int> FindEdgeLoop(Color32 targetColor, Vector2Int start)
        {
            List<Vector2Int> edgeLoop = new List<Vector2Int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            frontier.Enqueue(start);
            Vector2Int current = start;

            while (frontier.Count > 0)
            {
                current = frontier.Dequeue();
                edgeLoop.Add(current);
                visited.Add(current);
                Vector2Int[] neighbors = GetNeighbors(current);
                for(int i = 0; i < 8; i++)
                {
                    if (ValidUVCheck(neighbors[i]))
                    {
                        if (!visited.Contains(neighbors[i]) && imagePixels[ConvertUVToIndex(neighbors[i])].Equals(targetColor))
                        {
                            if (CheckForColorEdge(neighbors[i], targetColor))
                            {
                                frontier.Enqueue(neighbors[i]);
                                break;
                            }
                        }
                    }
                }
            }
            return edgeLoop;
        }
        //For fake purposes
        public void FakeDraw(MeshGenerationData data)
        {
            foreach (ProvinceData entry in data.provinceList)
            {
                foreach(Vector2Int position in entry.EdgePixels)
                {
                    GameObject point = Instantiate(fakeVertex);
                    point.transform.position = new Vector3(position.x, 0, position.y);
                    point.transform.SetParent(fakeContainer.transform);
                    //point.GetComponent<MeshRenderer>().material.color = entry.ProvinceColor;
                }
            }
        }

        //Checks to make sure that the UV's can pull useable indices
        //TODO: Make sure the 4 warnings that come out of this aren't impacting map gen
        private bool ValidUVCheck(Vector2Int start)
        {
            if(start.x < 0 || start.x >= imageScale.x)
            {
                Debug.Log(string.Format("UV {0} invalid in x", start));
                return false;
            }
            if(start.y < 0 || start.y >= imageScale.y)
            {
                Debug.Log(string.Format("UV {0} invalid in y", start));
                return false;
            }
            return true;
        }

        private Vector2Int ConvertIndexToUV(int i)
        {
            Vector2Int result = Vector2Int.zero;
            float r = (float)i / imageScale.x;
            result.y = Mathf.FloorToInt(r);
            result.x = Mathf.RoundToInt((r - result.y) * imageScale.x);
            return result;
        }

        private int ConvertUVToIndex(Vector2Int input)
        {
            return (input.y * imageScale.x) + input.x;
        }

        private Vector2Int[] GetNeighbors(Vector2Int start)
        {
            Vector2Int[] neighbors = new Vector2Int[8];

            neighbors[0] = new Vector2Int(start.x, start.y + 1); //North
            neighbors[1] = new Vector2Int(start.x + 1, start.y); //East
            neighbors[2] = new Vector2Int(start.x, start.y - 1); //South
            neighbors[3] = new Vector2Int(start.x - 1, start.y); //West
            neighbors[4] = new Vector2Int(start.x + 1, start.y + 1); //North East
            neighbors[5] = new Vector2Int(start.x + 1, start.y - 1); //South East
            neighbors[6] = new Vector2Int(start.x - 1, start.y - 1); //South West
            neighbors[7] = new Vector2Int(start.x - 1, start.y + 1); //North West

            return neighbors;
        }

        //Check the directly adjacent (no diagonals) for other colors
        private bool CheckForColorEdge(Vector2Int inputUV, Color32 pixelColor)
        {

            //Check the top and bottom pixels
            if(inputUV.y - 1 >= 0 && inputUV.y + 1 < imageScale.y)
            {
                if (!imagePixels[ConvertUVToIndex(new Vector2Int(inputUV.x, inputUV.y - 1))].Equals(pixelColor))
                {
                    return true;
                }
                if (!imagePixels[ConvertUVToIndex(new Vector2Int( inputUV.x, inputUV.y + 1))].Equals(pixelColor))
                {
                    return true;
                }
            }

            //Check the left and right pixels
            if (inputUV.x - 1 >= 0 && inputUV.x + 1 < imageScale.x)
            {
                if (!imagePixels[ConvertUVToIndex(new Vector2Int(inputUV.x - 1, inputUV.y))].Equals(pixelColor))
                {
                    return true;
                }
                if (!imagePixels[ConvertUVToIndex(new Vector2Int(inputUV.x + 1, inputUV.y ))].Equals(pixelColor))
                {
                    return true;
                }
            }

            return false;
        }

        private void TriangulateVertices(ProvinceData provinceData)
        {
            Vector2[] vertices2D = new Vector2[provinceData.EdgePixels.Length];
            Vector3[] vertices = new Vector3[provinceData.EdgePixels.Length];

            for(int i = 0; i < provinceData.EdgePixels.Length; i++)
            {
                vertices2D[i] = new Vector2(provinceData.EdgePixels[i].x, provinceData.EdgePixels[i].y);
                vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
            }

            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            GameObject newMesh = new GameObject(provinceData.Tag);
            newMesh.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = newMesh.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;
            newMesh.transform.SetParent(fakeContainer.transform);

        } 

    }
}

