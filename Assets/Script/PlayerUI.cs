using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WaveManager waveManager;

    [Header("Text")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text waveText;

    [Header("Player Gauge")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider staminaBar;

    [Header("Boss Gauge")]
    [SerializeField] private Slider bossHPBar;
    private void Update()
    {
        // Lv
        levelText.text =
            "Lv : " + playerStats.Level;

        // åÇîjêî
        killCountText.text =
            "åÇîjêî : " + playerStats.KillCount;

        // WAVE
        waveText.text =
            "WAVE : " + waveManager.CurrentWave;

        // HP
        hpBar.maxValue =
            playerStats.MaxHP;

        hpBar.value =
            playerStats.CurrentHP;

        // Stamina
        staminaBar.maxValue =
            playerStats.MaxStamina;

        staminaBar.value =
            playerStats.CurrentStamina;

        //É{ÉXHPUI
        if (bossHPBar != null)
        {
            GameObject bossObject =
                GameObject.FindGameObjectWithTag("Boss");

            if (bossObject != null)
            {
                Boss boss =
                    bossObject.GetComponent<Boss>();

                if (boss != null)
                {
                    bossHPBar.gameObject.SetActive(true);

                    bossHPBar.maxValue =
                        boss.MaxHP;

                    bossHPBar.value =
                        boss.CurrentHP;
                }
            }
            else
            {
                bossHPBar.gameObject.SetActive(false);
            }
        }
    }
}