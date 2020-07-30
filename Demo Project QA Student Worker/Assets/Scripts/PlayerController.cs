using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;     //Assign in inspector
    [SerializeField]
    private Rigidbody rb;   //Assign in inspector

    [SerializeField]
    private Camera cam;     //Assign in inspector



    private float horizontalInput, verticalInput, shootInput;
    [SerializeField]
    private KeyCode bulletTimeKey, redirectSpearKey;

    [Header("Other")]
    [SerializeField]
    private LayerMask spearCollisionLayers;

    [SerializeField]
    private float moveSpeed;

    private float rayMaxDist = 500f;


    [Header("Spear")]
    [SerializeField]
    private float spearSpeed;
    [SerializeField]
    private GameObject spearPrefab;
    [SerializeField]
    private GameObject spearInHand;
    private float reloadTimer;
    [SerializeField]
    private float reloadTime;
    private bool readyToShoot;

    [HideInInspector]
    public Spear mostRecentlyThrownSpear; //Names are hard. - public so spear can un-recent itself when colliding with walls/enemies.
    private bool isRedirectingSpear;
    private void Awake()
    {

    }
    private void Start()
    {

    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        shootInput = Input.GetAxisRaw("Fire1");
        bool bulletTimeOn = Input.GetKey(bulletTimeKey);
        bool pressedRedirectSpear = Input.GetKeyDown(redirectSpearKey);

        if (shootInput > 0.2f && readyToShoot)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayMaxDist, spearCollisionLayers))
            {
                Vector3 target;
                if (hit.collider.CompareTag("Enemy"))
                {
                    target = hit.collider.transform.position;
                }
                else
                {
                    target = new Vector3(hit.point.x, spearInHand.transform.position.y, hit.point.z);
                }
                ThrowSpear(target, spearSpeed);
                readyToShoot = false;
                reloadTimer = 0;

            }
        }

        if (bulletTimeOn)
        {
            Time.timeScale = 0.5f;          //Half speed.
            Time.fixedDeltaTime = 0.01f;    //0.02f default, halved interval to proportionalize FixedUpdate calls. 
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }


        if (pressedRedirectSpear)
        {
            isRedirectingSpear = true;
            if (mostRecentlyThrownSpear != null)
            {
                mostRecentlyThrownSpear.telegraphingSpear.SetActive(true);

            }
            else
            {
                isRedirectingSpear = false;
            }
        }
    

        if (isRedirectingSpear)
        {
            if (Input.GetKeyUp(redirectSpearKey))
            {
                mostRecentlyThrownSpear.rb.velocity = Vector3.zero;
                //Calculate new velocity.
            }
        }

        //Timers:
        if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }
        else
        {
            readyToShoot = true;
            spearInHand.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;
        controller.Move(direction * moveSpeed * Time.fixedDeltaTime);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayMaxDist, spearCollisionLayers))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.white);
            Vector3 lookPos = hit.point + new Vector3(0, 0, 0);
            transform.LookAt(lookPos);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z); //Prevents looking down.
            Debug.DrawLine(hit.point, lookPos, Color.black);

            if (isRedirectingSpear)
            {
                Transform spearTransform = mostRecentlyThrownSpear.telegraphingSpear.transform;
                
                spearTransform.LookAt(lookPos);
                spearTransform.rotation = Quaternion.Euler(0, spearTransform.rotation.eulerAngles.y, spearTransform.rotation.eulerAngles.z);
            }
        }
    }


    private void ThrowSpear(Vector3 target, float spearSpeed)
    {
        spearInHand.SetActive(false);
        GameObject spearObj = Instantiate(spearPrefab, spearInHand.transform.position, spearInHand.transform.rotation);
        Spear spear = spearObj.GetComponentInChildren<Spear>();
        Vector3 upModifier = new Vector3(0, 0.15f, 0);   //Ensures that the target - if an enemy, will likely be lifted by and fly with the spear.
        spear.rb.AddForce((target + upModifier - spearInHand.transform.position).normalized * spearSpeed, ForceMode.VelocityChange);

        mostRecentlyThrownSpear = spear;
        spear.playerController = this;
    }

    public void Die()
    {
        //Play some sound-effects
        //unparent all transforms/rigidbodies, free up all restrictions on them, apply a minor explosion-force centered a bit below the player.

        controller.enabled = false;
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        rb.constraints = RigidbodyConstraints.None;

        this.enabled = false;
    }
}
