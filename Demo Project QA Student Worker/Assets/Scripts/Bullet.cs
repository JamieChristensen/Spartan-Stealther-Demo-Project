using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private GameObject particleExplosionPrefab;




    private void OnCollisionEnter(Collision other)
    {
        Transform particleTransform = Instantiate(particleExplosionPrefab, transform.position, Quaternion.identity).GetComponent<Transform>();
        particleTransform.LookAt(other.transform);
        Destroy(gameObject);

    }
}
