using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(3f);
        //Instantiate explosion just before object is destroyed
        Instantiate(explosionCenterPrefab, transform.position, Quaternion.identity);
        int powerCounter = 1;
        while ( powerCounter < powerLevel )
        {
            //Instantiate explosion in all directions
            Instantiate(explosionLeftMiddlePrefab, transform.position + new Vector3(-powerCounter,0,0), Quaternion.identity);
            Instantiate(explosionRightMiddlePrefab, transform.position + new Vector3(powerCounter,0,0), Quaternion.identity);
            Instantiate(explosionUpMiddlePrefab, transform.position + new Vector3(0,powerCounter,0), Quaternion.identity);
            Instantiate(explosionDownMiddlePrefab, transform.position + new Vector3(0,-powerCounter,0), Quaternion.identity);
            powerCounter++;
        }

        Instantiate(explosionLeftCornerPrefab, transform.position + new Vector3(-powerCounter, 0, 0), Quaternion.identity);
        Instantiate(explosionRightCornerPrefab, transform.position + new Vector3(powerCounter, 0, 0), Quaternion.identity);
        Instantiate(explosionUpCornerPrefab, transform.position + new Vector3(0, powerCounter, 0), Quaternion.identity);
        Instantiate(explosionDownCornerPrefab, transform.position + new Vector3(0, -powerCounter, 0), Quaternion.identity);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
