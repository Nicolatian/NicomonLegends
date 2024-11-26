using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "New Move", menuName = "Move")]
public class MoveBlueprint : ScriptableObject
{
    public string moveName;
    public int power;
    public float accuracy; // Accuracy from 0 to 100
    public NicomonType elementType; // enum for element typing
    public int maxUsage; //PP limit
    private int currentUsage;

    public ParticleSystem moveParticleEffect; // Adds a particle effect for the move

    public void OnEnable()
    {
        currentUsage = maxUsage;
    }

    public bool CanUseThisMove
    {
        get { return currentUsage > 0; }
    }

    public void UseMove()
    {
        if (CanUseThisMove)
        {
            currentUsage--;
           
        }
        else
        {
            Debug.Log($"{moveName} has no uses left!");
        }
    }

    public bool isMoveSuccesfull()
    {
        return Random.Range(0f,100f) <= accuracy;
    }


}

public enum NicomonMoveType
{
    Grass,
    Water,
    Fire,
    Normal,
    Rock,
    Fighting,
    Flying
}
