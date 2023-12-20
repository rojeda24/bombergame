using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private GameObject bombPowerUpPrefab = null;
    private void OnDestroy()
    {
        Instantiate(bombPowerUpPrefab, transform.position, Quaternion.identity);
    }
}
