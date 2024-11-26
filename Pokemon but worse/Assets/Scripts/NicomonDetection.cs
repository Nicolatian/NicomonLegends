using UnityEngine;
using UnityEngine.AI;

public class NicomonDetection : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;

    public NicomonBlueprints nicomonBlueprint;

    public float randomMoveRange; // radius of random movement
    public float followDistance = 10.0f; // Distance at which the enemy will follow the player
    public float stopFollowDistance = 15.0f; // Distance at which the enemy will stop following the player

    public float detectionRange = 10f;
    public float detectionAngle = 45f;
    public float battleTriggerDistance = 5f;
    public LayerMask playerLayer;


    private bool playerDetected = false;
    private bool hasTriggered = false;

    private void Update()
    {
        if (!hasTriggered)
        {
            if (playerDetected)
            {
                FollowPlayer();
            }
            else
            {
                DetectPlayer();
            }
        }
        else
        {
            CheckBattleEnd(); // Monitor if the battle is finished
        }
    }

    private void DetectPlayer()
    {
        if (player.GetComponent<PlayerMovement>().IsCrouching()) // Check if the player is crouching
        {
            return; // Don't detect the player if they are crouching
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer <= detectionAngle)
            {
                if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerDetected = true;
                        agent.SetDestination(player.position);
                    }
                }
            }
        }
    }



    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            agent.SetDestination(player.position);

            if (distanceToPlayer <= battleTriggerDistance)
            {
                TriggerBattle();
            }
        }
        else
        {
            agent.SetDestination(transform.position);
            playerDetected = false;
        }
    }

    private void TriggerBattle()
    {
        hasTriggered = true;
        agent.isStopped = true;

        // Inform BattleManager which Nicomon triggered the battle
        BattleManager.instance.SetEnemyNicomon(this.nicomonBlueprint);

        BattleManager.instance.StartBattle();
    }

    private void CheckBattleEnd()
    {
        if (BattleManager.instance.currentBattleState == BattleManager.BattleStates.BattleDone)
        {
            DisappearAfterBattle();
        }
    }

    private void DisappearAfterBattle()
    {
        Debug.Log("Battle ended. Enemy will disappear.");
        Destroy(gameObject); // Remove the enemy from the scene
    }

}