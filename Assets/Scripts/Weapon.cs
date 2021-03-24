using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("子弹类型")]
    [SerializeField]
    protected string BulletType;
    [Header("枪口")]
    [SerializeField]
    protected Transform GunPoint;
    [Header("开火间隔")]
    [SerializeField]
    public float interval = 0.1f;

    [Header("子弹速度")]
    [SerializeField]
    protected float bulletSpeed;
    [Header("子弹速度Y的偏移")]
    [SerializeField]
    protected float bulletSpeedY = 1f;

    [Header("后坐力")]
    [SerializeField]
    public float backforce = -1f;

    [HideInInspector]
    public bool onfire = false;
    [HideInInspector]
    public int weaponDir;
    protected Movement _movement;
    
    protected Cinemachine.CinemachineCollisionImpulseSource MyInpulse;

    void Start()
    {
        _movement = GetComponent<Movement>();
        MyInpulse = GetComponent<Cinemachine.CinemachineCollisionImpulseSource>();

    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetButton("Fire1"))
        {
            onfire = true;
            Fire(_movement.side);
            MyInpulse.GenerateImpulse();
        }
        else
        {
            onfire = false;
        }
        
    }

    private float _interval = 0;
    private void Fire(int dir)
    {
        weaponDir = dir;
        _interval -= Time.deltaTime;
        if (_interval <= 0)
        {
            _interval = interval;
            AudioManager.instance.PlaySound("fire");
            OnFire(dir);
        }
    }

    protected virtual void OnFire(int dir)
    {
        var go = PoolManager.Spawn(BulletType, GunPoint.transform.position, Quaternion.identity);
        var bullet = go.GetComponent<Bullet>();
        bullet.dir = dir;
        bullet.moveSpeed = bulletSpeed;
        bullet.speedOffsetY = Random.Range(-bulletSpeedY, bulletSpeedY);;
        PoolManager.Spawn("gunEffect", GunPoint.transform.position, Quaternion.identity, 0.05f);
    }
}
