using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private Camera PlayerCamera;
    public float WalkSpeed = 5f;
    private Rigidbody2D rb;

    public float DefaultHealth = 100f;
    public float Health;
    public float Damage = 5f;
    public float AttackRange = 1f;
    public float AttackCoolTime = 1f;
    public LayerMask EnemyLayer;
    public LayerMask ItemLayer;

    private Vector2 lastFacingDirection = Vector2.right; // Default facing direction
    private Vector2 movementDirection;

    GameObject NetworkManagerUI;

    private void Start()
    {
        if (IsOwner)
        {
            PlayerCamera = Camera.main;
            PlayerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, PlayerCamera.transform.position.z);
            PlayerCamera.transform.SetParent(transform);
            rb = GetComponent<Rigidbody2D>();
            EnemyLayer = LayerMask.GetMask("Enemy");
            EnemyLayer = LayerMask.GetMask("Item");
            NetworkManagerUI = GameObject.Find("NetworkManagerUI");
            Health = DefaultHealth;
            NetworkManagerUI.GetComponent<NetworkManagerUI>().PlayerHPSlider(DefaultHealth);
        }
        else
        {
            enabled = false; // Disable this script for non-owners
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Movement input
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movementDirection = new Vector2(x, y).normalized; // Get the movement direction

        if (movementDirection != Vector2.zero)
        {
            // Update the last facing direction only if there's movement
            lastFacingDirection = movementDirection;
        }

        rb.velocity = movementDirection * WalkSpeed; // Move the player

        // Attack input
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            PickUpItem();
        }
    }

    void PickUpItem()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 1, ItemLayer);
        if (hit.collider != null)
        {
            //Inventory.instance.AddItem(hit.collider.gameObject.name);
            hit.collider.gameObject.SetActive(false);
        }
    }

    private void Attack()
    {
        LayerMask attackMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemy"));

        // Use OverlapCircle in the last facing direction
        Vector2 attackPosition = (Vector2)transform.position + (lastFacingDirection * AttackRange);
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, AttackRange, attackMask);

        foreach (Collider2D hit in hits)
        {
            // Check if the hit object is not the current game object
            if (hit.gameObject != this.gameObject)
            {
                // Handle damage based on what was hit
                if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    hit.gameObject.GetComponent<EnemyNetwork>().GetDamage(Damage);
                    break;
                }
                else if (hit.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    hit.gameObject.GetComponent<PlayerNetwork>().GetDamage(Damage);
                    break;
                }
            }
        }
    }

    public void GetDamage(float damage)
    {
        Health -= damage;
        NetworkManagerUI.GetComponent<NetworkManagerUI>().PlayerHPSlider(Health);
        if (Health <= 0)
        {
            gameObject.SetActive(false); // You may want to add network calls here to notify other clients
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    // Visualize the CircleCast area for the attack
    //    Gizmos.DrawWireSphere(transform.position + (Vector3)(lastFacingDirection * AttackRange), AttackRange);
    //}
}
