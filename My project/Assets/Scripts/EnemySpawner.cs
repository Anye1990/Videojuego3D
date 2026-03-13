using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public int numberOfEnemies;
        public float spawnDelay;
    }

    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    
    [Header("Wave Settings")]
    public Wave[] waves;
    public float timeBetweenWaves = 5f;
    
    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    private void Start()
    {
        if (spawnPoints.Length == 0)
        {
            // If no spawn points assigned, use this object's transform
            spawnPoints = new Transform[] { transform };
        }

        if (waves.Length > 0 && enemyPrefab != null)
        {
            StartCoroutine(SpawnWaves());
        }
        else
        {
            Debug.LogWarning("EnemySpawner is missing enemies to spawn or waves are not configured.");
        }
    }

    private IEnumerator SpawnWaves()
    {
        // 1. Esperamos obligatoriamente a que el jugador elija personaje y el estado del juego sea "Playing"
        while (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing)
        {
            yield return null; // Pausamos el spawner frame por frame hasta que sea cierto
        }

        while (currentWaveIndex < waves.Length)
        {
            // 2. Si abrimos otro men\u00fa o morimos, pausamos el spawn
            if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing)
            {
                yield return null;
                continue;
            }

            isSpawning = true;
            Debug.Log("Starting wave: " + waves[currentWaveIndex].waveName);

            for (int i = 0; i < waves[currentWaveIndex].numberOfEnemies; i++)
            {
                // 3. Volvemos a revisar por si el jugador muri\u00f3 durante la ola de enemigos
                if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.Playing)
                {
                    break;
                }

                SpawnEnemy();
                yield return new WaitForSeconds(waves[currentWaveIndex].spawnDelay);
            }

            isSpawning = false;
            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
        
        Debug.Log("All waves finished!");
    }

    private void SpawnEnemy()
    {
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Instantiate the enemy
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Notify GameManager so it knows how many have spawned or been killed (optional if not tracking kills to win)
        // GameManager.Instance.totalEnemiesInLevel++; 
    }
}
