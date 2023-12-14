using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    //Player attributes
    private Rigidbody2D rigidBody = null;
    [SerializeField]
    private float moveSpeed = 3f;

    //Control attributes
    private PlayerInput input = null;
    private Vector2 currentDirection = Vector2.zero; //Direction of current movement
    private Vector2 nextDirection = Vector2.zero; //Direction of next movement
    private Vector2 stepTarget = Vector2.zero; //Step target of current movement  
    private bool isMoving = false;

    //Environment attributes
    [SerializeField]
    private Tilemap wallsTilemap = null;


    private void Awake()
     {
        input = new PlayerInput();
        rigidBody = GetComponent<Rigidbody2D>();
         
        rigidBody.position = new Vector2(0.5f,5.5f);//TODO: change to spawn point
        stepTarget = rigidBody.position;
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

    /// <summary>
    /// Player's Rigidbody2D properties are updated here.
    /// </summary>
    private void FixedUpdate()
    {
        //If almost at step target, snap to step target
        if (Vector2.Distance(rigidBody.position, stepTarget) < 0.1f)
        {
            rigidBody.position = stepTarget;
        }

        //If at step target, but still moving, update step target
        if (rigidBody.position == stepTarget && isMoving)
        {
            stepTarget = stepTarget + nextDirection;
            currentDirection = nextDirection;
        }

        //Get left and right shoulder targets.
        //By shoulder we mean the left and right side of player.
        //Getting both shoulders is necessary to check for collisions when player is between two tiles.
        float leftShoulderX = stepTarget.x + currentDirection.x / 2 - currentDirection.y / 4;
        float leftShoulderY = stepTarget.y + currentDirection.y / 2 - currentDirection.x / 4;
        float rightShoulderX = stepTarget.x + currentDirection.x / 2 + currentDirection.y / 4;
        float rightShoulderY = stepTarget.y + currentDirection.y / 2 + currentDirection.x / 4;
        Vector3 leftShoulderTargetV3 = new Vector2(leftShoulderX, leftShoulderY);
        Vector3 rightShoulderTargetV3 = new Vector2(rightShoulderX, rightShoulderY);

        //Get cells of left and right shoulder targets
        Vector3Int cellCollisioningLeftShoulder = wallsTilemap.WorldToCell(leftShoulderTargetV3);
        Vector3Int cellCollisioningRightShoulder = wallsTilemap.WorldToCell(rightShoulderTargetV3);

        if (rigidBody.position == stepTarget)
        {
            rigidBody.velocity = Vector2.zero;
        } 
        else if (wallsTilemap.HasTile(cellCollisioningLeftShoulder) || wallsTilemap.HasTile(cellCollisioningRightShoulder))
        {
            rigidBody.velocity = Vector2.zero;
            stepTarget = rigidBody.position;
        }
        //Move to step target
        else
        {
            rigidBody.velocity = currentDirection * moveSpeed;
        }
    }

    /// <summary>
    /// When movement is performed, set direction and next step target
    /// <para>We avoid changing player's velocity here</para>
    /// </summary>
    /// <param name="context"></param> 
    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        isMoving = true;
        nextDirection = context.ReadValue<Vector2>();
        //If two direction buttons are pressed, ignore previous direction
        if ( Math.Abs(nextDirection.x) > 0 && Math.Abs(nextDirection.y) > 0)
        {
            if (Math.Abs(currentDirection.x) > Math.Abs(currentDirection.y))
            {
                nextDirection = new Vector2(0, nextDirection.y);
            }
            else
            {
                nextDirection = new Vector2(nextDirection.x, 0);
            }
        }

        //Avoid diagonal movement
        if (Math.Abs(nextDirection.x) > Math.Abs(nextDirection.y))
        {
            nextDirection = new Vector2(nextDirection.x < 0 ? -0.5f : 0.5f, 0); //x= -0.5 or 0.5
        }
        else
        {
            nextDirection = new Vector2(0, nextDirection.y < 0 ? -0.5f : 0.5f); //y= -0.5 or 0.5
        }
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        isMoving = false;
    }
}
