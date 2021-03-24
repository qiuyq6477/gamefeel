using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string EnemyType;
    public float Interval;
    public int Num;
    public int Dir = -1;
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        for (int i = 0; i < Num; i++)
        {
            var go = PoolManager.Spawn(EnemyType, transform.position, Quaternion.identity);
            var enemy = go.GetComponent<RobotInput>();
            enemy.x = Dir;
            enemy.xRaw = Dir;
            yield return new WaitForSeconds(Interval);
        }
    }
}
