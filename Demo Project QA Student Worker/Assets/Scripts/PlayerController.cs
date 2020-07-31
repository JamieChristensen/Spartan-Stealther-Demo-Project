using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller;     //Assign in inspector
    
    public Rigidbody rb;   //Assign in inspector

    [SerializeField]
    private Camera cam;     //Assign in inspector



    private float horizontalInput, verticalInput, shootInput;
    [SerializeField]
    private KeyCode bulletTimeKey, redirectSpearKey;

    [Header("Other")]
    [Tooltip("The layers that spear-targetting is impacted by, best as ground only, or ground+enemies")]
    [SerializeField]
    private LayerMask spearCollisionLayers;

    [SerializeField]
    private float moveSpeed;
    [Range(0, 20)]
    [SerializeField]
    private float controllerGravity;

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
    private bool doRedirectSpear;

    public Vector3 mousePos; //Used by camera

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
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("DeadEnemy"))
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

        if (pressedRedirectSpear || isRedirectingSpear)
        {
            isRedirectingSpear = true;
            bulletTimeOn = true;
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
                //Calculate new velocity in fixedUpdate:
                doRedirectSpear = true;
            }
        }

        if (bulletTimeOn)
        {
            Time.timeScale = 0.33f;          //Half speed.
            //FixedDeltaTime is changed to 0.02*0.33 in cameracontroller for lack of better place. No adjustment needed to suit timeScale w.r.t bullettime.
        }
        else
        {
            Time.timeScale = 1f;   
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
        controller.Move(Vector3.down * controllerGravity * Time.fixedDeltaTime);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayMaxDist, spearCollisionLayers))
        {
            //Debug.DrawLine(ray.origin, hit.point, Color.white);
            Vector3 lookPos = hit.point;
            mousePos = lookPos;
            transform.LookAt(lookPos);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z); //Prevents looking down.

            if (isRedirectingSpear && mostRecentlyThrownSpear != null)
            {
                Transform spearTransform = mostRecentlyThrownSpear.telegraphingSpear.transform;

                spearTransform.LookAt(lookPos);
                spearTransform.rotation = Quaternion.Euler(0, spearTransform.rotation.eulerAngles.y, spearTransform.rotation.eulerAngles.z);
            }
            if (doRedirectSpear)
            {
                Transform spearTransform = mostRecentlyThrownSpear.transform;
                mostRecentlyThrownSpear.rb.velocity = Vector3.zero;
                spearTransform.rotation = mostRecentlyThrownSpear.telegraphingSpear.transform.rotation;
                spearTransform.Rotate(new Vector3(90, 0, 0), Space.Self); //Spears are rotated proportionally to eachother..

                Vector3 target = new Vector3(hit.point.x, spearInHand.transform.position.y, hit.point.z);
                mostRecentlyThrownSpear.rb.AddForce((target - spearTransform.position).normalized * spearSpeed, ForceMode.VelocityChange);
                doRedirectSpear = false;
                isRedirectingSpear = false;
                mostRecentlyThrownSpear.telegraphingSpear.SetActive(false);
            }
        }


    }


    private void ThrowSpear(Vector3 target, float spearSpeed)
    {
        spearInHand.SetActive(false);
        GameObject spearObj = Instantiate(spearPrefab, spearInHand.transform.position, spearInHand.transform.rotation);
        Spear spear = spearObj.GetComponentInChildren<Spear>();
        target = new Vector3(target.x, spearInHand.transform.position.y, target.z);
        Vector3 upModifier = new Vector3(0,0.15f, 0); //Just to give some upwards lift on collisions.

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
        rb.useGravity = true;

        StartCoroutine(OnDeath());
        this.enabled = false;
    }

    public IEnumerator OnDeath()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}
