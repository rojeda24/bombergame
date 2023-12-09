using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput input = null;
    private Vector2 currentVector = Vector2.zero;
    private Vector2 nextVector = Vector2.zero;
    private Rigidbody2D rigidBody = null;
    public float moveSpeed = 5f;

    public Grid grid = null;
    private Vector2 currentTarget = Vector2.zero;
    private Vector2 nextTarget = Vector2.zero;

    private bool isMoving = false;

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

        //If at target, but still moving, change target
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

        //Avoid diagonal movement
        if(Math.Abs(nextVector.x) > Math.Abs(nextVector.y))
        {
            nextVector = new Vector2((float)Math.Ceiling(nextVector.x), 0);
        }
        else
        {
            nextVector = new Vector2(0, (float)Math.Ceiling(nextVector.y));
        }

        nextTarget = currentTarget + nextVector;


        if (rigidBody.position == currentTarget)
        {
            currentVector = nextVector;

            currentTarget = nextTarget;
            Debug.Log("Move Target: " + currentTarget);
        }

    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        isMoving = false;
    }
}
