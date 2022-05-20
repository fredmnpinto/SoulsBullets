using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    private ObjectPooler _objectPooler;

    [SerializeField] private GameObject enemyPrefab;

    [SerializeField, Tooltip("In case the unit is already initialized in the object pool")]
    private string enemyPoolKey;

    private int _defaultPoolSize = 15;
    private float _spawnRadius = 8f;

    [SerializeField, Tooltip("Entities/second to spawn")]
    private float spawnRatio;

    private float _lastSpawnTime;

    // Start is called before the first frame update
    void Start()
    {
        SetupPool();

        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (IsSpawnTime())
        {
            SpawnEntity();
        }
    }

    private void SpawnEntity()
    {
        SetupNewEntity(_objectPooler.Spawn(enemyPoolKey));

        ResetTimer();
    }

    private void SetupNewEntity(GameObject gObj)
    {
        gObj.transform.position = RandomLocation();
        gObj.transform.rotation = transform.rotation;

        // Resets its pathfinding and sets it to go to a random location around the spawner
        var entityNav = gObj.GetComponent<NavMeshAgent>();
        var entityDestination = RandomLocation();

        entityNav.SetDestination(entityDestination);
    }

    private void SetupPool()
    {
        _objectPooler = ObjectPooler.Instance;

        if (enemyPoolKey.Length > 0)
        {
            return;
        }

        _objectPooler.Pool(enemyPrefab, enemyPoolKey, _defaultPoolSize);
    }

    private Vector3 RandomLocation()
    {
        var randomLocation = _spawnRadius * /* Base distance */
            Random.insideUnitSphere + transform.position; /* Random location around the spawner */

        randomLocation.y = 0;

        return randomLocation;
    }

    private void ResetTimer()
    {
        _lastSpawnTime = Time.time;
    }

    private bool IsSpawnTime()
    {
        return _lastSpawnTime + GetSpawnDelay() <= Time.time;
    }

    private float GetSpawnDelay()
    {
        return 1 / spawnRatio;
    }
}