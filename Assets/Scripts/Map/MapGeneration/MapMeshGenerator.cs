using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace MapMeshGenerator
{
    public class MeshGenerationData
    {
        //Container class for input textures and materials. Could potentially be made redundant
        public Vector2Int imageScale;
        public Material faceMaterial;
        public Vector2[] vertexPositions = new Vector2[0];
        public Color32[] outlinePixels = new Color32[0];

        public Vector2 minCoords = Vector2.zero;
        public Vector2 maxCoords = Vector2.zero;
        public ProvinceData[] provinceList; //Master list of province data
        public Dictionary<string, MapTile> mapTiles = new Dictionary<string, MapTile>(); //Master list of final object tiles
        public GameObject[] columnArray;

        public void AssignNeighbors(ProvinceData inputData)
        {
            mapTiles[inputData.Data.TileTag].Neighbors = new MapTile[inputData.NeighborTags.Length];
            for(int i = 0; i < inputData.NeighborTags.Length; i++)
            {
                mapTiles[inputData.Data.TileTag].Neighbors[i] = mapTiles[inputData.NeighborTags[i]];
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
            Center = new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2);

            Radius = Vector2.Distance(Center, max);
            Dist = SDFHelperMethods.SignedDistance(vertices, Center);
            CellMax = Dist + Radius;
        }
        public Vector2 MaxPoint { get; set; }
        public Vector2 MinPoint { get; set; }
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
        public float Dist { get; set; }
        public float CellMax { get; set; }

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

        public class SDFPacket
        {
            public Vector3 POI;
            public Color[] pixelColors;
        }
        [SerializeField]
        private Material faceMaterial;
        [SerializeField]
        private GameObject tileContainer;
        [SerializeField]
        private GameObject tilePrefab;
        [SerializeField]
        private int sdfTextureSize = 64;
        [SerializeField]
        private float mapLimiter = 100f;

        Color32[] imagePixels = new Color32[0];
        //Vector2Int imageScale = Vector2Int.zero;
        float maxImageLength = 0;
        MeshGenerationData meshData;
        private Vector2 extremeZVertices = new Vector2(float.MaxValue, float.MinValue);

        public delegate void OnMapLoad(MeshGenerationData data, SaveData saveData);
        public static event OnMapLoad onMapLoad;

        public void OnEnable()
        {
            MainMenu.sendMapData += AsyncGenerateMap;
        }

        public void OnDisable()
        {
            MainMenu.sendMapData -= AsyncGenerateMap;
        }

        async void AsyncGenerateMap(string saveDataPath)
        {
            meshData = new MeshGenerationData();
            SaveData loadedSave = new SaveData();
            loadedSave.LoadFromFile(saveDataPath);

            GetProvinceData(meshData, loadedSave);
            meshData.faceMaterial = faceMaterial;


            await Task.Run(() => FindVertexPixels(meshData));

            for (int i = 0; i < meshData.provinceList.Length; i++)
            {
                meshData.mapTiles.Add(meshData.provinceList[i].Data.TileTag, TriangulateVertices(meshData.provinceList[i]));
            }

            SDFPacket[] sdfCalculation = new SDFPacket[meshData.provinceList.Length];
            await Task.Run(() => sdfCalculation = AsyncCalculateSDF());

            for(int i = 0; i < meshData.provinceList.Length; i++)
            {
                MapTile currentTile = meshData.mapTiles[meshData.provinceList[i].Data.TileTag];
                Texture2D sdfTex = new Texture2D(sdfTextureSize, sdfTextureSize);
                sdfTex.SetPixels(sdfCalculation[i].pixelColors);
                sdfTex.Apply();

                currentTile.InitializeSDFValues(sdfCalculation[i].POI, sdfTex);
            }
            if (loadedSave.loadedMapData.horizontalLooping)
            {
                meshData.columnArray = CreateVerticalGroups(meshData, 12);
            }
            else
            {
                meshData.columnArray = CreateVerticalGroups(meshData, 1);
            }

            if (Application.isPlaying)
            {
                tileContainer.transform.localScale = ResizeMap();
                //tileContainer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                onMapLoad?.Invoke(meshData, loadedSave);
            }
        }

        private Vector3 ResizeMap()
        {
            Vector3 result = Vector3.one;

            float mapHeight = extremeZVertices.y - extremeZVertices.x;
            float newScale = mapLimiter / mapHeight;
            result *= newScale;
            Debug.Log(result);

            return result;
        }

        private void GetProvinceData(MeshGenerationData data, SaveData loadedData)
        {
            MapColorData mapData = loadedData.loadedMapData;

            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/textures/maps"));
            Texture2D loadedTexture = assetBundle.LoadAsset<Texture2D>(mapData.MapTexturePath);
            data.imageScale.x = loadedTexture.width;
            data.imageScale.y = loadedTexture.height;
            imagePixels = loadedTexture.GetPixels32();

            maxImageLength = Mathf.Max(data.imageScale.x, data.imageScale.y);
            ProvinceData[] newData = new ProvinceData[mapData.TileList.Count];
            for(int i = 0; i<newData.Length; i++)
            {
                newData[i] = TileDataToProvinceData(mapData.TileList[i]);
            }

            data.provinceList = newData;
        }

        private ProvinceData TileDataToProvinceData(TileData tileData)
        {
            return new ProvinceData(
                new Color32((byte)tileData.TileColor.x, (byte)tileData.TileColor.y, (byte)tileData.TileColor.z, 255),
                new EdgeVertex[0],
                tileData
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
                        sortedProvinces[colorIterative].NeighborTags[j] = sortedProvinces[neighborColors[j]].Data.TileTag;
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

            ////For debug purposes
            //if (useTestGeo)
            //{
            //    GameObject testContainer = new GameObject();
            //    testContainer.name = string.Format("{0} Container", baseColor);
            //    testVertex.transform.position = Vector3.zero;
            //    for (int i = 0; i < result.Length; i++)
            //    {
            //        GameObject testVert = Instantiate(testVertex);
            //        testVert.transform.position = new Vector3(result[i].Pos.x, 0, result[i].Pos.y);
            //        testVert.transform.SetParent(testContainer.transform);
            //        testVert.name = string.Format("MainPoint_{0}_{1}", result[i].Pos, i);
            //        //Debug.Log(result[i].Pos);
            //    }
            //    for (int i = 0; i < uvInput.Length; i++)
            //    {
            //        GameObject testVert = Instantiate(testVertex);
            //        testVert.transform.position = new Vector3(uvInput[i].x, 0, uvInput[i].y);
            //        testVert.transform.SetParent(testContainer.transform);
            //        testVert.name = string.Format("UVPoint_{0}_{1}", uvInput[i], i);
            //        //Debug.Log(uvInput[i]);
            //    }
            //}

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
            return isMainPoint;
        }

        //Checks to make sure that the UV's can pull useable indices
        private bool ValidUVCheck(Vector2Int start)
        {
            if(start.x < 0 || start.x >= meshData.imageScale.x)
            {
                return false;
            }
            if(start.y < 0 || start.y >= meshData.imageScale.y)
            {
                return false;
            }
            return true;
        }

        private Vector2Int ConvertIndexToUV(int i)
        {
            Vector2Int result = Vector2Int.zero;
            float r = (float)i / meshData.imageScale.x;
            result.y = Mathf.FloorToInt(r);
            result.x = Mathf.RoundToInt((r - result.y) * meshData.imageScale.x);
            return result;
        }

        private int ConvertUVToIndex(Vector2Int input)
        {
            return (input.y * meshData.imageScale.x) + input.x;
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
            if(inputUV.y - 1 >= 0 && inputUV.y + 1 < meshData.imageScale.y)
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
            if (inputUV.x - 1 >= 0 && inputUV.x + 1 < meshData.imageScale.x)
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
            provinceData.CollapseEdgeVertex();
            Vector2[] vertices2D = provinceData.VertexPoints;
            Vector3[] vertices = new Vector3[provinceData.EdgeVertices.Length];

            for(int i = 0; i < provinceData.EdgeVertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
                extremeZVertices.x = Mathf.Min(extremeZVertices.x, vertices[i].x);
                extremeZVertices.y = Mathf.Max(extremeZVertices.y, vertices[i].z);
            }

            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            Mesh msh = new Mesh
            {
                name = string.Format("Province Mesh {0}", provinceData.Data.TileTag)
            };

            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            msh.RecalculateTangents();
            msh.uv = GenerateUVs(vertices2D, msh.bounds);
            msh.uv2 = GenerateUVs(vertices2D, new Vector2(0,0), new Vector2(maxImageLength, maxImageLength)) ;

            provinceData.MaxPoint = new Vector2(msh.bounds.max.x, msh.bounds.max.z);
            //Texture2D sdf = CreateRuntimeSDF(vertices2D, msh.bounds);

            GameObject newTile = Instantiate(tilePrefab, tileContainer.transform);
            newTile.transform.position = Vector3.zero;
            MapTile mapTile = newTile.GetComponent<MapTile>();

            mapTile.InitializePrefab(
                provinceData, msh, faceMaterial, msh.bounds);

            return mapTile;
        }

        Vector2[] GenerateUVs(Vector2[] vertices2D, Vector2 minPoint, Vector2 maxPoint)
        {
            Vector2[] uvs = new Vector2[vertices2D.Length];
            float uvScale = Mathf.Max(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);

            for (int i = 0; i < vertices2D.Length; i++)
            {
                uvs[i] = new Vector2((vertices2D[i].x - minPoint.x) / uvScale, (vertices2D[i].y - minPoint.y) / uvScale);
            }

            return uvs;
        }

        Vector2[] GenerateUVs(Vector2[] vertices2D, Bounds bounds)
        {
            Vector2[] uvs = new Vector2[vertices2D.Length];

            for (int i = 0; i < vertices2D.Length; i++)
            {
                uvs[i] = new Vector2((vertices2D[i].x - bounds.min.x) / bounds.size.x, (vertices2D[i].y - bounds.min.z) / bounds.size.z);
            }

            return uvs;
        }

        public SDFPacket[] AsyncCalculateSDF()
        {
            ProvinceData[] provinces = meshData.provinceList;
            SDFPacket[] result = new SDFPacket[provinces.Length];
            for (int i = 0; i < provinces.Length; i++)
            {
                result[i] = CreateRuntimeSDF(provinces[i].VertexPoints, meshData.mapTiles[provinces[i].Data.TileTag].MeshBounds);
                meshData.AssignNeighbors(provinces[i]);
            }
            return result;
        }

        public SDFPacket CreateRuntimeSDF(Vector2[] points, Bounds meshBounds)
        {
            SDFPacket result = new SDFPacket();
            result.pixelColors = new Color[sdfTextureSize * sdfTextureSize];

            float maxDistance = float.MinValue;
            Vector2 poiUV = new Vector2(0, 0);

            for (int y = 0; y < sdfTextureSize; y++)
            {
                for (int x = 0; x < sdfTextureSize; x++)
                {
                    Vector2 worldSpace = ConvertUVToPos(new Vector2(x + 0.5f, y + 0.5f), meshBounds, sdfTextureSize);
                    float signedDistance = SDFHelperMethods.SignedDistance(points, worldSpace) / 30f;
                    float newColor = signedDistance > 0 ? signedDistance : 0;

                    if(signedDistance > maxDistance)
                    {
                        maxDistance = signedDistance;
                        poiUV = new Vector2(x + 0.5f, y + 0.5f);
                    }
                    result.pixelColors[y * sdfTextureSize + x] = new Color(newColor, newColor, newColor);
                }
            }

            Vector2 finalPos = ConvertUVToPos(poiUV, meshBounds, sdfTextureSize);
            result.POI = new Vector3(finalPos.x, 0, finalPos.y);
            return result;
        }

        Vector2 ConvertUVToPos(Vector2 uvInput, Bounds bounds, int textureSize)
        {
            Vector2 result = new Vector2((float)uvInput.x / textureSize * bounds.size.x + bounds.min.x, (float)uvInput.y / textureSize * bounds.size.z + bounds.min.z);
            return result;
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

        GameObject[] CreateVerticalGroups(MeshGenerationData data, int columnNumber)
        {
            float columnWidth = data.imageScale.x / columnNumber;
            GameObject[] columnContainer = new GameObject[columnNumber];
            for(int i = 0; i < columnNumber; i++)
            {
                GameObject columnOffset = new GameObject(string.Format("TileColumn_Offset_{0}", i));
                columnOffset.transform.SetParent(tileContainer.transform);
                columnOffset.transform.position = new Vector3(i * columnWidth, 0, 0);

                GameObject newColumn = new GameObject(string.Format("TileColumn_{0}", i));
                newColumn.transform.SetParent(columnOffset.transform);
                newColumn.transform.position = Vector3.zero;

                columnContainer[i] = newColumn;
            }

            for(int i = 0; i < data.provinceList.Length; i++)
            {
                int index = Mathf.FloorToInt(data.provinceList[i].MaxPoint.x / columnWidth);
                try
                {
                    data.mapTiles[data.provinceList[i].Data.TileTag].transform.SetParent(columnContainer[index].transform);
                }
                catch
                {
                    Debug.LogError(string.Format("Error in Creating Vertical Groups: {0}, {1}, {2}, {3}", data.provinceList.Length, i, index, columnContainer.Length));
                }
                
            }
            return columnContainer;
        }

    }
}

