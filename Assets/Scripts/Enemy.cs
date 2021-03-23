using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int dir;
    public float moveSpeed;
    [SerializeField]
    int life;


    private Rigidbody2D _rigidbody2D;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(dir, 0), 0.5f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            dir = -dir;
        }

        var velocity = _rigidbody2D.velocity;
        velocity.x = dir * moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.velocity = velocity;
    }

    public void hurt(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            die();
        }
    }
    private void die()
    {
        PoolManager.Despawn(gameObject);
    }
}
