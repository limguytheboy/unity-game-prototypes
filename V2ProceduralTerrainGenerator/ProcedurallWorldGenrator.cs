using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralWorldGenerator : MonoBehaviour
{
    //Threading and ChunkLoader
    [Header("ChunkLoader")]
    Dictionary<Vector2Int, (Mesh, float[,])> ChunkDictionary = new Dictionary<Vector2Int, (Mesh, float[,])>();
    List<GameObject> PlaneList = new List<GameObject>();
    List<Vector2Int> OldMeshChunks = new List<Vector2Int>();
    public Transform PlayerPos;
    Vector3 previousPos;

    GameObject PlaneTemp;
    bool IsTaken;
    bool Cehck;
    LayerMask GroundLayer;

    bool IsChunkLoaderRunning;

    private int RootMapScale;
    private bool[,] MainRootMap;
    private float CellSize = 1;

    float viewDis = 32;

    Vector3[] vertices;
    int[] triangles;

    private bool[,] TempMap;

    private int RootOriginX;
    private int RootOriginY;
    private int CallLimitation;

    private int ChunkScale;
    private int MapScale;
    private bool[,] ChunkRoot;
    private float[,] Chunk;
    private float RootMapToChunkRasio = 1f;//0.05

    public void Start()
    {
        GroundLayer = LayerMask.NameToLayer("Ground");
        RootMapScale = 16;
        ChunkScale = 16; // Make sure this matches RootMapScale
        CellSize = 0.5f; // This affects the size of each vertex in the mesh
        MapScale = (int)(RootMapScale * CellSize - 0.5f);
        GenerateMapRoot();
        ChunkScale = RootMapScale;
        DisplayBoolArray(MainRootMap);
    }

    private void FixedUpdate()
    {
        if (!IsChunkLoaderRunning)
        {
            ChunkLoader();
        }
    }

    void DisplayBoolArray(bool[,] array)
    {
        string output = "";
        for (int i = 0; i < array.GetLength(0); i++) // Loop through rows
        {
            for (int j = 0; j < array.GetLength(1); j++) // Loop through columns
            {
                output += array[i, j] ? "# " : "... "; // Represent true as 1, false as 0
            }
            output += "\n"; // Newline after each row
        }
        Debug.Log(output); // Print to Unity Console
    }
    void DisplayFloatArray(float[,] array)
    {
        string output = "";
        for (int i = 0; i < array.GetLength(0); i++) // Loop through rows
        {
            for (int j = 0; j < array.GetLength(1); j++) // Loop through columns
            {
                output += array[i, j]; // Represent true as 1, false as 0
            }
            output += "\n"; // Newline after each row
        }
        Debug.Log(output); // Print to Unity Console
    }

    //Setting and Building details for each chunk
    private void BuildChunk(Vector2Int ChunkOffset)
    {
        ChunkRoot = new bool[ChunkScale, ChunkScale];
        int OriX = Random.Range(0, ChunkScale - 1), OriY = Random.Range(0, ChunkScale - 1);
        int countmap = 0;
        while (countmap < 20)
        {
            countmap = 0;
            NextBlock(OriX, OriY, 20, true);
            for (int i = 0; i < RootMapScale; i++)
            {
                for (int j = 0; j < RootMapScale; j++)
                {
                    if (TempMap[i, j])
                        countmap++;
                }
            }
        }
        ChunkRoot = TempMap;

        int RMTCRx = (int)(ChunkOffset.x * (1 / RootMapToChunkRasio - 1));
        int RMTCRy = (int)(ChunkOffset.y * (1 / RootMapToChunkRasio - 1));

        for (int x = 0; x < ChunkScale; x++)
        {
            for (int y = 0; y < ChunkScale; y++)
            {
                if (!ChunkRoot[x, y])
                {
                    // If RootMapToChunkRasio is 0.25f, then dividing by it is same as multiplying by 4
                    // This gives us the corresponding position in MainRootMap
                    int mainX = (int)(ChunkOffset.x * ChunkScale * RootMapToChunkRasio + x * RootMapToChunkRasio);
                    int mainY = (int)(ChunkOffset.y * ChunkScale * RootMapToChunkRasio + y * RootMapToChunkRasio);

                    if (mainX >= 0 && mainX < MainRootMap.GetLength(0) &&
                        mainY >= 0 && mainY < MainRootMap.GetLength(1))
                    {
                        ChunkRoot[x, y] = MainRootMap[mainX, mainY];
                    }
                    else
                    {
                        ChunkRoot[x, y] = false;
                    }
                }
            }
        }

        Chunk = new float[ChunkScale, ChunkScale];
        //for (int i = 0; i < ChunkScale; i++)
        //{
        //    for (int j=0; j < ChunkScale; j++)
        //    {
        //        Chunk[i, j] = 1;
        //    }
        //}
        BoolMapToFloat(ChunkRoot, ref Chunk, 5f);

        for (int i = 1; i < 3; i++)
        {
            PerlinNoiseMap(ChunkOffset, ref Chunk, i * 10f);
        }
        for (int x = 0; x < ChunkScale; x++)
        {
            for (int y = 0; y < ChunkScale; y++)
            {
                //if (ChunkRoot[x, y]) Chunk[x, y] = 3;
                Chunk[x, y] *= 5f;
            }
        }
        BlurringMap(ref Chunk, ChunkScale);
        // Apply smoothing
        SmoothingBTWChunks(ref Chunk, ChunkOffset);
        SyncChunks(ChunkOffset);
    }
    private void SyncChunks(Vector2Int ChunkPos)
    {
        Vector2Int[] direc = new Vector2Int[8]
        {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1,0),
            new Vector2Int(0,0),
            new Vector2Int(0,2),
            new Vector2Int(2,0),
            new Vector2Int(2,2),
        };
        Vector2Int PlayerChunk = new Vector2Int(Mathf.CeilToInt(PlayerPos.transform.position.x) / MapScale,
                             Mathf.CeilToInt(PlayerPos.transform.position.z) / MapScale);
        for (int i = 0; i < PlaneList.Count; i++)
        {
            PlaneTemp = PlaneList[i];

            for (int k = 0; k < 8; k++)
            {
                if (Mathf.CeilToInt(PlaneTemp.transform.position.x) / MapScale == ChunkPos.x + direc[k].x
                && Mathf.CeilToInt(PlaneTemp.transform.position.z) / MapScale == ChunkPos.y + direc[k].y
                && PlaneTemp.activeSelf)
                {
                    MeshGridBuilder(ChunkScale, ChunkScale, lfd, ChunkDictionary[new Vector2Int(ChunkPos.x + direc[k].x, ChunkPos.y + direc[k].y)].Item2, ref PlaneTemp);
                    break;
                }
            }
        }
    }
    int lfd = 2;
    private void MeshGridBuilder(int height, int width, int lfd, float[,] _Chunk, ref GameObject _Plane)
    {
        if (lfd >= ChunkScale)
        {
            lfd = ChunkScale - 1;
        }
        else if (lfd <= 0)
            lfd = 1;
        vertices = new Vector3[height * width * 6 / (lfd * lfd)];
        triangles = new int[vertices.Length];
        int vertexIndex = 0;

        int a = lfd;
        float CellSizeMulti = CellSize;
        for (int z = 0; z <= height - lfd; z += lfd)  // Changed < to <=
        {
            for (int x = 0; x <= width - lfd; x += lfd)  // Changed < to <=
            {
                // Calculate actual end coordinates considering array bounds
                int nextX = Math.Min(x + a, width - 1);
                int nextZ = Math.Min(z + a, height - 1);

                vertices[vertexIndex] = new Vector3(x * CellSizeMulti, _Chunk[x, z], z * CellSizeMulti);
                vertices[vertexIndex + 1] = new Vector3(x * CellSizeMulti, _Chunk[x, nextZ], nextZ * CellSizeMulti);
                vertices[vertexIndex + 2] = new Vector3(nextX * CellSizeMulti, _Chunk[nextX, z], z * CellSizeMulti);
                vertices[vertexIndex + 3] = new Vector3(nextX * CellSizeMulti, _Chunk[nextX, z], z * CellSizeMulti);
                vertices[vertexIndex + 4] = new Vector3(x * CellSizeMulti, _Chunk[x, nextZ], nextZ * CellSizeMulti);
                vertices[vertexIndex + 5] = new Vector3(nextX * CellSizeMulti, _Chunk[nextX, nextZ], nextZ * CellSizeMulti);

                triangles[vertexIndex] = vertexIndex;
                triangles[vertexIndex + 1] = vertexIndex + 1;
                triangles[vertexIndex + 2] = vertexIndex + 2;
                triangles[vertexIndex + 3] = vertexIndex + 3;
                triangles[vertexIndex + 4] = vertexIndex + 4;
                triangles[vertexIndex + 5] = vertexIndex + 5;
                vertexIndex += 6;
            }
        }
        _Plane.GetComponent<MeshFilter>().mesh.Clear();
        _Plane.GetComponent<MeshFilter>().mesh.vertices = vertices;
        _Plane.GetComponent<MeshFilter>().mesh.triangles = triangles;

        _Plane.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        Destroy(_Plane.GetComponent<MeshCollider>());
        _Plane.AddComponent<MeshCollider>();

        _Plane.GetComponent<MeshCollider>().sharedMesh = PlaneTemp.GetComponent<MeshFilter>().mesh;

    }

    //get height map for terrain
    private void PerlinNoiseMap(Vector2Int ChunkOffset, ref float[,] _map, float PerlinScale)
    {
        int XOffset = ChunkOffset.x;
        int YOffset = ChunkOffset.y;
        for (int x = 0; x < _map.GetLength(0); x++)
        {
            for (int y = 0; y < _map.GetLength(1); y++)
            {
                float XCoord = (((float)x / _map.GetLength(0)) * PerlinScale) + XOffset;
                float YCoord = (((float)y / _map.GetLength(1)) * PerlinScale) + YOffset;
                _map[x, y] *= Mathf.PerlinNoise(XCoord, YCoord);
            }
        }
    }

    private void BoolMapToFloat(bool[,] boolMap, ref float[,] floatMap, float height)
    {
        for (int x = 0; x < boolMap.GetLength(0); x++)
        {
            for (int y = 0; y < boolMap.GetLength(1); y++)
            {
                if (boolMap[x, y]) floatMap[x, y] = height;
                else floatMap[x, y] = 0f;
            }
        }
    }
    //blurring Map Root and shade
    private void BlurringMap(ref float[,] _Chunk, int BlurrMapScale)
    {
        float[,] BlurringMap = new float[BlurrMapScale, BlurrMapScale];

        // First pass
        for (int y = 0; y < BlurrMapScale; y++)
        {
            for (int x = 0; x < BlurrMapScale; x++)
            {
                float sum = 0;
                int count = 0;

                // Bottom-left
                if (x > 0 && y > 0)
                {
                    sum += _Chunk[x, y] * 0.35f +
                          (_Chunk[x - 1, y] + _Chunk[x, y - 1]) * 0.25f +
                          _Chunk[x - 1, y - 1] * 0.15f;
                    count++;
                }

                // Top-left
                if (x > 0 && y < BlurrMapScale - 1)
                {
                    sum += _Chunk[x, y] * 0.35f +
                          (_Chunk[x - 1, y] + _Chunk[x, y + 1]) * 0.25f +
                          _Chunk[x - 1, y + 1] * 0.15f;
                    count++;
                }

                // Bottom-right
                if (y > 0 && x < BlurrMapScale - 1)
                {
                    sum += _Chunk[x, y] * 0.35f +
                          (_Chunk[x + 1, y] + _Chunk[x, y - 1]) * 0.25f +
                          _Chunk[x + 1, y - 1] * 0.15f;
                    count++;
                }

                // Top-right
                if (y < BlurrMapScale - 1 && x < BlurrMapScale - 1)
                {
                    sum += _Chunk[x, y] * 0.35f +
                          (_Chunk[x + 1, y] + _Chunk[x, y + 1]) * 0.25f +
                          _Chunk[x + 1, y + 1] * 0.15f;
                    count++;
                }

                BlurringMap[x, y] = sum / count;
            }
        }

        // Second pass - neighbor averaging
        for (int x = 0; x < BlurrMapScale; x++)
        {
            for (int y = 0; y < BlurrMapScale; y++)
            {
                float sum = BlurringMap[x, y];
                int count = 1;  // Start with 1 for current pixel

                if (x > 0) { sum += BlurringMap[x - 1, y]; count++; }
                if (y > 0) { sum += BlurringMap[x, y - 1]; count++; }
                if (x < BlurrMapScale - 1) { sum += BlurringMap[x + 1, y]; count++; }
                if (y < BlurrMapScale - 1) { sum += BlurringMap[x, y + 1]; count++; }

                _Chunk[x, y] = sum / count;
            }
        }
    }

    private void SmoothingBTWChunks(ref float[,] _CM, Vector2Int offset)
    {
        int size = _CM.GetLength(0);
        Vector2Int[] direc = new Vector2Int[4]
        {
        new Vector2Int(0,1),
        new Vector2Int(1,0),
        new Vector2Int(0,-1),
        new Vector2Int(-1,0),
        };
        Vector2Int[] line = new Vector2Int[4]
        {
        new Vector2Int(0,size-1),
        new Vector2Int(size-1,0),
        new Vector2Int(0,0),
        new Vector2Int(0,0),
        };

        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighborPos = offset + direc[i];
            if (ChunkDictionary.TryGetValue(neighborPos, out var value))
            {
                for (int j = 0; j < _CM.GetLength(0); j++)
                {
                    Vector2Int posChunk = line[i] + new Vector2Int((i + 1) % 2, i % 2) * j;
                    int k = (i + 2) % 4;
                    Vector2Int posDict = line[k] + new Vector2Int((i + 1) % 2, i % 2) * j;

                    if (posChunk.x >= 0 && posChunk.x < _CM.GetLength(0) &&
                        posChunk.y >= 0 && posChunk.y < _CM.GetLength(1) &&
                        posDict.x >= 0 && posDict.x < value.Item2.GetLength(0) &&
                        posDict.y >= 0 && posDict.y < value.Item2.GetLength(1))
                    {
                        float v = (_CM[posChunk.x, posChunk.y] + value.Item2[posDict.x, posDict.y]) * 0.5f;
                        _CM[posChunk.x, posChunk.y] = v;
                        value.Item2[posDict.x, posDict.y] = v;
                    }
                }
                ChunkDictionary[neighborPos] = (null, value.Item2);
            }
        }
    }

    //generate the shape of terrain
    private void GenerateMapRoot()
    {
        MainRootMap = new bool[RootMapScale, RootMapScale];

        // Starting point of map generator
        RootOriginX = Random.Range((int)(RootMapScale * 0.3f), (int)(RootMapScale * 0.7f));
        RootOriginY = Random.Range((int)(RootMapScale * 0.3f), (int)(RootMapScale * 0.7f));

        RootOriginX = 0;
        RootOriginY = 0;

        CallLimitation = 30;
        int countmap = 0;
        while (countmap < 20)
        {
            countmap = 0;
            NextBlock(RootOriginX, RootOriginY, CallLimitation, true);
            for (int i = 0; i < RootMapScale; i++)
            {
                for (int j = 0; j < RootMapScale; j++)
                {
                    if (TempMap[i, j])
                        countmap++;
                }
            }
        }
        MainRootMap = TempMap;
    }

    // Generate the next block in a random direction
    private void NextBlock(int x, int y, int callLimit, bool Start)
    {
        if (Start)
        {
            TempMap = new bool[(int)RootMapScale, (int)RootMapScale];
            Array.Clear(TempMap, 0, TempMap.Length - 1);
        }
        // Where the next block will be
        int randomDirection = 0;

        // Place the current block
        TempMap[x, y] = true;

        // Decide to continue building or stop
        if (Random.Range(0, 12) != 0 && callLimit > 0)
        {
            callLimit--;
            // Decide to split into two blocks or not
            int splitAmount = Random.Range(0, 2) != 0 ? 1 : 2;
            for (int i = 0; i < splitAmount; i++)
            {
                int attempts = 10; // Limit the number of attempts to avoid infinite loops
                bool found = false; // Check if the block is in a fitting place
                int NX = x, NY = y;

                // Prevent an infinite loop
                while (attempts > 0 && !found)
                {
                    // Value of x and y to not overlap the original coordinate to misplace the block
                    NX = x;
                    NY = y;

                    // Prevent getting stuck in the loop because it cannot find the correct place
                    bool[] fitNum = new bool[4] { false, false, false, false };
                    int limitTheLoop = 0;
                    // Replace the block if the block has more than 2 blocks around
                    while (NX >= TempMap.GetLength(0) || NY >= TempMap.GetLength(1) || NX < 0 || NY < 0 || TempMap[NX, NY])
                    {
                        // Decide where to place
                        randomDirection = Random.Range(0, 4);
                        // Add or subtract x or y by direction value
                        NX = randomDirection == 0 ? x + 1 : randomDirection == 1 ? x - 1 : x;
                        NY = randomDirection == 2 ? y + 1 : randomDirection == 3 ? y - 1 : y;

                        //revent it from going in infinite loop
                        if (limitTheLoop > attempts)
                        {
                            NX = x;
                            NY = y;
                            break;
                        }
                        limitTheLoop++;
                    }

                    // Check the blocks around the new block
                    int count = 0;
                    if (NX < TempMap.GetLength(0) && NX >= 0 && NY < TempMap.GetLength(1) && NY >= 0)
                    {
                        if (NX + 1 < TempMap.GetLength(0) && TempMap[NX + 1, NY]) count++;
                        if (NX - 1 >= 0 && TempMap[NX - 1, NY]) count++;
                        if (NY + 1 < TempMap.GetLength(1) && TempMap[NX, NY + 1]) count++;
                        if (NY - 1 >= 0 && TempMap[NX, NY - 1]) count++;
                    }

                    // If the block is right in front, replace the block
                    if (NX + (NX - x) < TempMap.GetLength(0) && NX + (NX - x) >= 0 &&
                        NY + (NY - y) < TempMap.GetLength(1) && NY + (NY - y) >= 0 &&
                        TempMap[NX + (NX - x), NY + (NY - y)]) count = 4;

                    // Decision if the block is well placed
                    if (count < 2)
                    {
                        // If more than 2 blocks are around, it's wrongly placed
                        found = true;
                    }
                    else
                    {
                        // Too many attempts
                        attempts--;
                    }
                }

                // If the block is placed correctly, move on to the next block
                if (found)
                {
                    NextBlock(NX, NY, callLimit, false);
                }
            }
        }
    }

    private void ChunkLoader()
    {
        IsChunkLoaderRunning = true;

        if (PlaneList.Count == 0 || PlaneList[PlaneList.Count - 1].activeSelf && Cehck)
        {
            GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            newPlane.name = PlaneList.Count + " Plane";
            newPlane.SetActive(false);
            PlaneList.Add(newPlane);
        }
        Cehck = false;

        Vector2Int PlayerChunk = new Vector2Int(Mathf.CeilToInt(PlayerPos.transform.position.x) / MapScale,
                                                Mathf.CeilToInt(PlayerPos.transform.position.z) / MapScale);
        Vector3Int ChunkPos = new Vector3Int(PlayerChunk.x, 0, PlayerChunk.y);

        int Direction = 1;
        int RepeatDir = 0;
        int RepeatDirLimit = 2;

        for (int i = 0; i < PlaneList.Count; i++)
        {
            IsTaken = false;

            if (Mathf.Sqrt(Mathf.Pow(PlaneList[i].transform.position.x - PlayerChunk.x * MapScale, 2) +
                Mathf.Pow(PlaneList[i].transform.position.z - PlayerChunk.y * MapScale, 2)) > viewDis)
            {
                PlaneList[i].SetActive(false);
                continue;
            }

            if (Mathf.Sqrt(Mathf.Pow(ChunkPos.x - PlayerChunk.x, 2) + Mathf.Pow(ChunkPos.z - PlayerChunk.y, 2)) <= viewDis)
            {
                Cehck = true;
                for (int CheckCondition = 0; CheckCondition < PlaneList.Count; CheckCondition++)
                {
                    if (PlaneList[CheckCondition].transform.position == new Vector3(ChunkPos.x, ChunkPos.y, ChunkPos.z) * ((ChunkScale) / 2 - CellSize)
                        && PlaneList[CheckCondition].activeSelf)
                    {
                        IsTaken = true;
                        break;
                    }

                    if (PlaneTemp == null && !PlaneList[CheckCondition].activeSelf)
                    {
                        PlaneTemp = PlaneList[CheckCondition];
                    }
                }

                if (!IsTaken && PlaneTemp != null)
                {
                    //PlaneTemp.GetComponent<MeshFilter>().mesh.Clear();

                    ChunkDictionary.TryGetValue(new Vector2Int(ChunkPos.x, ChunkPos.z), out var value);
                    if (value.Item1)
                    {
                        vertices = value.Item1.vertices;
                        triangles = value.Item1.triangles;
                    }
                    else
                    {
                        Vector2Int chunkKey = new Vector2Int(ChunkPos.x, ChunkPos.z);
                        //Only build and add the chunk if it doesn't already exist
                        if (!ChunkDictionary.ContainsKey(chunkKey))
                        {
                            BuildChunk(chunkKey);
                            //reset meshFilter reload mesh
                            MeshGridBuilder(ChunkScale, ChunkScale, lfd, Chunk, ref PlaneTemp);
                            PlaneTemp.layer = GroundLayer;
                            ChunkDictionary.Add(chunkKey, (PlaneTemp.GetComponent<MeshFilter>().mesh, Chunk));
                        }
                        else
                        {
                            // If the chunk exists, use its existing data
                            var existingChunk = ChunkDictionary[chunkKey];
                            MeshGridBuilder(ChunkScale, ChunkScale, lfd, existingChunk.Item2, ref PlaneTemp);
                            PlaneTemp.layer = GroundLayer;
                        }
                    }
                    PlaneTemp.transform.position = new Vector3(ChunkPos.x, ChunkPos.y, ChunkPos.z) * ((ChunkScale) / 2 - CellSize);
                    PlaneTemp.gameObject.SetActive(true);
                }
                PlaneTemp = null;
            }
            
            if (Direction > 0)
            {
                if (Direction == 1)
                {
                    ChunkPos.x += Direction;
                }
                else
                {
                    ChunkPos.z += Direction / 2;
                }
            }
            else if (Direction < 0)
            {
                if (Direction == -1)
                {
                    ChunkPos.x += Direction;
                }
                else
                {
                    ChunkPos.z += Direction / 2;
                }
            }

            if (RepeatDir < RepeatDirLimit)
            {
                RepeatDir++;
                if (RepeatDir == RepeatDirLimit / 2) Direction *= 2;
            }
            if (RepeatDir >= RepeatDirLimit)
            {
                Direction = Direction > 0 ? Direction = -1 : Direction = 1;
                RepeatDirLimit += 2;
                RepeatDir = 0;

            }
        }
        IsChunkLoaderRunning = false;
    }
}
