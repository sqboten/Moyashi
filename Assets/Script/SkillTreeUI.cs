using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    [SerializeField] private GameObject skillTreePanel;

    public void OpenSkillTree()
    {
        skillTreePanel.SetActive(true);
    }

    public void CloseSkillTree()
    {
        skillTreePanel.SetActive(false);
    }
}