using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private GameObject skillTreePanel;

    private SkillManager skillManager;

    private void Start()
    {
        skillManager =
            FindFirstObjectByType<SkillManager>();
    }

    public void OpenSkillTree()
    {
        skillTreePanel.SetActive(true);
    }

    public void CloseSkillTree()
    {
        skillTreePanel.SetActive(false);
    }

    public void ResetSkills()
    {
        if (skillManager == null)
        {
            return;
        }

        skillManager.ResetSkills();

        SkillButton[] skillButtons =
            FindObjectsByType<SkillButton>(
                FindObjectsSortMode.None
            );

        foreach (SkillButton skillButton in skillButtons)
        {
            skillButton.UpdateUI();
        }

        Debug.Log("スキルをリセットしました");
    }
}