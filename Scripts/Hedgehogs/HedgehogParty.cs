using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HedgehogParty : MonoBehaviour
{
    [SerializeField] List<Hedgehog> hedgehogs;
    public List<Hedgehog> Hedgehogs
    {
        get
        {
            return hedgehogs;
        }
    }
    private void Start()
    {
        foreach (var hedgehog in hedgehogs)
        {
            hedgehog.Init();
        }
    }

    public Hedgehog GetHealthyHedgehog()
    {
        // Where loops through hedgehogs and return hedgehogs which satisfy the condition - return all hedgehogs that are non fainted
        return hedgehogs.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddHedgehog(Hedgehog newHedgehog)
    {
        if (hedgehogs.Count < 6)
        {
            hedgehogs.Add(newHedgehog);
        }
        else
        {
            // To Do: add to the PC once implemented
        }
    }
}