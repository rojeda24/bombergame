using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour
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
        //Instantiate explosion just before object is destroyed
        Instantiate(explosionCenterPrefab, transform.position, Quaternion.identity);

        bool isLeftBlocked = false;
        bool isRightBlocked = false;
        bool isUpBlocked = false;
        bool isDownBlocked = false;

        Vector3 newPosition;//This will change for each direction we check

        Vector3Int cellToCheck;//This will change for each direction and newPosition value

        int powerCounter = 1;
        while ( powerCounter < powerLevel )
        {
            //Instantiate explosion in all directions
            //Left
            newPosition = transform.position + new Vector3(-powerCounter, 0, 0);
            cellToCheck = wallsTilemap.WorldToCell(newPosition);
            if (isLeftBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isLeftBlocked = true;
            } 
            else
            {
                Instantiate(explosionLeftMiddlePrefab, newPosition, Quaternion.identity);
            }

            //Right
            newPosition = transform.position + new Vector3(powerCounter, 0, 0);
            cellToCheck = wallsTilemap.WorldToCell(newPosition);
            if (isRightBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isRightBlocked = true;
            }
            else
            {
                Instantiate(explosionLeftMiddlePrefab, newPosition, Quaternion.identity);
            }

            //Up
            newPosition = transform.position + new Vector3(0, powerCounter, 0);
            cellToCheck = wallsTilemap.WorldToCell(newPosition);
            if (isUpBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isUpBlocked = true;
            }
            else
            {
                Instantiate(explosionUpMiddlePrefab, newPosition, Quaternion.identity);
            }

            //Down
            newPosition = transform.position + new Vector3(0, -powerCounter, 0);
            cellToCheck = wallsTilemap.WorldToCell(newPosition);
            if (isDownBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isDownBlocked = true;
            }
            else
            {
                Instantiate(explosionDownMiddlePrefab, newPosition, Quaternion.identity);
            }

            powerCounter++;
        }

        //Left
        newPosition = transform.position + new Vector3(-powerCounter, 0, 0);
        cellToCheck = wallsTilemap.WorldToCell(newPosition);
        if (!isLeftBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionLeftCornerPrefab, newPosition, Quaternion.identity);

        //Right
        newPosition = transform.position + new Vector3(powerCounter, 0, 0);
        cellToCheck = wallsTilemap.WorldToCell(newPosition);
        if (!isRightBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionRightCornerPrefab, newPosition, Quaternion.identity);

        //Up
        newPosition = transform.position + new Vector3(0, powerCounter, 0);
        cellToCheck = wallsTilemap.WorldToCell(newPosition);
        if (!isUpBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionUpCornerPrefab, newPosition, Quaternion.identity);

        //Down
        newPosition = transform.position + new Vector3(0, -powerCounter, 0);
        cellToCheck = wallsTilemap.WorldToCell(newPosition);
        if (!isDownBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionDownCornerPrefab, newPosition, Quaternion.identity);

        Destroy(gameObject);
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
