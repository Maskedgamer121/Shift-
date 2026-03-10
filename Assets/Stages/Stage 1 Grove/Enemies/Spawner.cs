using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private int maxEnemies = 20;
    [SerializeField] private float spawnPadding = 1f;

    public static int currentEnemyCount = 0;

    private Camera mainCamera;
    private float nextSpawnTime = 0f;

    private void Start()
    {
        mainCamera = Camera.main;
        currentEnemyCount = 0;
    }

    private void Update()
    {
        if (enemyPrefab == null) return;
        if (currentEnemyCount >= maxEnemies) return;

        if (Time.time >= nextSpawnTime)
        {
            nextSpawnTime = Time.time + spawnRate;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos = GetSpawnPosition();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemyCount++;
    }

    private Vector3 GetSpawnPosition()
    {
        float camHeight = mainCamera.orthographicSize + spawnPadding;
        float camWidth  = (mainCamera.orthographicSize * mainCamera.aspect) + spawnPadding;
        Vector3 camPos  = mainCamera.transform.position;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: return new Vector3(Random.Range(camPos.x - camWidth, camPos.x + camWidth), camPos.y + camHeight, 0f);
            case 1: return new Vector3(Random.Range(camPos.x - camWidth, camPos.x + camWidth), camPos.y - camHeight, 0f);
            case 2: return new Vector3(camPos.x - camWidth, Random.Range(camPos.y - camHeight, camPos.y + camHeight), 0f);
            default: return new Vector3(camPos.x + camWidth, Random.Range(camPos.y - camHeight, camPos.y + camHeight), 0f);
        }
    }
}