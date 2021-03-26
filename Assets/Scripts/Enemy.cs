using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    int life;

    private Animator animator;
    private Rigidbody2D _rigidbody2D;
    private int damageDir;
    private float damageForce;
    private float staticTime;
    private Movement _movement;
    private Collision _collision;
    private RobotInput _input;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        _movement = GetComponent<Movement>();
        _collision = GetComponent<Collision>();
        _input = GetComponent<RobotInput>();
        gameObject.layer = 10;
        _movement.canMove = true;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 10;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (life <= 0)
        {
            if (_collision.onGround && _movement.rb.velocity.y < 0)
            {
                _rigidbody2D.bodyType = RigidbodyType2D.Static;
            }

            if (stopTime > 0)
            {
                Time.timeScale = 1;
            }
        }
        else
        {
            if (backTime > 0)
            {
                backTime -= Time.unscaledDeltaTime;
                var cur = _rigidbody2D.velocity;
                cur.x += damageDir * damageForce;
                _rigidbody2D.velocity = cur;
                // if (backTime <= 0)
                // {
                //     _rigidbody2D.velocity = lastVelocity;
                // }
            }
            if (stopTime > 0)
            {
                stopTime -= Time.unscaledDeltaTime;
                if (stopTime <= 0)
                {
                    Time.timeScale = 1;
                }
            }
        }
    }

    private float backTime;
    private float stopTime;
    private Vector2 lastVelocity;
    public void hurt(int damage, int dir, float force)
    {
        if (life <= 0) return;
        damageDir = dir;
        damageForce = force;
        AudioManager.instance.PlaySound("hurt");
        life -= damage;
        animator.SetTrigger("hurt");
        backTime = 0.1f;
        stopTime = 0.03f;
        Time.timeScale = 0;
        lastVelocity = _rigidbody2D.velocity;
        if (life <= 0)
        {
            die();
        }
    }
    private void die()
    {
        AudioManager.instance.PlaySound("dead");
        animator.SetTrigger("dead");
        gameObject.layer = 12;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 12;
        }
        _movement.canMove = false;
        _rigidbody2D.velocity = new Vector2(Math.Abs(_rigidbody2D.velocity.x) * damageDir, _movement.jumpForce);
        staticTime = 0.2f;

        // PoolManager.Despawn(gameObject, 1.0f);
    }
}
