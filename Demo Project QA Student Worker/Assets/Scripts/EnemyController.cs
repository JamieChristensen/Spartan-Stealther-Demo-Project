using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle, Seeking /*when alarmed/aware of player presence*/, FightingPlayer
    }
    public NavMeshAgent navMeshAgent;

    [SerializeField]
    private Transform playerTransform; //Assign in inspector.

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
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        navMeshAgent.SetDestination(playerTransform.position);
    }

}
