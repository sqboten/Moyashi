using UnityEngine;
using TMPro;

public class SkillPointUI : MonoBehaviour
{
    [SerializeField] private TMP_Text skillPointText;

    private SkillManager skillManager;

    private void Start()
    {
        skillManager =
            FindFirstObjectByType<SkillManager>();

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (skillManager == null)
        {
            return;
        }

        skillPointText.text =
            "Skill Point : " +
            skillManager.SkillPoint +
            " pt";
    }
}