using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Joystick joystick;
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer;

    public event Action OnEncountered;
    private bool isMoving;
    private Vector2 input;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void HandleUpdate() // change to handleupdate so its not called automatically
    {
        input.x = 0;
        input.y = 0;
        if(!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            
            
            //input.x = joystick.Horizontal;
            //input.y = joystick.Vertical;
            // remove diagonal moving, cannot both be non zero at a time
            if (input.x != 0) input.y = 0;


            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                
                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
                
            }
        }
        animator.SetBool("isMoving", isMoving);
    }
    // move player from current position from starting position over a period of time
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        //first called - check if difference between target position and players current position is greater 
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // move player towards target position by amount
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; //stop execution of coding and resume in next update function call
        }
        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if (UnityEngine.Random.Range(1,101) <= 10) //1-100
            {
                animator.SetBool("isMoving", false);
                OnEncountered();

            }
        }
    }
}
