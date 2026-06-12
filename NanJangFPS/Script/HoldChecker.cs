using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldChecker : MonoBehaviour
{
    public static HoldChecker instance;

    public GameObject Item;
    public GameObject HoldPoint;
    public Transform cam;

    public float ThrowSpeed;
    public int damage;
    public float HoldMassMulti;
    public float holdForce = 50f;
    public float holdDamping = 0.5f;
    public float positionThreshold = 0.1f;
    public float objSize;

    private float objMass;
    private bool Pickable;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject); // Prevent duplicate instances
    }

    private void Start()
    {
        Pickable = true;
    }

    private void Update()
    {
        // Adjust the position to follow HoldPoint with an offset based on objSize
        transform.localPosition = new Vector3(0, -0.1f, 2f + objSize);

        if (Item == null)
            Pickable = true;

        if (Item != null && !Pickable)
        {
            Rigidbody itemRigidbody = Item.GetComponentInParent<Rigidbody>();
            Vector3 targetPosition = HoldPoint.transform.position;
            Vector3 difference = targetPosition - Item.transform.position;

            // Apply holding force only when beyond the threshold
            if (difference.magnitude > positionThreshold)
            {
                Vector3 force = difference * holdForce - itemRigidbody.velocity * holdDamping;
                itemRigidbody.AddForce(force, ForceMode.Acceleration);
            }
        }

        if (Input.GetMouseButtonDown(0) && Pickable)
        {
            PickUpItem();
        }
        else if (Input.GetMouseButtonDown(0) && !Pickable && Item != null)
        {
            ReleaseItem();
        }
    }

    private void PickUpItem()
    {
        RaycastHit hit;
        Vector3 boxHalfExtents = new Vector3(0.2f, 0.2f, 0.5f); // Adjust the size as needed
        
        Vector3 boxCenter = cam.position + cam.forward * 0.8f;         // Position the box slightly in front of the camera

        if (Physics.BoxCast(boxCenter, boxHalfExtents, cam.forward, out hit, cam.rotation, 2f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Object"))
            {
                Pickable = false;
                Item = hit.collider.gameObject;
                Rigidbody itemRb = Item.GetComponentInParent<Rigidbody>();
                objMass = itemRb.mass;

                // Make sure item is not allowing player to fly
                itemRb.mass = objMass / HoldMassMulti;

                itemRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                Item.GetComponentInParent<ObjectController>().SetHanded(true);
                itemRb.useGravity = false;
            }
        }
    }


    private void ReleaseItem()
    {
        Pickable = true;
        Item.GetComponentInParent<ObjectController>().SetHanded(false);
        Item.GetComponentInParent<ObjectController>().SetDamage(damage);

        Rigidbody itemRigidbody = Item.GetComponentInParent<Rigidbody>();
        itemRigidbody.mass = objMass;
        itemRigidbody.useGravity = true;
        itemRigidbody.AddForce(cam.forward * ThrowSpeed * 1000f * Time.deltaTime,ForceMode.Impulse);
        itemRigidbody.AddForce(cam.up * ThrowSpeed * 300f * Time.deltaTime,ForceMode.Impulse);

        Item = null;
    }


    //private void OnDrawGizmos()
    //{
    //    if (cam != null)
    //    {
    //        // Define the box dimensions
    //        Vector3 boxHalfExtents = new Vector3(0.2f, 0.2f, 0.5f); // Match these to your BoxCast size
    //        Vector3 boxCenter = cam.position + cam.forward * 0.8f;         // Match the BoxCast center

    //        // Set Gizmo color
    //        Gizmos.color = Color.green;

    //        // Draw the box as a wireframe to visualize the BoxCast
    //        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCenter, cam.rotation, Vector3.one);
    //        Gizmos.matrix = rotationMatrix;
    //        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2); // Multiply by 2 to get full size
    //    }
    //}
}



//데미지바, 봇 무빙,봇 모션(간단하게 맞았을때 tilt 시키기, 피가 없을때 넘어지게),
//죽었을때 그 사람이 쓰던 매인 스킬 오브젝트를 드랍


//특수무기(연필(쏠때 앞에 보고, 부딛힌 오브젝트에 박히게, 수량 100게)),
