using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

public class Player : MonoBehaviour
{
    //Player attributes
    public int id;
    public Rigidbody2D rigidBody = null;
    public float moveSpeed = 3f;
    private int bombsDroppedCount = 0;
    public int maxBombs = 1;
    public int powerLevel = 1;

    //Control attributes
    public InputReader input = null;
    private float stepSize = 0.5f; //Distance between two tiles
    private Vector2 nextDirection = Vector2.zero; //Direction of next movement where x= -stepSize, 0 or stepSize and y= -stepSize, 0 or stepSize
    private Vector2 stepTarget = Vector2.zero; //Step target of current movement  
    private bool isMoving = false;
    private Vector2 currentMovementInput = Vector2.zero;

    //Environment attributes
    [SerializeField]
    public Tilemap wallsTilemap = null;
    [SerializeField]
    private Bomb bombPrefab;

    public event Action<Player> DeadEvent;

    private void Awake()
     {
        rigidBody = GetComponent<Rigidbody2D>();
        stepTarget = rigidBody.position;
    }

    public void Die()
    {
        DeadEvent?.Invoke(this);
        GetComponent<SpriteRenderer>().color = Color.black;
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
            Vector2 alternativeDirection;
            Vector2 alternativeTarget;
            if (Math.Abs(nextDirection.x) > Math.Abs(nextDirection.y))
            {
                alternativeDirection = new Vector2(0, currentMovementInput.y < 0 ? -stepSize : stepSize); 
            }
            else
            {
                alternativeDirection = new Vector2(currentMovementInput.x < 0 ? -stepSize : stepSize, 0);
            }

            alternativeTarget = rigidBody.position + alternativeDirection;

            
            //If both directions are pressed in control, and next direction is blocked,
            //but alternative direction is not, go to alternative direction
            if (Math.Abs(currentMovementInput.x) > 0 
                && Math.Abs(currentMovementInput.y) > 0 
                && IsTargetBlocked(rigidBody.position + nextDirection) 
                && !IsTargetBlocked(alternativeTarget))
            {
                stepTarget = alternativeTarget;
            }
            else
            {
                stepTarget = stepTarget + nextDirection;
            }
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
            rigidBody.velocity = GetStepDirection() * moveSpeed;
        }
    }

    /// <summary>
    /// Check if player's target is blocked by a wall, bomb or block.
    /// </summary>
    /// <param name="target">Destination to check</param>
    /// <returns></returns>
    private bool IsTargetBlocked(Vector2 target = default)
    {
        if (target == default)
        {
            target = stepTarget;
        }

        Vector2 direction = target - rigidBody.position; //Vector2(int x,int y) where x and y are [-1,0,1]1

        /*
         * CHECKING FOR WALLS WHEN PLAYER IS IN THE MIDDLE OF TWO TILES
         */
        //Get left and right shoulder targets.
        //By shoulder we mean the left and right side of player.
        //Getting both shoulders is necessary to check for collisions when player is between two tiles.
        float leftShoulderX = target.x + direction.x / 2 - direction.y / 4;
        float leftShoulderY = target.y + direction.y / 2 - direction.x / 4;
        float rightShoulderX =target.x + direction.x / 2 + direction.y / 4;
        float rightShoulderY = target.y + direction.y / 2 + direction.x / 4;
        Vector3 leftShoulderTargetV3 = new Vector2(leftShoulderX, leftShoulderY);
        Vector3 rightShoulderTargetV3 = new Vector2(rightShoulderX, rightShoulderY);

        //Get cells of left and right shoulder targets
        Vector3Int cellCollisioningLeftShoulder = wallsTilemap.WorldToCell(leftShoulderTargetV3);
        Vector3Int cellCollisioningRightShoulder = wallsTilemap.WorldToCell(rightShoulderTargetV3);

        /*
         * CHECK FOR BOMBS AND BLOCKS COLLIDERS
         */
        //Check if target is colliding with bomb or block to stop movement
        Vector2 farTarget = target + direction / 2; //Avoid being between two tiles
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

    /// <summary>
    /// Get direction of next step using stepTarget and rigidBody.position
    /// </summary>
    /// <returns>Vector2 stepDirection</returns>
    private Vector2 GetStepDirection()
    {
        Vector2 direction = stepTarget - rigidBody.position; //Vector2(int x,int y) where x and y are [-1,0,1]1
        float stepX = Mathf.Abs(direction.x) > 0 ? Mathf.Sign(direction.x) * stepSize : 0;
        float stepY = Mathf.Abs(direction.y) > 0 ? Mathf.Sign(direction.y) * stepSize : 0;
        return new Vector2(stepX, stepY);
    }

    private void Start()
    {
        input.MoveEvent += OnMovementPerformed;
        input.MoveCancelledEvent += OnMovementCancelled;
        input.BombEvent += OnBombPerformed;
    }

    /// <summary>
    /// When movement is performed, set direction and next step target
    /// <para>We avoid changing player's velocity here</para>
    /// </summary>
    /// <param name="context"></param> 
    private void OnMovementPerformed(Vector2 move)
     {
        isMoving = true;
        //If two direction buttons are pressed, ignore blocked direction
        if ( Math.Abs(move.x) > 0 && Math.Abs(move.y) > 0)
        {
            if (Math.Abs(GetStepDirection().x) > Math.Abs(GetStepDirection().y))
            {
                move = new Vector2(0, move.y);
            }
            else
            {
                move = new Vector2(move.x, 0);
            }
        }

        //Avoid diagonal movement
        if (Math.Abs(move.x) > Math.Abs(move.y))
        {
            move = new Vector2(move.x < 0 ? -stepSize : stepSize, 0);
        }
        else
        {
            move = new Vector2(0, move.y < 0 ? -stepSize : stepSize); 
        }

        nextDirection = move;
    }

    private void OnMovementCancelled()
    {
        isMoving = false;
        currentMovementInput = Vector2.zero;
    }

    private void OnBombPerformed()
    {
        Vector2 direction = stepTarget - rigidBody.position; //Vector2(int x,int y) where x and y are [-1,0,1]1

        //Check if player can drop more bombs
        if (bombsDroppedCount >= maxBombs)
        {
            return;
        }

        //Center position to cell
        Vector3Int cellPosition = wallsTilemap.WorldToCell(rigidBody.position + direction / 10); //Take into account player's direction
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
        //this.bombUnsubscriber = bomb.Subscribe(this);//To know when bomb is destroyed
        bomb.OnExplode += () => 
        { 
            bombsDroppedCount--; 
            Debug.Log("Bomb destroyed. Remaining bombs: " + bombsDroppedCount); 
        };
        bombsDroppedCount++;
        Debug.Log("Bomb added. Remaining bombs: " + bombsDroppedCount);
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
