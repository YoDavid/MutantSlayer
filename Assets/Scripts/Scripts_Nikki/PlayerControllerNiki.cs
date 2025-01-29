using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerNiki : MonoBehaviour, IDataPersistenceNiki
{
    public void LoadData(DataNiki data)
    {
        this.transform.position = data.playerPosition;
    }
    public void SaveData(ref DataNiki data)
    {
        data.playerPosition = this.transform.position;
    }

    public float movementSpeed;
    Rigidbody2D rb2d;
    public float highJump;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position -= transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb2d.AddForce(Vector3.up * highJump);
        }
    }
}