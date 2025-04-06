using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image lifeBar;
    public Image shieldBar;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundText;

    [Header("Panels")]
    public GameObject[] gamePanels; // 0 Menu // 1 Gameplay // 2 Shop // 3 Over

    [Header("Buttons")]
    public GameObject[] storeButtons;

    private EnemyWaveManager enemyWaveManager;
    private GameManager gameManager;
    private Player player;
    private int playerCoins = 0;

    private void OnEnable()
    {
        EnemyWaveManager.OnWaveCompleted += ShowStore;
        Player.OnGameOver += ShowGameOver;
        Player.OnStatsChanged += UpdateStatsUI;
        Coin.OnCoinCollected += UpdateCoinsUI;
    }

    private void OnDisable()
    {
        EnemyWaveManager.OnWaveCompleted -= ShowStore;
        Player.OnGameOver -= ShowGameOver;
        Player.OnStatsChanged -= UpdateStatsUI;
        Coin.OnCoinCollected -= UpdateCoinsUI;
    }

    private void Awake()
    {
        enemyWaveManager = FindAnyObjectByType<EnemyWaveManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        player = FindAnyObjectByType<Player>();
    }


    public void PurchaseUpgrade(int upgradeIndex)
    {
        if(gameManager.PurchaseUpgrade(upgradeIndex, playerCoins))
        {
            UpdateCoinsUI(-gameManager.upgradeCosts[upgradeIndex]);
            UpdateStoreButtonStates();
        }
    }

    public void UpdateStoreButtonStates()
    {
        for (int i = 0; i < storeButtons.Length && i < gameManager.upgradeCosts.Length; i++)
        {
            Button button = storeButtons[i].GetComponent<Button>();
            button.interactable = playerCoins >= gameManager.upgradeCosts[i];
        }
    }

    public void UpdateScoreUI()
    {
        scoreText.text = "Score: " + gameManager.playerScore.ToString();
        roundText.text = "Round: " + enemyWaveManager.waveCount.ToString();
    }

    private void ShowStore()
    {
        GameManager.isGamePaused = true;
        gamePanels[2].SetActive(true);
        gamePanels[1].SetActive(false);
        UpdateStoreButtonStates();
    }

    private void ShowGameOver()
    {
        GameManager.isGamePaused = true;
        gamePanels[1].SetActive(false);
        gamePanels[3].SetActive(true);
        UpdateScoreUI();
    }

    private void UpdateCoinsUI(int amount)
    {
        playerCoins += amount;
        coinText.text = playerCoins.ToString();
    }

    private void UpdateStatsUI(int currentHealth, int maxHealth, int currentShield, int maxShield)
    {
        lifeBar.fillAmount = (float)currentHealth / maxHealth;
        shieldBar.fillAmount = (float)currentShield / maxShield;
    }

    public void Continue()
    {
        GameManager.isGamePaused = false;
        enemyWaveManager.StartNextWave();
        gamePanels[2].SetActive(false);
        gamePanels[1].SetActive(true);
    }

    public void RestartScene()
    {
        GameManager.isGamePaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame() => Application.Quit();
}
