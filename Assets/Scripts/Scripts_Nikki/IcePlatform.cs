using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePlatform : MonoBehaviour
{
    // טאג של השחקן
    public string playerTag = "Player";
    // מהירות ההאצה
    public float acceleration = 2f;
    // כוח חיכוך (כדי להאט את השחקן)
    public float friction = 0.1f;
    // מהירות מינימלית
    public float minSpeed = 0.1f;
    // רגישות כיוון
    public float directionThreshold = 0.1f;

    private Rigidbody2D playerRb;
    private bool playerOnPlatform = false;
    private Vector2 moveDirection;
    private Vector2 lastMoveDirection;

    void Update()
    {
        if (playerOnPlatform && playerRb != null)
        {
            // שמירת כיוון התנועה
            moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);

            // האצת השחקן בכיוון התנועה
            playerRb.AddForce(moveDirection * acceleration);

            // שמירת כיוון התנועה האחרון
            if (moveDirection.magnitude > directionThreshold)
            {
                lastMoveDirection = moveDirection;
            }

            // הוספת חיכוך (כדי להאט את השחקן)
            if (Mathf.Abs(playerRb.velocity.x) > minSpeed && moveDirection.x == 0)
            {
                playerRb.AddForce(new Vector2(-playerRb.velocity.x * friction, 0f));
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            playerOnPlatform = true;
            playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            playerOnPlatform = false;
            playerRb = null;
        }
    }

    void FixedUpdate()
    {
        if (playerOnPlatform && playerRb != null && moveDirection.x == 0)
        {
            // האטה הדרגתית של השחקן
            if (Mathf.Abs(playerRb.velocity.x) > minSpeed)
            {
                playerRb.AddForce(new Vector2(-playerRb.velocity.x * friction, 0f));
            }
            else
            {
                playerRb.velocity = new Vector2(0, playerRb.velocity.y);
            }
        }
        else if (!playerOnPlatform && playerRb != null && lastMoveDirection.magnitude > 0)
        {
            // המשך תנועה בכיוון האחרון
            if (Mathf.Abs(playerRb.velocity.x) > minSpeed)
            {
                playerRb.AddForce(lastMoveDirection.normalized * friction * -playerRb.velocity.x);
            }
            else
            {
                playerRb.velocity = new Vector2(0, playerRb.velocity.y);
            }
        }
    }
}