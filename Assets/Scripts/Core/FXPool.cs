using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXPool : MonoBehaviour
{
    public static FXPool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }

        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        StartCoroutine(ReturnAfterDuration(prefab, obj));
        return obj;
    }

    private IEnumerator ReturnAfterDuration(GameObject prefab, GameObject obj)
    {
        yield return new WaitForSeconds(GetFXDuration(obj));
        Return(prefab, obj);
    }

    private void Return(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        if (_pools.TryGetValue(prefab, out Queue<GameObject> pool))
            pool.Enqueue(obj);
    }

    private static float GetFXDuration(GameObject obj)
    {
        float max = 2f;
        ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            float d = main.duration + main.startLifetime.constantMax;
            if (d > max) max = d;
        }
        return max;
    }
}
