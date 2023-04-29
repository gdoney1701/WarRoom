using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMeshGenerator
{

    public class ProvinceData
    {
        public ProvinceData(Color32 provinceColor, string tag, Vector2[] edgePixels)
        {
            ProvinceColor = provinceColor;
            Tag = tag;
            EdgePixels = edgePixels;
            VertexOrder = new List<int>();
        }
        public Color32 ProvinceColor { get; }
        public string Tag { get; }
        public Vector2[] EdgePixels { get; set; }
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
        private Color32 backgroundColor;
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
                new ProvinceData(a1Color, "A1", new Vector2[0]),
                new ProvinceData(a2Color, "A2", new Vector2[0]),
                new ProvinceData(a3Color, "A3", new Vector2[0])
            };

            FindVertexPixels(data);

            for (int i = 0; i < data.provinceList.Length; i++)
            {
                TriangulateVertices(data.provinceList[i]);
            }
        }

        //Iterate through the image and create Province data for each differently colored section
        public void FindVertexPixels(MeshGenerationData data)
        {
            Dictionary<Color32, List<Vector2>> provinceColors = new Dictionary<Color32, List<Vector2>>();
            for(int i = 0; i< imagePixels.Length; i++)
            {
                Color32 colorIterative = imagePixels[i];
                if (!provinceColors.ContainsKey(colorIterative) && !colorIterative.Equals(backgroundColor))
                {
                    provinceColors.Add(colorIterative, new List<Vector2>());
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
        private List<Vector2> FindEdgeLoop(Color32 targetColor, Vector2Int start)
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
            List<Vector2> result = new List<Vector2>();
            Vector2[] mainPoints = SimplifyEdgeLoop(edgeLoop.ToArray(), targetColor);
            result.AddRange(mainPoints);
            return result;
        }

        private Vector2[] SimplifyEdgeLoop(Vector2Int[] uvInput, Color32 baseColor)
        {
            List<Vector2> mainPoints = new List<Vector2>();
            for(int i = 0; i < uvInput.Length; i++)
            {
                List<Vector2> newPoint = new List<Vector2>();
                if(CheckForMainPoint(uvInput[i], baseColor, out newPoint))
                {
                    for(int j = 0; j < newPoint.Count; j++)
                    {
                        if (!mainPoints.Contains(newPoint[j]))
                        {
                            mainPoints.Add(newPoint[j]);
                        }
                    }


                }
            }
            List<Vector2> result = new List<Vector2>();
            for(int i = 0; i < mainPoints.Count; i++)
            {
                Vector2[] neighbors = new Vector2[3];

                neighbors[0] = i == 0 ? mainPoints[mainPoints.Count - 1] : mainPoints[i - 1];
                neighbors[1] = mainPoints[i];
                neighbors[2] = i == mainPoints.Count - 1 ? mainPoints[0] : mainPoints[i + 1];

                if (!IsCollinear(neighbors))
                {
                    result.Add(mainPoints[i]);
                }
            }

            return result.ToArray();
        }
        private bool IsCollinear(Vector2[] input)
        {

            float slopeA = input[0].x == input[1].x ? float.MaxValue : (input[1].y - input[0].y) / (input[1].x - input[0].x);
            float slobeB = input[0].x == input[2].x ? float.MaxValue : (input[2].y - input[1].y) / (input[2].x - input[1].x);
            return slopeA == slobeB;
        }

        private bool CheckForMainPoint(Vector2Int startPos, Color32 baseColor, out List<Vector2> mainPointUV )
        {
            bool isMainPoint = false;
            mainPointUV = new List<Vector2>();
            Vector2Int[] neighbors = GetNeighbors(startPos);

            for(int corner = 4; corner < neighbors.Length; corner++)
            {
                Color32 cornerColor = imagePixels[ConvertUVToIndex(neighbors[corner])];

                if (!cornerColor.Equals(baseColor))
                {
                    int plus = corner + 1 > 7 ? 0 : corner - 3;
                    int minus = corner - 4;

                    Color32 plusColor = imagePixels[ConvertUVToIndex(neighbors[plus])];
                    Color32 minusColor = imagePixels[ConvertUVToIndex(neighbors[minus])];

                    if (!plusColor.Equals(cornerColor) || !minusColor.Equals(cornerColor))
                    {
                        isMainPoint = true;
                        if (!plusColor.Equals(baseColor) && !minusColor.Equals(baseColor))
                        {
                            mainPointUV.Clear();
                            mainPointUV.Add(new Vector2(
                                (neighbors[plus].x + neighbors[minus].x) / 2f,
                                (neighbors[plus].y + neighbors[minus].y) / 2f
                                ));
                            break;
                        }
                        mainPointUV.Add( new Vector2(
                            (neighbors[plus].x + neighbors[minus].x) / 2f,
                            (neighbors[plus].y + neighbors[minus].y) / 2f
                            ));
                    }
                }
            }
            
            return isMainPoint;
        }

        //Checks to make sure that the UV's can pull useable indices
        //TODO: Make sure the 4 warnings that come out of this aren't impacting map gen
        private bool ValidUVCheck(Vector2Int start)
        {
            if(start.x < 0 || start.x >= imageScale.x)
            {
                return false;
            }
            if(start.y < 0 || start.y >= imageScale.y)
            {
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

            float uvMinf = Mathf.Min(provinceData.EdgePixels[0].x, provinceData.EdgePixels[0].y);
            float uvMaxf = Mathf.Max(provinceData.EdgePixels[0].x, provinceData.EdgePixels[0].y);

            for(int i = 0; i < provinceData.EdgePixels.Length; i++)
            {
                vertices2D[i] = new Vector2(provinceData.EdgePixels[i].x, provinceData.EdgePixels[i].y);
                vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
                Vector2 edgePixel = provinceData.EdgePixels[i];

                uvMaxf = Mathf.Max(uvMaxf, edgePixel.x, edgePixel.y);
                uvMinf = Mathf.Min(uvMinf, edgePixel.x, edgePixel.y);
            }

            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            Mesh msh = new Mesh
            {
                name = string.Format("Province Mesh {0}", provinceData.Tag)
            };

            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            msh.RecalculateTangents();
            msh.uv = GenerateUVs(vertices2D, uvMinf, uvMaxf);

            GameObject newMesh = new GameObject(provinceData.Tag);
            newMesh.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = newMesh.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;
            newMesh.GetComponent<MeshRenderer>().material = faceMaterial;
            newMesh.transform.SetParent(fakeContainer.transform);
        }
        
        Vector2[] GenerateUVs(Vector2[] vertices2D, float uvMin, float uvMax)
        {
            Vector2[] uvs = new Vector2[vertices2D.Length];
            float uvScale = uvMax - uvMin;
            for(int i = 0; i < vertices2D.Length; i++)
            {
                uvs[i] = new Vector2((vertices2D[i].x - uvMin) / uvScale, (vertices2D[i].y - uvMin) / uvScale);
            }

            return uvs;
        }

    }
}

