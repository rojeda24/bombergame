using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour, IObserver<Bomb>
{
    //Player attributes
    private Rigidbody2D rigidBody = null;
    public float moveSpeed = 3f;
    private int bombsDroppedCount = 0;
    public int maxBombs = 1;
    public int powerLevel = 1;

    //Control attributes
    private PlayerInput input = null;
    private Vector2 currentDirection = Vector2.zero; //Direction of current movement
    private Vector2 nextDirection = Vector2.zero; //Direction of next movement
    private Vector2 stepTarget = Vector2.zero; //Step target of current movement  
    private bool isMoving = false;

    //Environment attributes
    [SerializeField]
    private Tilemap wallsTilemap = null;
    [SerializeField]
    private Bomb bombPrefab;


    private void Awake()
     {
        input = new PlayerInput();
        rigidBody = GetComponent<Rigidbody2D>();

        //Center position to cell
        Vector3Int cellPosition = wallsTilemap.WorldToCell(rigidBody.position);
        Vector3 cellCenterPosition = wallsTilemap.GetCellCenterWorld(cellPosition);
        rigidBody.position = cellCenterPosition;//TODO: change to spawn point
        stepTarget = rigidBody.position;
    }

    /// <summary>
    /// Player's Rigidbody2D properties are updated here.
    /// </summary>
    private void FixedUpdate()
    {
        /*
         * SNAPING FINAL MOVEMENT TO STEP TARGET
         */
        //If almost at step target, snap to step target
        if (Vector2.Distance(rigidBody.position, stepTarget) < 0.1f)
        {
            rigidBody.position = stepTarget;
        }

        /*
         * CONTINUOUS MOVEMENT
         */
        //If at step target, but still moving, update step target
        if (rigidBody.position == stepTarget && isMoving)
        {
            stepTarget = stepTarget + nextDirection;
            currentDirection = nextDirection;
        }

        /*
         * TAKING MOVEMENT DECISIONS
         */
        if (rigidBody.position == stepTarget)
        {
            rigidBody.velocity = Vector2.zero;
        } 
        else if (IsTargetBlocked())
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
    /// Check if player's target is blocked by a wall, bomb or block.
    /// </summary>
    /// <returns></returns>
    private bool IsTargetBlocked()
    {
        /*
         * CHECKING FOR WALLS WHEN PLAYER IS IN THE MIDDLE OF TWO TILES
         */
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

        /*
         * CHECK FOR BOMBS AND BLOCKS COLLIDERS
         */
        //Check if target is colliding with bomb or block to stop movement
        Vector2 farTarget = stepTarget + currentDirection / 2; //Avoid being between two tiles
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(farTarget);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Bomb") || hitCollider.CompareTag("Block"))
            {
                //avoid locked by bombs in the same cell as player
                Vector3Int playerCellPosition = wallsTilemap.WorldToCell(rigidBody.position);
                Vector3Int objectCollidingCellPosition = wallsTilemap.WorldToCell(hitCollider.gameObject.transform.position);
                if (playerCellPosition == objectCollidingCellPosition)
                {
                    continue;
                }
                return true;
            }
        }

        //Check for walls tilemaps
        if (wallsTilemap.HasTile(cellCollisioningLeftShoulder) || wallsTilemap.HasTile(cellCollisioningRightShoulder))
            return true;

        return false;
    }


    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCancelled;
        input.Player.Bomb.performed += OnBombPerformed;
    }
    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCancelled;
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

    private void OnBombPerformed(InputAction.CallbackContext context)
    {
        //Check if player can drop more bombs
        if (bombsDroppedCount >= maxBombs)
        {
            return;
        }

        //Center position to cell
        Vector3Int cellPosition = wallsTilemap.WorldToCell(rigidBody.position + currentDirection/10); //Take into account player's direction
        Vector3 cellCenterPosition = wallsTilemap.GetCellCenterWorld(cellPosition);
        //Check if there is a bomb already in cellCenterPosition
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(cellCenterPosition);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Bomb"))
            {
                return;
            }
        }

        Bomb bomb = Instantiate(bombPrefab, cellCenterPosition, Quaternion.identity);
        bomb.powerLevel = powerLevel;
        this.bombUnsubscriber = bomb.Subscribe(this);//To know when bomb is destroyed
        bombsDroppedCount++;
        Debug.Log("Bomb added. Remaining bombs: " + bombsDroppedCount);
    }

    //IObserver implementation
    private IDisposable bombUnsubscriber;

    //IObserver implementation
    public void OnNext(Bomb value)
    {
        bombsDroppedCount--;
        Debug.Log("Bomb destroyed. Remaining bombs: " + bombsDroppedCount);
    }

    //IObserver implementation
    public void OnError(Exception error)
    {
        Debug.Log("Error: " + error.Message);
    }

    //IObserver implementation
    public void OnCompleted()
    {
        //Unsubscribe from bomb
        bombUnsubscriber.Dispose();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if collision with power up
        if (collision.CompareTag("PowerUp"))
        {
            IPowerUp powerUp = collision.gameObject.GetComponent<IPowerUp>();
            powerUp.ApplyPowerUp(this);
            Destroy(collision.gameObject);
        }
    }
}
