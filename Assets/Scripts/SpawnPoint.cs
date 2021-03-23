using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string EnemyType;
    public float Interval;
    public int Num;
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        for (int i = 0; i < Num; i++)
        {
            var go = PoolManager.Spawn(EnemyType, transform.position, Quaternion.identity);
            var enemy = go.GetComponent<Enemy>();
            enemy.dir = -1;
            yield return new WaitForSeconds(Interval);
        }
    }
}
