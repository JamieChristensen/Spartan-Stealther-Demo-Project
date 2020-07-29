using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle, Patrolling, Seeking /*when alarmed/aware of player presence*/, FightingPlayer, Dead
    }
    public NavMeshAgent navMeshAgent;

    [SerializeField]
    private Transform playerTransform; //Assign in inspector.

    int currentPatrolPointIndex;

    [Tooltip("Minimum two patrol-points, or the agent won't enter patrol-state. Agent starts going to 0th index.")]
    [SerializeField]
    Vector3[] patrolPoints = new Vector3[2]; //Assign in inspector 

    private EnemyState state;

    private Color defaultGizmoColor = new Color(1, 1, 0, 0.55f);
    private Color currentPointGizmoColor = new Color(0, 1, 0, 0.55f);

    //private bool playerDetected = false; //Might use for communicating info to other agents or tracking state - not necessary for now.

    private Vector3 lastKnownPlayerPosition;

    [HideInInspector]
    public bool isDead = false; //Is set to true by spear on collision, used for death FOV-fadeout. 
    [SerializeField]
    private float deathFadeFOVTime, deathFadeShaderTime;

    [SerializeField]
    private FieldOfView fieldOfView; //Assign in inspector
    [SerializeField]
    private MeshRenderer viewMeshRenderer;
    [SerializeField]
    private Color deathFOVColor;
    [SerializeField]
    private float viewRange;

    [SerializeField]
    private float desiredFightingDistance;

    [Header("Shooting-related:")]
    [SerializeField]
    private float projectileSpeed;
    [SerializeField]
    private GameObject bulletPrefab; //Assign in inspector
    [SerializeField]
    private Transform bulletSpawn;
    [SerializeField]
    private float reloadTime = 0.8f;
    private float reloadTimer;

    Vector3 targetPosition;

    [Header("Movement-tuning:")]
    [SerializeField]
    private float turnSmoothTime = 0.12f; //Used for turning towards player when player is closer than desired shooting distance. Might be used if strafing enemies are implemented
    private float turnsSmoothVelocity;

    [SerializeField]
    private float baseMoveSpeed, fightingMoveSpeed, baseTurnSpeed, fightingTurnSpeed; //Navmesh agent rotation-speed.




    void OnDrawGizmosSelected()
    {
        Gizmos.color = defaultGizmoColor;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.color = i == currentPatrolPointIndex ? currentPointGizmoColor : defaultGizmoColor;

            if (i == 0 || i == patrolPoints.Length - 1)
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.8f);
            }
            else
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.5f);
            }
        }

        Gizmos.DrawCube(targetPosition, new Vector3(0.8f, 0.8f, 0.8f));
    }

    void Awake()
    {
        //Guarantee references: 
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<PlayerController>().transform;
        }

        if (patrolPoints.Length <= 1)
        {
            state = EnemyState.Idle;
        }
        else
        {
            state = EnemyState.Patrolling;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (state == EnemyState.Patrolling)
        {
            currentPatrolPointIndex = 0;
            navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);
        }

        fieldOfView.viewRadius = viewRange;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead && state != EnemyState.Dead)
        {
            state = EnemyState.Dead;
            navMeshAgent.enabled = false;
            viewMeshRenderer.material.SetColor("_Color", deathFOVColor);
            StartCoroutine(DeathSequence());
        }

        if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }

        switch (state)
        {
            case EnemyState.Idle:
                //Just doing nothing.
                break;

            case EnemyState.Dead:
                //Just doing nothing.
                break;

            case EnemyState.Patrolling:
                navMeshAgent.speed = baseMoveSpeed;
                navMeshAgent.angularSpeed = baseTurnSpeed;

                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentPatrolPointIndex++;
                    currentPatrolPointIndex %= patrolPoints.Length;
                    navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);
                }

                if (IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.FightingPlayer;
                    reloadTimer = 0; //So the enemy doesn't shoot immediately when seeing the player..
                    lastKnownPlayerPosition = playerTransform.position;

                    //transform.LookAt(playerTransform);
                }

                break;

            case EnemyState.Seeking:

                navMeshAgent.SetDestination(lastKnownPlayerPosition);
                break;

            case EnemyState.FightingPlayer:
                //The following modifies movement and lookdirection for combat - the enemies try to get to a fighting distance on the player, while shooting at the player whenever their gun is loaded. 
                navMeshAgent.speed = fightingMoveSpeed;
                navMeshAgent.angularSpeed = fightingTurnSpeed;

                if (!IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.Seeking;
                }
                else
                {
                    //For looking at player:
                    lastKnownPlayerPosition = playerTransform.position;
                    if (Vector3.Distance(playerTransform.position, transform.position) < desiredFightingDistance)
                    {
                        targetPosition = transform.position; //Could add: Raycast behind self to figure out if strafing or backing up is desirable
                        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
                        float targetAngle = Mathf.Atan2(dirToPlayer.x, dirToPlayer.z) * Mathf.Rad2Deg;
                        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnsSmoothVelocity, turnSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    }
                    else
                    {
                        targetPosition = (playerTransform.position - transform.position).normalized * desiredFightingDistance + transform.position;
                    }
                    navMeshAgent.SetDestination(targetPosition); //Could add: strafing left/right of this point, could add corner prediction to avoid LOS'ing too easily. 

                    //Shooting at player:
                    if (reloadTimer > reloadTime)
                    {
                        FireBullet(bulletSpawn.position, playerTransform.position, projectileSpeed);
                        reloadTimer = 0;
                    }
                }
                break;
        }
    }

    private bool IsTransformInFOV(Transform transform, FieldOfView fieldOfView)
    {
        if (fieldOfView.visibleTargets.Contains(transform))
        {
            return true;
        }
        return false;
    }

    IEnumerator DeathSequence()
    {
        bool hasFadedFOV = false;
        float fovFadeTimer = 0f;


        float maxAngle = fieldOfView.viewAngle + (fieldOfView.viewAngle * 0.1f);
        float maxDist = fieldOfView.viewRadius + (fieldOfView.viewRadius * 0.1f);


        while (!hasFadedFOV)
        {
            if (fovFadeTimer >= deathFadeFOVTime)
            {
                hasFadedFOV = true;
            }
            float t = fovFadeTimer / deathFadeFOVTime;

            fieldOfView.viewAngle = Mathf.Lerp(maxAngle, 0f, t);
            fieldOfView.viewRadius = Mathf.Lerp(maxDist, 0, t);
            fovFadeTimer += Time.deltaTime;
            yield return new WaitForSeconds(0f);
        }
        yield return null;
    }

    private void FireBullet(Vector3 spawnPosition, Vector3 targetPosition, float bulletSpeed)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bulletObj.transform.LookAt(targetPosition, Vector3.up);
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        Vector3 direction = (targetPosition - spawnPosition).normalized;

        rb.AddForce(direction * bulletSpeed, ForceMode.VelocityChange);
        rb.AddRelativeTorque(transform.forward * 45, ForceMode.VelocityChange);
    }
}
