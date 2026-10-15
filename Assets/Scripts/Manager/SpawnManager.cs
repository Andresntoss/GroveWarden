using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [Header("Inimigos")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Configurações de Spawn")]
    public float spawnInterval = 3f;
    public int maxEnemies = 10;
    
    [Header("Spawn Aleatório")]
    public float spawnRadius = 3f; // <--- NOVO: O raio da área de spawn
    
    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time;
    }

    void Update()
    {
        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        
        if (currentEnemies < maxEnemies && Time.time > nextSpawnTime)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);

        if (enemyPrefabs == null || enemyPrefabs.Length == 0 ||
            spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "SpawnManager: configure Enemy Prefabs e Spawn Points.", this);

            enabled = false;
            return;
        }

        int randomEnemy = Random.Range(0, enemyPrefabs.Length);
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);

        GameObject prefab = enemyPrefabs[randomEnemy];
        Transform point = spawnPoints[randomSpawnPoint];

        if (prefab == null || point == null)
        {
            Debug.LogError(
                "SpawnManager: existe uma referência ausente ou destruída. " +
                "Use prefabs da janela Project em Enemy Prefabs.", this);

            enabled = false;
            return;
        }

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 position = point.position;
        position += new Vector3(offset.x, offset.y, 0f);

        Instantiate(prefab, position, Quaternion.identity);
    }
}