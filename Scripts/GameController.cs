using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Solve issue when using keys in battle affecting players position in the freeroam map
public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    
    GameState state;
    GameState stateBeforePause;
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    public static GameController Instance { get; private set; }

    public GameObject Joystick;
    Vector3 originalpos;

    private void Awake()
    {
        Instance = this;
        //ConditionsDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        originalpos = Joystick.transform.position;
        
        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
        };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        // Disable main camera and enable battle
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        
        var playerParty = playerController.GetComponent<HedgehogParty>();
        var wildHedgehog = CurrentScene.GetComponent<MapArea>().GetRandomWildHedgehog();

        var wildHedgehogCopy = new Hedgehog(wildHedgehog.Base, wildHedgehog.Level);

        battleSystem.StartBattle(playerParty, wildHedgehogCopy);

        //Joystick.transform.position = originalpos + 200;
        Joystick.transform.localPosition = new Vector3(-100000f, originalpos.y);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        // Disable main camera and enable battle
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        
        var playerParty = playerController.GetComponent<HedgehogParty>();
        var trainerParty = trainer.GetComponent<HedgehogParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);

        //Joystick.transform.position = originalpos + 200;
        Joystick.transform.localPosition = new Vector3(-100000f, originalpos.y);
    }

    public void OnEnterTraintersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

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
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }
}
