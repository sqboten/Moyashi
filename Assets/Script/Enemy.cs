using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHP = 100f;

    private float currentHP;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        currentHP -= damage;

        Debug.Log("Enemy Damage : " + damage);
        Debug.Log("Enemy HP : " + currentHP);

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy Dead");

        Destroy(gameObject);
    }
}