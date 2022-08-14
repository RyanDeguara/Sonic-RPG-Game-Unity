using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// classes will only be shown in inspector if we use this attribute
[System.Serializable]
public class Hedgehog
{
    [SerializeField] HedgehogBase _base;
    [SerializeField] int level;

    public Hedgehog(HedgehogBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public HedgehogBase Base 
    {
        get 
        {
            return _base;
        }
    }
    public int Level {
        get
        {
            return level;
        }
    }

    public int Exp { get; set; }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    // dictionary - like a list but along with the value, stores a key
    public Dictionary<Stat, int> Stats { get; private set; }

    // dictionary for stats that can be increased and decreased for each move
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }

    // used to store a list of elements like a list but can also take elements out of a queue in order of which added to the queue
    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public void Init()
    {
        
        // When we create a pokemon this code will create the moves based on its level
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= HedgehogBase.MaxNumOfMoves)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(level);

        CalculateStats();
        HP = MaxHp;
        StatusChanges = new Queue<string>();
        ResetStatBoost();
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        //apply stat boost
        int boost = StatBoosts[stat];

        // if value of boost is 1 multiply stat value by 1.5, if negative do the same but instead of multiplying we divide 
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }
        
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault(); //returns list of learnable moves that has current level
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > HedgehogBase.MaxNumOfMoves)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack 
    {
        //get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    // determine who gets first move
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp
    {
        get; 
        private set;
    }

    public DamageDetails TakeDamage(Move move, Hedgehog attacker)
    {
        // critical hits
        float critical = 1f;
        if (Random.value * 100f <= 6.25f) // bring up 6.25 for more critical hit percentage chance
        {
            critical = 2f;
        }

        // Super or non effective hit?
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        
        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        // formula used by pokemon typical games
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        
        UpdateHP(damage);
        
        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionID)
    {
        Status = ConditionsDB.Conditions[conditionID];
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.Pp > 0).ToList();
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this); // only invoke if not null
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }
}


// details of the damage
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}    
