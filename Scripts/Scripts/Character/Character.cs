using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    CharacterAnimator animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }

    // move player from current position from starting position over a period of time
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null) // move vector can find target position
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f); // min and max
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos)) // if target position is not a walkable tile
        {
            yield break;
        }

        IsMoving = true;

        //first called - check if difference between target position and players current position is greater 
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // move player towards target position by amount
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; //stop execution of coding and resume in next update function call
        }
        transform.position = targetPos;
        IsMoving = false;

        OnMoveOver?.Invoke(); // if not null dont call it - ?
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        //create a box collider
        var diff = targetPos - transform.position;
        var dir = diff.normalized; // normalizing will have same vector but length of 1

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer) == true) // add dir to add 1 unit to direction that wont go in the path, magnitude should be 1 less than this
        {
            return false;
        }

        return true;
        
        // returns true if there is a boxcast in the area so its not clear
    } 
    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    public void LookTowards(Vector3 targetPos)
    { // make npc look toward player

        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x); //mathf makes it float
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f); 
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
        {
            Debug.LogError("Error in look towards: you cant ask the character to look diagonally");
        }
    }

    public CharacterAnimator Animator
    {
        get => animator; 
    }
}
