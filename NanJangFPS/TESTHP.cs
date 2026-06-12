using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TESTHP : MonoBehaviour
{
    public TMP_Text a;
    public int HP;

    public float Speed;

    private void Update()
    {
        if (transform.position.z > 5f && Speed > 0)
            Speed = -Speed;
        else if (transform.position.z < -5f && Speed < 0)
            Speed = -Speed;

        transform.position += Vector3.forward * Speed;
    }

    public float tiltAngle = 30f; // Angle to tilt when hit
    public float fallSpeed = 2f; // Speed of fall when dead
    private bool isDead = false;
    private Quaternion originalRotation;

    public void GetDamage(int dmg)
    {
        originalRotation = transform.rotation;
        HP -= dmg;

        a.text = "" + HP;
    }

}
