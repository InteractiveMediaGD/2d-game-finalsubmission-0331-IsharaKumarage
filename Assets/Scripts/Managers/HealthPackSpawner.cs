using UnityEngine;

/// <summary>
/// Spawns Health Packs at random intervals and positions in front of the player.
/// </summary>
public class HealthPackSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject healthPackPrefab;

    [Header("Spawning Settings")]
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;
    public float spawnDistanceX = 15f; // how far in front of player
    public float groundY = -4f; // Spawn exactly on the floor line


    private Transform player;
    private float nextSpawnTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    void Update()
    {
        if (player == null) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnPack();
            nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
        }
    }

    void SpawnPack()
    {
        if (healthPackPrefab == null) return;

        // Spawn further to the right (ahead of player)
        Vector3 spawnPos = new Vector3(
            player.position.x + spawnDistanceX * 1.5f, 
            groundY, 
            0
        );

        Instantiate(healthPackPrefab, spawnPos, Quaternion.identity);
    }
}
