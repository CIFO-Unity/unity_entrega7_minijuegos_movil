using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size = 10;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            GameObject parent = new GameObject(pool.tag + "_pool");
            parent.transform.parent = transform;

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, parent.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Queue<GameObject> q = poolDictionary[tag];

        if (q.Count == 0)
        {
            Debug.LogWarning("Pool empty for tag " + tag);
            return null;
        }

        GameObject obj = q.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
