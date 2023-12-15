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
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(3f);
        //Instantiate explosion just before object is destroyed
        Instantiate(explosionCenterPrefab, transform.position, Quaternion.identity);
        int powerCounter = 1;
        bool isLeftBlocked = false;
        bool isRightBlocked = false;
        bool isUpBlocked = false;
        bool isDownBlocked = false;

        Vector3 leftNewPosition;
        Vector3 rightNewPosition;
        Vector3 upNewPosition;
        Vector3 downNewPosition;

        Vector3Int cellToCheck;

        while ( powerCounter < powerLevel )
        {
            //Instantiate explosion in all directions
            //Left
            leftNewPosition = transform.position + new Vector3(-powerCounter, 0, 0);
            cellToCheck = wallsTilemap.WorldToCell(leftNewPosition);
            if (isLeftBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isLeftBlocked = true;
            } 
            else
            {
                Instantiate(explosionLeftMiddlePrefab, leftNewPosition, Quaternion.identity);
            }

            //Right
            rightNewPosition = transform.position + new Vector3(powerCounter, 0, 0);
            cellToCheck = wallsTilemap.WorldToCell(rightNewPosition);
            if (isRightBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isRightBlocked = true;
            }
            else
            {
                Instantiate(explosionLeftMiddlePrefab, rightNewPosition, Quaternion.identity);
            }

            //Up
            upNewPosition = transform.position + new Vector3(0, powerCounter, 0);
            cellToCheck = wallsTilemap.WorldToCell(upNewPosition);
            if (isUpBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isUpBlocked = true;
            }
            else
            {
                Instantiate(explosionUpMiddlePrefab, upNewPosition, Quaternion.identity);
            }

            //Down
            downNewPosition = transform.position + new Vector3(0, -powerCounter, 0);
            cellToCheck = wallsTilemap.WorldToCell(downNewPosition);
            if (isDownBlocked || wallsTilemap.HasTile(cellToCheck))
            {
                isDownBlocked = true;
            }
            else
            {
                Instantiate(explosionDownMiddlePrefab, downNewPosition, Quaternion.identity);
            }

            powerCounter++;
        }

        leftNewPosition = transform.position + new Vector3(-powerCounter, 0, 0);
        cellToCheck = wallsTilemap.WorldToCell(leftNewPosition);
        if (!isLeftBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionLeftCornerPrefab, leftNewPosition, Quaternion.identity);

        rightNewPosition = transform.position + new Vector3(powerCounter, 0, 0);
        cellToCheck = wallsTilemap.WorldToCell(rightNewPosition);
        if (!isRightBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionRightCornerPrefab, rightNewPosition, Quaternion.identity);

        upNewPosition = transform.position + new Vector3(0, powerCounter, 0);
        cellToCheck = wallsTilemap.WorldToCell(upNewPosition);
        if (!isUpBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionUpCornerPrefab, upNewPosition, Quaternion.identity);

        downNewPosition = transform.position + new Vector3(0, -powerCounter, 0);
        cellToCheck = wallsTilemap.WorldToCell(downNewPosition);
        if (!isDownBlocked && !wallsTilemap.HasTile(cellToCheck))
            Instantiate(explosionDownCornerPrefab, downNewPosition, Quaternion.identity);

        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
