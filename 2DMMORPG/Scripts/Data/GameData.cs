using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public static int health;

    public float WalkSpeed;
    public float RunSpeed;

    public Transform WayPoint;

    public float DetectRadius = 1.0f;
    public float maxDistance = 10.0f;

    public float AttackDistance;
    public float Damage;
    public float AttackSpeed;

    public GameData()
    {
        health = 0;
    }
}
