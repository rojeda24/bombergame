using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour, IObservable<Bomb>
{

    [SerializeField]
    private int powerLevel = 1;

    [SerializeField] private GameObject explosionCenterPrefab;

    [SerializeField] private GameObject explosionLeftMiddlePrefab;
    [SerializeField] private GameObject explosionRightMiddlePrefab;
    [SerializeField] private GameObject explosionUpMiddlePrefab;
    [SerializeField] private GameObject explosionDownMiddlePrefab;

    [SerializeField] private GameObject explosionLeftCornerPrefab;
    [SerializeField] private GameObject explosionRightCornerPrefab;
    [SerializeField] private GameObject explosionUpCornerPrefab;
    [SerializeField] private GameObject explosionDownCornerPrefab;

    [SerializeField] private Tilemap wallsTilemap = null;

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
        Instantiate(explosionCenterPrefab, transform.position, Quaternion.identity);

        int powerCounter = 1;
        while (powerCounter < powerLevel)
        {
            //Instantiate middle explosions in all directions
            if (!isLeftBlocked)
                isLeftBlocked = !TryToExplodeByDistance(explosionLeftMiddlePrefab, new Vector2(-powerCounter, 0));

            if (!isRightBlocked)
                isRightBlocked = !TryToExplodeByDistance(explosionRightMiddlePrefab, new Vector2(powerCounter, 0));

            if (!isUpBlocked)
                isUpBlocked = !TryToExplodeByDistance(explosionUpMiddlePrefab, new Vector2(0, powerCounter));

            if (!isDownBlocked)
                isDownBlocked = !TryToExplodeByDistance(explosionDownMiddlePrefab, new Vector2(0, -powerCounter));

            powerCounter++;
        }
        //Instantiate corners
        if (!isLeftBlocked)
            TryToExplodeByDistance(explosionLeftCornerPrefab, new Vector2(-powerCounter, 0));

        if (!isRightBlocked)
            TryToExplodeByDistance(explosionRightCornerPrefab, new Vector2(powerCounter, 0));

        if (!isUpBlocked)
            TryToExplodeByDistance(explosionUpCornerPrefab, new Vector2(0, powerCounter));

        if (!isDownBlocked)
            TryToExplodeByDistance(explosionDownCornerPrefab, new Vector2(0, -powerCounter));

        Destroy(gameObject);
    }

    /// <summary>
    /// Checks position away of bomb and instantiate explosion if there is no wall or block.
    /// </summary>
    /// <param name="prefabExplosion"></param>
    /// <param name="positionAwayOfBomb"></param>
    /// <returns>true if explosion should continue</returns>
    private bool TryToExplodeByDistance(GameObject prefabExplosion, Vector3 positionAwayOfBomb)
    {
        Vector3 newPosition = transform.position + positionAwayOfBomb;

        //Check if there is a wall
        Vector3Int cellToCheck = wallsTilemap.WorldToCell(newPosition);
        if (wallsTilemap.HasTile(cellToCheck))
        {
            return false;
        }

        //Instantiate explosion
        Instantiate(prefabExplosion, newPosition, Quaternion.identity);

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
