#region Using Statements

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

[Serializable]
public class PrefabPool
{
    #region Fields

    /// <summary>
    /// The number of this prefab to allocate when requesting from an empty pool.
    /// </summary>
    public int AllocBlock = 1;

    /// <summary>
    /// True if excess prefabs should be culled.
    /// </summary>
    public bool Cull = false;

    /// <summary>
    /// The maximum number of the prefab to maintain in the pool.
    /// </summary>
    public int CullAbove = 8;

    /// <summary>
    /// The frequency at which to cull excess prefabs.
    /// </summary>
    public float CullDelay = 10f;

    /// <summary>
    /// True if no instances beyond the limit should be created.
    /// </summary>
    public bool HardLimit = false;

    /// <summary>
    /// The limit for hard limited pools.
    /// </summary>
    public int Limit = 8;

    /// <summary>
    /// The number of the prefab to pre-allocate.
    /// </summary>
    public int PreAlloc = 8;

    /// <summary>
    /// The prefab managed by this class.
    /// </summary>
    public PoolObject Prefab;

    /// <summary>
    /// The pool.
    /// </summary>
    private Stack<GameObject> Pool;

    /// <summary>
    /// How many instances have been requested?
    /// </summary>
    private int SpawnCount;

    /// <summary>
    /// The time of the last cull.
    /// </summary>
    private float TimeOfLastCull = float.MinValue;

    /// <summary>
    /// 在战斗场景中的当前pool的初始renderOrder（*100）
    /// </summary>
    public int RenderOrderInScene = 0;

    /// <summary>
    /// 当前激活个数(实际在显示中的个数，目前用于限制实际在显示中限制个数)
    /// </summary>
    private int activeNum = 0;
    public int ActiviNum
    {
        get
        {
            return activeNum;
        }
    }

    /// <summary>
    /// 当前池子的缓存数量
    /// </summary>
    public int PoolSize
    {
        get
        {
            return Pool.Count;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Local initialize.
    /// </summary>
    public void Awake()
    {
        SetRenderOrder();
        m_root = new GameObject("Pool_" + Prefab.PrefabName);
        m_root.transform.SetParent(PoolManager.Instance.transform);
        Pool = new Stack<GameObject>(PreAlloc);
        Allocate(PreAlloc);
        SetTemplateToRoot();
    }

    /// <summary>
    /// set Particle render order
    /// </summary>
    private void SetRenderOrder()
    {
        Renderer[] rds = Prefab.transform.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rds.Length; i++)
        {
            Renderer render = rds[i];
            //美术必须给不同材质或不同贴图的粒子特效设置不同的order
            render.sortingOrder = RenderOrderInScene * 100 + render.sortingOrder;
        }
    }

    /// <summary>
    /// Adds the specified count of prefab instances to the pool.
    /// </summary>
    public void Allocate(int count)
    {
        if (HardLimit && Pool.Count + count > Limit)
            count = Limit - Pool.Count;
        for (int n = 0; n < count; n++)
        {
            var go = Object.Instantiate(Prefab.gameObject, m_root.transform, true);
            //go.name = go.name + n.ToString();
            PoolObject poolObj = go.GetComponent<PoolObject>();
            poolObj.PrefabName = Prefab.PrefabName;
            poolObj.Init();
            Pool.Push(go);
        }
    }

    /// <summary>
    /// Get a pooled item if one is available, or if legal create a new instance.
    /// </summary>
    public GameObject Pop()
    {
        if (HardLimit && SpawnCount >= Limit)
            return null;

        if (Pool.Count > 0)
        {
            SpawnCount++;
            return Pool.Pop();
        }

        Allocate(AllocBlock);
        return Pop();
    }

    /// <summary>
    /// Return an item to the pool.
    /// </summary>
    public void Push(GameObject go)
    {
        if (HardLimit && Pool.Count >= Limit)
            return;

        SpawnCount = Mathf.Max(SpawnCount - 1, 0);
        Pool.Push(go);
    }

    /// <summary>
    /// Poll for culling.
    /// </summary>
    public void Poll()
    {
        if (!Cull || Pool.Count <= CullAbove) return;

        if (Time.time > TimeOfLastCull + CullDelay)
        {
            TimeOfLastCull = Time.time;
            for (int n = CullAbove; n <= Pool.Count; n++)
                GameObject.Destroy(Pool.Pop());
        }
    }

    /// <summary>
    /// Spawn an object from the pool.
    /// </summary>
    public GameObject Spawn(float autoDespawnTime)
    {
        GameObject go = Pop();
        if (go != null)
        {
            var po = go.GetComponent<PoolObject>();
            po.AutoDespawnTime = autoDespawnTime;
            po.OnSpawn();
            activeNum ++;
        }
        return go;
    }

    /// <summary>
    /// Despawn an object back to the pool.
    /// </summary>
    public void Despawn(GameObject go)
    {
        var po = go.GetComponent<PoolObject>();
        if (po == null || po.PrefabName != Prefab.PrefabName)
            return;
        activeNum --;
        go.transform.parent = m_root.transform;
        po.OnDespawn();
        Push(go);
    }

    private void SetTemplateToRoot()
    {
        Prefab.Init();
        Prefab.transform.SetParent(m_root.transform);
    }

    #endregion

    private GameObject m_root;
}