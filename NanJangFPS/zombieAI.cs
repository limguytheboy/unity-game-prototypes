using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class zombieAI : MonoBehaviour
{
    public NavMeshAgent navAgent;
    Rigidbody rb;
    public enum ZombieState { Idle, Fall, Chase, Attack, Dead }
    public ZombieState currentState = ZombieState.Idle;
    public Transform Player;
    public float chaseDistance = 10f;
    public float attackDistance = 2f;
    public float attackCooldown = 2f;
    public float attackDelay = 1.5f;
    private bool isAttacking;
    private float lastAttackTime;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        lastAttackTime = attackCooldown;
    }

    private void Update()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                if(Vector3.Distance(transform.position, Player.transform.position) <= chaseDistance)
                        currentState = ZombieState.Idle;
                break;
            case ZombieState.Chase:
                navAgent.SetDestination(Player.position);
                if(Vector3.Distance(transform.position, Player.position) <= attackDistance)
                        currentState = ZombieState.Chase;
                break;
            case ZombieState.Attack:
                navAgent.SetDestination(transform.position);
                Debug.Log("attackPlayer");
                if(Vector3.Distance(transform.position, Player.position)>attackDistance)
                    currentState = ZombieState.Chase;
                break;
            case ZombieState.Dead:
                Debug.Log("dead");
                break;
        }

        if(transform.rotation.x != 0f && transform.rotation.z != 0f && currentState != ZombieState.Fall)
        {
            currentState = ZombieState.Fall;
            Invoke(nameof(StandUp), 2f);
        }
    }

    private void StandUp()
    {
        // Get current rotation
        Vector3 current = transform.eulerAngles;
        // Apply rotation (keep Y unchanged)
        transform.rotation = Quaternion.Euler(0, current.y, 0);
        currentState = ZombieState.Idle;
    }
}
