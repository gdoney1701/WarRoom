using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterMeshGenerator {

    public class MeshGeneratorData {
        // just so we can pass this around and not have to deal with returning multiple things.
        public Texture2D imageTexture;
        public Texture2D imageOutline;
        public Material faceMaterial; // the cardboard edge to use!
        public Material edgeMaterial; // the cardboard edge to use!
        public MeshFilter meshFilter;
        public Color vertexPixelColor = Color.black;
        public List<int> vertexOrder = new List<int>(); // for the order of the face!
        public List<Vector2Int> vertexPixelLocations = new List<Vector2Int>();
        public List<bool> vertexPixelOnEdge = new List<bool>(); // is the associated vertex pixel on the edge of transparency?
        public List<float> distanceToEdge = new List<float>(); // is the associated vertex pixel on the edge of transparency?
        public Vector2[] vertexPositions = new Vector2[0];
        public Color[] outlinePixels = new Color[0];

        public Vector2 minCoords = Vector2.zero;
        public Vector2 maxCoords = Vector2.zero;

        public Color GetOutlinePixelColor(int x, int y) {
            int index = y * imageOutline.width + x; // convert it to index!
            if (index < 0 || index >= outlinePixels.Length) {
                Debug.LogError("Error trying pixel outside of rect");
                return new Color(0, 0, 0, 0); // I guess just make it transparent.
            }
            return outlinePixels[index];
        }

        public bool CoordsInsideImage(int x, int y) {
            return x >= 0 && x < imageOutline.width && y >= 0 && y < imageOutline.height;
        }

        public Color SmartGetOutlinePixelColor(int x, int y) {
            if (!CoordsInsideImage(x, y)) {
                return new Color(0, 0, 0, 0);
            }
            return GetOutlinePixelColor(x, y);
        }

        public List<Vector3> GetVertexPositionsForMesh(Vector3 offset) {
            List<Vector3> verts = new List<Vector3>();
            for (int i = 0; i < vertexPositions.Length; i++) {
                verts.Add(offset + (Vector3)vertexPositions[i]);
            }
            return verts;
        }

        public float VertexOrderEdgeSum() {
            // loop over the order and sum the distances or whatever to check if they're clockwise or not
            float sum = 0;
            for (int i = 0; i < vertexOrder.Count; i++) {
                Vector2 i1 = vertexPixelLocations[vertexOrder[i]];
                Vector2 i2 = vertexPixelLocations[vertexOrder[(i+1)%vertexOrder.Count]];
                sum += (i2.x - i1.x) * (i2.y + i1.y);
            }
            return sum;
        }

        public List<Vector2> GetUVsForMesh() {
            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < vertexOrder.Count; i++) {
                Vector2 uvPos = new Vector2((float)vertexPixelLocations[vertexOrder[i]].x / imageOutline.width,
                            (float)vertexPixelLocations[vertexOrder[i]].y / imageOutline.height);
                uvs.Add(uvPos);
            }
            return uvs;
        }

        public void ConvertPixelVertexPositionsToVector2s(float scale = 1) {
            List<Vector2> realLocations = new List<Vector2>();
            minCoords = new Vector2(1000, 1000);
            maxCoords = new Vector2(-1000, -1000);
            string output = "Pixel order:";
            for (int i = 0; i < vertexOrder.Count; i++) {
                // also put them in order!
                Vector2Int pixelLoc = vertexPixelLocations[vertexOrder[i]];
                output += "\n" + pixelLoc.ToString();
                Vector2 pos = new Vector2((float) pixelLoc.x / imageOutline.width, (float) pixelLoc.y / imageOutline.height) * scale;
                minCoords = Vector2.Min(minCoords, pos);
                maxCoords = Vector2.Max(maxCoords, pos);
                realLocations.Add(pos);
            }
            // Debug.Log(output);
            // Debug.Log("Min " + min + " Max " + max);
            vertexPositions = realLocations.ToArray();
            // Debug.Log("here vert positions count " + vertexPositions.Length);
        }

        public bool CheckVertexAngle(Vector2Int startPos, Vector2Int currentPos, Vector2Int vertexToCheck, float maxAngleDegrees) {
            Vector2 currentLine = currentPos - startPos;
            Vector2 lineToCheck = vertexToCheck - startPos;
            if (Vector2.Angle(currentLine, lineToCheck) < maxAngleDegrees) {
                return true;
            }
            return false;
        }

        public static float ManhattanCoords(int x, int y, int x2, int y2) {
            return Mathf.Abs(x - x2) + Mathf.Abs(y - y2);
        }

        public List<Vector2> GetEdgeUVs(float scale, bool includeExtra = true, float startingOffset = 0) {
            // get the UV coords for the cardboard edge! We're doing it based on distance between them which makes sense.
            List<Vector2> uvs = new List<Vector2>();
            List<Vector2> backUvs = new List<Vector2>();
            // first the front then the back makes enough sense yeah.
            float xPos = startingOffset;
            int numToAdd = vertexPositions.Length;
            if (includeExtra) {
                numToAdd++; // to account for connecting the UVs better with a jump but not a weird morph along the entire length.
            }
            for (int i = 0; i < numToAdd; i++) {
                // just compare the distances!
                uvs.Add(new Vector2(xPos, 0));
                backUvs.Add(new Vector2(xPos, 1));
                float distance = scale * Vector2.Distance(vertexPositions[i%vertexPositions.Length], vertexPositions[(i+1)%vertexPositions.Length]);
                xPos += distance;
            }
            uvs.AddRange(backUvs);
            return uvs;
        }

        public float DistanceToEdge(int x, int y) {
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(new Vector2Int(x, y));
            visited.Add(new Vector2Int(x, y));
            Vector2Int current;
            while (frontier.Count > 0) {
                current = frontier.Dequeue();
                if (SmartGetOutlinePixelColor(current.x, current.y).a == 0) {
                    return Vector2.Distance(current, new Vector2(x, y));
                }
                // otherwise add the neighbors to the queue!
                Vector2Int left = new Vector2Int(current.x - 1, current.y);
                if (!visited.Contains(left)) {
                    visited.Add(left);
                    frontier.Enqueue(left);
                }
                Vector2Int right = new Vector2Int(current.x + 1, current.y);
                if (!visited.Contains(right)) {
                    visited.Add(right);
                    frontier.Enqueue(right);
                }
                Vector2Int up = new Vector2Int(current.x, current.y - 1);
                if (!visited.Contains(up)) {
                    visited.Add(up);
                    frontier.Enqueue(up);
                }
                Vector2Int down = new Vector2Int(current.x, current.y + 1);
                if (!visited.Contains(down)) {
                    visited.Add(down);
                    frontier.Enqueue(down);
                }
            }
            Debug.LogError("Couldn't find transparent pixel from (" + x + ", " + y + ") after visiting " + visited.Count);
            return -1; // I guess? Not great....
        }

        public void FloodFillFromFirstPoint() {
            // for the first vertex on the list we don't know which way we're traveling, so we check both ways until we find the vertex.
            if (vertexOrder.Count != 1) {
                Debug.LogError("Error flood fill first vertex count is " + vertexOrder.Count);
                return;
            }
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(vertexPixelLocations[vertexOrder[0]]);
            Vector2Int current = vertexPixelLocations[vertexOrder[0]];
            List<Vector2Int> foundVertexPixels = new List<Vector2Int>();
            // now loop over the frontier until we find a new vertex position!
            bool foundNext = false;
            while (frontier.Count > 0) {
                current = frontier.Dequeue();
                if (visited.Contains(current)) {
                    continue;
                }
                visited.Add(current);
                List<Vector2Int> newNeighbors = GetEdgeNeighbors(current.x, current.y, visited, true);
                for (int i = 0; i < newNeighbors.Count; i++) {
                    frontier.Enqueue(newNeighbors[i]);
                    // visited.Add(newNeighbors[i]); // we actually do that once we visit it now to prevent getting stuck after we find the
                    // second vertex.
                    if (vertexPixelLocations.Contains(newNeighbors[i]) && !foundVertexPixels.Contains(newNeighbors[i])) {
                        // then we found our next point! Now we should keep building the frontier from this point onwards likely.
                        vertexOrder.Add(vertexPixelLocations.IndexOf(newNeighbors[i]));
                        foundVertexPixels.Add(newNeighbors[i]);
                        foundNext = true;
                        current = newNeighbors[i];
                        break;
                    }
                    int insideConnectedVertex = CheckIfConnectedToInsideVertex(newNeighbors[i].x, newNeighbors[i].y);
                    if (insideConnectedVertex != -1) {
                        // also check if the distance to an inside vertex is less than the distance from it to the edge!
                        // then we got connected!
                        vertexOrder.Add(insideConnectedVertex);
                        foundNext = true;
                        current = vertexPixelLocations[insideConnectedVertex];
                        foundVertexPixels.Add(vertexPixelLocations[insideConnectedVertex]);
                        break;
                    }
                }
                if (foundNext) {
                    break;
                }
            }

            if (!foundNext) {
                Debug.LogError("Couldn't find next! Uh Oh!");
            }

            // now we should actually just continue on and find all the rest from that order...
            // make sure to check if we've found the inside vertex or not as well!
            // we started at the first pixel location and we're currently at the Current variable location pixel (which is on the edge even if
            // we found an inside vertex pixel. Just keep searching from that pixel with a new frontier)
            // we may need to actually only add to visted once it becomes current otherwise we may have issues with not being able to 
            // move on from this pixel locaiton.
            // did that, now just continue the flood fill.
            Queue<Vector2Int> oldFrontier = frontier;
            frontier = new Queue<Vector2Int>();
            // now we continue with frontier from that location.
            frontier.Enqueue(current);
            visited.Remove(current); // make sure we can start from this location!
            while (frontier.Count > 0 && vertexOrder.Count < vertexPixelLocations.Count) {
                // loop around until we find all the vertices!
                current = frontier.Dequeue();
                if (visited.Contains(current)) {
                    continue;
                }
                visited.Add(current);
                List<Vector2Int> newNeighbors = GetEdgeNeighbors(current.x, current.y, visited, true);
                for (int i = 0; i < newNeighbors.Count; i++) {
                    frontier.Enqueue(newNeighbors[i]);
                    // visited.Add(newNeighbors[i]); // we actually do that once we visit it now to prevent getting stuck after we find the
                    // second vertex.
                    if (vertexPixelLocations.Contains(newNeighbors[i]) && !foundVertexPixels.Contains(newNeighbors[i])) {
                        // then we found our next point! Now we should keep building the frontier from this point onwards likely.
                        vertexOrder.Add(vertexPixelLocations.IndexOf(newNeighbors[i]));
                        foundVertexPixels.Add(newNeighbors[i]);
                        // foundNext = true;
                        // current = newNeighbors[i];
                        // break;
                    }
                    int insideConnectedVertex = CheckIfConnectedToInsideVertex(newNeighbors[i].x, newNeighbors[i].y);
                    if (insideConnectedVertex != -1) {
                        // also check if the distance to an inside vertex is less than the distance from it to the edge!
                        // then we got connected!
                        vertexOrder.Add(insideConnectedVertex);
                        foundVertexPixels.Add(vertexPixelLocations[insideConnectedVertex]);
                        // foundNext = true;
                        // current = vertexPixelLocations[insideConnectedVertex];
                        // break;
                    }
                }
                // if (foundNext) {
                //     break;
                // }
            }

            if (vertexOrder.Count < vertexPixelLocations.Count) {
                Debug.LogError("Error couldn't find all the vertices");
            }
            // Debug.Log("Found vertex order " + vertexOrder.Count + " and vertex pixel locs " + vertexPixelLocations.Count);
        }
        
        // public bool CheckForInsideVertexConnections() {
        //     // loop over all the vertices to see if this line could go straight towards a vertex that's inside the border
        // nah we're just going to compare distances to the pixels
        // }

        public int CheckIfConnectedToInsideVertex(int x, int y) {
            // just check if the distance from this pixel is close to any of the un-used vertices that aren't on the edge!
            for (int i = 0; i < vertexPixelLocations.Count; i++) {
                if (vertexPixelOnEdge[i] || vertexOrder.Contains(i)) {
                    // it's not inside the shape or it's already visted.
                    continue;
                }
                // now check the radius!
                // float currentRadius = Vector2.Distance(new Vector2(x, y), vertexPixelLocations[i]);
                float currentDistance = ManhattanCoords(x, y, vertexPixelLocations[i].x, vertexPixelLocations[i].y);
                if (currentDistance <= distanceToEdge[i]) {
                    // Debug.LogWarning("Found an inside one");
                    return i;
                }
            }
            return -1; // didn't find anything to connect to, which is expected 99% of the time.
        }

        public static List<int> GetTrianglesConnectingQuadLoops(int numVerts, int firstOffset, int secondOffset) {
            List<int> tris = new List<int>();
            for (int i = 0; i < numVerts; i++) {
                int a1 = firstOffset + i;
                int a2 = firstOffset + ((i + 1) % numVerts);
                int b1 = secondOffset + i;
                int b2 = secondOffset + ((i + 1) % numVerts);
                tris.AddRange(GetTrianglesConnectingQuad(a1, a2, b1, b2));
            }
            return tris;
        }

        public static List<int> GetTrianglesConnectingQuad(int a1, int a2, int b1, int b2) {
            // connect a quad between these I guess? We'll have to see how it goes. This is for making the edge of the shape.
            // the shape is this:
            // a1 --- a2
            // | \     |
            // |  \    |
            // |   \   |
            // |    \  |
            // |     \ |
            // b1 --- b2
            // with the idea that the A verts are on one flat level/loop and the B verts are on another
            return new List<int>() { a1, b2, b1, a1, a2, b2 };
        }

        public List<Vector2Int> GetEdgeNeighbors(int x, int y, HashSet<Vector2Int> visted, bool includeDiagonals = true) {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            if (CoordsInsideImage(x - 1, y) && IsPixelOnBorder(x - 1, y, includeDiagonals)) {
                Vector2Int loc = new Vector2Int(x - 1, y);
                if (!visted.Contains(loc)) {
                    neighbors.Add(loc);
                }
            }
            if (CoordsInsideImage(x + 1, y) && IsPixelOnBorder(x + 1, y, includeDiagonals)) {
                Vector2Int loc = new Vector2Int(x + 1, y);
                if (!visted.Contains(loc)) {
                    neighbors.Add(loc);
                }
            }
            if (CoordsInsideImage(x, y - 1) && IsPixelOnBorder(x, y - 1, includeDiagonals)) {
                Vector2Int loc = new Vector2Int(x, y - 1);
                if (!visted.Contains(loc)) {
                    neighbors.Add(loc);
                }
            }
            if (CoordsInsideImage(x, y + 1) && IsPixelOnBorder(x, y + 1, includeDiagonals)) {
                Vector2Int loc = new Vector2Int(x, y + 1);
                if (!visted.Contains(loc)) {
                    neighbors.Add(loc);
                }
            }
            if (includeDiagonals) {
                if (CoordsInsideImage(x - 1, y - 1) && IsPixelOnBorder(x - 1, y - 1, includeDiagonals)) {
                    Vector2Int loc = new Vector2Int(x - 1, y - 1);
                    if (!visted.Contains(loc)) {
                        neighbors.Add(loc);
                    }
                }
                if (CoordsInsideImage(x + 1, y - 1) && IsPixelOnBorder(x + 1, y - 1, includeDiagonals)) {
                    Vector2Int loc = new Vector2Int(x + 1, y - 1);
                    if (!visted.Contains(loc)) {
                        neighbors.Add(loc);
                    }
                }
                if (CoordsInsideImage(x - 1, y + 1) && IsPixelOnBorder(x - 1, y + 1, includeDiagonals)) {
                    Vector2Int loc = new Vector2Int(x - 1, y + 1);
                    if (!visted.Contains(loc)) {
                        neighbors.Add(loc);
                    }
                }
                if (CoordsInsideImage(x + 1, y + 1) && IsPixelOnBorder(x + 1, y + 1, includeDiagonals)) {
                    Vector2Int loc = new Vector2Int(x + 1, y + 1);
                    if (!visted.Contains(loc)) {
                        neighbors.Add(loc);
                    }
                }
            }
            return neighbors;
        }

        public bool IsPixelOnBorder(int x, int y, bool includeDiagonals = true) {
            // return true if the pixel coords are for a solid pixel and at least one of the neighbors is transparent.
            Color c = GetOutlinePixelColor(x, y);
            if (c.a < 1) {
                // then it's not a border since it's not solid.
                return false;
            }
            c = SmartGetOutlinePixelColor(x - 1, y);
            if (c.a == 0) {
                return true;
            }
            c = SmartGetOutlinePixelColor(x + 1, y);
            if (c.a == 0) {
                return true;
            }
            c = SmartGetOutlinePixelColor(x, y - 1);
            if (c.a == 0) {
                return true;
            }
            c = SmartGetOutlinePixelColor(x, y + 1);
            if (c.a == 0) {
                return true;
            }
            if (includeDiagonals) {
                c = SmartGetOutlinePixelColor(x - 1, y - 1);
                if (c.a == 0) {
                    return true;
                }
                c = SmartGetOutlinePixelColor(x + 1, y + 1);
                if (c.a == 0) {
                    return true;
                }
                c = SmartGetOutlinePixelColor(x + 1, y - 1);
                if (c.a == 0) {
                    return true;
                }
                c = SmartGetOutlinePixelColor(x - 1, y + 1);
                if (c.a == 0) {
                    return true;
                }
            }
            return false; // not on the border!
        }
    }

    public class CharacterMeshGenerator : MonoBehaviour
    {
        public float cardboardWidth = .05f;
        public float cardboardEdgeTextureScale = 1;
        public Texture2D imageTexture;
        public Texture2D imageOutline;
        public Material originalFaceMaterial; // the cardboard edge to use!
        public Material edgeMaterial; // the cardboard edge to use!
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Color vertexPixelColor = Color.black;

        [Tooltip("The additional distance to fuzz it by when searching for the inside pixels so that floating point issues don't mess it up")]
        public float distanceToEdgeFuzz = 1;

        public void OnValidate() {
            if (meshFilter == null) {
                meshFilter = GetComponent<MeshFilter>();
            }
        }

        public void EditorEnsureTextureReadable(Texture2D input) {
            #if UNITY_EDITOR
            if (input != null) {
                string path = UnityEditor.AssetDatabase.GetAssetPath(input);
                UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
                importer.isReadable = true;
                importer.filterMode = FilterMode.Point;
                UnityEditor.TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                platformSettings.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(platformSettings);
                importer.SaveAndReimport();
                // Debug.Log("Set texture at " + path + " to be readable");
            }
            #endif
        }

        public Texture2D RuntimeEnsureTextureIsReadable(Texture2D input) {
            #if UNITY_EDITOR
            EditorEnsureTextureReadable(input); // gotta make sure it has no compression etc.
            return input; // in the editor we'll just edit the asset.
            #else

            if (input.isReadable) {
                return input;
            }

            // in runtime we'll create a new texture that is readable
            // RenderTexture prevActive = RenderTexture.active;
            // UnityEngine.Experimental.Rendering.GraphicsFormat format = input.graphicsFormat;
            

            // RenderTexture outrt = new RenderTexture(input.width, input.height, 64, input.graphicsFormat);
            // Texture2D output = new Texture2D(input.width, input.height, input.format, false);
            // Graphics.Blit(input, output);

            // RenderTexture.active = prevActive;
            // return output;
            Debug.LogWarning("Not implemented section to convert texture to readable");
            return input;
            #endif
        }

        // [ContextMenu("Test Generate Mesh")]
        public MeshGeneratorData GenerateMesh() {
            MeshGeneratorData data = new MeshGeneratorData();
            data.imageTexture = imageTexture;
            data.imageOutline = RuntimeEnsureTextureIsReadable(imageOutline);
            data.edgeMaterial = edgeMaterial;
            data.meshFilter = meshFilter;
            data.vertexPixelColor = vertexPixelColor;
            data.outlinePixels = data.imageOutline.GetPixels(0, 0, data.imageOutline.width, data.imageOutline.height);

            FindVertexPixels(data);
            if (data.vertexPixelLocations.Count == 0) {
                Debug.LogError("Error couldn't find vertex pixels");
                return data;
            }
            // Debug.Log("Vert pixl locations count " + data.vertexPixelLocations.Count);
            int topLeft = FindTopLeftPoint(data, true);
            data.vertexOrder.Add(topLeft);
            // Debug.Log("Vert count should be 1 and is: " + data.vertexOrder.Count);
            // now we need to loop around them!
            // just flood fill from the first point until we find an edge, we can figure out if it's clockwise or counterclockwise later
            // Debug.Log("vert pos count here before flood " + data.vertexPositions.Length + " order " + data.vertexOrder.Count);
            // Debug.Log("vertex index " + data.vertexOrder[0]);
            data.FloodFillFromFirstPoint(); // this will actually just do the whole thing it should be renamed...
            // Debug.Log("vert pos count first " + data.vertexPositions.Length + " order " + data.vertexOrder.Count);
            // Debug.Log(data.VertexOrderEdgeSum());
            if (data.VertexOrderEdgeSum() > 0) {
                data.vertexOrder.Reverse(); // while the triangulator can handle it the edge loops can't so correct the order!
            }
            
            data.ConvertPixelVertexPositionsToVector2s();

            Debug.Assert(data.vertexOrder.Count == data.vertexPixelLocations.Count);

            float centerX = -(data.minCoords.x + data.maxCoords.x) / 2 + .5f;
            

            Vector3 centeringOffset = new Vector3(centerX, -data.minCoords.y, -cardboardWidth / 2);

            // now we have the vertexPositions array which we should copy since we'll need that for a few things!
            List<Vector3> topVertices = data.GetVertexPositionsForMesh(centeringOffset);
            List<Vector3> middleTopVertices = data.GetVertexPositionsForMesh(centeringOffset);
            List<Vector3> middleBottomVertices = data.GetVertexPositionsForMesh(centeringOffset + cardboardWidth * Vector3.forward);
            List<Vector3> bottomVertices = data.GetVertexPositionsForMesh(centeringOffset + cardboardWidth * Vector3.forward);
            // now for the middle mesh we add overlapping edges so that the uv coords at least mesh if not perfectly.
            middleTopVertices.Add(middleTopVertices[0]);
            middleBottomVertices.Add(middleBottomVertices[0]);
            
            List<Vector2> frontFaceUVs = data.GetUVsForMesh();

            List<Vector3> allVerts = new List<Vector3>();
            allVerts.AddRange(topVertices);
            allVerts.AddRange(bottomVertices);
            allVerts.AddRange(middleTopVertices);
            allVerts.AddRange(middleBottomVertices);

            List<Vector2> uvs = new List<Vector2>();
            uvs.AddRange(frontFaceUVs);
            uvs.AddRange(frontFaceUVs); // for the back face!
            uvs.AddRange(data.GetEdgeUVs(cardboardEdgeTextureScale));
            // uvs.AddRange(frontFaceUVs); // temp so that it's the same size
            // uvs.AddRange(frontFaceUVs); // temp so that it's the same size

            // Debug.Log("vert pos count " + data.vertexPositions.Length);
            // now we _should_ have our vertices!

            Triangulator triangulator = new Triangulator(data.vertexPositions);
            // now actually make the mesh side I guess?

            List<int> faceTris = new List<int>();
            faceTris.AddRange(triangulator.Triangulate()); // for the front face
            //faceTris.AddRange(triangulator.Triangulate(topVertices.Count, true)); // for the back face?
            
            int numEdgeVerts = middleTopVertices.Count;
            List<int> edgeTris = MeshGeneratorData.GetTrianglesConnectingQuadLoops(numEdgeVerts, topVertices.Count * 2,
                                            topVertices.Count * 2 + numEdgeVerts);

            Mesh m = new Mesh();
            m.name = imageTexture.name + " Mesh";

            // List<Vector3> vertices = new List<Vector3>();
            // vertices.AddRange(data.GetVertexPositionsForMesh(Vector3.zero)); // start with the base mesh face for testing.
            
            // Debug.Log("Vert count " + vertices.Count);
            // Debug.Log("tri count " + triangulator.Triangulate().Length);

            m.subMeshCount = 2;
            m.SetVertices(allVerts);
            m.SetUVs(0, uvs);
            m.SetTriangles(faceTris, 0);
            m.SetTriangles(edgeTris, 1); // for the side rings!
            m.RecalculateNormals();
            m.RecalculateBounds();

            data.meshFilter.mesh = m;

            Material faceMat = new Material(originalFaceMaterial);
            faceMat.name = data.imageTexture.name + " face mat";
            faceMat.SetTexture("_BaseMap", data.imageTexture);
            // Debug.Log(string.Join(", ", faceMat.GetTexturePropertyNames()));
            data.faceMaterial = faceMat;

            return data;
        }

        [ContextMenu("Create and use")]
        public MeshGeneratorData CreateAndUseMesh() {
            MeshGeneratorData data = GenerateMesh();
            // meshRenderer.material = data.faceMaterial;
            meshRenderer.materials = new Material[2] { data.faceMaterial, data.edgeMaterial };
            // meshRenderer.gameObject.GetComponent<MeshCollider>().sharedMesh = data.meshFilter.sharedMesh;
            return data;
        }

        [ContextMenu("Create Character Prefab")]
        public MeshGeneratorData CreateCharacterPrefab() {
            MeshGeneratorData data = GenerateMesh();
            // meshRenderer.material = data.faceMaterial;
            meshRenderer.materials = new Material[2] { data.faceMaterial, data.edgeMaterial };
            meshRenderer.gameObject.GetComponent<MeshCollider>().sharedMesh = data.meshFilter.sharedMesh;
            // now set the name of the prefab!
            string newName = imageTexture.name;
            if (imageTexture.name.Length > 0) {
                // then make sure it's capital to start!
                newName = ("" + imageTexture.name[0]).ToUpper() + newName.Substring(1);
            }
            newName += "Prefab";
            gameObject.name = newName;
            return data;
        }

        public float DistanceToEdgeOLD(int x, int y, MeshGeneratorData data) {
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(new Vector2Int(x, y));
            visited.Add(new Vector2Int(x, y));
            Vector2Int current;
            while (frontier.Count > 0) {
                current = frontier.Dequeue();
                if (data.SmartGetOutlinePixelColor(current.x, current.y).a == 0) {
                    return Vector2.Distance(current, new Vector2(x, y));
                }
                // otherwise add the neighbors to the queue!
                Vector2Int left = new Vector2Int(current.x - 1, current.y);
                if (!visited.Contains(left)) {
                    visited.Add(left);
                    frontier.Enqueue(left);
                }
                Vector2Int right = new Vector2Int(current.x + 1, current.y);
                if (!visited.Contains(right)) {
                    visited.Add(right);
                    frontier.Enqueue(right);
                }
                Vector2Int up = new Vector2Int(current.x, current.y - 1);
                if (!visited.Contains(up)) {
                    visited.Add(up);
                    frontier.Enqueue(up);
                }
                Vector2Int down = new Vector2Int(current.x, current.y + 1);
                if (!visited.Contains(down)) {
                    visited.Add(down);
                    frontier.Enqueue(down);
                }
            }
            Debug.LogError("Couldn't find transparent pixel from (" + x + ", " + y + ") after visiting " + visited.Count);
            return -1; // I guess? Not great....
        }

        public bool ComparePixelColors(Color a, Color b, bool ignoreAlpha = true) {
            // Debug.LogWarning(Vector4.Distance(new Vector4(a.r, a.g, a.b, a.a), new Vector4(b.r, b.g, b.b, b.a)));
            // Debug.LogWarning(a);
            if (ignoreAlpha) {
                return Vector3.Distance(new Vector3(a.r, a.g, a.b), new Vector3(b.r, b.g, b.b)) < 1/255f; // just make it similar enough I guess?
            }
            // return Vector4.Distance(new Vector4(a.r, a.g, a.b, a.a), new Vector4(b.r, b.g, b.b, b.a)) < 1/255f;
            if (a.a != b.a) {
                return false; // not the same! the alphas are too dissimilar
            }
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
            // return Vector3.Distance(new Vector3(a.r, a.g, a.b), new Vector3(b.r, b.g, b.b)) < .25f; // just make it similar enough I guess?
        }

        public List<Vector2Int> FindVertexPixels(MeshGeneratorData data) {
            // find the black pixels on the image!
            List<Vector2Int> pixels = new List<Vector2Int>();
            int numOffEdge = 0;
            List<bool> onEdge = new List<bool>();
            List<float> distanceToEdge = new List<float>();
            if (data.imageOutline == null) {
                return pixels; // can't do it with no image!
            }
            HashSet<Color> colors = new HashSet<Color>();
            // now loop over the image?
            Color[] imagePixels = data.imageOutline.GetPixels(0, 0, data.imageOutline.width, data.imageOutline.height);
            // Debug.Log("Num pixels in 512x512: " + imagePixels.Length + " should be " + (512*512) + " should be " + (data.imageOutline.width * data.imageOutline.height));
            // Debug.Log(" widht height " + data.imageOutline.width + " and " + data.imageOutline.height);
            for (int y = 0; y < data.imageOutline.height; y++) {
                for (int x = 0; x < data.imageOutline.width; x++) {
                    // is it a vertex pixel?
                    int index = y * data.imageOutline.width + x; // convert it to index!
                    // for testing
                    // Debug.Assert(imagePixels[index] == data.imageOutline.GetPixel(x, y));
                    colors.Add(imagePixels[index]);
                    // if (imagePixels[index] == vertexPixelColor) {
                    if (ComparePixelColors(imagePixels[index], vertexPixelColor, false)) {
                        pixels.Add(new Vector2Int(x, y));
                        bool vertexOnEdge = data.IsPixelOnBorder(x, y);
                        onEdge.Add(vertexOnEdge);
                        if (vertexOnEdge) {
                            distanceToEdge.Add(0);
                        } else {
                            // now we need to flood fill until we find a transparent pixel!
                            distanceToEdge.Add(data.DistanceToEdge(x, y) + distanceToEdgeFuzz);
                            // Debug.Log(new Vector2Int(x, y) + " " + distanceToEdge[distanceToEdge.Count - 1]);
                            numOffEdge++;
                        }
                    }
                }
            }
            // Debug.Log("Num pixels not on edge: " + numOffEdge);
            Debug.Assert(pixels.Count == onEdge.Count);
            Debug.Assert(pixels.Count == distanceToEdge.Count);
            data.vertexPixelLocations = pixels;
            data.vertexPixelOnEdge = onEdge;
            data.distanceToEdge = distanceToEdge;

            return pixels;
        }

        public int FindTopLeftPoint(MeshGeneratorData data, bool mustBeOnEdge) {
            List<Vector2Int> points = data.vertexPixelLocations;
            List<bool> onEdge = data.vertexPixelOnEdge;
            if (points.Count == 0) {
                Debug.LogError("Tried to find top left point of no points");
                return -1;
            }
            Vector2Int pt = new Vector2Int(0, 0);
            int ptIndex = -1;

            for (int i = 1; i < points.Count; i++) {
                if (mustBeOnEdge && !onEdge[i]) {
                    continue;
                }
                if (ptIndex == -1 || points[i].y < pt.y || (points[i].y == pt.y && points[i].x < pt.x)) {
                    pt = points[i];
                    ptIndex = i;
                }
            }

            return ptIndex;
        }


        // // Start is called before the first frame update
        // void Start()
        // {
            
        // }

        // // Update is called once per frame
        // void Update()
        // {
            
        // }
    }

}
