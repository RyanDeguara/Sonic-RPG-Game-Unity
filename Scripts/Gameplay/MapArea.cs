using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Hedgehog> wildHedgehogs;

    public Hedgehog GetRandomWildHedgehog()
    {
        var wildHedgehog = wildHedgehogs[Random.Range(0, wildHedgehogs.Count)];
        wildHedgehog.Init();
        return wildHedgehog;
    }
}
