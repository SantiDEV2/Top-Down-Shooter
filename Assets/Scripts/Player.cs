using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Pawn
{
    [Header("Player Variables")]
    public int runSpeed = 4;

    [Header("Shield Settings")]
    public int maxShield = 100;
    [HideInInspector] public int currentShield;
    public float regenDelay = 5f;
    public int regenAmount = 10;
    private float lastDamageTime;
    private bool isRegenerating = false;

    [Header("Rocket Settings")]
    public GameObject rocketPrefab;
    public Transform rocketPoint;
    public int maxRockets = 3;
    public float rocketSpeed = 15f;
    public float rocketFireRate = 0.5f;
    [HideInInspector] private float nextRocketFireTime = 0f;
    private int currentRockets;

    [Header("Rocket Pool Settings")]
    private List<GameObject> rocketPool = new List<GameObject>();
    private int rocketPoolSize = 2;

    [Header("Weapon Models")]
    public GameObject aK;
    public GameObject rocketLauncher;

    public static event Action<int, int, int, int> OnStatsChanged;
    public static event Action OnGameOver;
    private bool gameStarted = false;

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
    }

    private void OnGameStart() => gameStarted = true;

    private void Start()
    {
        InitializeRocketPool();
        ResetRocketCount();
        currentShield = maxShield;
        lastDamageTime = -regenDelay;

        NotifyStatsChanged();
    }

    private void Update()
    {
        if(currentShield < maxShield && Time.time - lastDamageTime >= regenDelay && !isRegenerating)
        {
            StartCoroutine(RegenerateShield());
        }
    }

    private void FixedUpdate()
    {
        if (!gameStarted || GameManager.isGamePaused) return;

        //Player Movement
        Vector3 movementDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
        Movement(movementDir);

        //Player Rotation
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y - transform.position.y;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = worldPos - transform.position;
        direction.y = 0f;
        Rotation(direction);

        //Player Shooting Mecanic
        if (Input.GetMouseButton(0))
        {
            aK.SetActive(true);
            rocketLauncher.SetActive(false);
            Shoot();
        }
        if (Input.GetMouseButton(1)) 
        {
            aK.SetActive(false);
            rocketLauncher.SetActive(true);
            ShootRocket();
        }
    }

    private IEnumerator RegenerateShield()
    {
        isRegenerating = true;
        while (currentShield < maxShield)
        {
            int previousShield = currentShield;
            currentShield += Mathf.CeilToInt(regenAmount * Time.deltaTime);
            currentShield = Mathf.Min(currentShield, maxShield);
            if(previousShield != currentShield)
            {
                NotifyStatsChanged();
            }

            if(Time.time - lastDamageTime < regenDelay)
            {
                isRegenerating =false;
                yield break;
            }
            yield return null;
        }
        isRegenerating=false;
    }

    public override void OnDamage(int damage)
    {
        lastDamageTime = Time.time;

        if (currentShield > 0)
        {
            if (currentShield >= damage)
            {
                currentShield -= damage;
                NotifyStatsChanged();
                return;
            }
            else
            {
                int remainDamage = damage - currentShield;
                currentShield = 0;
                currentHealth -= remainDamage;
                if(currentHealth <= 0)
                {
                    currentHealth = 0;
                    Die();
                }
                NotifyStatsChanged();
                return;
            }
        }
        else 
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            NotifyStatsChanged();
        }
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke(currentHealth, maxHealth, currentShield, maxShield);
    }

    private void InitializeRocketPool()
    {
        for(int i = 0; i < rocketPoolSize; i++)
        {
            GameObject rocket = Instantiate(rocketPrefab);
            rocket.SetActive(false);
            rocketPool.Add(rocket);
        }
    }

    private GameObject GetPoolRocket()
    {
        for (int i = 0; i < rocketPool.Count; i++)
        {
            if (!rocketPool[i].activeInHierarchy)
                return rocketPool[i];
        }
        if (rocketPrefab != null)
        {
            GameObject rocket = Instantiate(rocketPrefab);
            rocket.SetActive(false);
            rocketPool.Add(rocket);
            return rocket;
        }
        return null;
    }

    public override void Movement(Vector3 direction)
    {
        int animState;
        if (direction.magnitude > 0)
        {
            animState = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
            speed = animState == 2 ? runSpeed : walkSpeed;
        }
        else
        {
            animState = 0;
        }

        animator.SetInteger("moveState", animState);
        base.Movement(direction);
    }

    public void ShootRocket()
    {
        if (Time.time < nextRocketFireTime || currentRockets <= 0) return;

        GameObject rocket = GetPoolRocket();
        if(rocket != null)
        {
            currentRockets--;
            rocket.transform.SetPositionAndRotation(rocketPoint.position, rocketPoint.rotation);
            rocket.SetActive(true);

            Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();
            rocketRb.linearVelocity = rocketPoint.forward * rocketSpeed;

            nextRocketFireTime = Time.time + (1f / rocketFireRate);

            Bullets rocketScript = rocket.GetComponent<Bullets>();
            rocketScript.owner = this.gameObject;
        }
    }

    public void ResetRocketCount()
    {
        currentRockets = maxRockets;
    }

    public void RegenerateFullHealth()
    {
        currentHealth = maxHealth;
        NotifyStatsChanged();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        NotifyStatsChanged();
    }

    public void IncreaseMaxShield(int amount)
    {
        maxShield += amount;
        NotifyStatsChanged();
    }

    public void IncreaseMaxRockets(int amount)
    {
        maxRockets += amount;
    }

    public void IncreaseShieldRegenRate(float amount)
    {
        regenAmount += Mathf.RoundToInt(amount * regenAmount);
    }

    protected override void Die()
    {
        OnGameOver?.Invoke();
    }
}
