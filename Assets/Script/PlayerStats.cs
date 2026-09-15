using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHP = 50f;
    [SerializeField] private float attackPower = 50f;
    [SerializeField] private float maxStamina = 50f;

    [Header("Stamina")]
    [SerializeField] private float staminaRecovery = 10f;

    private float currentHP;
    private float currentStamina;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

    public float AttackPower => attackPower;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    private void Awake()
    {
        currentHP = maxHP;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        RecoverStamina();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
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

    private void Die()
    {
        Debug.Log("Player Dead");
    }
}