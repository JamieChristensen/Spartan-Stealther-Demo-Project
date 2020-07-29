using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public Rigidbody rb;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private bool hitEnemy;

    [SerializeField]
    private float dropOffDistance = 50f;
    private float distanceTravelled;


    // Start is called before the first frame update
    void Start()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        distanceTravelled += Vector3.Distance(transform.position, previousPosition);
        if (distanceTravelled > dropOffDistance && !rb.useGravity)
        {
            rb.useGravity = true;
        }
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
            GetComponent<Collider>().enabled = false;
        }

        if (other.transform.CompareTag("Enemy"))
        {
            if (hitEnemy == false) //Ensures spear only impales one enemy for now.
            {
                EnemyController controller = other.transform.GetComponent<EnemyController>();

                controller.isDead = true;
                controller.navMeshAgent.isStopped = true;

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
