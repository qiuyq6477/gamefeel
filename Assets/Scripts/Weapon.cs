using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IWeapon
{
    bool IsFire { get; set; }
    float Interval { get; set; }
    int Dir { get; set; }
    float BackForce{ get; set; }
    void OnFire(int dir);
}

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("子弹类型")]
    [SerializeField]
    protected string BulletType;
    [Header("枪口")]
    [SerializeField]
    protected Transform GunPoint;

    [Header("开火间隔")] [SerializeField] 
    private float interval = 0.1f;

    public float Interval
    {
        get { return interval; }
        set { interval = value; }
    }

    [Header("子弹速度")]
    [SerializeField]
    protected float bulletSpeed;
    [Header("子弹速度Y的偏移")]
    [SerializeField]
    protected float bulletSpeedY = 1f;

    [Header("后坐力")]
    [SerializeField]
    private float backforce = -1.0f;
    public float BackForce
    {
        get { return backforce; }
        set { backforce = value; }
    }

    public bool IsFire { get; set; }
    private int dir;
    public int Dir {
        get { return dir; }
        set { dir = value; }
    }
    
    protected Movement _movement;
    
    protected Cinemachine.CinemachineCollisionImpulseSource MyInpulse;
    private Player _player;
    protected virtual void Start()
    {
        _movement = GetComponent<Movement>();
        MyInpulse = GetComponent<Cinemachine.CinemachineCollisionImpulseSource>();
        _player = GetComponent<Player>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_player.death) return;
        if (Input.GetButton("Fire1"))
        {
            IsFire = true;
            Fire(_movement.side);
            MyInpulse.GenerateImpulse();
        }
        else
        {
            IsFire = false;
        }
        
    }

    private float _interval = 0;
    private void Fire(int dir)
    {
        this.dir = dir;
        _interval -= Time.deltaTime;
        if (_interval <= 0)
        {
            _interval = Interval;
            AudioManager.instance.PlaySound("fire");
            OnFire(dir);
        }
    }

    public virtual void OnFire(int dir)
    {
        var go = PoolManager.Spawn(BulletType, GunPoint.transform.position, Quaternion.identity);
        var bullet = go.GetComponent<Bullet>();
        bullet.dir = dir;
        bullet.moveSpeed = bulletSpeed;
        bullet.speedOffsetY = Random.Range(-bulletSpeedY, bulletSpeedY);;
        PoolManager.Spawn("gunEffect", GunPoint.transform.position, Quaternion.identity, 0.05f);
    }
}
