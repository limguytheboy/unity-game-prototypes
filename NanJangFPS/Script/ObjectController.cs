using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public int Damage;
    public LayerMask PlayerLayer;
    Rigidbody rb;

    //던저진 물건인가?
    public bool handed = false;

    public void SetDamage(int dmg)
    {
        Damage = dmg;
    }

    public void SetHanded(bool Isit)
    {
        handed = Isit;
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb = GetComponent<Rigidbody>();
        PlayerLayer = LayerMask.GetMask("Player");

        if (!handed)
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (Damage > 0 && !handed && (PlayerLayer == (PlayerLayer | (1 << collision.gameObject.layer))))
        {
            //collision.gameObject.GetComponent<PlayerController>().GetDamage(Damage);
            collision.gameObject.GetComponent<TESTHP>()?.GetDamage(Damage);
            Damage = 0;
        }
    }
}