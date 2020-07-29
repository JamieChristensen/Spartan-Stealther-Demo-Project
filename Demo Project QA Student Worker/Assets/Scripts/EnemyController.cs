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

    public bool isDead = false; //Is set to true by spear on collision, used for death FOV-fadeout. 

    [SerializeField]
    private FieldOfView fieldOfView; //Assign in inspector

    [SerializeField]
    private float viewRange;

    [SerializeField]
    private float desiredShootingDistance;

    [SerializeField]
    private float projectileSpeed;


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
            StartCoroutine(DeathSequence());
        }

        switch (state)
        {
            case EnemyState.Idle:
                //Just doing nothing.
                break;
            case EnemyState.Patrolling:
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentPatrolPointIndex++;
                    currentPatrolPointIndex %= patrolPoints.Length;
                    navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);
                }

                if (IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.FightingPlayer;
                    lastKnownPlayerPosition = playerTransform.position;
                }

                break;
            case EnemyState.Seeking:

                break;
            case EnemyState.FightingPlayer:
                if (!IsTransformInFOV(playerTransform, fieldOfView))
                {
                    state = EnemyState.Seeking;
                }
                else
                {
                    lastKnownPlayerPosition = playerTransform.position;
                    Vector3 targetPosition;
                    if (Vector3.Distance(playerTransform.position, transform.position) < desiredShootingDistance)
                    {
                        targetPosition = transform.position; //Could add: Raycast behind self to figure out if strafing or backing up is desirable
                    }
                    else
                    {
                        targetPosition = (transform.position - playerTransform.position).normalized * desiredShootingDistance;
                    }
                    navMeshAgent.SetDestination(targetPosition); //Could add: strafing left/right of this point, could add corner prediction to avoid LOS'ing too easily. 
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
        yield return null;
    }

}
