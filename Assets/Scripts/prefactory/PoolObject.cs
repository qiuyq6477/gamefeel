using System;
using UnityEngine;
using System.Collections.Generic;

public class PoolObject : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// Called when the object is despawned.
    /// </summary>
    public EventHandler Despawned;

    /// <summary>
    /// Dynamic objects do not cache their hierarchy.
    /// </summary>
    public bool IsDynamic = false;

    /// <summary>
    /// AutoDespawn this object at how many seconds later
    /// 0 means don't AutoDespawn
    /// </summary>
    public float AutoDespawnTime = 0f;

    /// <summary>
    /// The prefab name for this object. GameObject name can be changed
    /// at runtime, and the prefab name is stored to facilitate pooling.
    /// </summary>
    public string PrefabName
    {
        set;
        get;
    }

    /// <summary>
    /// Preload Num by joseph
    /// </summary>
    public int PreAlloc = 1;

    public bool IsSpawned { get; private set; }

    public float TimeLastSpawned
    {
        get { return m_timeLastSpawned; }
        private set { m_timeLastSpawned = value; }
    }

    public float TimeLastDespawned
    {
        get { return m_timeLastDespawned; }
        private set { m_timeLastDespawned = value; }
    }

    /// <summary>
    /// Get the age of the object since spawn.
    /// </summary>
    private float Age
    {
        get
        {
            if (!IsSpawned)
                return -1f;
            return Time.time - TimeLastSpawned;
        }
    }

    /// <summary>
    /// Get the age of the object as a scalar relating to the despawn timer. If
    /// a timed despawn has not been triggered, the object's age will always be 1.
    /// </summary>
    private float AgeAsScalar
    {
        get
        {
            if (!IsSpawned)
                return -1f;
            if (DespawnDelay <= 0f)
                return 1f;
            return (Time.time - DespawnTimerInitialized)/DespawnDelay;
        }
    }

    /// <summary>
    /// Called when the object is spawned.
    /// </summary>
    public event EventHandler Spawned;

    /// <summary>
    /// The total time until the despawn timer is completed.
    /// </summary>
    private float DespawnDelay = -1f;

    /// <summary>
    /// The time that the despawn timer was initialized.
    /// </summary>
    private float DespawnTimerInitialized = -1f;

    /// <summary>
    /// Cache a list of attached GameObjects for non-dynamic objects.
    /// </summary>
    private List<GameObject> GameObjectCache;

    /// <summary>
    /// Cache a list of attached Renderers for non-dynamic objects.
    /// </summary>
    private List<Renderer> RendererCache;

    /// <summary>
    /// The time that the PoolObject was despawned.
    /// </summary>
    private float m_timeLastDespawned = -1f;

    /// <summary>
    /// The time that the PoolObject was spawned.
    /// </summary>
    private float m_timeLastSpawned = -1f;

    #endregion

    #region Methods

    /// <summary>
    /// Local initialize.
    /// </summary>
    public void Init()
    {
        GameObjectCache = new List<GameObject>();
        RendererCache = new List<Renderer>();
        if (IsDynamic)
        {
            RefreshCache();
        }
        SetActive(false);
    }

    /// <summary>
    /// Calculate the GameObject and Renderer hierarchy.
    /// </summary>
    private void RefreshCache()
    {
        GameObjectCache.Clear();
        RendererCache.Clear();
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
            GameObjectCache.Add(t.gameObject);
        foreach (GameObject go in GameObjectCache)
            foreach (Renderer r in go.GetComponents<Renderer>())
                RendererCache.Add(r);
    }

    /// <summary>
    /// Set active recursively (cached).
    /// </summary>
    private void SetActive(bool active)
    {
        for (int i = 0; i < GameObjectCache.Count; i++)
            GameObjectCache[i].SetActive(active);

        for (int i = 0; i < RendererCache.Count; i++)
            RendererCache[i].enabled = active;
        gameObject.SetActive(active);

        Animator a = gameObject.GetComponentInChildren<Animator>(true);
        if (a != null)
        {
            a.enabled = active;
        }
    }

    /// <summary>
    /// Enable the GameObject and all children.
    /// </summary>
    public void OnSpawn()
    {
        if (IsSpawned)
            return;

        IsSpawned = true;
        TimeLastSpawned = Time.time;

        if (IsDynamic)
            RefreshCache();

        SetActive(true);

        if (Spawned != null)
            Spawned(this, null);

        if (AutoDespawnTime > 0.001f)
        {
            PoolManager.Despawn(gameObject, AutoDespawnTime);
        }

    }

    /// <summary>
    /// Disable the GameObject and all children.
    /// </summary>
    public void OnDespawn()
    {
        if (!IsSpawned)
            return;

        IsSpawned = false;
        TimeLastDespawned = Time.time;
        DespawnTimerInitialized = -1f;
        DespawnDelay = -1f;
        StopAllCoroutines();

        if (IsDynamic)
            RefreshCache();

        SetActive(false);

        if (Despawned != null)
            Despawned(this, null);
    }

    #endregion
}