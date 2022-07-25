using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Solve issue when using keys in battle affecting players position in the freeroam map
public enum GameState { FreeRoam, Battle }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    
    GameState state;
    public GameObject Joystick;
    Vector3 originalpos;

    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
        originalpos = Joystick.transform.position;

    }

    void StartBattle()
    {
        state = GameState.Battle;
        // Disable main camera and enable battle
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        
        var playerParty = playerController.GetComponent<HedgehogParty>();
        var wildHedgehog = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildHedgehog();

        battleSystem.StartBattle(playerParty, wildHedgehog);

        //Joystick.transform.position = originalpos + 200;
        Joystick.transform.localPosition = new Vector3(-100000f, originalpos.y);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        Joystick.transform.position = originalpos;

    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}
