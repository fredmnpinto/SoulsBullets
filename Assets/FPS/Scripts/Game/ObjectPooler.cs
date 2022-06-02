using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;

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

	public void Pool(GameObject prefab, string poolKey, int quantity)
	{
		SetupPool(new PoolableObject(prefab, quantity, poolKey));
	}

	public void IncreasePool(string key, int newAmount)
	{
		Queue<GameObject> pool = (Queue<GameObject>) pools[key];
		PoolableObject poolable = GetPoolable(key);

		for (int i = 0; i < newAmount - poolable.quantity; i++)
		{
			var go = Instantiate(poolable.prefab);
			go.SetActive(false);
			pool.Enqueue(go);
		}
	}

	private PoolableObject GetPoolable(string key)
	{
		foreach (var poolable in objectsToPool)
		{
			if (poolable.poolKey == key)
			{
				return poolable;
			}
		}

		throw new Exception($"Poolable with key = '{key}' does not exist");
	}

	/**
     * Checks if a GameObject has been pooled
     */
	public bool IsPooled(GameObject obj)
	{
		foreach (PoolableObject pool in pools)
		{
			if (PrefabUtility.GetPrefabInstanceHandle(obj) == pool.prefab)
				return true;
		}

		return false;
	}

	public GameObject Spawn(string poolKey)
	{
		if (!pools.ContainsKey(poolKey) || ((Queue<GameObject>)pools[poolKey]).Count == 0)
		{
			string poolsWarning = "[";
			foreach (string key in pools.Keys)
			{
				poolsWarning += $"Pool key: {key} | Pool Count: {((Queue<GameObject>)pools[key]).Count},\n";
			}

			poolsWarning += "]";
			
			Debug.LogWarning($"Trying to spawn object not pooled: {poolKey}\n" +
				$"List of pools: {poolsWarning}");
			throw new Exception($"Trying to spawn object not pooled: {poolKey}\n" +
			                    $"List of pools: {pools.Keys}");
		}

		var objPool = (Queue<GameObject>)pools[poolKey];
		var newSpawn = objPool.Dequeue();
		objPool.Enqueue(newSpawn);

		/* Bandaid para chamar todos os OnActive */
		newSpawn.SetActive(false);
		newSpawn.SetActive(true);

		return newSpawn;
	}

	public void Vanish(GameObject obj, [DefaultValue("0.0F")] float delay)
	{

		if (obj.CompareTag("OnDestroyDependant") || !IsPooled(obj))
		{
			Destroy(obj, delay);
			return;
		}

		// Deactivates in *delay* seconds
		Task.Run(() =>
		{
			Task.Delay(TimeSpan.FromSeconds(delay));
			obj.SetActive(false);
		});
	}

	private void SetupPool(PoolableObject obj)
	{
		Queue<GameObject> pool = new Queue<GameObject>();

		while (pool.Count < obj.quantity)
		{
			GameObject instance = Instantiate(obj.prefab);
			instance.SetActive(false);

			pool.Enqueue(instance);
		}

		pools.Add(obj.poolKey, pool);
	}
}