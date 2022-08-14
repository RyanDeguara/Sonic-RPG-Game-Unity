using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Create instances of scriptableobject
[CreateAssetMenu(fileName = "Hedgehog", menuName = "Hedgehog/Create")]
public class HedgehogBase : ScriptableObject
{
    [SerializeField] string name; // use this variable outside of this class
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] HedgehogType type1;
    [SerializeField] HedgehogType type2;
    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;
    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    public static int MaxNumOfMoves { get; set;} = 4;

    //properties
    /*public string GetName()
    {
        return name;
    }*/

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return 6 * (level * level * level) / 5 - 15 * (level * level) + 100 * level - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            return GetFluctuating(level);
        }

        return -1; // error growth rate doesnt exist
    }

    public int GetFluctuating(int level)
    {
        if (level <= 15)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor((level + 1) / 3) + 24) / 50));
        }
        else if (level >= 15 && level <= 36)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level + 14) / 50));
        }
        else
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(level / 2) + 32) / 50));
        }
    }

    public string Name 
    {
        get { return name; }
    }

    public string Description 
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public HedgehogType Type1
    {
        get { return type1; }
    }

    public HedgehogType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }
    
    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public int CatchRate => catchRate; // short way to write if only need getter

    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;

}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase Base 
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}

public enum HedgehogType
{
    None, 
    Normal,
    Speed,
    Power,
    Fly,
    Metal,
    Robot,
    Chaos,
    Explosive
}

public enum GrowthRate
{
    Fast, 
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float [][] chart =
    {
        //                      NOR SPE POW FLY MET ROB CHA EXP
        /* Normal*/ new float[] {1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /* Speed */ new float[] {1f, 0.5f, 1f, 0.5f, 2f, 2f, 0.5f, 1f}, // 2 weak 2 strong
        /* Power */ new float[] {1f, 1f, 0.5f, 2f, 0.5f, 2f, 0.5f, 1f}, // 2 weak 2 strong 
        /* Fly */   new float[] {1f, 2f, 2f, 0.5f, 0.5f, 1f, 0.5f, 1f}, // 2 weak 2 strong
        /* Metal */ new float[] {1f, 0.5f, 2f, 2f, 0.5f, 1f, 0.5f, 1f}, // 2 weak 2 strong
        /* Robot */ new float[] {1f, 0.5f, 0.5f, 1f, 1f, 0.5f, 0.5f, 1f}, // 3 weak
        /* Chaos */ new float[] {2f, 2f, 2f, 2f, 2f, 2f, 0.5f, 2f}, //  6 strong
        /* Explosive */ new float[] {2f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f}
    };

    public static float GetEffectiveness(HedgehogType attackType, HedgehogType defenseType)
    {
        if (attackType == HedgehogType.None || defenseType == HedgehogType.None)
        {
            return 1;
        }

        int row = (int) attackType - 1;
        int col = (int) defenseType - 1;
        return chart[row][col];

    }

}