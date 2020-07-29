using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle, Patrolling, Seeking /*when alarmed/aware of player presence*/, FightingPlayer
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
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case EnemyState.Idle:
                //Just doing nothing.
                break;
            case EnemyState.Patrolling:
                Debug.Log(navMeshAgent.isStopped + " at " + Vector3.Distance(navMeshAgent.destination, transform.position));
                navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);

                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentPatrolPointIndex++;
                    currentPatrolPointIndex %= patrolPoints.Length;
                    navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex]);
                }
                break;
            case EnemyState.Seeking:

                break;
            case EnemyState.FightingPlayer:

                break;
        }

    }

    private bool IsPlayerInFOV()
    {
        return false;
    }

}
