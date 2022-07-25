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
    [SerializeField] int pp; // no. of times a move can be performed

    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
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

    public int Pp
    {
        get { return pp; }
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
    public List<StatBoost> Boosts
    {
        get { return boosts; }
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