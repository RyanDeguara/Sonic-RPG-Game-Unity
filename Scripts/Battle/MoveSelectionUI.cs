using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Color highlightedColor;
    int currentSelection = 0;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleDialogBox dialogBox;
    MoveBase moveToLearn;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; ++i) // loop will set the current name of the moves to this list
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, HedgehogBase.MaxNumOfMoves);
        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void HandleNextMoveSelection()
    {
        ++currentSelection;

        if (currentSelection == HedgehogBase.MaxNumOfMoves+1)
        {
            currentSelection = 0;
        }

        UpdateMoveSelection(currentSelection);
    }


    public void UpdateMoveSelection(int selection)
    {
        for (int i =0; i < HedgehogBase.MaxNumOfMoves+1; i++)
        {
            if (i == selection)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                if (i == HedgehogBase.MaxNumOfMoves)
                {
                    moveTexts[i].color = Color.yellow;
                }
                else
                {
                    moveTexts[i].color = Color.black;
                }
            }
        }
    }

    public int CurrentSelection
    {
        //get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
        get { return currentSelection; }
    }
}
