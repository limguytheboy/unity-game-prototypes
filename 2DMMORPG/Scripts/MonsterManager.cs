using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public GameObject[] Monsters;
    public Vector2 MapSize;
    public int MonstersPerChunk = 5;
    public float ChunkSize = 16f;

    private Vector2[,] chunkCenters;

    void Start()
    {
        int chunksX = Mathf.FloorToInt(MapSize.x / ChunkSize);
        int chunksY = Mathf.FloorToInt(MapSize.y / ChunkSize);

        chunkCenters = new Vector2[chunksX, chunksY];

        for (int x = 0; x < chunksX; x++)
        {
            for (int y = 0; y < chunksY; y++)
            {
                Vector2 chunkCenter = new Vector2(
                    (x * ChunkSize) + (ChunkSize / 2) - (MapSize.x / 2),
                    (y * ChunkSize) + (ChunkSize / 2) - (MapSize.y / 2)
                );

                chunkCenters[x, y] = chunkCenter;
                SpawnMonstersInChunk(chunkCenter);
            }
        }
    }

    public void IsDead(GameObject enemy)
    {
        // Find an unoccupied chunk to place the enemy
        Vector2 newChunkCenter = FindUnoccupiedChunk();

        //if (newChunkCenter != Vector2.zero)
        {
            // Move the enemy to the new chunk
            enemy.transform.position = newChunkCenter;

            // Optionally, call any reset methods if needed
            // For instance, resetting health or any other attributes
            enemy.GetComponent<EnemyNetwork>().ReviveSet();

            // Assign new target position for the enemy to move around in the chunk
            enemy.GetComponent<EnemyNetwork>().SetTargetPosition(newChunkCenter);
        }
    }

    Vector2 FindUnoccupiedChunk()
    {
        Vector2 leastOccupiedChunk = Vector2.zero;
        int minOccupancy = int.MaxValue; // Start with the maximum possible value

        // Loop through chunks to find the least occupied chunk
        for (int x = 0; x < chunkCenters.GetLength(0); x++)
        {
            for (int y = 0; y < chunkCenters.GetLength(1); y++)
            {
                Vector2 chunkCenter = chunkCenters[x, y];
                Collider2D[] colliders = Physics2D.OverlapCircleAll(chunkCenter, ChunkSize / 2);

                int currentOccupancy = colliders.Length;

                // Check if this chunk is less occupied than the current minimum
                if (currentOccupancy < minOccupancy)
                {
                    minOccupancy = currentOccupancy;
                    leastOccupiedChunk = chunkCenter; // Update least occupied chunk
                }
            }
        }

        return leastOccupiedChunk; // Return the least occupied chunk found
    }


    void SpawnMonstersInChunk(Vector2 chunkCenter)
    {
        for (int i = 0; i < MonstersPerChunk; i++)
        {
            GameObject selectedMonster = Monsters[Random.Range(0, Monsters.Length)];

            Vector2 spawnPosition = chunkCenter + new Vector2(
                Random.Range(-ChunkSize / 2, ChunkSize / 2),
                Random.Range(-ChunkSize / 2, ChunkSize / 2)
            );

            GameObject spawnedMonster = Instantiate(selectedMonster, spawnPosition, Quaternion.identity);

            EnemyNetwork monster = spawnedMonster.GetComponent<EnemyNetwork>();
            if (monster != null)
            {
                monster.SetTargetPosition(chunkCenter);
            }
        }
    }
}
