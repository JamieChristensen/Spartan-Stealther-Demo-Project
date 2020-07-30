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

    public PlayerController playerController;

    public GameObject telegraphingSpear;

    private Vector3 previousVelocity;

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

        if (rb == null)
        {
            return;
        }
        if (distanceTravelled > dropOffDistance && !rb.useGravity)
        {
            rb.useGravity = true;
        }
    }

    void LateUpdate()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;
        if (rb != null)
        {
            previousVelocity = rb.velocity;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        telegraphingSpear.SetActive(false);

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

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }

        if (other.transform.CompareTag("Enemy"))
        {
            if (hitEnemy == false) //Ensures spear only impales one enemy for now.
            {
                EnemyController controller = other.transform.GetComponent<EnemyController>();

                controller.isDead = true;
                if (controller.navMeshAgent.enabled)
                {
                    controller.navMeshAgent.isStopped = true;
                }
                other.transform.parent = transform;
                other.transform.parent.tag = "DeadEnemy";
                hitEnemy = true;
                //rb.useGravity = true;
                rb.velocity = previousVelocity;
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }

        if (other.transform.CompareTag("DeadEnemy"))
        {
            hitEnemy = true;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Destroy(rb);
            
            transform.position = previousPosition;
            transform.rotation = previousRotation;

            transform.parent = other.transform;

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }


    }
}
