using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController player;

    public Vector3 offset;

    private Vector3 currentVel;
    [SerializeField]
    private float smoothVel;

    //smoothvel;s

    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }
    private void Update()
    {
        Vector3 directionToMouse = (player.mousePos - player.transform.position);
        directionToMouse = new Vector3(directionToMouse.x, 0, directionToMouse.z).normalized;
        Vector3 dirOffset = offset + (directionToMouse * 2f);
        Vector3 target = dirOffset + player.transform.position;

        transform.position = Vector3.SmoothDamp(transform.position, target, ref currentVel, smoothVel, 20f, Time.deltaTime);

        //transform.position = dirOffset + player.transform.position;
    }
}
