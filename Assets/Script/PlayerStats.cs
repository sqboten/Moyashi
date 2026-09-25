using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHP = 50f;
    [SerializeField] private float attackPower = 50f;
    [SerializeField] private float maxStamina = 50f;

    [Header("Stamina")]
    [SerializeField] private float staminaRecovery = 5f;

    [Header("Level")]
    [SerializeField] private int level = 1;
    [SerializeField] private int maxLevel = 80;
    [SerializeField] private float water = 0f;

    [Header("Kill Count")]
    private int killCount;
    public int KillCount => killCount;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverDisplayTime = 3f;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private float currentHP;
    private float currentStamina;

    private bool _isCharging;
    private bool _isDead;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

    public float AttackPower => attackPower;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    public float Water => water;

    public int Level => level;
    public int MaxLevel => maxLevel;

    private void Awake()
    {
        currentHP = maxHP;
        currentStamina = maxStamina;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        RecoverStamina();
    }

    public void SetCharging(bool isCharging)
    {
        _isCharging = isCharging;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        if (_isDead)
        {
            return;
        }

        currentHP -= damage;

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    public bool TryUseStamina(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (currentStamina < amount)
        {
            return false;
        }

        currentStamina -= amount;

        return true;
    }

    private void RecoverStamina()
    {
        if (_isDead)
        {
            return;
        }

        if (_isCharging)
        {
            return;
        }

        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            return;
        }

        currentStamina += staminaRecovery * Time.deltaTime;

        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
    }

    public void AddWater(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        water += amount;

        Debug.Log("Water : " + water);

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (level < maxLevel)
        {
            float requiredWater = GetRequiredWater();

            if (water < requiredWater)
            {
                break;
            }

            water -= requiredWater;

            LevelUp();
        }
    }

    private float GetRequiredWater()
    {
        int levelGroup = (level - 1) / 3;

        float requiredWater =
            25f * Mathf.Pow(1.5f, levelGroup);

        return Mathf.Ceil(requiredWater);
    }

    private void LevelUp()
    {
        level++;

        maxHP += 40f;
        attackPower += 30f;
        maxStamina += 30f;

        currentHP = maxHP;
        currentStamina = maxStamina;

        Debug.Log("Level Up!");
        Debug.Log("Level : " + level);
        Debug.Log("Max HP : " + maxHP);
        Debug.Log("Attack Power : " + attackPower);
        Debug.Log("Max Stamina : " + maxStamina);
        Debug.Log("Water : " + water);
    }

    public void AddKillCount()
    {
        killCount++;
    }

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        Debug.Log("Game Over");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(
            gameOverDisplayTime
        );

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }
}