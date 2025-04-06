using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public int value = 1;
    private float rotationSpeed = 90f;

    public static event Action<int> OnCoinCollected;

    private void Update()
    {
        if(GameManager.isGamePaused) return;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCoinCollected?.Invoke(value);
            gameObject.SetActive(false);
        }
    }
}
