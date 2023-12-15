using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{

    [SerializeField]
    private int powerLevel = 1;

    [SerializeField]
    private GameObject explosionCenterPrefab = null;
    [SerializeField]
    private GameObject explosionRightCornerPrefab = null;

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
        Instantiate(explosionRightCornerPrefab, transform.position + new Vector3(1,0,0), Quaternion.identity);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
