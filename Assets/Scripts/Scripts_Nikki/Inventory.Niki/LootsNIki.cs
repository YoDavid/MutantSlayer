using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LootsNIki : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Collider2D collider2D;
    [SerializeField] private float moveSpeed;

    private ItemNiki itemNiki;

    public void Initialize(ItemNiki item)
    {
        this.itemNiki = item;
        sr.sprite = item.image;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "TestPlayer")
        {
            bool canAdd = InventotyManager.instance.AddItem(itemNiki);
            if (canAdd)
            {
                StartCoroutine(MoveAndCollect(other.transform));
            }
            Destroy(other.gameObject);
        }
    }

    private IEnumerator MoveAndCollect(Transform target)
    {
        Destroy(collider2D);
        while (transform.position != target.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return 0;
        }
        Destroy(gameObject);
    }

}
