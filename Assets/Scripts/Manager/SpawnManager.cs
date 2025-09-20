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
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
        
        int randomEnemy = Random.Range(0, enemyPrefabs.Length);

        // --- NOVO: Encontra um ponto aleatório dentro do raio
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 spawnPosition = (Vector2)spawnPoints[randomSpawnPoint].position + randomOffset;
        
        Instantiate(enemyPrefabs[randomEnemy], spawnPosition, Quaternion.identity);

        nextSpawnTime = Time.time + spawnInterval;
    }
}