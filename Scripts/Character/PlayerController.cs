using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    public Joystick joystick;
    public float moveSpeed;
    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }
    public void HandleUpdate() // change to handleupdate so its not called automatically
    {
        input.x = 0;
        input.y = 0;
        if(!character.IsMoving)
        {
            //input.x = Input.GetAxisRaw("Horizontal");
            //input.y = Input.GetAxisRaw("Vertical");
            
            
            input.x = joystick.Horizontal;
            input.y = joystick.Vertical;
            // remove diagonal moving, cannot both be non zero at a time
            if (input.x != 0) input.y = 0;


            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver)); // when player has moved from one tile to another check for encounters
            }
        }
        
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Interact();
        }
    }

    void Interact()
    {
        character.Animator.IsMoving = false;
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null) // if object is an npc, call the interact function of the npc controller
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        // returns first game object of which it overlapped - OverlapCircle, we need all so we use All
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffSetY), 0.2f, GameLayers.i.TriggerableLayers);
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                triggerable.onPlayerTriggered(this);
                break;
            }
        }
    }

    /* Old Code - new approach by moving code by using interfaces that is used for all triggerable objects
    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
           OnEnterTraintersView?.Invoke(collider);
        }
    }
    */

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }

    public Character Character => character;
}
