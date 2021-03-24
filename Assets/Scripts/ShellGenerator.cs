using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellGenerator : MonoBehaviour
{
    public string BulletShellType;
    public Transform shellInitPosition;
    public Vector2 force;
    public Vector2 rotate;
    private Weapon _weapon;
    void Start()
    {
        _weapon = GetComponentInParent<Weapon>();
    }

    private float _interval = 0;
    void Update()
    {
        if (_weapon.onfire)
        {
            _interval -= Time.deltaTime;
            if (_interval <= 0)
            {
                _interval = _weapon.interval;
                var go = PoolManager.Spawn(BulletShellType, shellInitPosition.position, Quaternion.identity);
                var rb = go.GetComponent<Rigidbody2D>();
                rb.AddForce(new Vector2(-_weapon.weaponDir * Random.Range(force.x, force.y), Random.Range(force.x, force.y)));
                rb.AddTorque(Random.Range(rotate.x, rotate.y));
            }
        }
    }
}
