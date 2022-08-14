using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    const float offsetY = 0.3f;
    public Joystick joystick;
    public float moveSpeed;

    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTraintersView;
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
        CheckForEncounters();
        CheckIfInTrainersView();
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1,101) <= 10) //1-100
            {
                character.Animator.IsMoving = false;
                OnEncountered();
            }
        }
    }

    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
           OnEnterTraintersView?.Invoke(collider);
        }
    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}
