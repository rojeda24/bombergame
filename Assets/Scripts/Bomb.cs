using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour
{
    //Define get;set; for PowerLevel
    public int PowerLevel { get; set; }

    [SerializeField] private GameObject _explosionPrefab;

    //Explosion sprites
    [SerializeField] private Sprite _explosionCenterSprite;

    [SerializeField] private Sprite _explosionMiddleLeftSprite;
    [SerializeField] private Sprite _explosionMiddleRightSprite;
    [SerializeField] private Sprite _explosionMiddleUpSprite;
    [SerializeField] private Sprite _explosionMiddleDownSprite;

    [SerializeField] private Sprite _explosionCornerLeftSprite;
    [SerializeField] private Sprite _explosionCornerRightSprite;
    [SerializeField] private Sprite _explosionCornerUpSprite;
    [SerializeField] private Sprite _explosionCornerDownSprite;

    private Tilemap _wallsTilemap = null;

    public event Action ExplodeEvent;//Event to notify when bomb explodes

    // Start is called before the first frame update
    void Start()
    {
        _wallsTilemap = GameObject.Find("WallsTilemap").GetComponent<Tilemap>();
        StartCoroutine(LightingFuse());
    }

    private IEnumerator LightingFuse()
    {
        yield return new WaitForSeconds(3f);
        Explode();
    }

    private void Explode()
    {

        bool isLeftBlocked = false;
        bool isRightBlocked = false;
        bool isUpBlocked = false;
        bool isDownBlocked = false;

        //Instantiate explosion in the center
        _explosionPrefab.GetComponent<SpriteRenderer>().sprite = _explosionCenterSprite;
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        int powerCounter = 1;
        while (powerCounter < PowerLevel)
        {
            //Instantiate middle explosions in all directions
            if (!isLeftBlocked)
                isLeftBlocked = !TryToExplodeByDistance(_explosionMiddleLeftSprite, -powerCounter, 0);

            if (!isRightBlocked)
                isRightBlocked = !TryToExplodeByDistance(_explosionMiddleRightSprite, powerCounter, 0);

            if (!isUpBlocked)
                isUpBlocked = !TryToExplodeByDistance(_explosionMiddleUpSprite, 0, powerCounter);

            if (!isDownBlocked)
                isDownBlocked = !TryToExplodeByDistance(_explosionMiddleDownSprite, 0, -powerCounter);

            powerCounter++;
        }
        //Instantiate corners
        if (!isLeftBlocked)
            TryToExplodeByDistance(_explosionCornerLeftSprite, -powerCounter, 0);

        if (!isRightBlocked)
            TryToExplodeByDistance(_explosionCornerRightSprite, powerCounter, 0);

        if (!isUpBlocked)
            TryToExplodeByDistance(_explosionCornerUpSprite, 0, powerCounter);

        if (!isDownBlocked)
            TryToExplodeByDistance(_explosionCornerDownSprite, 0, -powerCounter);

        ExplodeEvent?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks position away of bomb and instantiate explosion if there is no wall or block.
    /// </summary>
    /// <param name="sprite">sprite to represent explosion in tile</param>
    /// <param name="xAway">x tiles away from bomb</param>
    /// <param name="yAway">y tiles away from bomb</param>
    /// <returns>true if explosion should continue</returns>
    private bool TryToExplodeByDistance(Sprite sprite, int xAway, int yAway)
    {
        //Check if there is a wall
        Vector3 newPosition = transform.position + new Vector3(xAway, yAway,0);
        Vector3Int cellToCheck = _wallsTilemap.WorldToCell(newPosition);
        if (_wallsTilemap.HasTile(cellToCheck))
        {
            return false;
        }

        //Instantiate explosion
        _explosionPrefab.GetComponent<SpriteRenderer>().sprite = sprite;
        Instantiate(_explosionPrefab, newPosition, Quaternion.identity);

        //Check if there is a block
        Collider2D[] colliders = Physics2D.OverlapPointAll(newPosition);
        foreach(Collider2D collider in colliders)
        {
            if (collider.CompareTag("Block"))
            {
                collider.GetComponent<Block>().Explode();
                return false;
            }
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check if collision with explosion
        if (collision.CompareTag("Explosion"))
        {
            Explode();
        }
    }
}
