using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Singleton instance
    public static ObjectPooler Instance { get; private set; }
    
    [Serializable]
    class PoolableObject
    {
        public int quantity;
        public GameObject prefab;
        public string poolKey;

        public PoolableObject(GameObject prefab, int quantity, string poolKey)
        {
            this.prefab = prefab;
            this.quantity = quantity;
            this.poolKey = poolKey;
        }
    }

    [SerializeField] private List<PoolableObject> objectsToPool;
    private Hashtable pools;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        pools = new Hashtable();

        foreach (var obj in objectsToPool)
        {
            SetupPool(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pool(GameObject prefab, string poolKey, int quantity)
    {
        SetupPool(new PoolableObject(prefab, quantity, poolKey));
    }
    
    public GameObject Spawn(string poolKey)
    {
        var objPool = (Queue<GameObject>)pools[poolKey];
        var newSpawn = objPool.Dequeue();
        
        newSpawn.SetActive(true);
        objPool.Enqueue(newSpawn);

        return newSpawn;
    }

    public void Vanish(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void SetupPool(PoolableObject obj)
    {
        Queue<GameObject> pool = new Queue<GameObject>();
        
        while(pool.Count < obj.quantity)
        {
            GameObject instance = Instantiate(obj.prefab);
            instance.SetActive(false);
            
            pool.Enqueue(instance);
        }

        pools.Add(obj.poolKey, pool);
    }
}
