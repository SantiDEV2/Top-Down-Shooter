using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float spawnDelay = 0.5f;
    public bool infiniteWaves = true;
    public int enemyMultiplayer = 3;

    [Header("Enemy Refferences")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPositions;

    [Header("Pool Settings")]
    private List<GameObject> pooledEnemys = new List<GameObject>();
    private int amountPool = 5;

    [HideInInspector] public int enemysAlive;
    [HideInInspector] public int enemyCount; 
    [HideInInspector] public int waveCount = 0;

    private bool waveCompleted = false;
    private bool waveStarted = false;

    public static event Action OnWaveCompleted;

    private void OnEnable()
    {
        GameManager.OnGameStart += InitializeWaves;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= InitializeWaves;
    }

    private void Awake()
    {
        InitializePool();
        UpdateEnemyCount();
    }

    private void InitializeWaves() => StartNextWave();

    private void Update()
    {
        //Debug.Log(enemysAlive);

        if (enemysAlive <= 0 && waveStarted && !waveCompleted)
        {
            waveCompleted = true;
            waveStarted = false;
            waveCount++;

            OnWaveCompleted?.Invoke();

            if (infiniteWaves)
            {
                UpdateEnemyCount();
            }
        }
    }

    public void StartNextWave()
    {
        waveCompleted = false;
        waveStarted = true;
        StartCoroutine(SpawnEnemys());
    }

    private IEnumerator SpawnEnemys()
    {
        enemysAlive = enemyCount;
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemy()
    {
        if (GameManager.isGamePaused) return;

        GameObject enemyObj = GetPooledObject();

        if (enemyObj == null)
        {
            int enemyType = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            enemyObj = Instantiate(enemyPrefabs[enemyType]);
            pooledEnemys.Add(enemyObj);
        }

        int randomSpawnIndex = UnityEngine.Random.Range(0, spawnPositions.Length);
        enemyObj.transform.position = spawnPositions[randomSpawnIndex].position;
        enemyObj.transform.rotation = spawnPositions[randomSpawnIndex].rotation;
        
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemy.currentHealth = enemy.maxHealth;

        enemyObj.SetActive(true);
    }

    private void UpdateEnemyCount()
    {
        enemyCount = 5 + (waveCount * enemyMultiplayer);
    }

    public void EnemyDefeated() 
    {    
        enemysAlive--;
    }
        

    private void InitializePool()
    {
        for (int i = 0; i < amountPool; i++)
        {
            int enemyType = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            GameObject enemyObj = Instantiate(enemyPrefabs[enemyType]);
            enemyObj.SetActive(false);
            pooledEnemys.Add(enemyObj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledEnemys.Count; i++)
        {
            if (!pooledEnemys[i].activeInHierarchy)
                return pooledEnemys[i];
        }
        return null;
    }
}
