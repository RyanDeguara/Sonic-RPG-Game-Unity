using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// need a var to store state of the battle
public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    HedgehogParty playerParty;
    Hedgehog wildHedgehog;

    public GameObject ok1;
    public GameObject ok2;
    public GameObject next1;
    public GameObject next2;
    public GameObject ok3;
    public GameObject next3;

    public void StartBattle(HedgehogParty playerParty, Hedgehog wildHedgehog)
    {
        this.playerParty = playerParty;
        this.wildHedgehog = wildHedgehog;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyHedgehog());
        enemyUnit.Setup(wildHedgehog);

        /*ok2.SetActive(false);
        next2.SetActive(false);
        ok1.SetActive(false);
        next1.SetActive(false);
        */
        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Hedgehog.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Hedgehog.Base.Name} appeared.");

        //instead of just calling ActionSelection():
        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Hedgehog.Speed >= enemyUnit.Hedgehog.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }


    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Hedgehogs.ForEach(p => p.OnBattleOver());
        OnBattleOver(won); // event that notifies controller if battle is over

    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;

        // StartCoroutine(dialogBox.TypeDialog("Choose an action")); or ->
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
        
        /*ok1.SetActive(true);
        next1.SetActive(true);
        ok2.SetActive(false);
        next2.SetActive(false);
        */
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Hedgehogs);
        partyScreen.gameObject.SetActive(true);
        /*ok1.SetActive(false);
        next1.SetActive(false);
        ok2.SetActive(false);
        next2.SetActive(false);
        */
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
        /*ok1.SetActive(false);
        next1.SetActive(false);
        ok2.SetActive(true);
        next2.SetActive(true);*/
    }

    // coroutine - player hedgehog will perform move and enemy hedgehog will take damage
    IEnumerator PlayerMove()
    {
        /*ok1.SetActive(false);
        next1.SetActive(false);
        ok2.SetActive(false);
        next2.SetActive(false);*/
        state = BattleState.PerformMove;
        var move = playerUnit.Hedgehog.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        // if the battle state wasnt changed by RunMove, go to next step
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
        
        
    }

    IEnumerator EnemyMove()
    {
        /*ok1.SetActive(false);
        next1.SetActive(false);
        ok2.SetActive(false);
        next2.SetActive(false);*/
        state = BattleState.PerformMove;
        var move = enemyUnit.Hedgehog.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        // if the battle state wasnt changed by RunMove, go to next step
        if (state == BattleState.PerformMove)
        {
            // go back to action selection
            ActionSelection();
        }
        
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.Pp--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Hedgehog.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Hedgehog, targetUnit.Hedgehog);
        }
        else // if not a status move
        {
            var damageDetails = targetUnit.Hedgehog.TakeDamage(move, sourceUnit.Hedgehog);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (targetUnit.Hedgehog.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Hedgehog.Base.Name} Fainted");
            targetUnit.PlayFaintAnimation();  
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Hedgehog source, Hedgehog target)
    {
        var effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else // if foe
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }
        
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    //check if any messages inside status changes queue and show all of them in dialog box
    IEnumerator ShowStatusChanges(Hedgehog hedgehog)
    {
        while (hedgehog.StatusChanges.Count > 0)
        {
            var message = hedgehog.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextHedgehog = playerParty.GetHealthyHedgehog();
            if (nextHedgehog != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        /*ok1.SetActive(false);
        next1.SetActive(false);
        ok2.SetActive(false);
        next2.SetActive(false);*/
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");

        }
        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if ( damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        /*if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
          if (currentAction > 0)
            {
                --currentAction;
            }  
        }*/
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if ( currentAction == 1)
            {
                // Bag
            }
            else if ( currentAction == 2)
            {
                // Hedgehog
                OpenPartyScreen();
            }
            else if ( currentAction == 3)
            {
                // Run
                
            }
        }
    }

    public void NextHandleSelection2()
    {

        ++currentAction;
        if (currentAction == 4)
        {
            currentAction = 0;
        }

        dialogBox.UpdateActionSelection(currentAction);
        
    }

    public void EnterSelection()
    {
        dialogBox.UpdateActionSelection(currentAction);
        if (currentAction == 0)
        {
            //fight
            MoveSelection();
        }
        if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if ( currentAction == 1)
            {
                // Bag
            }
            else if ( currentAction == 2)
            {
                // Hedgehog
                OpenPartyScreen();
            }
            else if ( currentAction == 3)
            {
                // Run
                
            }
    }

    void HandleMoveSelection()
    {
        /*
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Hedgehog.Moves.Count - 1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
          if (currentMove > 0)
            {
                --currentMove;
            }  
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
          if (currentMove < playerUnit.Hedgehog.Moves.Count - 2)
            {
                currentMove += 2;
            }  
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
          if (currentMove > 1)
            {
                currentMove -= 2;
            }  
        }
        */
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Hedgehog.Moves.Count-1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Hedgehog.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    public void NextMoveSelection()
    {
        ++currentMove;

        if (currentMove == playerUnit.Hedgehog.Moves.Count)
        {
            currentMove = 0;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Hedgehog.Moves[currentMove]);
        
    }

    public void EnterMoveSelection()
    {
        dialogBox.UpdateActionSelection(currentAction);
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);
        StartCoroutine(PlayerMove());
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Hedgehogs.Count-1);

        partyScreen.UpdateMemberSelection(currentMember);
    
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Hedgehogs[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send a fainted hedgehog");
                return;
            }
            if (selectedMember == playerUnit.Hedgehog)
            {
                partyScreen.SetMessageText("You can't switch with the same hedgehog");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchHedgehog(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }

    }

    public void NextPartySelection()
    {

        ++currentMember;

        if (currentMember == playerParty.Hedgehogs.Count)
        {
            currentMember = 0;
        }

        partyScreen.UpdateMemberSelection(currentMember);

    }

    public void SelectPartySelection()
    {
        var selectedMember = playerParty.Hedgehogs[currentMember];
        if (selectedMember.HP <= 0)
        {
            partyScreen.SetMessageText("You can't send a fainted hedgehog");
            return;
        }
        if (selectedMember == playerUnit.Hedgehog)
        {
            partyScreen.SetMessageText("You can't switch with the same hedgehog");
            return;
        }
        partyScreen.gameObject.SetActive(false);
        state = BattleState.Busy;
        StartCoroutine(SwitchHedgehog(selectedMember));
    }

    IEnumerator SwitchHedgehog(Hedgehog newHedgehog)
    {
        bool currentHedgehogFainted = true;
        if (playerUnit.Hedgehog.HP > 0)
        {
            currentHedgehogFainted = false;
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Hedgehog.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newHedgehog);

        dialogBox.SetMoveNames(newHedgehog.Moves);

        yield return dialogBox.TypeDialog($"Go {newHedgehog.Base.Name}!");
        

        if (currentHedgehogFainted)
        {
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
}
