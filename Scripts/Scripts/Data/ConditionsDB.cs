using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{
   public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>() 
   {
        { 
            ConditionID.elec,
            new Condition()
            {
                Name = "Electrify",
                StartMessage = "has been zapped",
                // if called reduce hp by small amount
                OnAfterTurn = (Hedgehog hedgehog) =>
                {
                    hedgehog.UpdateHP(hedgehog.MaxHp / 8);
                    hedgehog.StatusChanges.Enqueue($"{hedgehog.Base.Name} has been electrified");
                }
            }
        }
   };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        /*
        else if (condition.ID= == ConditionID.slp || condition.ID == Condition.frz)
        {
            return 2f;
        }
        else if (condition.ID == ConditionID.par || condition.ID == ConditionID.psn || condition.ID == ConditionID.elec || condition.ID == ConditionID.brn)
        {
            return 1.5f;
        }
        */
        return 1f;
    }

}



public enum ConditionID
{
    none,
    elec,
    psn,
    brn,
    slp,
    par,
    frz
}