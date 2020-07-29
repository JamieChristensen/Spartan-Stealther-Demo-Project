using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private GameObject particleExplosionPrefab;




    private void OnCollisionEnter(Collision other)
    {
        Instantiate(particleExplosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        
    }
}
