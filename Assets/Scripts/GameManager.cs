using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Variables")]
    [HideInInspector] public int playerScore = 0;

    [Header("StoreSettings")]
    public int[] upgradeCosts = new int[6];

    [Header("Upgrade Amounts")]
    public int healthUpgradeAmount = 10;
    public int shieldUpgradeAmount = 10;
    public int rocketUpgradeAmount = 1;
    public float shieldRegenRateUpgrade = 0.5f;

    [Header("References")]
    private UIManager uiManager;
    private Player player;

    public static event Action OnGameStart;
    public static bool isGamePaused = false;

    private void Awake()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        player = FindAnyObjectByType<Player>();
    }
    public void AddScore(int points)
    {
        playerScore += points;
    }

    public void StartGame() => OnGameStart?.Invoke();

    public bool PurchaseUpgrade(int upgradeIndex, int playerCoins)
    {
        if (upgradeIndex < 0 || upgradeIndex >= upgradeCosts.Length)
            return false;

        int cost = upgradeCosts[upgradeIndex];
        if (playerCoins < cost)
            return false;

        ApplyUpgrade(upgradeIndex);

        return true;
    }

    private void ApplyUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0: // Regen all health
                RegenAllHealth();
                break;
            case 1: // Increase maxHealth
                IncreaseMaxHealth();
                break;
            case 2: // Increase maxShield
                IncreaseMaxShield();
                break;
            case 3: // Regen all rockets
                RegenAllRockets();
                break;
            case 4: // Increase maxRockets
                IncreaseMaxRockets();
                break;
            case 5: // Increase shield regen rate
                IncreaseShieldRegenRate();
                break;
        }
    }

    private void RegenAllHealth() => player.RegenerateFullHealth();

    private void IncreaseMaxHealth() => player.IncreaseMaxHealth(healthUpgradeAmount);

    private void IncreaseMaxShield() => player.IncreaseMaxShield(shieldUpgradeAmount);

    private void RegenAllRockets() => player.ResetRocketCount();

    private void IncreaseMaxRockets() => player.IncreaseMaxRockets(rocketUpgradeAmount);

    private void IncreaseShieldRegenRate() => player.IncreaseShieldRegenRate(shieldRegenRateUpgrade);

}
