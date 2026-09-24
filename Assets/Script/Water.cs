using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] private float waterAmount = 1f;

    public float WaterAmount => waterAmount;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerStats playerStats =
            other.GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            return;
        }

        playerStats.AddWater(waterAmount);

        Destroy(gameObject);
    }
}