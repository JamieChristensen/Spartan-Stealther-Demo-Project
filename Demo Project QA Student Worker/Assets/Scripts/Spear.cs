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

    [SerializeField]
    private SubSpear[] subSpears; //Assign in inspector.
    private int activatedSubSpears;

    [SerializeField]
    private GameObject impactParticles, impactBloodParticles; //Assign in inspector.


    void Start()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;

    }

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
        if (telegraphingSpear != null)
        {
            telegraphingSpear.SetActive(false);
        }

        if (other.transform.CompareTag("Wall"))
        {
            Destroy(Instantiate(impactParticles, other.GetContact(0).point, Quaternion.identity), 5);

            rb.velocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = previousPosition;
            transform.rotation = previousRotation;

            GetComponent<Collider>().enabled = false;
            // Destroy(rb);

            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                child.gameObject.tag = "DeadEnemy";
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
            this.enabled = false;
        }

        if (other.transform.CompareTag("DeadEnemy"))
        {
            if (!hitEnemy)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                rb.angularVelocity = Vector3.zero;
                rb.velocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                transform.position = previousPosition;
                transform.rotation = previousRotation;

                transform.parent = other.transform;

                rb.isKinematic = true;

                if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
                {
                    playerController.mostRecentlyThrownSpear = null;
                }
            }
            hitEnemy = true;
        }

        if (other.transform.CompareTag("Enemy"))
        {
            if (hitEnemy == false)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                EnemyController controller = other.transform.GetComponent<EnemyController>();

                controller.OnSpearHit();

                other.transform.tag = "ImpaledEnemy";
                other.transform.gameObject.layer = LayerMask.NameToLayer("ImpaledEnemy");
                other.transform.parent = transform;


                rb.velocity = previousVelocity;
                if (subSpears[activatedSubSpears] != null)
                {
                    subSpears[activatedSubSpears].gameObject.SetActive(true);
                    activatedSubSpears++;
                }
                GetComponent<Collider>().isTrigger = true;
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }


    }

    public void OnSubSpearCollision(SubSpear subSpear, Collision other)
    {


        if (telegraphingSpear != null)
        {
            telegraphingSpear.SetActive(false);
        }

        if (other.transform.CompareTag("Wall"))
        {
            Destroy(Instantiate(impactParticles, other.GetContact(0).point, Quaternion.identity), 5);

            rb.velocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = previousPosition;
            transform.rotation = previousRotation;

            GetComponent<Collider>().enabled = false;

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }

            foreach (SubSpear sSpear in subSpears)
            {
                sSpear.gameObject.SetActive(false);
            }
            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                child.gameObject.tag = "DeadEnemy";
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
            this.enabled = false;
        }

        if (other.transform.CompareTag("DeadEnemy"))
        {
            if (!subSpear.hitEnemy)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                rb.angularVelocity = Vector3.zero;
                rb.velocity = Vector3.zero;

                rb.constraints = RigidbodyConstraints.FreezeAll;
                transform.position = previousPosition;
                transform.rotation = previousRotation;

                transform.parent = other.transform;

                if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
                {
                    playerController.mostRecentlyThrownSpear = null;
                }
            }
            hitEnemy = true;
        }
        if (other.transform.CompareTag("Enemy"))
        {
            if (subSpear.hitEnemy == false)
            {
                Destroy(Instantiate(impactBloodParticles, other.GetContact(0).point, Quaternion.identity, other.transform), 5);
                EnemyController controller = other.transform.GetComponent<EnemyController>();

                controller.OnSpearHit();

                other.transform.tag = "ImpaledEnemy";
                other.transform.gameObject.layer = LayerMask.NameToLayer("ImpaledEnemy");
                other.transform.parent = transform;
                hitEnemy = true;
                //rb.useGravity = true;
                rb.velocity = previousVelocity;


                if (activatedSubSpears < subSpears.Length)
                {
                    subSpears[activatedSubSpears].gameObject.SetActive(true);
                    activatedSubSpears++;
                }
                subSpear.GetComponent<Collider>().isTrigger = true;
            }

            if (playerController.mostRecentlyThrownSpear == this) //To be sure a spear doesn't unspear some other spear.
            {
                playerController.mostRecentlyThrownSpear = null;
            }
        }
    }
}
