using System;
using System.Collections;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int damageAmount = 10;
    public float lifetime = 5f;
    [HideInInspector] public GameObject owner;

    public Action<GameObject, Vector3, int> OnHit;

    private void OnEnable()
    {
        StartCoroutine(DisableAfterTime());
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;

        OnHit?.Invoke(other.gameObject, transform.position, damageAmount);
        gameObject.SetActive(false);
    }

    private IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        this.gameObject.SetActive(false);
    }
}
