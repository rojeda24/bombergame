using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour, IObservable<Bomb>
{
    public int powerLevel = 1;

    [SerializeField] private GameObject explosionPrefab;

    //Explosion sprites
    [SerializeField] private Sprite explosionCenterSprite;

    [SerializeField] private Sprite explosionMiddleLeftSprite;
    [SerializeField] private Sprite explosionMiddleRightSprite;
    [SerializeField] private Sprite explosionMiddleUpSprite;
    [SerializeField] private Sprite explosionMiddleDownSprite;

    [SerializeField] private Sprite explosionCornerLeftSprite;
    [SerializeField] private Sprite explosionCornerRightSprite;
    [SerializeField] private Sprite explosionCornerUpSprite;
    [SerializeField] private Sprite explosionCornerDownSprite;

    [SerializeField] private Tilemap wallsTilemap = null;

    //Constructor defining power level
    public Bomb(int powerLevel)
    {
        this.powerLevel = powerLevel;
    }

    // Start is called before the first frame update
    void Start()
    {
        wallsTilemap = GameObject.Find("WallsTilemap").GetComponent<Tilemap>();
        StartCoroutine(LightingFuse());
    }

    private IEnumerator LightingFuse()
    {
        yield return new WaitForSeconds(3f);
        Explode();
    }

    public void Explode()
    {

        bool isLeftBlocked = false;
        bool isRightBlocked = false;
        bool isUpBlocked = false;
        bool isDownBlocked = false;

        //Instantiate explosion in the center
        explosionPrefab.GetComponent<SpriteRenderer>().sprite = explosionCenterSprite;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        int powerCounter = 1;
        while (powerCounter < powerLevel)
        {
            //Instantiate middle explosions in all directions
            if (!isLeftBlocked)
                isLeftBlocked = !TryToExplodeByDistance(explosionMiddleLeftSprite, -powerCounter, 0);

            if (!isRightBlocked)
                isRightBlocked = !TryToExplodeByDistance(explosionMiddleRightSprite, powerCounter, 0);

            if (!isUpBlocked)
                isUpBlocked = !TryToExplodeByDistance(explosionMiddleUpSprite, 0, powerCounter);

            if (!isDownBlocked)
                isDownBlocked = !TryToExplodeByDistance(explosionMiddleDownSprite, 0, -powerCounter);

            powerCounter++;
        }
        //Instantiate corners
        if (!isLeftBlocked)
            TryToExplodeByDistance(explosionCornerLeftSprite, -powerCounter, 0);

        if (!isRightBlocked)
            TryToExplodeByDistance(explosionCornerRightSprite, powerCounter, 0);

        if (!isUpBlocked)
            TryToExplodeByDistance(explosionCornerUpSprite, 0, powerCounter);

        if (!isDownBlocked)
            TryToExplodeByDistance(explosionCornerDownSprite, 0, -powerCounter);

        Destroy(gameObject);
    }

    /// <summary>
    /// Checks position away of bomb and instantiate explosion if there is no wall or block.
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="xAway"></param>
    /// <param name="yAway"></param>
    /// <returns>true if explosion should continue</returns>
    private bool TryToExplodeByDistance(Sprite sprite, int xAway, int yAway)
    {
        //Check if there is a wall
        Vector3 newPosition = transform.position + new Vector3(xAway, yAway,0);
        Vector3Int cellToCheck = wallsTilemap.WorldToCell(newPosition);
        if (wallsTilemap.HasTile(cellToCheck))
        {
            return false;
        }

        //Instantiate explosion
        explosionPrefab.GetComponent<SpriteRenderer>().sprite = sprite;
        Instantiate(explosionPrefab, newPosition, Quaternion.identity);

        //Check if there is a block
        Collider2D[] colliders = Physics2D.OverlapPointAll(newPosition);
        foreach(Collider2D collider in colliders)
        {
            if (collider.CompareTag("Block"))
            {
                Destroy(collider.gameObject, 1f);
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

    //IObservable implementation
    private IObserver<Bomb> observer;

    private void OnDestroy()
    {
        observer?.OnNext(this);
        observer?.OnCompleted();
    }

    //IObservable implementation
    public IDisposable Subscribe(IObserver<Bomb> observer)
    {
        this.observer = observer;
        return new Unsubscriber<Bomb>(observer);
    }

    //IObservable implementation
    private class Unsubscriber<Bomb> : IDisposable
    {
        private IObserver<Bomb> _observer;

        public Unsubscriber(IObserver<Bomb> observer)
        {
            this._observer = observer;
        }

        public void Dispose()
        {
            _observer = null;
        }
    }
}
