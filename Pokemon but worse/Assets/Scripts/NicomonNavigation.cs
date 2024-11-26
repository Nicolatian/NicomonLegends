using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NicomonNavigation : MonoBehaviour
{
    public NavMeshAgent agent;
    public float randomMoveRange; // radius of random movement
    public Transform centrePoint; // Centre of random movement area (optional)
    public Transform player; // Reference to the player's transform
    public float followDistance = 10.0f; // Distance at which the enemy will follow the player
    public float stopFollowDistance = 15.0f; // Distance at which the enemy will stop following the player

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= followDistance) // Player is within follow range
        {
            agent.SetDestination(player.position);
        }
        else if (distanceToPlayer > stopFollowDistance) // Player is too far, switch to random movement
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 point;
                if (RandomPoint(centrePoint.position, randomMoveRange, out point)) // Random movement within the defined area
                {
                    Debug.DrawRay(point, Vector3.up, Color.yellow, 1.0f);
                    agent.SetDestination(point);
                }
            }
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; // Generate a random point
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}