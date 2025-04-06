using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pawn: MonoBehaviour
{
    [Header("General Variables")]
    public float walkSpeed = 2f;
    protected float speed;

    [Header("Health Settings")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Functionability")]
    private float speedRotation = 8f;
    private Quaternion targetRotation;
    protected Animator animator;

    [Header("Shoot Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 1f;
    protected float nextFireTime = 0f;

    [Header("Pool Settings")]
    [HideInInspector] public static List<GameObject> sharedBulletPool = new List<GameObject>();
    [HideInInspector] public static bool poolInitialized = false;
    private int poolSize = 4;

    private void Awake()
    {
        speed = walkSpeed;
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        if (!poolInitialized && bulletPrefab != null)
        {
            InitializeSharedPool();
        }
    }

    private void InitializeSharedPool()
    {
        if(!poolInitialized && bulletPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject bullet = Instantiate(bulletPrefab);
                bullet.SetActive(false);
                sharedBulletPool.Add(bullet);
            }
            poolInitialized = true;
        }
    }

    protected GameObject GetPoolObject()
    {
        if(!poolInitialized && bulletPrefab != null)
        {
            InitializeSharedPool();
        }

        for (int i = 0; i < sharedBulletPool.Count; i++)
        {
            if (!sharedBulletPool[i].activeInHierarchy)
                return sharedBulletPool[i];
        }

        if(bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            sharedBulletPool.Add(bullet);
            return bullet;
        }
        return null;
    }

    public virtual void Movement(Vector3 direction)
    {
        if (GameManager.isGamePaused) return;

        Vector3 targetPosition = transform.position + speed * Time.deltaTime * direction;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.8f);
    }

    public virtual void Rotation(Vector3 direction)
    {
        if (GameManager.isGamePaused) return;

        if (direction != Vector3.zero)
        {
            direction.y = 0;
            direction = direction.normalized;
            targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotation * Time.deltaTime);
        }
    }

    public virtual void Shoot()
    {
        if (GameManager.isGamePaused || Time.time < nextFireTime) return;

        GameObject bullet = GetPoolObject();
        if (bullet != null)
        {
            bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
            bullet.SetActive(true);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
            
            nextFireTime = Time.time + (1f / fireRate);

            Bullets bulletsScript = bullet.GetComponent<Bullets>();

            bulletsScript.owner = this.gameObject;
            bulletsScript.OnHit = null;
            bulletsScript.OnHit += BulletHit;
        }
    }

    public virtual void BulletHit(GameObject hitObject, Vector3 hitPos, int damage) 
    {
        Pawn hitPawn = hitObject.GetComponent<Pawn>();
        if (hitPawn != null)
        {
            SkinnedMeshRenderer renderer = hitPawn.GetComponentInChildren<SkinnedMeshRenderer>();
            StartCoroutine(FlashColor(renderer));
            hitPawn.OnDamage(damage);
        }
    }

    private IEnumerator FlashColor(SkinnedMeshRenderer renderer)
    {
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(.15f);
        renderer.material.color = Color.white;
    }

    public virtual void OnDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {

    }
}
