using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    public static GM instance;
    public GameObject Machine;
    public GameObject Earth;
    public GameObject grass;

    public float coin;
    public float coinMultiplier;

    public float Ruby;

    public float GrowingSpeed = 3.6f;
    public float MowerSpeed = 50f;
    public float EarthRadius;

    public int EquipedMower;

    float side;

    int Count = 0;

    Vector3 grassRotation;

    void Start()
    {
        side = Mathf.Sqrt(MowerSpeed + MowerSpeed);
    }
    // Update is called once per frame
    void Update()
    {
        side = Mathf.Sqrt(MowerSpeed + MowerSpeed);
        EarthRadius = Earth.transform.localScale.x / 2;
        side = Input.GetAxis("Horizontal") * -30;
        Machine.transform.Rotate(MowerSpeed * Time.deltaTime, 0, side * Time.deltaTime);
        while (Count < 15000)
        {
            grassRotation = new Vector3(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360));
            Instantiate(grass, Earth.transform.position,
                Quaternion.Euler(grassRotation))
                .transform.parent = Earth.transform;
            Count++;
        }

    }
}
