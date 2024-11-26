using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Nicomon", menuName = "Nicomon")]
public class NicomonBlueprints : ScriptableObject
{
    public GameObject NicomonModel;
    public string uniqueID; // A unique identifier for this Nicomon

    public NicomonType elementType1;
    public NicomonType elementType2;

    public int maxHP = 5;
    public int currentHP = 2;

    public int attack;
    public int defence;
    public int speed;

    public List<LearnableMoves> learnableMoves;

    public void ResetHP()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }
}

[System.Serializable]
public struct LearnableMoves
{
    public MoveBlueprint moveBlueprint;

    public MoveBlueprint move { get; internal set; }
}

public enum NicomonType
{
    Grass,
    Water,
    Fire,
    Normal,
    Rock,
    Fighting,
    Flying
}
