using UnityEngine;

/// <summary>
/// Spawns Enemies at random intervals and positions in front of the player.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Spawning Settings")]
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 8f;
    public float spawnDistanceX = 20f; // how far in front of player
    public float groundY = -2f; // adjust to match your ground level

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
            SpawnEnemy();
            nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = new Vector3(
            player.position.x + spawnDistanceX,
            groundY,
            0
        );

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
