using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    //Threading and ChunkLoader
    [Header("ChunkLoader")]
    Dictionary<Vector2Int, (Mesh, bool[,])> ChunkDictionary = new Dictionary<Vector2Int, (Mesh, bool[,])>();
    List<GameObject> PlaneList = new List<GameObject>();
    public Transform PlayerPos;

    GameObject PlaneTemp;
    bool IsTaken;
    bool Cehck;

    bool IsChunkLoaderRunning;


    //map data storage
    [Header("main map values")]
    public float[,] Map;
    public bool[,] mapRoot;
    public float[,] heightMap;
    public float[,] BlurredMap;
    int MapScale = 16;

    //Map Root generation
    [Header("Map root generation value")]
    public int RootMapScale = 8; // specify the width or height of the map
    public float RootMapCellSize = 1.0f; // size of each cell in the map    

    private int RootMapPosX;//x for starting point
    private int RootMapPosY;//y for starting point

    private int CallLimitation;//limit the blocks from generating next block

    //Perlin Noise
    [Header("PerlinNoise value")]
    public int NoiseScale;//perlin noise width and height
    public float PerlinScale, XOffset, YOffset;//location point of perlin noise, size of noise

    //mesh builder
    [Header("mesh builder values")]
    Mesh mesh;//mesh data storage
    MeshCollider meshCollider;//Mesh Collider;

    Vector3[] vertices;//position of vertices for mesh
    int[] triangles;//order of triangles

    public float MeshCellSize = 1;//size of each triangles
    public Vector3 gridOffset;//location of grid
    public int MeshGridSize;//size of grid

    public float multi = 1f;

    public int MeshDetailLevel;
    public int MaxMeshDetailLevel;

    //Map Blurrer value
    [Header("Map Blurrer Values")]
    public int BlurrMapScale;

    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //DELETE THE ELSE IN THE MESHGRIDBUILDER TO NOT REGERERATE GROUND MESH
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = gameObject.GetComponent<MeshCollider>();
        RootMapScale = MapScale / 2;
        NoiseScale = MapScale;
        MeshGridSize = MapScale;
        IsChunkLoaderRunning = false;
    }

    private void Start()
    {
        CallAllFunctions(new Vector2Int((int)PlayerPos.transform.position.x,(int)PlayerPos.transform.position.z));
        PlaneList.Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
        PlaneList[PlaneList.Count - 1].name = PlaneList.Count + " Plane";
        PlaneList[PlaneList.Count - 1].SetActive(false);
    }


    private void Update()
    {
        if(!IsChunkLoaderRunning)
        {
            ChunkLoader();
        }
    }

    //Call all functions
    private void CallAllFunctions(Vector2Int Chunk)
    {
        GenerateMapRoot(Chunk);
        PerlinNoiseMap(Chunk);
        BlurringMap();
        CombineMapDatas(Chunk);
        MeshGridBuilder();
    }

    //combine all the datas setted
    private void CombineMapDatas(Vector2Int Chunk)
    {
        Map = new float[RootMapScale * 2, RootMapScale * 2];
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                Map[x, y] = BlurredMap[x, y] * heightMap[x, y] * 5f;
            }
        }

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (ChunkDictionary.TryGetValue(Chunk + new Vector2Int(x, y), out var value))
                {
                    for (int i = 0; i < MapScale - 1; i++)
                    {
                        // Calculate target indices more safely
                        int rowIndex = x * (MapScale - 1) + i;
                        int colIndex = y * (MapScale - 1) + i;

                        // Add bounds checking
                        if (rowIndex < Map.GetLength(0) && colIndex < Map.GetLength(1))
                        {
                            // Simplify vertex index calculation
                            int vertexIndex = i * (MapScale - 1);
                            if (vertexIndex < value.Item1.vertices.Length)
                            {
                                Map[rowIndex, colIndex] += value.Item1.vertices[vertexIndex].y;
                                Map[rowIndex, colIndex] /= 2;
                            }
                        }
                    }
                }
            }
        }
    }

    //blurring Map Root and shade
    private void BlurringMap()
    {
        float[,] BlurringMap = new float[BlurrMapScale, BlurrMapScale];
        float[,] RootMapData = new float[RootMapScale, RootMapScale];

        for (int i = 0; i < RootMapScale; i++)
        {
            for (int j = 0; j < RootMapScale; j++)
            {
                if (mapRoot[i, j]) RootMapData[i, j] = 1f;
                if (!mapRoot[i, j]) RootMapData[i, j] = 0f;
            }
        }

        for (int y = 0; y < BlurrMapScale; y += 2)
        {
            for (int x = 0; x < BlurrMapScale; x += 2)
            {
                int Mx = (int)Mathf.Ceil((float)x / 2);
                int My = (int)Mathf.Ceil((float)y / 2);
                if (Mx > 0 && My > 0)
                    BlurringMap[x, y] = (RootMapData[Mx, My] * 0.35f + (RootMapData[Mx - 1, My] + RootMapData[Mx, My - 1]) * 0.25f + RootMapData[Mx - 1, My - 1] * 0.15f);
                if (Mx > 0 && My < RootMapData.GetLength(1) - 1)
                    BlurringMap[x, y + 1] = (RootMapData[Mx, My] * 0.35f + (RootMapData[Mx - 1, My] + RootMapData[Mx, My + 1]) * 0.25f + RootMapData[Mx - 1, My + 1] * 0.15f);
                if (My > 0 && Mx < RootMapData.GetLength(0) - 1)
                    BlurringMap[x + 1, y] = (RootMapData[Mx, My] * 0.35f + (RootMapData[Mx + 1, My] + RootMapData[Mx, My - 1]) * 0.25f + RootMapData[Mx + 1, My - 1] * 0.15f);
                if (My < RootMapData.GetLength(1) - 1 && Mx < RootMapData.GetLength(0) - 1)
                    BlurringMap[x + 1, y + 1] = (RootMapData[Mx, My] * 0.35f + (RootMapData[Mx + 1, My] + RootMapData[Mx, My + 1]) * 0.25f + RootMapData[Mx + 1, My + 1] * 0.15f);
            }
        }

        BlurrMapScale = RootMapData.GetLength(0) * 2;
        BlurredMap = new float[BlurrMapScale, BlurrMapScale];

        for (int x = 0; x < BlurringMap.GetLength(0); x++)
        {
            for (int y = 0; y < BlurringMap.GetLength(1); y++)
            {
                float Average = BlurringMap[x, y];
                if (x > 0) Average += BlurringMap[x - 1, y];
                if (y > 0) Average += BlurringMap[x, y - 1];
                if (x < BlurringMap.GetLength(0) - 1) Average += BlurringMap[x + 1, y];
                if (y < BlurringMap.GetLength(1) - 1) Average += BlurringMap[x, y + 1];
                BlurredMap[x, y] = Average / 5f;
            }
        }
    }

    //get height map for terrain
    private void PerlinNoiseMap(Vector2Int Chunk)
    {
        XOffset = Chunk.x;
        YOffset = Chunk.y;
        heightMap = new float[NoiseScale, NoiseScale];
        for (int x = 0; x < NoiseScale; x++)
        {
            for (int y = 0; y < NoiseScale; y++)
            {
                float XCoord = (((float)x / NoiseScale) * PerlinScale) + XOffset;
                float YCoord = (((float)y / NoiseScale) * PerlinScale) + YOffset;
                heightMap[x, y] = Mathf.PerlinNoise(XCoord, YCoord);
            }
        }
    }

    //generate the shape of terrain
    private void GenerateMapRoot(Vector2Int Chunk)
    {
        mapRoot = new bool[RootMapScale, RootMapScale];

        // Starting point of map generator
        RootMapPosX = Random.Range(0, mapRoot.GetLength(0));
        RootMapPosY = Random.Range(0, mapRoot.GetLength(1));

        CallLimitation = 15;

        //for(int x = 0;x < 4;x++)
        //{
        //    Vector2Int CheckChunk = new Vector2Int(x == 2 ? x = 0 : x == 3 ? x = -1 : x, x == 0 ? x = -1 : x == 1 ? x = 0 : x == 2 ? x = 1 : x = 0);
        //
        //    if(ChunkDictionary.TryGetValue(Chunk + CheckChunk,out var value))
        //    {
        //        for(int i = 0; i < value.Item2.GetLength(0); i++)
        //        {
        //            if(Random.Range(0, 32)==0)
        //            {
        //                if (x == 0 && value.Item2[i + 1, RootMapScale - 1] == false && value.Item2[i - 1, RootMapScale - 1] == false && value.Item2[i, RootMapScale - 2] == false)
        //                {
        //                    // Generating the first block
        //                    NextBlock(i, RootMapScale - 1, CallLimitation);
        //                    return;
        //                }
        //                else if (x == 1 && value.Item2[0, i + 1] == false && value.Item2[0, i - 1] == false && value.Item2[1, i] == false)
        //                {
        //                    // Generating the first block
        //                    NextBlock(0, i, CallLimitation);
        //                    return;
        //                }
        //                else if (x == 2 && value.Item2[i + 1, 0] == false && value.Item2[i - 1, 0] == false && value.Item2[i, 1] == false)
        //                {
        //                    // Generating the first block
        //                    NextBlock(i, 0, CallLimitation);
        //                    return;
        //                }
        //                else if (x == 3 && value.Item2[RootMapScale - 1, i + 1] == false && value.Item2[RootMapScale - 1, i - 1] == false && value.Item2[RootMapScale - 2, i] == false)
        //                {
        //                    // Generating the first block
        //                    NextBlock(RootMapScale - 1, i, CallLimitation);
        //                    return;
        //                }
        //            }
        //        }
        //    }
        //}
        // Generating the first block
        NextBlock(RootMapPosX, RootMapPosY, CallLimitation);
        
    }

    // Generate the next block in a random direction
    private void NextBlock(int x, int y, int callLimit)
    {
        // Where the next block will be
        int randomDirection = 0;

        // Place the current block
        mapRoot[x, y] = true;

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
                    while (NX >= mapRoot.GetLength(0) || NY >= mapRoot.GetLength(1) || NX < 0 || NY < 0 || mapRoot[NX, NY])
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
                    if (NX < mapRoot.GetLength(0) && NX >= 0 && NY < mapRoot.GetLength(1) && NY >= 0)
                    {
                        if (NX + 1 < mapRoot.GetLength(0) && mapRoot[NX + 1, NY]) count++;
                        if (NX - 1 >= 0 && mapRoot[NX - 1, NY]) count++;
                        if (NY + 1 < mapRoot.GetLength(1) && mapRoot[NX, NY + 1]) count++;
                        if (NY - 1 >= 0 && mapRoot[NX, NY - 1]) count++;
                    }

                    // If the block is right in front, replace the block
                    if (NX + (NX - x) < mapRoot.GetLength(0) && NX + (NX - x) >= 0 &&
                        NY + (NY - y) < mapRoot.GetLength(1) && NY + (NY - y) >= 0 &&
                        mapRoot[NX + (NX - x), NY + (NY - y)]) count += 4;

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
                    NextBlock(NX, NY, callLimit);
                }
            }
        }
    }

    //terrain mesh builder
    private void MeshGridBuilder()
    {
        int verticesPerLine = MeshGridSize + 1;

        // Set array sizes
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        // Set tracker integers
        int v = 0;
        int t = 0;

        // Calculate the offset to center the grid
        float offset = MeshGridSize * MeshCellSize * 0.5f;

        for (int x = 0; x <= MeshGridSize; x++)
        {
            for (int y = 0; y <= MeshGridSize; y++)
            {
                vertices[v] = new Vector3(x * MeshCellSize - offset, 0, y * MeshCellSize - offset);
                if (x < MeshGridSize && y < MeshGridSize)
                    vertices[v].y = Map[x, y];

                if (x < MeshGridSize && y < MeshGridSize && x < verticesPerLine - 1 && y < verticesPerLine - 1)
                {
                    triangles[t] = v;
                    triangles[t + 1] = v + 1;
                    triangles[t + 2] = v + verticesPerLine;
                    triangles[t + 3] = v + verticesPerLine;
                    triangles[t + 4] = v + 1;
                    triangles[t + 5] = v + verticesPerLine + 1;

                    t += 6;
                }
                v++;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        else
            meshCollider.sharedMesh = null; // Clear the existing mesh to force an update
        meshCollider.sharedMesh = mesh;
    }

    //needs to do the mesh detail size setting to give less stress on computer
    //Chunk Loader
    private void ChunkLoader()
    {
        //wait for the function to end
        IsChunkLoaderRunning = true;

        //view distance
        float viewDis = 64;

        //Plane Duplication if there isn't enough planes
        if(PlaneList[PlaneList.Count-1].activeSelf && Cehck)
        {
            PlaneList.Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
            PlaneList[PlaneList.Count - 1].name = PlaneList.Count + " Plane";
            PlaneList[PlaneList.Count - 1].SetActive(false);
        }Cehck = false;
        

        Vector2Int PlayerChunk = new Vector2Int(Mathf.CeilToInt(PlayerPos.transform.position.x) / MapScale,
                                                Mathf.CeilToInt(PlayerPos.transform.position.z) / MapScale);
        Vector3Int ChunkPos = new Vector3Int(PlayerChunk.x,0,PlayerChunk.y);

        //for direction desition for plane loader
        int Direction = 1;
        int RepeatDir = 0;
        int RepeatDirLimit = 2;

        for(int i = 0; i < PlaneList.Count; i++)
        {
            IsTaken = false;

            //Set Mesh Detail level
            //chanegklwajkleajfl;jsda;flkjasl;dkfj;asklfjl;kasdjfl;aksjfl;kasjdfl;sj change if the detail if to low change the division with viewdis
            MaxMeshDetailLevel = Mathf.FloorToInt(viewDis / 16);
            MeshDetailLevel = MaxMeshDetailLevel - RepeatDirLimit / 2;

            //set active false the plane out of view distance
            if (Mathf.Sqrt(Mathf.Pow(PlaneList[i].transform.position.x - PlayerChunk.x * MapScale, 2) +
                Mathf.Pow(PlaneList[i].transform.position.z - PlayerChunk.y * MapScale, 2)) > viewDis)
            {
                PlaneList[i].SetActive(false);
                continue;
            }
            //check if the plane is in the view distance
            if (Mathf.Sqrt(Mathf.Pow(ChunkPos.x - PlayerChunk.x, 2) + Mathf.Pow(ChunkPos.z - PlayerChunk.y, 2)) <= viewDis / 16)
            {
                Cehck = true;
                //check planes conditions
                for(int CheckCondition = 0; CheckCondition < PlaneList.Count; CheckCondition++)
                {
                    //if the chunk aready got a plane, skip the chunk for loading
                    if (PlaneList[CheckCondition].transform.position == ChunkPos * MapScale && PlaneList[CheckCondition].activeSelf) 
                    {
                        IsTaken = true;
                        break;
                    }
                    //if nothing in the PlaneTemp(Not Used plane in PlaneTemp)
                    //give Plane that isn't loaded
                    if (PlaneTemp == null && !PlaneList[CheckCondition].activeSelf)
                    {
                        PlaneTemp = PlaneList[CheckCondition];
                    }
                }
                //setting up a plane for loading
                if (!IsTaken && PlaneTemp != null) 
                {
                    PlaneTemp.GetComponent<MeshFilter>().mesh.Clear();
                    //call existing chunk data if there is 
                    if(ChunkDictionary.TryGetValue(new Vector2Int(ChunkPos.x,ChunkPos.z), out var value))
                    {
                        //I was working on mesh detail level
                        vertices = value.Item1.vertices;
                        triangles = value.Item1.triangles;
                    }
                    //create new chunk data
                    else
                    {
                        CallAllFunctions(new Vector2Int(ChunkPos.x,ChunkPos.z));
                        ChunkDictionary.Add(new Vector2Int(ChunkPos.x, ChunkPos.z), (mesh, mapRoot));
                    }
                    PlaneTemp.GetComponent<MeshFilter>().mesh.vertices = vertices;
                    PlaneTemp.GetComponent<MeshFilter>().mesh.triangles = triangles;

                    PlaneTemp.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                    //create new mesh colider with new mesh
                    Destroy(PlaneTemp.GetComponent<MeshCollider>());
                    PlaneTemp.AddComponent<MeshCollider>();
                    PlaneTemp.GetComponent<MeshCollider>().sharedMesh = null;

                    PlaneTemp.GetComponent<MeshCollider>().sharedMesh = mesh;

                    //give position and let it become a terrain
                    PlaneTemp.transform.position = ChunkPos * MapScale;
                    PlaneTemp.gameObject.SetActive(true);
                }
                PlaneTemp = null;
            }

            //for plane direction directing, 
            //this lets the planes to be loaded from middle to side making circle
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