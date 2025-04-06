using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Pawn
{
    [Header("Enemy Variables")]
    public float detectionRange;
    public float timeBetweenShoots;
    private float nextShootAttempt;
    private Transform target;

    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public float dropChance = 0.5f;

    [Header("Coin Pool Settings")]
    private static List<GameObject> coinPool = new List<GameObject>();
    private static bool coinPoolInitialized = false;
    private static int coinPoolSize = 5;

    private void Start()
    {
        nextShootAttempt = Time.time + timeBetweenShoots;

        if(target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (!coinPoolInitialized && coinPrefab != null)
        {
            InitializeCoinPool();
        }
    }

    private void InitializeCoinPool()
    {
        for (int i = 0; i < coinPoolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coinPool.Add(coin);
        }
        coinPoolInitialized = true;
    }

    private GameObject GetPooledCoin()
    {
        for (int i = 0; i < coinPool.Count; i++)
        {
            if (!coinPool[i].activeInHierarchy)
                return coinPool[i];
        }

        if (coinPrefab != null)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coinPool.Add(coin);
            return coin;
        }
        return null;
    }

    private void FixedUpdate()
    {
        //Animation Controller
        int animState = 0;
        if (target != null)
        {
            //Movement
            Vector3 direction = target.position - transform.position;
            float distance = Vector3.Distance(transform.position, target.position);
            animState = 2;

            if(distance > detectionRange)
            {
                Movement(direction.normalized);
                animState = 2;
            }
            else if(distance < detectionRange - 0.1f)
            {
                Movement(-direction.normalized);
                animState = 2;
            }
            else
            {
                animState = 0;
            }

            //Rotation
            Rotation(direction);

            //Enemy Shooting Mechanic
            if (Time.time >= nextShootAttempt && distance <= detectionRange)
            {
                Shoot();
                animState = 0;
                nextShootAttempt = Time.time + timeBetweenShoots;
            }
        }
        animator.SetInteger("animState", animState);
    }

    protected override void Die()
    {
        if (UnityEngine.Random.value <= dropChance)
            DropCoin();

        GameManager gameManager = FindAnyObjectByType<GameManager>();
        gameManager.AddScore(10);

        gameObject.SetActive(false);
        FindAnyObjectByType<EnemyWaveManager>().EnemyDefeated();
    }

    private void DropCoin()
    {
        GameObject coin = GetPooledCoin();
        if (coin != null)
        {
            coin.transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
            coin.SetActive(true);
        }
    }

    [Obsolete]
    private void OnDestroy()
    {
        if (FindObjectsOfType<Pawn>().Length <= 1)
        {
            coinPoolInitialized = false;
            poolInitialized = false;
            sharedBulletPool.Clear();
            coinPool.Clear();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
