using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// need a var to store state of the battle
public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver}
public enum BattleAction { Move, SwitchHedgehog, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject sonicBallSprite;
 
    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = true;

    HedgehogParty playerParty;
    HedgehogParty trainerParty;
    Hedgehog wildHedgehog;
    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;

    public void StartBattle(HedgehogParty playerParty, Hedgehog wildHedgehog)
    {
        this.playerParty = playerParty;
        this.wildHedgehog = wildHedgehog;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(HedgehogParty playerParty, HedgehogParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {

        //HUD wont be shown at start of battle
        playerUnit.Clear();
        enemyUnit.Clear();
  
        if (!isTrainerBattle)
        {
            //Wild battle
            playerUnit.Setup(playerParty.GetHealthyHedgehog());
            enemyUnit.Setup(wildHedgehog);
            dialogBox.SetMoveNames(playerUnit.Hedgehog.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Hedgehog.Base.Name} appeared.");
        }
        else
        {
            //trainer battle

            //show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            //send out first trainer character
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyHedgehog = trainerParty.GetHealthyHedgehog();
            enemyUnit.Setup(enemyHedgehog);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyHedgehog.Base.Name}");

            //send out first hedgehog of player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerHedgehog = playerParty.GetHealthyHedgehog();
            playerUnit.Setup(playerHedgehog);
            yield return dialogBox.TypeDialog($"Go {playerHedgehog.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Hedgehog.Moves);
        }
        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
        
    }

    /*
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
    */


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
        
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Hedgehogs);
        partyScreen.gameObject.SetActive(true);
        
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Hedgehog newHedgehog)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to send out {newHedgehog.Base.Name}. Do you want to change your fighter?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        //if player performs a move, perform move 1 by 1 and if player switches hedgehog, switch and then let enemy perform a move
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Hedgehog.CurrentMove = playerUnit.Hedgehog.Moves[currentMove];
            enemyUnit.Hedgehog.CurrentMove = enemyUnit.Hedgehog.GetRandomMove();

            int playerMovePriority = playerUnit.Hedgehog.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Hedgehog.CurrentMove.Base.Priority;
            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority) // only if the enemy and player have the same priority check their speed
            {
                playerGoesFirst = playerUnit.Hedgehog.Speed > enemyUnit.Hedgehog.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondHedgehog = secondUnit.Hedgehog;

            //First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Hedgehog.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) 
            {
                yield break;
            }

            if (secondHedgehog.HP > 0)
            {
                //Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Hedgehog.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) 
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchHedgehog)
            {
                var selectedHedgehog = playerParty.Hedgehogs[currentMember];
                state = BattleState.Busy;
                yield return SwitchHedgehog(selectedHedgehog);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowBall();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            // Enemy turn
            var enemyMove = enemyUnit.Hedgehog.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) 
            {
                yield break;
            }
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }

    }


    /*
    // coroutine - player hedgehog will perform move and enemy hedgehog will take damage
    IEnumerator PlayerMove()
    {
        state = BattleState.RunningTurn;
        var move = playerUnit.Hedgehog.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        // if the battle state wasnt changed by RunMove, go to next step
        if (state == BattleState.RunningTurn)
        {
            StartCoroutine(EnemyMove());
        }
        
        
    }
    */
    /*
    IEnumerator EnemyMove()
    {
        state = BattleState.RunningTurn;
        var move = enemyUnit.Hedgehog.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        // if the battle state wasnt changed by RunMove, go to next step
        if (state == BattleState.RunningTurn)
        {
            // go back to action selection
            ActionSelection();
        }
        
    }
    */

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.Pp--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Hedgehog.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Hedgehog, targetUnit.Hedgehog))
        {        
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Hedgehog, targetUnit.Hedgehog, move.Base.Target);
            }
            else // if not a status move
            {
                var damageDetails = targetUnit.Hedgehog.TakeDamage(move, sourceUnit.Hedgehog);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Hedgehog.HP > 0)
            {
                foreach (var secondary in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1,101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Hedgehog, targetUnit.Hedgehog, secondary.Target);
                    }
                }
            }

            if (targetUnit.Hedgehog.HP <= 0)
            {
                yield return HandleHedgehogFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Hedgehog.Base.Name}'s attack missed");
        }

        
    }

    bool CheckIfMoveHits(Move move, Hedgehog source, Hedgehog target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f /3f, 5f /3f, 2f, 7f / 3f, 8f/3f, 3f};
        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }
        
        return UnityEngine.Random.Range(1,101) <= moveAccuracy;
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Hedgehog source, Hedgehog target, MoveTarget moveTarget)
    {
        // Stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else // if foe
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver)
        {
            yield break;
        }

        // pause execution until condition (runningTurn) is true
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        
        //statuses hurt after hedgehog move
        sourceUnit.Hedgehog.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Hedgehog);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Hedgehog.HP <= 0)
        {
            yield return HandleHedgehogFainted(sourceUnit);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
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

    IEnumerator HandleHedgehogFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Hedgehog.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();  
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //Exp Gain
            int expYield = faintedUnit.Hedgehog.Base.ExpYield;
            int enemyLevel = faintedUnit.Hedgehog.Level;
            float trainerBonus = (isTrainerBattle)? 1.5f : 1f; // if is trainer battle it is 1.5 otherwise it is 1

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7); //formula from pokemon games
            playerUnit.Hedgehog.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Hedgehog.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            //Check if gained enough xp to level up
            while (playerUnit.Hedgehog.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Hedgehog.Base.Name} grew to level {playerUnit.Hedgehog.Level}");

                //Try to learn a new move
                var newMove = playerUnit.Hedgehog.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Hedgehog.Moves.Count < HedgehogBase.MaxNumOfMoves)
                    {
                        playerUnit.Hedgehog.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Hedgehog.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Hedgehog.Moves);
                    }
                    else
                    {
                        // To Do: Option to forget a move
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }
            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextHedgehog = trainerParty.GetHealthyHedgehog();
                if (nextHedgehog != null)
                {
                    //send out next hedgehog
                    StartCoroutine(AboutToUse(nextHedgehog));
                }
                else
                {
                    BattleOver(true);
                }
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {

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
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
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
            EnterSelection();
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
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if ( currentAction == 2)
            {
                // Hedgehog
                prevState = state;
                OpenPartyScreen();
            }
            else if ( currentAction == 3)
            {
                // Run
                StartCoroutine(RunTurns(BattleAction.Run));
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
            EnterMoveSelection();
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
        var move = playerUnit.Hedgehog.Moves[currentMove];
        if (move.Pp == 0)
        {
            return;
        }
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);
        StartCoroutine(RunTurns(BattleAction.Move));
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
            SelectPartySelection();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            //player must select hedgehog to continue
            if (playerUnit.Hedgehog.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a fighter to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerHedgehog());
            }
            else
            {
                ActionSelection();
            }
            
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
        if (prevState == BattleState.ActionSelection)
        { // if player switches hedgehog during a turn, run as a turn
            prevState = null;
            StartCoroutine(RunTurns(BattleAction.SwitchHedgehog));
        }
        else
        { // otherwise switch hedgehog
            state = BattleState.Busy;
            StartCoroutine(SwitchHedgehog(selectedMember));
        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            selectUse();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerHedgehog());
        }
    }

    public void nextUse()
    {
        aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);
    }

    public void selectUse()
    {
        dialogBox.EnableChoiceBox(false);
        if (aboutToUseChoice == true)
        {
            // Yes option
            prevState = BattleState.AboutToUse;
            OpenPartyScreen();
        }
        else
        {
            // no option
            StartCoroutine(SendNextTrainerHedgehog());
        }
    }

    IEnumerator SwitchHedgehog(Hedgehog newHedgehog)
    {
        if (playerUnit.Hedgehog.HP > 0)
        {
            dialogBox.EnableActionSelector(false);
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Hedgehog.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newHedgehog);

        dialogBox.SetMoveNames(newHedgehog.Moves);

        yield return dialogBox.TypeDialog($"Go {newHedgehog.Base.Name}!");
        
        /*
        if (currentHedgehogFainted)
        {
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }*/

        if (prevState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if (prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerHedgehog());
        }
    }

    IEnumerator SendNextTrainerHedgehog()
    {
        state = BattleState.Busy; // so nothing else happens when being switched

        var nextHedgehog = trainerParty.GetHealthyHedgehog();
        enemyUnit.Setup(nextHedgehog);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextHedgehog.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowBall()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You cant steal the trainer's fighter!");
            state = BattleState.RunningTurn;
            yield break;
        }
        playerUnit.gameObject.SetActive(false);
        playerImage.gameObject.SetActive(true);
        playerImage.sprite = player.Sprite;
        yield return dialogBox.TypeDialog($"{player.Name} used a ring on Sonic!");
        
        var sonicspinObj = Instantiate(sonicBallSprite, playerUnit.transform.position, Quaternion.identity); // quaternion used if dont want any rotation
        var ball = sonicspinObj.GetComponent<SpriteRenderer>();

        //Animations
        yield return ball.transform.DOJump(enemyUnit.transform.position, 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        //yield return ball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();
        int shakeCount = TryToCatchHedgehog(enemyUnit.Hedgehog);
        
        /*
        for (int i = 0; i<Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return ball.transform.DoPunchRotation(new Vector3(0,0,10f), 0.8)).WaitForCompletion();
        }
        */

        if (shakeCount == 4)
        {
            // hedgehog is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Hedgehog.Base.Name} was knocked out and captured!");
            yield return ball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddHedgehog(enemyUnit.Hedgehog);
            yield return dialogBox.TypeDialog($"{enemyUnit.Hedgehog.Base.Name} has joined your team");
            playerUnit.gameObject.SetActive(true);
            playerImage.gameObject.SetActive(false);
            Destroy(ball);
            BattleOver(true);
            
        }
        else
        {
            // hedgehog broke out
            yield return new WaitForSeconds(1f);
            ball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();
            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Hedgehog.Base.Name} was not knocked out");
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Hedgehog.Base.Name} was almost knocked out");
            }
            
            Destroy(ball);
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            state = BattleState.RunningTurn; // continue the battle
            
        }
    }

    int TryToCatchHedgehog(Hedgehog hedgehog)
    {
        // algorithm from pokemon games that determine whether can catch or not
        float a = (3 * hedgehog.MaxHp - 2 * hedgehog.HP) * hedgehog.Base.CatchRate * ConditionsDB.GetStatusBonus(hedgehog.Status) / (3 * hedgehog.MaxHp);

        if (a >= 255)
        {
            return 4;
        }
        float b = 1048560 / MathF.Sqrt(MathF.Sqrt(16711680 / a));
        int shakeCount = 0;

        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            dialogBox.EnableActionSelector(false);
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break; 
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Hedgehog.Speed;
        int enemySpeed = enemyUnit.Hedgehog.Speed;
        
        if (enemySpeed < playerSpeed)
        {
            dialogBox.EnableActionSelector(false);
            yield return dialogBox.TypeDialog($"Ran away safetly!");
            BattleOver(true);
        }
        else
        {
            // formula used to calculate f value from pokemon games
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts; // escape attempts - no. of times player tried to escape
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                dialogBox.EnableActionSelector(false);
                yield return dialogBox.TypeDialog($"Ran away safetly!");
                BattleOver(true);
            }
            else
            {
                dialogBox.EnableActionSelector(false);
                yield return dialogBox.TypeDialog($"Cannot escape! Enemy is too quick!");
                state = BattleState.RunningTurn;
            }
        }
    }

}
