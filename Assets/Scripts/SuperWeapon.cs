using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperWeapon : Weapon
{
    public float Angle;
    public int Num;

    public override void OnFire(int dir)
    {
        int angle = (int)Angle / Num;
        int start = angle;
        for (int i = 0; i < Num; i++)
        {
            var go = PoolManager.Spawn(BulletType, GunPoint.transform.position, Quaternion.identity);
            var bullet = go.GetComponent<Bullet>();
            bullet.dir = dir;
            bullet.moveSpeed = bulletSpeed;
            bullet.speedOffsetY = start;
            start -= angle;
        }
        PoolManager.Spawn("gunEffect", GunPoint.transform.position, Quaternion.identity, 0.05f);

    }
}
