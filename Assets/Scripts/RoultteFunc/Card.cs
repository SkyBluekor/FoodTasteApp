using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public void MoveCard(Vector2 direction, float force)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direction * force);
    }

    public void StartMoving()
    {
        Vector2 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        float force = Random.Range(1f, 10f);
        MoveCard(direction, force);
    }

    public void ShakeCard()
    {
        Vector2 direction = new Vector2(Random.Range(-1f,1f), Random.Range(-1f, 1f));
        float force = Random.Range(10f, 20f);
        MoveCard(direction, force);
    }
}
