using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public Rigidbody rb;

    private Vector3 previousPosition; //Used to prevent awkward movement post-collision when lodging into walls.
    private Quaternion previousRotation;
    private bool hitEnemy;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision other)
    {
        //cases:
        //Wall
        //Enemy
        if (other.transform.CompareTag("Wall"))
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = previousPosition;
            transform.rotation = previousRotation;
            //Debug.Log("Hit a wall");

            this.enabled = false;
        }

        if (other.transform.CompareTag("Enemy"))
        {
            EnemyController controller = other.transform.GetComponent<EnemyController>();

            controller.navMeshAgent.enabled = false;
            controller.isDead = true;

            if (hitEnemy == false) //Ensures spear only impales one enemy for now.
            {
                other.transform.parent = transform;
                other.transform.parent.tag = "DeadEnemy";
                hitEnemy = true;
                rb.useGravity = true;
            }

        }

        if (other.transform.CompareTag("DeadEnemy"))
        {
            hitEnemy = true;

            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = previousPosition;
            transform.rotation = previousRotation;

            transform.parent = other.transform;
        }
    }
}
