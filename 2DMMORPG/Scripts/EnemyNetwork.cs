using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;

public class EnemyNetwork : NetworkBehaviour
{
    public float WalkSpeed = 1f;
    public float RunSpeed = 8.5f;
    public float Health;
    public float ReviveTime = 5f;

    public float DefaultHealth = 10f;

    public float DetectDistance = 5f;
    public float AttackDistance = 2f;
    public float AttackDamage = 5f;
    public float AttackCoolTime = 2f;
    public LayerMask PlayerLayer;
    public LayerMask ObstacleLayer; // Layer for obstacles to check against

    public bool IsAttacking = false;

    public float MinSeparationDistance = 1.5f; // Minimum distance to keep between enemies
    public float SeparationForce = 2f; // The force applied to push enemies apart

    private Vector3 RandomDestination;
    private Vector2 Chunk;

    private Vector3 lastKnownPosition; // Store the player's last known position
    private bool playerDetected = false; // Track whether the player was detected

    public Slider slider;
    public GameObject canvas;

    GameObject MonsterM;

    bool Stop;

    bool Dead;

    private void Start()
    {
        PlayerLayer = LayerMask.GetMask("Player"); // Ensure "Player" layer exists
        ObstacleLayer = LayerMask.GetMask("Object"); // Ensure "Obstacle" layer exists
        ReviveSet();
        MonsterM = GameObject.Find("GameManager");
    }

    private void Update()
    {
        if (!Dead)
        {
            DetectPlayer();
            SeparateFromOtherEnemies();
        }
    }
    
    public void ReviveSet()
    {
        slider.maxValue = DefaultHealth;
        slider.value = DefaultHealth;

        lastKnownPosition = transform.position;
        RandomDestination = transform.position;
        Health = DefaultHealth;
        if (canvas != null)
        {
            canvas.SetActive(true);
        }
        Dead = false;
    }

    void DetectPlayer()
    {
        // Use OverlapCircle to detect players in a circular area around the enemy
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, DetectDistance, PlayerLayer);

        if (playerCollider != null)
        {
            // Perform a raycast to check for obstacles between the enemy and the player
            Vector2 directionToPlayer = (playerCollider.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, DetectDistance, ObstacleLayer);

            // Check if raycast hits anything
            if (hit.collider == null) // No obstacle detected
            {
                // Player detected and there is a clear line of sight
                lastKnownPosition = playerCollider.transform.position;
                playerDetected = true; // Mark that the player has been detected
            }
        }

        // If the player has been detected, move towards the last known position
        if (playerDetected)
        {
            // If the enemy has reached the last known position, stop
            if (Vector2.Distance(transform.position, lastKnownPosition) <= AttackDistance)
            {
                playerDetected = false; // Stop moving after reaching the last known position
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, lastKnownPosition, RunSpeed * Time.deltaTime);
            }

            // Check if enemy is close enough to attack the player
            if (Vector2.Distance(transform.position, lastKnownPosition) <= AttackDistance)
            {
                if (!IsAttacking && playerCollider != null)
                {
                    StartCoroutine(AttackPlayer(playerCollider));
                    IsAttacking = true;
                }
            }
        }
        if (playerCollider == null && !playerDetected)
        {
            MoveRandomly();
        }
    }

    void MoveRandomly()
    {
        if(RandomDestination == null || Vector2.Distance(transform.position, RandomDestination) < 0.1f)
        {
            if (!Stop)
            {
                StartCoroutine(DelayBtwMoves(Random.Range(0f, 5f)));
                Stop = true;
            }
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position, RandomDestination, DetectDistance);
        if(hit.collider == null)
        {
            transform.position = Vector2.MoveTowards(transform.position, RandomDestination, WalkSpeed * Time.deltaTime);
        }
        else
        {
            RandomDestination = transform.position;
        }
        
    }

    IEnumerator AttackPlayer(Collider2D Player)
    {
        // Calculate the direction to the player
        Vector2 directionToPlayer = (Player.transform.position - transform.position).normalized;

        // Calculate the attack position based on attack distance
        Vector2 attackPosition = (Vector2)transform.position + directionToPlayer * AttackDistance;

        // Perform the attack using OverlapCircle
        Collider2D hit = Physics2D.OverlapCircle(attackPosition, AttackDistance);
        if (hit != null)
        {
            Player.gameObject.GetComponent<PlayerNetwork>().GetDamage(AttackDamage);
        }

        // Wait for the attack cooldown
        yield return new WaitForSeconds(AttackCoolTime);

        // Set attacking state to false after cooldown
        IsAttacking = false;
    }

    void SeparateFromOtherEnemies()
    {
        // Get all nearby enemies within a certain range (MinSeparationDistance)
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, MinSeparationDistance, LayerMask.GetMask("Enemy"));

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy != null && enemy.gameObject != this.gameObject) // Ignore itself
            {
                Vector2 directionAway = (transform.position - enemy.transform.position).normalized;
                transform.position += (Vector3)(directionAway * SeparationForce * Time.deltaTime); // Move away from other enemies
            }
        }
    }

    IEnumerator DelayBtwMoves(float Time)
    {
        yield return new WaitForSeconds(Time);

        MoveRandomlyTowardsChunk(Chunk);

        Stop = false;
    }

    public void GetDamage(float Damage)
    {
        Health -= Damage;
        slider.value = Health;
        if(Health <= 0)
        {
            Dead = true;
            StartCoroutine(ReviveCoolTime(ReviveTime));
        }
    }

    public void SetTargetPosition(Vector2 chunkCenter)
    {
        Chunk = chunkCenter;
    }

    void MoveRandomlyTowardsChunk(Vector2 chunkCenter)
    {
        // Check if the object is more than 16 units away from the chunk center
        if (Vector2.Distance(transform.position, chunkCenter) > 16f)
        {
            // Move towards the chunk center gradually
            RandomDestination = Vector2.MoveTowards(transform.position, chunkCenter, Random.Range(1f, 3f));
        }
        else
        {
            // Move randomly within the local area if within 16 units of the chunk center
            if (RandomDestination == null || Vector2.Distance(transform.position, RandomDestination) < 0.1f)
            {
                RandomDestination = transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
            }
        }

        // Move towards the calculated destination
        transform.position = Vector2.MoveTowards(transform.position, RandomDestination, WalkSpeed * Time.deltaTime);
    }

    IEnumerator ReviveCoolTime(float Time)
    {
        if (canvas != null)
        {
            canvas.SetActive(false);
        }

        yield return new WaitForSeconds(Time);

        MonsterM.GetComponent<MonsterManager>().IsDead(this.gameObject);
    }

    //Show the Detection range
    //Show the shortest way to detected player
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, DetectDistance); // Visualize detection range
    //    // Draw a line for raycast (optional, for debugging)
    //    Gizmos.color = Color.red;
    //    if (playerDetected)
    //    {
    //        Gizmos.DrawLine(transform.position, lastKnownPosition);
    //    }

    //    // Set a default or hypothetical direction for visualizing purposes
    //    Vector2 directionToPlayer = Vector2.right; // Example: pointing to the right

    //    // Calculate the attack position based on attack distance
    //    Vector2 attackPosition = (Vector2)transform.position + directionToPlayer * AttackDistance;

    //    // Set the color for the Gizmo
    //    Gizmos.color = Color.red;

    //    // Draw a wireframe sphere to visualize the attack range
    //    Gizmos.DrawWireSphere(attackPosition, AttackDistance);
    //}
}
