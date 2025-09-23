using UnityEngine;
using System.Collections.Generic;

public class AreaManager : MonoBehaviour
{
    public List<GameObject> initialEnemies; // Inimigos que existem no início
    public Transform[] spawnPoints; // Pontos onde os inimigos podem ser spawnados

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool playerIsInside = false;

    // Chamado quando o jogador entra na área
    public void EnterArea()
    {
        if (playerIsInside) return;
        playerIsInside = true;

        SpawnInitialEnemies();
    }

    // Chamado quando o jogador sai da área
    public void ExitArea()
    {
        if (!playerIsInside) return;
        playerIsInside = false;
        
        DespawnEnemies();
    }

    private void SpawnInitialEnemies()
    {
        // Se a lista já tem inimigos, não faz nada
        if (activeEnemies.Count > 0)
        {
            return;
        }

        foreach (GameObject enemyPrefab in initialEnemies)
        {
            // Instancia o inimigo na posição inicial
            GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            activeEnemies.Add(newEnemy);
        }
    }

    private void DespawnEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            Destroy(enemy);
        }
        activeEnemies.Clear();
    }
}