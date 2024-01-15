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
    private Rigidbody2D _rigidBody = null;
    private int _bombsDroppedCount = 0;
    public int Id { get; set; } = 1;
    public float MoveSpeed { get; set; } = 3f;
    public int MaxBombs { get; set; } = 1;
    public int PowerLevel { get; set; } = 1;

    //Control attributes
    private float _stepSize = 0.5f; //Normalized distance to move in one step
    private Vector2 _nextDirection = Vector2.zero; //Direction of next movement where x= -_stepSize, 0 or _stepSize and y= -_stepSize, 0 or _stepSize
    private Vector2 _stepTarget = Vector2.zero; //Step target of current movement  
    private bool _isMoving = false;
    private Vector2 _currentMovementInput = Vector2.zero;
    public InputReader InputReader { get; set; } = null; //Game Manager will set this

    //Environment attributes
    [SerializeField]
    private Bomb _bombPrefab;
    public Tilemap WallsTilemap { get; set; } = null; //Game Manager will set this

    public event Action<Player> DeadEvent;//Event to notify when player dies

    private void Awake()
     {
        _rigidBody = GetComponent<Rigidbody2D>();
        _stepTarget = _rigidBody.position;
    }

    private void Start()
    {
        InputReader.MoveEvent += OnMovementPerformed;
        InputReader.MoveCancelledEvent += OnMovementCancelled;
        InputReader.BombEvent += OnBombPerformed;
    }

    private void OnDisable()
    {
        InputReader.MoveEvent -= OnMovementPerformed;
        InputReader.MoveCancelledEvent -= OnMovementCancelled;
        InputReader.BombEvent -= OnBombPerformed;
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
        if (Vector2.Distance(_rigidBody.position, _stepTarget) < 0.1f)
        {
            _rigidBody.position = _stepTarget;
        }

        /*
         * CONTINUOUS MOVEMENT
         */
        //If at step target, but still moving, update step target
        if (_rigidBody.position == _stepTarget && _isMoving)
        {
            Vector2 alternativeDirection;
            Vector2 alternativeTarget;
            if (Math.Abs(_nextDirection.x) > Math.Abs(_nextDirection.y))
            {
                alternativeDirection = new Vector2(0, _currentMovementInput.y < 0 ? -_stepSize : _stepSize); 
            }
            else
            {
                alternativeDirection = new Vector2(_currentMovementInput.x < 0 ? -_stepSize : _stepSize, 0);
            }

            alternativeTarget = _rigidBody.position + alternativeDirection;

            
            //If both directions are pressed in control, and next direction is blocked,
            //but alternative direction is not, go to alternative direction
            if (Math.Abs(_currentMovementInput.x) > 0 
                && Math.Abs(_currentMovementInput.y) > 0 
                && IsTargetBlocked(_rigidBody.position + _nextDirection) 
                && !IsTargetBlocked(alternativeTarget))
            {
                _stepTarget = alternativeTarget;
            }
            else
            {
                _stepTarget = _stepTarget + _nextDirection;
            }
        }

        /*
         * TAKING MOVEMENT DECISIONS
         */
        if (_rigidBody.position == _stepTarget)
        {
            _rigidBody.velocity = Vector2.zero;
        } 
        else if (IsTargetBlocked())
        {
            _rigidBody.velocity = Vector2.zero;
            _stepTarget = _rigidBody.position;
        }
        //Move to step target
        else
        {
            _rigidBody.velocity = GetStepDirection() * MoveSpeed;
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
            target = _stepTarget;
        }

        Vector2 direction = target - _rigidBody.position; //Vector2(int x,int y) where x and y are [-1,0,1]1

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
        Vector3Int cellCollisioningLeftShoulder = WallsTilemap.WorldToCell(leftShoulderTargetV3);
        Vector3Int cellCollisioningRightShoulder = WallsTilemap.WorldToCell(rightShoulderTargetV3);

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
                Vector3Int playerCellPosition = WallsTilemap.WorldToCell(_rigidBody.position);
                Vector3Int objectCollidingCellPosition = WallsTilemap.WorldToCell(hitCollider.gameObject.transform.position);
                if (playerCellPosition == objectCollidingCellPosition)
                {
                    continue;
                }
                return true;
            }
        }

        //Check for walls tilemaps
        if (WallsTilemap.HasTile(cellCollisioningLeftShoulder) || WallsTilemap.HasTile(cellCollisioningRightShoulder))
            return true;

        return false;
    }

    /// <summary>
    /// Get direction of next step using _stepTarget and _rigidBody.position
    /// </summary>
    /// <returns>Vector2 stepDirection</returns>
    private Vector2 GetStepDirection()
    {
        Vector2 direction = _stepTarget - _rigidBody.position; //Vector2(int x,int y) where x and y are [-1,0,1]1
        float stepX = Mathf.Abs(direction.x) > 0 ? Mathf.Sign(direction.x) * _stepSize : 0;
        float stepY = Mathf.Abs(direction.y) > 0 ? Mathf.Sign(direction.y) * _stepSize : 0;
        return new Vector2(stepX, stepY);
    }

    /// <summary>
    /// When movement is performed, set direction and next step target
    /// <para>We avoid changing player's velocity here</para>
    /// </summary>
    /// <param name="context"></param> 
    private void OnMovementPerformed(Vector2 move)
     {
        _isMoving = true;
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
            move = new Vector2(move.x < 0 ? -_stepSize : _stepSize, 0);
        }
        else
        {
            move = new Vector2(0, move.y < 0 ? -_stepSize : _stepSize); 
        }

        _nextDirection = move;
    }

    private void OnMovementCancelled()
    {
        _isMoving = false;
        _currentMovementInput = Vector2.zero;
    }

    private void OnBombPerformed()
    {
        Vector2 direction = _stepTarget - _rigidBody.position; //Vector2(int x,int y) where x and y are [-1,0,1]1

        //Check if player can drop more bombs
        if (_bombsDroppedCount >= MaxBombs)
        {
            return;
        }

        //Center position to cell
        Vector3Int cellPosition = WallsTilemap.WorldToCell(_rigidBody.position + direction / 10); //Take into account player's direction
        Vector3 cellCenterPosition = WallsTilemap.GetCellCenterWorld(cellPosition);
        //Check if there is a bomb already in cellCenterPosition
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(cellCenterPosition);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Bomb"))
            {
                return;
            }
        }

        Bomb bomb = Instantiate(_bombPrefab, cellCenterPosition, Quaternion.identity);
        bomb.PowerLevel = PowerLevel;
        bomb.ExplodeEvent += () =>//Subscribe to know when bomb is destroyed
        { 
            _bombsDroppedCount--; 
            Debug.Log("Bomb destroyed. Remaining bombs: " + _bombsDroppedCount); 
        };
        _bombsDroppedCount++;
        Debug.Log("Bomb added. Remaining bombs: " + _bombsDroppedCount);
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
