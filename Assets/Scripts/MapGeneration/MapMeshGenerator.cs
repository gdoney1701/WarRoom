using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapMeshGenerator
{

    public struct ProvinceData
    {

        public ProvinceData(Color32 provinceColor, string tag, Vector2Int[] edgePixels)
        {
            ProvinceColor = provinceColor;
            Tag = tag;
            EdgePixels = edgePixels;
        }
        public Color32 ProvinceColor { get; }
        public string Tag { get; }
        public Vector2Int[] EdgePixels { get; set; }

    }
    public class MeshGenerationData
    {
        public Texture2D imageTexture;
        public Material faceMaterial;
        public MeshFilter meshFilter;
        public List<int> vertexOrder = new List<int>();
        public List<Vector2Int> vertexPixelLocations = new List<Vector2Int>();
        public List<bool> vertexPixelOnEdge = new List<bool>();
        public List<float> distanceToEdge = new List<float>();
        public Vector2[] vertexPositions = new Vector2[0];
        public Color32[] outlinePixels = new Color32[0];

        public Vector2 minCoords = Vector2.zero;
        public Vector2 maxCoords = Vector2.zero;
        public ProvinceData[] provinceList;

    }
    public class MapMeshGenerator : MonoBehaviour
    {
        [SerializeField]
        private Texture2D inputTexture;
        [SerializeField]
        private Material faceMaterial;
        [SerializeField]
        private MeshFilter meshFilter;
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

            data.provinceList = new ProvinceData[]
            {
                new ProvinceData(a1Color, "A1", new Vector2Int[0]),
                new ProvinceData(a2Color, "A2", new Vector2Int[0]),
                new ProvinceData(a3Color, "A3", new Vector2Int[0])
            };

            FindVertexPixels(data);
            FakeDraw(data);
        }

        public void FindVertexPixels(MeshGenerationData data)
        {
            Dictionary<Color32, List<Vector2Int>> provinceColors = new Dictionary<Color32, List<Vector2Int>>();
            for(int i = 0; i< imagePixels.Length; i++)
            {
                Color32 colorIterative = imagePixels[i];
                if (!provinceColors.ContainsKey(colorIterative))
                {
                    provinceColors.Add(colorIterative, new List<Vector2Int>());
                }
                Vector2Int newUV = ConvertIndexToUV(i);
                provinceColors[colorIterative].Add(newUV);
            }

            for (int i = 0; i < data.provinceList.Length; i++)
            {
                Color32 provCol = data.provinceList[i].ProvinceColor;
                if (provinceColors.ContainsKey(provCol))
                {
                    List<Vector2Int> tempList = new List<Vector2Int>();
                    foreach(Vector2Int pixel in provinceColors[provCol])
                    {
                        if(CheckForColorEdge(pixel, provCol))
                        {
                            tempList.Add(pixel);
                        }
                    }
                    if(tempList.Count != 0)
                    {
                        data.provinceList[i].EdgePixels = tempList.ToArray();
                    }
                }
            }
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
                    point.GetComponent<MeshRenderer>().material.color = entry.ProvinceColor;
                }
            }
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
    }
}

