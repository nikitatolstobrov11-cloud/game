using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;          // префаб врага (твой скелет)
    public int maxEnemies = 5;               // максимум врагов одновременно
    public float spawnInterval = 5f;         // интервал спавна (секунды)

    [Header("Spawn Points")]
    public Transform[] spawnPoints;          // массив точек спавна (перетащить в инспекторе)
    public bool useRandomPoints = true;      // если true – выбирается случайная точка из массива

    [Header("Spawn Area (если нет точек)")]
    public Vector3 spawnAreaCenter;          // центр области спавна (например, (0,0,0))
    public Vector3 spawnAreaSize;            // размер области (например, (20, 0, 20))

    [Header("Player Reference")]
    public skeleton playerSkeleton;           // ссылка на игрока (перетащить в инспекторе)

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        // Если ссылка на игрока не назначена, пытаемся найти автоматически
        if (playerSkeleton == null)
            playerSkeleton = FindObjectOfType<skeleton>();

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Если игрок мёртв – не спавним новых врагов
            if (playerSkeleton != null && playerSkeleton.IsDead)
            {
                Debug.Log("Игрок мёртв – спавн остановлен");
                yield break; // полностью выходим из корутины
            }

            // Очищаем список от уничтоженных врагов
            activeEnemies.RemoveAll(enemy => enemy == null);

            // Если врагов меньше максимума – спавним нового
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        // Определяем позицию спавна
        Vector3 spawnPos;

        if (spawnPoints != null && spawnPoints.Length > 0 && useRandomPoints)
        {
            int index = Random.Range(0, spawnPoints.Length);
            spawnPos = spawnPoints[index].position;
        }
        else
        {
            float randomX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
            float randomZ = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2);
            float randomY = spawnAreaCenter.y;
            spawnPos = new Vector3(randomX, randomY, randomZ);
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(newEnemy);
        Debug.Log($"Враг заспавнен в {spawnPos}. Всего врагов: {activeEnemies.Count}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}