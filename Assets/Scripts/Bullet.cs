using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public float moveSpeed;

    public float speedOffsetY;
    [Header("爆炸半径")]
    public float explosionRadius;

    [Header("爆炸概率")] 
    public float explosionRate = 0.5f;
    
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        velocity.x = dir * moveSpeed;
        velocity.y = speedOffsetY;
        // _spriteRenderer.flipX = dir != 1;

        Vector2 h = new Vector2(velocity.x, 0);
        Vector2 v = new Vector2(0, velocity.y);
        Vector2 r = h + v;
        transform.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.FromToRotation(Vector3.right, r);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log(other.gameObject.layer);
        if (other.transform.CompareTag("Enemy"))
        {
            other.transform.GetComponent<Enemy>().hurt(dmg, dir);
            
            float r = Random.Range(0.0f, 1.0f);
            if (r >= explosionRate)
            {
                PoolManager.Spawn("explosion", transform.position, Quaternion.identity, 0.2f);
                AudioManager.instance.PlaySound("explosion");
                var collider2Ds =
                    Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));
                foreach (var collider in collider2Ds)
                {
                    collider.gameObject.GetComponent<Enemy>().hurt(dmg, Random.Range(-1, 1) > 0.5 ? 1 : -1);
                }
            }
        }

        var pos = transform.position;
        pos.x += dir * 0.65f;
        PoolManager.Spawn("hitEffect1", pos, transform.localRotation, 0.07f);
        Destroy(this.gameObject); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rigidbody2D.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
    }
}
