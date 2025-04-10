using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public GameObject cam;
    [SerializeField]
    private float startPosX, startPosY , length;
    public float parallaxEffectX, parallaxEffectY;

    private void Start()
    {
        startPosX = transform.position.x;
        //startPosY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }
    private void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffectX;
        float distanceY = cam.transform.position.y * -1 * parallaxEffectY;
        transform.position = new Vector3(startPosX + distance, startPosY + distanceY, transform.position.z);
        
        float movement = cam.transform.position.x * (1 - parallaxEffectX);
        if (movement > startPosX + length)
        {
            startPosX += length;
        }
        else if (movement < startPosX - length)
        {
            startPosX -= length;
        }
    }
}