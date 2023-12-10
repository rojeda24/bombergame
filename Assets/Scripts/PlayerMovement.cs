using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Player attributes
    private Rigidbody2D rigidBody = null;
    [SerializeField]
    private float moveSpeed = 3f;

    //Control attributes
    private PlayerInput input = null;
    private Vector2 currentVector = Vector2.zero;
    private Vector2 nextVector = Vector2.zero;
    private Vector2 currentTarget = Vector2.zero;
    private Vector2 nextTarget = Vector2.zero;
    private bool isMoving = false;

    //Environment attributes
    [SerializeField]
    private Grid grid = null;


    private void Awake()
    {
        input = new PlayerInput();
        rigidBody = GetComponent<Rigidbody2D>();

        rigidBody.position = grid.GetCellCenterWorld(Vector3Int.zero);//TODO: change to spawn point
        currentTarget = rigidBody.position;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCancelled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCancelled;
    }

    private void FixedUpdate()
    {
        //If almost at target, snap to target
        if (Vector2.Distance(rigidBody.position, currentTarget) < 0.1f)
        {
            rigidBody.position = currentTarget;
        }

        //If at target, but still moving, change target and update current vector
        if (rigidBody.position == currentTarget && isMoving)
        {
            currentTarget = currentTarget + nextVector;
            currentVector = nextVector;
            Debug.Log("Changing target... to " + nextTarget); //TODO CHECK WHY NOT WORKING IF NOT CHANGING DIRECTION
        }

        //If not at target, move towards target
        if (rigidBody.position != currentTarget)
        {
            rigidBody.velocity = currentVector * moveSpeed;
        } 
        //If at target, stop moving
        else
        {
            rigidBody.velocity = Vector2.zero;
        }           
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        isMoving = true;

        nextVector = context.ReadValue<Vector2>();
        //If both directions are pressed, ignore previous direction
        if ( Math.Abs(nextVector.x) > 0 && Math.Abs(nextVector.y) > 0)
        {
            if (Math.Abs(currentVector.x) > Math.Abs(currentVector.y))
            {
                nextVector = new Vector2(0, nextVector.y);
            }
            else
            {
                nextVector = new Vector2(nextVector.x, 0);
            }
        }


        //Avoid diagonal movement
        if (Math.Abs(nextVector.x) > Math.Abs(nextVector.y))
        {
            nextVector = new Vector2( nextVector.x < 0 ? -1 : 1 , 0); //x= 1 or -1
        }
        else
        {
            nextVector = new Vector2(0, nextVector.y < 0 ? -1 : 1); //y= 1 or -1
        }

        //Define next intented target
        nextTarget = currentTarget + nextVector;
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        isMoving = false;
    }
}
