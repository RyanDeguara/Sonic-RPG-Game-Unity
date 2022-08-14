using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//instances of this class
[CreateAssetMenu(fileName = "Move", menuName = "Hedgehog/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] HedgehogType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp; // no. of times a move can be performed
    [SerializeField] int priority;

    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaryEffects;  
    [SerializeField] MoveTarget target;

    public string Name 
    {
        get { return name; }
    }

    public string Description 
    {
        get { return description; }
    }

    public HedgehogType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }
    
    public int Accuracy 
    {
        get { return accuracy; }
    }
    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    public int Pp
    {
        get { return pp; }
    }

    public int Priority
    {
        get { return priority; }
    }
    
    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects 
    {
        get { return effects; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }

    public List<SecondaryEffects> SecondaryEffects
    {
        get { return secondaryEffects; }
    }

    /*public bool isSpecial
    {
        get {
            if (type == HedgehogType.Speed || type == HedgehogType.Power || type == HedgehogType.Fly || type == HedgehogType.Metal || type == HedgehogType.Robot || type == HedgehogType.Chaos)
            {
                return true;
            }
            else
            {
                return false;
            } 
        }
    }*/

}

[System.Serializable] // show up in inspector
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get {return status; }
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects // inherit all of moveeffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance
    {
        get { return chance; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical, 
    Special, 
    Status
}

public enum MoveTarget
{
    Foe, 
    Self
}

/*
    None, 
    Normal,
    Speed,
    Power,
    Fly,
    Metal,
    Robot,
    Chaos
*/