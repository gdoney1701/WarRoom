using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMeshGenerator
{
    public class MeshGenerationData
    {
        //Container class for input textures and materials. Could potentially be made redundant
        public Texture2D imageTexture;
        public Material faceMaterial;
        public Vector2[] vertexPositions = new Vector2[0];
        public Color32[] outlinePixels = new Color32[0];

        public Vector2 minCoords = Vector2.zero;
        public Vector2 maxCoords = Vector2.zero;
        public ProvinceData[] provinceList; //Master list of province data
        public Dictionary<string, MapTile> mapTiles = new Dictionary<string, MapTile>(); //Master list of final object tiles

        public void AssignNeighbors(ProvinceData inputData)
        {
            mapTiles[inputData.Tag].Neighbors = new MapTile[inputData.NeighborTags.Length];
            for(int i = 0; i < inputData.NeighborTags.Length; i++)
            {
                mapTiles[inputData.Tag].Neighbors[i] = mapTiles[inputData.NeighborTags[i]];
            }
        }

        public MapTile[] ExtractMapTiles()
        {
            List<MapTile> result = new List<MapTile>();
            result.AddRange(mapTiles.Values);
            return result.ToArray();
        }
    }

    public class SDFCell
    {
        public SDFCell(Vector2 min, Vector2 max, Vector2[] vertices)
        {
            MaxPoint = max;
            MinPoint = min;
            //Debug.LogError(string.Format("Max {0}, Min {1}, Center {2}", max, min, (max + min) / 2));
            Center = new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2);

            Radius = Vector2.Distance(Center, max);
            Dist = SignedDistance(vertices);
            CellMax = Dist + Radius;
        }
        public Vector2 MaxPoint { get; set; }
        public Vector2 MinPoint { get; set; }
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
        public float Dist { get; set; }
        public float CellMax { get; set; }

        float SignedDistance(Vector2[] vertices)
        {
            bool inside = false;
            float minDistSq = float.MaxValue;

            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                Vector2 a = vertices[i];
                Vector2 b = vertices[j];

                if ((((a.y <= Center.y) && (Center.y < b.y)) ||
                    ((b.y <= Center.y) && (Center.y < a.y))) &&
                    (Center.x < (b.x - a.x) * (Center.y - a.y) / (b.y - a.y) + a.x))
                    inside = !inside;
                minDistSq = Mathf.Min(minDistSq, SegmentDistance(a, b, Center));
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

        public void SubdivideCell(Vector2[]vertices, Queue<SDFCell> CellQueue)
        {

            CellQueue.Enqueue(new SDFCell(
                new Vector2(MinPoint.x, (MaxPoint.y - MinPoint.y) / 2 + MinPoint.y),
                new Vector2((MaxPoint.x - MinPoint.x) / 2 + MinPoint.x, MaxPoint.y),
                vertices));

            CellQueue.Enqueue(new SDFCell(
                Center,
                MaxPoint,
                vertices));

            CellQueue.Enqueue(new SDFCell(
                new Vector2((MaxPoint.x - MinPoint.x) / 2 + MinPoint.x, MinPoint.y),
                new Vector2(MaxPoint.x, (MaxPoint.y - MinPoint.y) / 2 + MinPoint.y),
                vertices));

            CellQueue.Enqueue(new SDFCell(
                MinPoint,
                Center,
                vertices));

        }
    }

    public class MapMeshGenerator : MonoBehaviour
    {
        [SerializeField]
        private Texture2D inputTexture;
        [SerializeField]
        private Material faceMaterial;
        [SerializeField]
        private string colorDataPath = "SmallMapDemo";
        [SerializeField]
        private GameObject tileContainer;
        [SerializeField]
        private GameObject tilePrefab;
        [SerializeField]
        private GameObject testVertex;
        [SerializeField]
        private bool useTestGeo;
        [SerializeField, Range(0.1f, 1f)]
        private float mapScale = 0.5f;

        Color32[] imagePixels = new Color32[0];
        Vector2Int imageScale = Vector2Int.zero;

        public delegate void OnMapLoad(Dictionary<string, MapTile> tileDict);
        public static event OnMapLoad onMapLoad;
        private void Start()
        {
            GenerateMesh();
        }

        [ContextMenu("Test Generation")]
        public void GenerateMesh()
        {
            MeshGenerationData data = new MeshGenerationData();
            data.imageTexture = inputTexture;
            data.faceMaterial = faceMaterial;

            imageScale.x = data.imageTexture.width;
            imageScale.y = data.imageTexture.height;
            imagePixels = data.imageTexture.GetPixels32();
            
            //Read the input data json containing tile tags and tile colors
            data.provinceList = GetProvinceData();

            FindVertexPixels(data);

            for (int i = 0; i < data.provinceList.Length; i++)
            {
                data.mapTiles.Add(data.provinceList[i].Tag, TriangulateVertices(data.provinceList[i]));
            }
            for(int i = 0; i < data.provinceList.Length; i++)
            {
                data.AssignNeighbors(data.provinceList[i]);
            }
            //float uniScale = (mapScale * 64f) / inputTexture.width;

            //tileContainer.transform.localScale = new Vector3(uniScale, 1, uniScale);

            if (Application.isPlaying)
            {
                onMapLoad?.Invoke(data.mapTiles);
            }
        }

        private ProvinceData[] GetProvinceData()
        {
            MapColorData mapData = new MapColorData();
            mapData.LoadFromFile(colorDataPath);

            ProvinceData[] newData = new ProvinceData[mapData.TileList.Count];
            for(int i = 0; i<newData.Length; i++)
            {
                newData[i] = TileDataToProvinceData(mapData.TileList[i]);
            }

            return newData;
        }

        private ProvinceData TileDataToProvinceData(MapColorData.TileData tileData)
        {
            return new ProvinceData(
                new Color32((byte)tileData.TileColor.x, (byte)tileData.TileColor.y, (byte)tileData.TileColor.z, 255),
                tileData.TileTag,
                new EdgeVertex[0]
                );
        }

        //Iterate through the image and create Province data for each differently colored section
        public void FindVertexPixels(MeshGenerationData data)
        {
            Color32 backgroundColor = new Color32(255, 255, 255, 255);

            Dictionary<Color32, ProvinceData> sortedProvinces = new Dictionary<Color32, ProvinceData>();
            List<Color32> foundColors = new List<Color32>();

            for (int i = 0; i<data.provinceList.Length; i++)
            {
                sortedProvinces.Add(data.provinceList[i].ProvinceColor, data.provinceList[i]);
            }

            for(int i = 0; i< imagePixels.Length; i++)
            {
                Color32 colorIterative = imagePixels[i];
                if(!foundColors.Contains(colorIterative) && !colorIterative.Equals(backgroundColor))
                {
                    sortedProvinces[colorIterative].EdgeVertices = FindEdgeLoop(colorIterative, ConvertIndexToUV(i));
                    Color32[] neighborColors = ExtractNeighborColors(sortedProvinces[colorIterative]);
                    sortedProvinces[colorIterative].NeighborTags = new string[neighborColors.Length];
                    for(int j = 0; j < neighborColors.Length; j++)
                    {
                        sortedProvinces[colorIterative].NeighborTags[j] = sortedProvinces[neighborColors[j]].Tag;
                    }
                    foundColors.Add(colorIterative);
                }
            }
            
        }

        //Uses a modified Breadth First Search to find the adjacent edge tile 
        //Edge pixels are prioritized in E, S, W, N, SE, SW, NW, NE order
        private EdgeVertex[] FindEdgeLoop(Color32 targetColor, Vector2Int start)
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
            return SimplifyEdgeLoop(edgeLoop.ToArray(), targetColor);
        }

        private EdgeVertex[] SimplifyEdgeLoop(Vector2Int[] uvInput, Color32 baseColor)
        {
            List<Vector2> mainPoints = new List<Vector2>();
            for(int i = 0, p = uvInput.Length - 1; i < uvInput.Length; p = i++)
            {
                List<Vector2> newPoint = new List<Vector2>();
                if(CheckForMainPoint(uvInput[i], uvInput[p], baseColor, out newPoint))
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
            List<Vector2> collinearReduction = new List<Vector2>();
            collinearReduction.AddRange(mainPoints);
            //for (int i = 0; i < mainPoints.Count; i++)
            //{
            //    Vector2[] neighbors = new Vector2[3];

            //    neighbors[0] = i == 0 ? mainPoints[mainPoints.Count - 1] : mainPoints[i - 1];
            //    neighbors[1] = mainPoints[i];
            //    neighbors[2] = i == mainPoints.Count - 1 ? mainPoints[0] : mainPoints[i + 1];

            //    if (!IsCollinear(neighbors))
            //    {
            //        collinearReduction.Add(mainPoints[i]);
            //    }
            //}

            EdgeVertex[] result = new EdgeVertex[collinearReduction.Count];
            for(int i = 0; i < collinearReduction.Count; i++)
            {
                Vector2 pos = collinearReduction[i];
                result[i] = new EdgeVertex(pos, GetUVColorsFromVertex(pos, baseColor));
            }

            //For debug purposes
            if (useTestGeo)
            {
                GameObject testContainer = new GameObject();
                testContainer.name = string.Format("{0} Container", baseColor);
                testVertex.transform.position = Vector3.zero;
                for (int i = 0; i < result.Length; i++)
                {
                    GameObject testVert = Instantiate(testVertex);
                    testVert.transform.position = new Vector3(result[i].Pos.x, 0, result[i].Pos.y);
                    testVert.transform.SetParent(testContainer.transform);
                    testVert.name = string.Format("MainPoint_{0}_{1}", result[i].Pos, i);
                    //Debug.Log(result[i].Pos);
                }
                for (int i = 0; i < uvInput.Length; i++)
                {
                    GameObject testVert = Instantiate(testVertex);
                    testVert.transform.position = new Vector3(uvInput[i].x, 0, uvInput[i].y);
                    testVert.transform.SetParent(testContainer.transform);
                    testVert.name = string.Format("UVPoint_{0}_{1}", uvInput[i], i);
                    //Debug.Log(uvInput[i]);
                }
            }

            return result;
        }

        private Color32[] GetUVColorsFromVertex(Vector2 inputCoord, Color32 baseColor)
        {
            Color32 background = new Color32(255, 255, 255, 255);
            List<Color32> result = new List<Color32>();
            Color32[] corners = new Color32[4];

            int minX = Mathf.FloorToInt(inputCoord.x);
            int maxX = Mathf.CeilToInt(inputCoord.x);
            int minY = Mathf.FloorToInt(inputCoord.y);
            int maxY = Mathf.CeilToInt(inputCoord.y);

            Vector2Int NW = new Vector2Int(minX, maxY);
            Vector2Int NE = new Vector2Int(maxX, maxY);
            Vector2Int SE = new Vector2Int(maxX, minY);
            Vector2Int SW = new Vector2Int(minX, minY);

            corners[0] = imagePixels[ConvertUVToIndex(NW)];
            corners[1] = imagePixels[ConvertUVToIndex(NE)];
            corners[2] = imagePixels[ConvertUVToIndex(SE)];
            corners[3] = imagePixels[ConvertUVToIndex(SW)];

            for(int i = 0; i < 4; i++)
            {
                if (!result.Contains(corners[i]) && !corners[i].Equals(background) && !corners[i].Equals(baseColor))
                    result.Add(corners[i]);
            }
            return result.ToArray();
        }
        private bool IsCollinear(Vector2[] input)
        {
            float slopeA = input[0].x == input[1].x ? float.MaxValue : (input[1].y - input[0].y) / (input[1].x - input[0].x);
            float slobeB = input[0].x == input[2].x ? float.MaxValue : (input[2].y - input[1].y) / (input[2].x - input[1].x);
            return slopeA == slobeB;
        }

        //Determines if an edge point is considered important enough to be preserved. Does not check collinearity 
        private bool CheckForMainPoint(Vector2Int startPos, Vector2Int prevPos, Color32 baseColor, out List<Vector2> mainPointUV )
        {

            bool isMainPoint = false;
            mainPointUV = new List<Vector2>();
            Vector2Int[] neighbors = GetNeighbors(startPos);

            for(int i = 7, j = 6; i > 3; i--,j--)
            {
                if (j == 3)
                    j = 7;
                int corner = prevPos.y != startPos.y && prevPos.x == startPos.x ? j : i;
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
            //Doesn't work because some Non Main Points don't have corresponding UV points on the other edge loop

            //if (!isMainPoint)
            //{
            //    Debug.LogWarning(string.Format("Point {0} has no corner neighbors, attempting adjacent", startPos));
            //    for(int i = 3; i > 0; i--)
            //    {
            //        Color32 adjacentColor = imagePixels[ConvertUVToIndex(neighbors[i])];
            //        Debug.Log(string.Format("Point {0}, Neighbor {1} has color {2}", startPos, neighbors[i], adjacentColor));
            //        if (!adjacentColor.Equals(baseColor))
            //        {
            //            isMainPoint = true;
            //            mainPointUV.Add(new Vector2(
            //                (neighbors[i].x + startPos.x) / 2f,
            //                (neighbors[i].y + startPos.y) / 2f
            //                ));
            //            break;
            //        }
            //    }
            //}
            return isMainPoint;
        }

        //Checks to make sure that the UV's can pull useable indices
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
          
            neighbors[0] = new Vector2Int(start.x + 1, start.y); //East
            neighbors[1] = new Vector2Int(start.x, start.y - 1); //South
            neighbors[2] = new Vector2Int(start.x - 1, start.y); //West
            neighbors[3] = new Vector2Int(start.x, start.y + 1); //North       
            neighbors[4] = new Vector2Int(start.x + 1, start.y - 1); //South East
            neighbors[5] = new Vector2Int(start.x - 1, start.y - 1); //South West
            neighbors[6] = new Vector2Int(start.x - 1, start.y + 1); //North West
            neighbors[7] = new Vector2Int(start.x + 1, start.y + 1); //North East

            return neighbors;
        }

        //Check the directly adjacent (no diagonals) for other colors
        private bool CheckForColorEdge(Vector2Int inputUV, Color32 pixelColor)
        {
            //Check the top and bottom pixels
            if(inputUV.y - 1 >= 0 && inputUV.y + 1 < imageScale.y)
            {
                if (!imagePixels[ConvertUVToIndex(new Vector2Int(inputUV.x, inputUV.y + 1))].Equals(pixelColor))
                {
                    return true;
                }
                if (!imagePixels[ConvertUVToIndex(new Vector2Int(inputUV.x, inputUV.y - 1))].Equals(pixelColor))
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
                if (!imagePixels[ConvertUVToIndex(new Vector2Int(inputUV.x + 1, inputUV.y))].Equals(pixelColor))
                {
                    return true;
                }
            }

            return false;
        }

        private MapTile TriangulateVertices(ProvinceData provinceData)
        {
            Vector2[] vertices2D = new Vector2[provinceData.EdgeVertices.Length];
            Vector3[] vertices = new Vector3[provinceData.EdgeVertices.Length];

            Vector2 minPoint = provinceData.EdgeVertices[0].Pos;
            Vector2 maxPoint = provinceData.EdgeVertices[0].Pos;

            for(int i = 0; i < provinceData.EdgeVertices.Length; i++)
            {
                vertices2D[i] = provinceData.EdgeVertices[i].Pos;
                vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);

                minPoint = Vector2.Min(minPoint, vertices2D[i]);
                maxPoint = Vector2.Max(maxPoint, vertices2D[i]);               
            }
            Debug.LogWarning(string.Format("For Tile {0}, minPoint = {1}, maxPoint = {2}", provinceData.Tag, minPoint, maxPoint));

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
            msh.uv = GenerateUVs(vertices2D, minPoint, maxPoint);
            msh.uv2 = vertices2D;

            GameObject newTile = Instantiate(tilePrefab, tileContainer.transform);
            newTile.transform.position = Vector3.zero;
            MapTile mapTile = newTile.GetComponent<MapTile>();

            mapTile.InitializePrefab(
                provinceData, msh, faceMaterial, CalculatePOI(vertices2D,minPoint, maxPoint), maxPoint, minPoint);

            return mapTile;
        }
        
        Vector2[] GenerateUVs(Vector2[] vertices2D, Vector2 minPoint, Vector2 maxPoint)
        {
            Vector2[] uvs = new Vector2[vertices2D.Length];
            float uvScale = Mathf.Max(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);

            for(int i = 0; i < vertices2D.Length; i++)
            {
                uvs[i] = new Vector2((vertices2D[i].x - minPoint.x) / uvScale, (vertices2D[i].y - minPoint.y) / uvScale);
            }

            return uvs;
        }

        Vector3 CalculatePOI(Vector2[] vertices, Vector2 minPoint, Vector2 maxPoint)
        {
            float uvScale = Mathf.Max(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);
            Vector2 uvMaxPoint = new Vector2(minPoint.x + uvScale, minPoint.y + uvScale);

            SDFCell initialCell = new SDFCell(minPoint, uvMaxPoint, vertices);
            SDFCell bestCell = initialCell;

            Queue<SDFCell> frontier = new Queue<SDFCell>();
            initialCell.SubdivideCell(vertices, frontier);
            int fallBack = 0;
            while(frontier.Count > 0)
            {
                SDFCell currentCell = frontier.Dequeue();
                if (currentCell.Dist < 0)
                    continue;

                if (currentCell.Dist >= bestCell.Dist)
                {
                    //Debug.LogWarning(string.Format("New Best Cell Found: {0} from {1}", currentCell, bestCell));
                    bestCell = currentCell;               
                }
                    

                //if (currentCell.CellMax - bestCell.Dist <= 1)
                //    continue;

                currentCell.SubdivideCell(vertices, frontier);
                fallBack++;

                if(fallBack > 64)
                {
                    //Debug.LogWarning("Had to Fall Back after 32 iterations");
                    break;
                }
            }


            return new Vector3(bestCell.Center.x, 0, bestCell.Center.y);
        }

        Color32[] ExtractNeighborColors(ProvinceData provinceData)
        {
            List<Color32> uniqueNeighbors = new List<Color32>();
            for(int i = 0; i < provinceData.EdgeVertices.Length; i++)
            {
                for(int j = 0; j < provinceData.EdgeVertices[i].EdgeColors.Length; j++)
                {
                    if (!uniqueNeighbors.Contains(provinceData.EdgeVertices[i].EdgeColors[j]))
                        uniqueNeighbors.Add(provinceData.EdgeVertices[i].EdgeColors[j]);
                }
            }

            return uniqueNeighbors.ToArray();
        }

    }
}

