using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private string skillName;
    [SerializeField] private int requiredPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text requiredPointText;
    [SerializeField] private Button button;

    private SkillManager skillManager;

    private void Start()
    {
        skillManager =
            FindFirstObjectByType<SkillManager>();

        UpdateUI();
    }

    public void Unlock()
    {
        if (skillManager == null)
        {
            return;
        }

        bool success =
            skillManager.TryUnlockSkill(
                skillName,
                requiredPoint
            );

        if (success)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (skillNameText != null)
        {
            skillNameText.text =
                skillName;
        }

        if (requiredPointText != null)
        {
            requiredPointText.text =
                requiredPoint + " pt";
        }

        if (skillManager == null)
        {
            return;
        }

        if (skillManager.IsUnlocked(skillName))
        {
            if (button != null)
            {
                button.interactable = false;
            }

            if (requiredPointText != null)
            {
                requiredPointText.text =
                    "âï˙çœÇ›";
            }
        }
    }
}