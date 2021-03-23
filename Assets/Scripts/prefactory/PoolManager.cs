using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// Public list maintained for Unity editing. Converted to a Dictionary for fast lookups at run-time.
    /// </summary>
    public List<PrefabPool> PrefabPoolCollection;

    public static PoolManager Instance { get; private set; }

    
    /// <summary>
    /// Collection of pools.
    /// </summary>
    private static readonly Dictionary<string, PrefabPool> Pools = new Dictionary<string, PrefabPool>();

    #endregion

    #region Methods

    /// <summary>
    /// local initialize.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance.Clear();
        Instance = null;
    }

    /// <summary>
    /// On level loaded, clear the existing pools.
    /// </summary>
    public void Clear()
    {
        Pools.Clear();
    }

    /// <summary>
    /// Returns true if a pool of the specified name already exists.
    /// </summary>
    public static bool PoolExists(string name)
    {
        return Pools.ContainsKey(name);
    }

    /// <summary>
    /// Returns true if a pool of the specified type already exists.
    /// </summary>
    public static bool PoolExists(GameObject go)
    {
        return (go != null && Pools.ContainsKey(go.GetComponent<PoolObject>().PrefabName));
    }    

    /// <summary>
    /// 新增一个池
    /// </summary>
    public static void AddPool(string prefabPath)
    {
        //使用的时候创建poolManager，使用者不再需要关心初始化
        if (Instance == null)
        {
            GameObject poolManagerObj = new GameObject("PoolManager");
            poolManagerObj.AddComponent<PoolManager>();
        }
        
        PrefabPool pp;
        if (!Pools.TryGetValue(prefabPath, out pp))
        {
            GameObject go = Instantiate(Resources.Load<GameObject>(prefabPath));

            if (go == null)
                return;
            var po = go.GetComponent<PoolObject>();

            pp = new PrefabPool();

            if (po == null)
            {
                po = go.AddComponent<PoolObject>();
            }

            po.PrefabName = prefabPath;
            pp.RenderOrderInScene = Pools.Count;
            pp.Prefab = po;
            pp.PreAlloc = po.PreAlloc;

            pp.Awake();
            Pools.Add(prefabPath, pp);
        }
        else
        {
            //目前只考虑PVE情况（即玩家只能操控多个军团的其中一个，S1预加载3份资源a，S2预加载5份资源a，则实际只需要预加载5份）
            if (pp.PoolSize >= pp.PreAlloc) return;
            pp.Allocate(pp.PreAlloc - pp.PoolSize);
            //考虑PVP多个玩家的情况（即多个玩家军团同时使用同一资源，P1预加载3份资源a，P2预加载5份资源a，则实际需要预加载8份）
            //pp.Allocate(pp.PreAlloc);
        }
    }

    public static GameObject Spawn(string prefabPath, int customLimitNum = -1, float autoDespawnTime = 0)
    {
        try
        {
            if (!Pools.ContainsKey(prefabPath))
            {
                AddPool(prefabPath);
            }

            if (customLimitNum != -1 && Pools[prefabPath].ActiviNum >= customLimitNum)
            {
                return null;
            }
            return Pools[prefabPath].Spawn(autoDespawnTime);
        }
        catch (Exception e)
        {
            Debug.LogError(e + " " + prefabPath);
            throw;
        }
    }
    
    public static GameObject Spawn(string prefabPath, Vector3 position, Quaternion rotation, float autoDespawnTime = 0, int customLimitNum = -1)
    {
        GameObject go = Spawn(prefabPath, customLimitNum, autoDespawnTime);
        if (go != null)
        {
            go.transform.position = position;
            go.transform.rotation = rotation;
        }
        return go;
    }

    struct DelayDespawn
    {
        public float despawnTime;
        public GameObject go;
    }

    private static List<DelayDespawn> delayDespawns = new List<DelayDespawn>();
    
    /// <summary>
    /// Despawn the specified GameObject, returning it to its pool.
    /// If the GameObject has no pool, it is destroyed instead.
    /// </summary>
    public static void Despawn(GameObject go, float delay = 0)
    {
        if (go == null)
            return;

        if (delay > 0.001f)
        {
            DelayDespawn despawn = new DelayDespawn();
            despawn.despawnTime = Time.time + delay;
            despawn.go = go;
            delayDespawns.Add(despawn);
            return;
        }

        var po = go.GetComponent<PoolObject>();
        if (po == null || !PoolExists(po.PrefabName))
            Destroy(go);
        else
        {
            Pools[po.PrefabName].Despawn(go);
        }
    }

    /// <summary>
    /// Update the PoolManager.
    /// </summary>
    private void Update()
    {
        for (int i = 0; PrefabPoolCollection != null && i < PrefabPoolCollection.Count; ++i )
        {
            PrefabPoolCollection[i].Poll();
        }

        for (int i = delayDespawns.Count - 1; i >= 0; i--)
        {
            var despawn = delayDespawns[i];
            if (despawn.despawnTime <= Time.time)
            {
                Despawn(despawn.go);
                delayDespawns.RemoveAt(i);
            }
        }
    }    

    #endregion
    
   
}