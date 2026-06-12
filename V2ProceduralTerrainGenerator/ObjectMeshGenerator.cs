using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

class SetForNext
{
    private int x;
    private int y;
    private int LastBoneNum;
    public SetForNext(int _x, int _y, int _LastBoneNum)
    {
        x = _x;
        y = _y;
        LastBoneNum = _LastBoneNum;
    }
    public int Getx { get { return x; } set { x = value; } }
    public int Gety { get { return y; } set { y = value; } }
    public int Getlbm { get { return LastBoneNum; } set { LastBoneNum = value; } }
}

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ObjectMeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    Vector3[] vertices;
    int[] triangles;

    int height;
    int width;
    List<Vector3> Bone;
    List<int> BoneC;
    List<int[]> Joints;

    //Map Root generation
    [Header("Map root generation value")]
    public bool[,] mapRoot;
    public float RootMapScale = 0.1f; // specify the width or height of the map
    public int NewMapScale; // specify the width or height of the map
    public int NextMapScale; // specify the width or height of the map
    int center;
    bool running;
    private int CallLimitation = 30;//limit the blocks from generating next block
    List<SetForNext> SFN = new List<SetForNext>();

    //Object random settings(for tree)
    float thickness = 0.1f;//0.001f
    //number of mesh for each section
    int detail = 4;//4
    //나무가지간의 거리
    float wideness = 0.02f;//0.001f
    //Branch Distance when its splited
    float MeshHeight = 1f;//1f

    private void Awake()
    {
        // Create a new unique Mesh instance
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh; // Assign the new mesh to the MeshFilter

        Joints = new List<int[]> { };
        Bone = new List<Vector3> { };
    }
    private void Start()
    {
        MakeMeshBoneStructure();
        MakeMeshData();
        CreateMesh();

    }
    void Display2DArray(bool[,] array)
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

    void MakeMeshData()
    {
        height = Bone.Count;
        width = detail;
        List<Vector3> verticeTemp = new List<Vector3>();
        BoneC = new List<int> { };
        vertices = new Vector3[height * width * 6];
        for (int y = 0; y < height - 2; y++)
        {
            int current = Joints[y][0];
            int next = Joints[y][1];

            for (int x = 0; x < width; x++)
            {
                // Calculate the angle for each object
                float angle = Mathf.PI * 2 / width;

                Vector3 firstVector = new Vector3(Mathf.Cos(angle * x), 0, Mathf.Sin(angle * x)) * thickness;
                Vector3 secondVector = new Vector3(Mathf.Cos(angle * (x + 1)), 0, Mathf.Sin(angle * (x + 1))) * thickness;

                verticeTemp.Add(Bone[current] + firstVector);
                verticeTemp.Add(Bone[next] + firstVector);
                verticeTemp.Add(Bone[current] + secondVector);
                verticeTemp.Add(Bone[current] + secondVector);
                verticeTemp.Add(Bone[next] + firstVector);
                verticeTemp.Add(Bone[next] + secondVector);
            }
            if (BoneC.Contains(next))
                BoneC.Remove(next);
            else
                BoneC.Add(next);
            if (BoneC.Contains(current))
                BoneC.Remove(current);
            else
                BoneC.Add(current);
        }
        for (int i = 0; i < BoneC.Count - 1; i++)
        {
            // Create pyramid/cone shape
            for (int x = 0; x < width; x++)
            {
                float angle = Mathf.PI * 2 / width;
                Vector3 currentPoint = new Vector3(
                    Mathf.Cos(angle * x),
                    0,
                    Mathf.Sin(angle * x)
                ) * thickness;
                Vector3 nextPoint = new Vector3(
                    Mathf.Cos(angle * (x + 1)),
                    0,
                    Mathf.Sin(angle * (x + 1))
                ) * thickness;

                // Add triangle: current base point -> next base point -> pyramid tip
                verticeTemp.Add(Bone[BoneC[i]] + new Vector3(0, 0.1f, 0));  // Tip of the pyramid
                verticeTemp.Add(Bone[BoneC[i]] + nextPoint);
                verticeTemp.Add(Bone[BoneC[i]] + currentPoint);
            }
        }
        triangles = new int[verticeTemp.Count];
        vertices = new Vector3[verticeTemp.Count];
        for (int i = 0; i < verticeTemp.Count; i++)
        {
            vertices[i] = verticeTemp[i];
            triangles[i] = i;
        }
    }
    void MakeMeshBoneStructure()
    {
        Bone.Add(new Vector3(0, 0, 0));
        Bone.Add(new Vector3(0, 1, 0));
        Joints.Add(new int[2] { 0, 1 });
        mapRoot = new bool[Mathf.CeilToInt(Mathf.Sqrt(1 / RootMapScale)), Mathf.CeilToInt(Mathf.Sqrt(1 / RootMapScale))];
        CallLimitation = 50;
        NextMapScale = 3;
        SFN.Add(new SetForNext(1, 1, Bone.Count - 1));
        controlTower(1);
    }
    int SFNCount;
    void controlTower(int Xheight)
    {
        SFNCount = SFN.Count;
        //Debug.Log(SFNCount);

        if (SFNCount == 0) return;

        NewMapScale = NextMapScale;
        NextMapScale = Mathf.CeilToInt(Mathf.Sqrt(Xheight * 9));

        //Debug.Log(NextMapScale + "  " + NewMapScale);

        mapRoot = new bool[NewMapScale, NewMapScale];
        center = (int)(NewMapScale * 0.5f);
        Array.Clear(mapRoot, 0, mapRoot.Length);
        int initialCount = SFNCount;  // Store initial count
        for (int i = 0; i < initialCount; i++)  // Use initial count instead of SFNCount
        {
            SetForNext temp = new SetForNext(SFN.First().Getx, SFN.First().Gety, SFN.First().Getlbm);
            SFN.RemoveAt(0);
            NextBlock(temp.Getx, temp.Gety, CallLimitation, temp.Getlbm, 50);
            //Display2DArray(mapRoot);
        }

        //if (SFN.Count != initialCount)  // Only continue if we actually made progress
        if (Xheight < 30)
        {
            controlTower(Xheight + 1);
        }
    }

    private void NextBlock(int x, int y, int callLimit, int Previous, int max)
    {
        // Where the next block will be
        int randomDirection = 0;

        // Place the current block
        mapRoot[x, y] = true;

        //Add Bone structure
        Bone.Add(Bone[Previous] + new Vector3((x - center) * wideness, 0.1f * MeshHeight, (y - center) * wideness));

        //make group of connected bone points
        int Current = Bone.Count - 1;
        Joints.Add(new int[2] { Previous, Current });

        bool Continue = Random.Range(0, 30) != 0;
        bool anyPathSucceeded = false;  // Track if any path was successfully created

        int SFNMapMulti = Mathf.FloorToInt(((NextMapScale - 1) - (NewMapScale - 1)) * 0.5f);

        if (!Continue || callLimit <= 0 || max <= 0)
        {
            SFN.Add(new SetForNext(x + SFNMapMulti, y + SFNMapMulti, Bone.Count - 1));
            return;
        }

        // Decide to continue building or stop
        if (Continue && callLimit > 0)
        {
            callLimit--;
            // Decide to split into two blocks or not
            int splitAmount = Random.Range(1, 3);
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
                    int limitTheLoop = 0;
                    // Replace the block if the block has more than 2 blocks around
                    while (NX >= mapRoot.GetLength(0) || NY >= mapRoot.GetLength(1) || NX < 0 || NY < 0 || mapRoot[NX, NY])
                    {
                        // Decide where to place
                        randomDirection = Random.Range(0, 4);
                        // Add or subtract x or y by direction value
                        NX = randomDirection == 0 ? x + 1 : randomDirection == 1 ? x - 1 : x;
                        NY = randomDirection == 2 ? y + 1 : randomDirection == 3 ? y - 1 : y;
                        if (Vector2.Distance(new Vector2(NX, NY), new Vector2(center, center)) > Vector2.Distance(new Vector2(x, y), new Vector2(center, center)))
                        {
                            break;
                        }
                        //revent it from going in infinite loop
                        if (limitTheLoop > attempts)
                        {
                            NX = x;
                            NY = y;
                            return;
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
                    if (NX + (NX - x) < mapRoot.GetLength(0) - 1 && NX + (NX - x) > 0 &&
                        NY + (NY - y) < mapRoot.GetLength(1) - 1 && NY + (NY - y) > 0 &&
                        mapRoot[NX + (NX - x), NY + (NY - y)]) count += 4;

                    for (int a = 0; a < SFN.Count - 1; a++)
                    {
                        if (SFN[a].Getx == NX && SFN[a].Gety == NY)
                        {
                            count = 4;
                            break;
                        }
                    }

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
                if (found && (NX >= 0 && NX < mapRoot.GetLength(0) && NY >= 0 && NY < mapRoot.GetLength(1)))
                {
                    NextBlock(NX, NY, callLimit, Current, max--);
                    anyPathSucceeded = true;
                }
            }
        }
        // If no paths were successfully created from this point, it's an end point
        if (!anyPathSucceeded)
        {
            SFN.Add(new SetForNext(x + SFNMapMulti, y + SFNMapMulti, Bone.Count - 1));
        }
    }

    void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}