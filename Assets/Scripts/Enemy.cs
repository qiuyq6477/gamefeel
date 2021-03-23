using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int dir;
    public float moveSpeed;
    [SerializeField]
    int life;

    private Animator animator;
    private SpriteRenderer _renderer;
    private Rigidbody2D _rigidbody2D;
    private Collider2D _collider;
    private int damageDir;
    private float staticTime;
    private Movement _movement;
    private Collision _collision;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _movement = GetComponent<Movement>();
        _collision = GetComponent<Collision>();
        gameObject.layer = 10;
        _movement.canMove = true;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 10;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (life <= 0)
        {
            if (_collision.onGround && _movement.rb.velocity.y < 0)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Static;
            }
        }
        // RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(dir, 0), 0.5f, LayerMask.GetMask("Ground"));
        // if (hit.collider != null)
        // {
        //     dir = -dir;
        //     _renderer.flipX = !_renderer.flipX;
        // }
        //
        // var velocity = _rigidbody2D.velocity;
        // velocity.x = dir * moveSpeed * Time.fixedDeltaTime;
        // if (life <= 0)
        // {
        //     velocity.x = Math.Abs(velocity.x);
        //     velocity.x = damageDir == 1 ? velocity.x : -velocity.x;
        // }
        // _rigidbody2D.velocity = velocity;
        //
        // if (life <= 0)
        // {
        //     staticTime -= Time.deltaTime;
        //     if (staticTime <= 0)
        //     {
        //         RaycastHit2D ground = Physics2D.Raycast(transform.position, new Vector2(0, -1), 1f, LayerMask.GetMask("Ground"));
        //         if (ground.collider != null)
        //         {
        //             _rigidbody2D.bodyType = RigidbodyType2D.Static;
        //             return;
        //         }
        //     }
        // }
        //
        // animator.SetFloat("speedx", Math.Abs(velocity.x));
    }

    public void hurt(int damage, int dir)
    {
        if (life <= 0) return;
        damageDir = dir;
        AudioManager.instance.PlaySound("hurt");
        life -= damage;
        animator.SetTrigger("hurt");
        if (life <= 0)
        {
            die();
        }
    }
    private void die()
    {
        AudioManager.instance.PlaySound("dead");
        animator.SetTrigger("dead");
        gameObject.layer = 9;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 9;
        }
        _movement.canMove = false;
        _rigidbody2D.velocity = new Vector2(Math.Abs(_rigidbody2D.velocity.x) * damageDir, _movement.jumpForce);
        staticTime = 0.2f;

        // PoolManager.Despawn(gameObject, 1.0f);
    }
}
