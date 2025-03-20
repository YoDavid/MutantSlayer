using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingPlatform : MonoBehaviour
{
    // טאג של השחקן
    public string playerTag = "Player";
    // מהירות ההחלקה
    public float slideSpeed = 5f;
    // כוח חיכוך (כדי להאט את השחקן)
    public float friction = 0.1f;
    // מהירות מינימלית
    public float minSpeed = 1f;

    private Rigidbody2D playerRb;
    private bool playerOnPlatform = false;
    private Vector2 lastVelocity;

    void Update()
    {
        if (playerOnPlatform && playerRb != null)
        {
            // שמירת מהירות השחקן
            lastVelocity = playerRb.velocity;

            // העברת השחקן באותו כיוון שהוא נע
            playerRb.AddForce(new Vector2(lastVelocity.x * slideSpeed, 0f));

            // הוספת חיכוך (כדי להאט את השחקן)
            if (Mathf.Abs(playerRb.velocity.x) > minSpeed)
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
        if (!playerOnPlatform && playerRb != null)
        {
            // המשך תנועה בכיוון האחרון
            if (Mathf.Abs(lastVelocity.x) > minSpeed)
            {
                playerRb.velocity = new Vector2(lastVelocity.x, playerRb.velocity.y);

                // הוספת חיכוך (כדי להאט את השחקן)
                if (Mathf.Abs(playerRb.velocity.x) > minSpeed)
                {
                    playerRb.AddForce(new Vector2(-playerRb.velocity.x * friction, 0f));
                }
            }
            else
            {
                playerRb.velocity = new Vector2(0, playerRb.velocity.y);
            }
        }
    }
}