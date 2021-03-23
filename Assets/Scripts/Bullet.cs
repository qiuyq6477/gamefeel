using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int dir;
    [Header("当前速度")]
    [SerializeField]
    Vector3 velocity;
    [Header("伤害")]
    [SerializeField]
    int dmg;
    [Header("飞行距离")]
    [SerializeField]
    float distance;
    [Header("飞行速度")]
    [SerializeField]
    float moveSpeed;
    
    
    private Rigidbody2D _rigidbody2D;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        velocity.x = dir * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            other.transform.GetComponent<Enemy>().hurt(dmg);
        }
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rigidbody2D.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
    }
}
