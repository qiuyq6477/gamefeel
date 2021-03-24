using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnim : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private Weapon _weapon;
    void Start()
    {
        _weapon = GetComponentInParent<Weapon>();
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponentInParent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_weapon.IsFire)
        {
            _animator.SetFloat("speedx", -1);
        }
        else
        {
            _animator.SetFloat("speedx", 1);
        }
        _animator.SetFloat("speedy", _rigidbody2D.velocity.y);
    }
}
