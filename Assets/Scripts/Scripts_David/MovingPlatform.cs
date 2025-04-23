using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingRight = true;

    void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + Vector3.right * moveDistance;
    }

    void Update()
    {
        if (movingRight)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, moveSpeed * Time.deltaTime);
            if (transform.position == endPosition)
            {
                movingRight = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            if (transform.position == startPosition)
            {
                movingRight = true;
            }
        }
    }
}