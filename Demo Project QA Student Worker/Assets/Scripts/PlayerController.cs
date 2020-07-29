using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController controller; //Assign in inspector

    [SerializeField]
    private Camera cam; //Assign in inspector

    private float horizontalInput, verticalInput, shootInput;
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
        }


    }


    private void ThrowSpear(Vector3 target, float spearSpeed)
    {
        spearInHand.SetActive(false);
        GameObject spearObj = Instantiate(spearPrefab, spearInHand.transform.position, spearInHand.transform.rotation);
        Spear spear = spearObj.GetComponentInChildren<Spear>();
        Vector3 upModifier = new Vector3(0, 0.15f, 0);   //Ensures that the target - if an enemy, will likely be lifted by and fly with the spear.
        spear.rb.AddForce((target+upModifier - spearInHand.transform.position).normalized * spearSpeed, ForceMode.VelocityChange);
    }
}
