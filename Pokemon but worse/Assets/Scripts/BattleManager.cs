using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;
using static UnityEngine.ParticleSystem;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    public NicomonBlueprints nicomon;

    [Header("UI Elements")]
    public GameObject battleMenu;
    public Image playerHealthBar;
    public Image enemyHealthBar;
    public TextMeshProUGUI battleLog;
    public Button[] movesButtons;
    public TextMeshProUGUI playerNicomonName;
    public TextMeshProUGUI enemyNicomonName;
    public TextMeshProUGUI playerHpText;


    [Header("Nicomon Information")]
    public NicomonBlueprints playerNicomon;
    public NicomonBlueprints[] enemyNicomons; // Array of potential enemy Nicomons
    private NicomonBlueprints currentEnemyNicomon; // Currently selected enemy
    private GameObject playerNicomonInstance; // Add this field to store the instance'

    public Transform playerSpawnPosition;
    public Transform enemySpawnPosition;

    [Header("Settings for UI and music")]
    public float textSpeed = 0.075f;
    private bool isTyping = false;
    private bool playerWonResult;
    private bool rotationSet = false; // Tracks if rotation has already been set
    AudioSource music;
    public enum BattleStates
    {
        StartBattle,
        PlayerTurn,
        EnemyTurn,
        BattleDone
    }

    public BattleStates currentBattleState;

    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        battleMenu.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!rotationSet && playerNicomonInstance != null && currentEnemyNicomon != null && currentEnemyNicomon.NicomonModel != null)
        {
            // Enemy looks at the player's Nicomon
            Vector3 directionToPlayer = playerNicomonInstance.transform.position - currentEnemyNicomon.NicomonModel.transform.position;
            directionToPlayer.y = 0; // Lock to horizontal plane
            currentEnemyNicomon.NicomonModel.transform.rotation = Quaternion.LookRotation(directionToPlayer);

            // Player looks at the enemy's Nicomon
            Vector3 directionToEnemy = currentEnemyNicomon.NicomonModel.transform.position - playerNicomonInstance.transform.position;
            directionToEnemy.y = 0; // Lock to horizontal plane
            playerNicomonInstance.transform.rotation = Quaternion.LookRotation(directionToEnemy);

            // Set the flag to true to prevent re-execution
            rotationSet = true;
        }
    }

    public void SetEnemyNicomon(NicomonBlueprints nicomonBlueprint)
    {
        // Find the matching Nicomon in the enemy array
        NicomonBlueprints foundNicomon = enemyNicomons.FirstOrDefault(n => n == nicomonBlueprint);

        if (foundNicomon != null)
        {
            // Ensure we set the correct Nicomon without modifying the scene objects incorrectly
            currentEnemyNicomon = foundNicomon;
            Debug.Log($"Set enemy Nicomon to: {currentEnemyNicomon.name}");

            
        }
        else
        {
            Debug.LogError("Enemy Nicomon not found in the array!");
        }
    }
    private void UpdatePlayerHpUI()
    {
        playerHpText.text = $"HP: {playerNicomon.currentHP}/{playerNicomon.maxHP}";
    }

    public void StartBattle()
    {
        rotationSet = false; // Reset the flag 
        battleMenu.SetActive(true);
        currentBattleState = BattleStates.StartBattle;
        music = GetComponent<AudioSource>();
        music.Play();



        playerNicomon.currentHP = playerNicomon.maxHP;
        currentEnemyNicomon.currentHP = currentEnemyNicomon.maxHP;

        playerHealthBar.fillAmount = playerNicomon.currentHP / (float)playerNicomon.maxHP;
        enemyHealthBar.fillAmount = currentEnemyNicomon.currentHP / (float)currentEnemyNicomon.maxHP;
        playerNicomonName.text = playerNicomon.name;
        enemyNicomonName.text = currentEnemyNicomon.name;



        GameObject enemyObject = GameObject.FindWithTag("EnemyNicomon");
        if (enemyObject != null)
        {
            currentEnemyNicomon.NicomonModel = enemyObject;  // Assign the actual GameObject to the Nicomon
        }

        playerNicomonInstance = Instantiate(playerNicomon.NicomonModel, playerSpawnPosition.position, playerSpawnPosition.rotation);


        // Start the typewriter effect for battle log
        StartCoroutine(TypeText($"A wild {currentEnemyNicomon.name} appeared!"));
        SetUpMoveButtons();

        // Delay before starting the player's turn
        Invoke("PlayerTurn", 3f);
    }


    private void SetUpMoveButtons()
    {
        // Get the list of moves from the player's Nicomon
        List<LearnableMoves> playerLearnableMoves = playerNicomon.learnableMoves;

        for (int i = 0; i < movesButtons.Length; i++)
        {
            if (i < playerLearnableMoves.Count)
            {
                movesButtons[i].gameObject.SetActive(true);

                movesButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = playerLearnableMoves[i].moveBlueprint.moveName;

                int index = i; // Capture the correct index for the listener
                movesButtons[i].onClick.RemoveAllListeners();
                movesButtons[i].onClick.AddListener(() => UsePlayerMove(index));
            }
            else
            {
                // If the index exceeds the number of moves, hide the button
                movesButtons[i].gameObject.SetActive(false);
            }
        }
    }


    private void PlayerTurn()
    {
        if (currentBattleState == BattleStates.BattleDone) return; // Avoid player turn if battle is over

        currentBattleState = BattleStates.PlayerTurn;
        StartCoroutine(TypeText("Choose your move!"));
    }

    public void UsePlayerMove(int moveIndex)
    {
        if (currentBattleState != BattleStates.PlayerTurn) return;

        MoveBlueprint selectedMove = playerNicomon.learnableMoves[moveIndex].moveBlueprint;
        PerformMove(selectedMove, currentEnemyNicomon);

        if (currentEnemyNicomon.currentHP <= 0)
        {
            if (currentBattleState != BattleStates.BattleDone)
            {
                BattleDone(true); // Player wins the battle
            }
            return;
        }

        // Switch to the enemy's turn
        currentBattleState = BattleStates.EnemyTurn;
        StartCoroutine(EnemyTurn()); // Switch to enemy's turn with proper wait   
    }

    private IEnumerator EnemyTurn()
    {
        if (currentBattleState != BattleStates.EnemyTurn) yield break;

        // Delay the enemy's turn slightly to simulate thinking or animation
        yield return new WaitForSeconds(5f);

        if (currentEnemyNicomon.learnableMoves.Count > 0)
        {
            MoveBlueprint enemyMove = currentEnemyNicomon.learnableMoves[Random.Range(0, currentEnemyNicomon.learnableMoves.Count)].moveBlueprint;

            if (enemyMove.CanUseThisMove)
            {
                PerformMove(enemyMove, playerNicomon);

                if (playerNicomon.currentHP <= 0)
                {
                    BattleDone(false);
                    yield break;
                }
            }
        }

        currentBattleState = BattleStates.PlayerTurn;
        yield return new WaitForSeconds(3f);

        PlayerTurn(); 
    }


    private void PerformMove(MoveBlueprint move, NicomonBlueprints targetNicomon)
    {
        NicomonBlueprints attackerNicomon = null;  // Identify the attacker
        Transform attackerTransform = null;       // Transform of the attacker

        // Determine the attacker based on the target
        if (targetNicomon == currentEnemyNicomon)
        {
            // Player is attacking the enemy
            attackerNicomon = playerNicomon;
            attackerTransform = playerNicomonInstance.transform;
        }
        else
        {
            // Enemy is attacking the player
            attackerNicomon = currentEnemyNicomon;
            attackerTransform = currentEnemyNicomon.NicomonModel.transform;
        }

        if (move.CanUseThisMove)
        {
            // Damage calculation
            float damage = ((float)attackerNicomon.attack / (float)targetNicomon.defence) * (float)move.power;
            float stabMultiplier = (move.elementType == attackerNicomon.elementType1 || move.elementType == attackerNicomon.elementType2) ? 1.25f : 1f;
            float typeEffectivenessMultiplier = TypeEffectiveness.GetEffectiveness(move.elementType, targetNicomon.elementType1) * TypeEffectiveness.GetEffectiveness(move.elementType, targetNicomon.elementType2);
            damage *= stabMultiplier * typeEffectivenessMultiplier;

            int finalDamage = Mathf.FloorToInt(damage);
            targetNicomon.TakeDamage(finalDamage);

            // Instantiate the particle system at the attacker's position and facing direction
            Vector3 spawnPosition = attackerTransform.position;
            Quaternion spawnRotation = attackerTransform.rotation * Quaternion.Euler(0, 0, 0);

            if (move.moveParticleEffect != null)
            {
                // Instantiate the particle system
                ParticleSystem particleInstance = Instantiate(move.moveParticleEffect, spawnPosition, spawnRotation);

                ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            }

            



            // Update health bar for the target Nicomon
            if (targetNicomon == currentEnemyNicomon)
            {
                enemyHealthBar.fillAmount = currentEnemyNicomon.currentHP / (float)currentEnemyNicomon.maxHP;
                StartCoroutine(TypeText($"{playerNicomon.name} used {move.moveName} and dealt {finalDamage} damage!"));
            }
            else
            {
                playerHealthBar.fillAmount = playerNicomon.currentHP / (float)playerNicomon.maxHP;
                StartCoroutine(TypeText($"{currentEnemyNicomon.name} used {move.moveName} and dealt {finalDamage} damage!"));
            }

            // Apply move usage logic
            move.UseMove();
        }
        else
        {
            StartCoroutine(TypeText($"{move.moveName} cannot be used because there are no uses left!"));
        }

        UpdatePlayerHpUI();
    }


    private void BattleDone(bool playerWon)
    {
        currentBattleState = BattleStates.BattleDone;
        StartCoroutine(TypeText(playerWon ? "You won the battle!" : "You lost the battle!"));

        StartCoroutine(EndBattle(playerWon));
    }

    private IEnumerator EndBattle(bool playerWon)
    {
        yield return new WaitForSeconds(3f);

        StartCoroutine(TypeText(playerWon ? "You won the battle!" : "You lost the battle!"));

        yield return new WaitForSeconds(3f);

        if (playerNicomonInstance != null)
        {
            Destroy(playerNicomonInstance);
        }

        battleMenu.SetActive(false);
        music = GetComponent<AudioSource>();
        music.Stop();

    }

    private IEnumerator TypeText(string message)
    {
        // Only start typing if no other text is being typed
        if (isTyping)
            yield break;

        isTyping = true;
        battleLog.text = ""; // Clear existing text

        foreach (char letter in message.ToCharArray())
        {
            battleLog.text += letter;
            yield return new WaitForSeconds(textSpeed); // Wait based on text speed
        }

        isTyping = false;
    }
}

// ChatGPT helped me with this way of utlizing my pokemon typing.
public class TypeEffectiveness
{
    // Effectiveness matrix
    // Row: Attacker type
    // Column: Defender type
    private static readonly float[,] effectivenessMatrix =
    {
    // Grass, Water, Fire, Normal, Rock, Fighting, Flying
    {0.5f, 1.25f, 0.5f, 1f, 1.25f, 1f, 1f},  // Grass
    {1f, 0.5f, 1.25f, 1f, 1.25f, 1f, 1f},    // Water
    {1.25f, 0.5f, 0.5f, 1f, 0.5f, 1f, 1f},  // Fire
    {1f, 1f, 1f, 1f, 0.5f, 0.5f, 1f},       // Normal
    {1f, 1f, 1.25f, 1f, 0.5f, 1f, 1.25f},   // Rock
    {1f, 1f, 1f, 1.25f, 1.25f, 0.5f, 0.5f},   // Fighting
    {1.25f, 1f, 1f, 1f, 0.5f, 1.25f, 0.5f}   // Flying
};

    // Use this method to get the effectiveness for an attack
    public static float GetEffectiveness(NicomonType attackerType, NicomonType defenderType)
    {
        // Convert enum values to array indices
        int attackerIndex = (int)attackerType;
        int defenderIndex = (int)defenderType;

        // Return the effectiveness value
        return effectivenessMatrix[attackerIndex, defenderIndex];
    }
}
