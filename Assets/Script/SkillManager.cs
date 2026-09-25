using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private const string SkillPointKey = "SkillPoint";

    private HashSet<string> unlockedSkills =
        new HashSet<string>();

    public int SkillPoint
    {
        get
        {
            return PlayerPrefs.GetInt(
                SkillPointKey,
                0
            );
        }
    }

    private void Awake()
    {
        LoadUnlockedSkills();
    }

    public bool IsUnlocked(string skillName)
    {
        if (unlockedSkills.Contains(skillName))
        {
            return true;
        }

        return PlayerPrefs.GetInt(
            "Skill_" + skillName,
            0
        ) == 1;
    }

    public bool TryUnlockSkill(
        string skillName,
        int requiredPoint)
    {
        if (IsUnlocked(skillName))
        {
            return false;
        }

        int currentPoint = SkillPoint;

        if (currentPoint < requiredPoint)
        {
            return false;
        }

        currentPoint -= requiredPoint;

        PlayerPrefs.SetInt(
            SkillPointKey,
            currentPoint
        );

        PlayerPrefs.SetInt(
            "Skill_" + skillName,
            1
        );

        PlayerPrefs.Save();

        unlockedSkills.Add(skillName);

        Debug.Log(
            "Skill Unlock : " + skillName
        );

        return true;
    }

    public void AddSkillPoint(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int currentPoint = SkillPoint;

        currentPoint += amount;

        PlayerPrefs.SetInt(
            SkillPointKey,
            currentPoint
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Skill Point : " + currentPoint
        );
    }

    private void LoadUnlockedSkills()
    {
        unlockedSkills.Clear();

        string[] skills =
        {
            "Dodge",
            "Parry",
            "ChargeAttack",
            "SpinAttack",
            "RangedAttack"
        };

        foreach (string skill in skills)
        {
            if (PlayerPrefs.GetInt(
                "Skill_" + skill,
                0
            ) == 1)
            {
                unlockedSkills.Add(skill);
            }
        }
    }
    public void ResetSkills()
    {
        string[] skills =
        {
        "Dodge",
        "Parry",
        "ChargeAttack",
        "SpinAttack",
        "RangedAttack"
    };

        int refundPoint = 0;

        SkillButton[] skillButtons =
            FindObjectsByType<SkillButton>(
                FindObjectsSortMode.None
            );

        foreach (string skill in skills)
        {
            if (!IsUnlocked(skill))
            {
                continue;
            }

            foreach (SkillButton skillButton in skillButtons)
            {
                if (skillButton.SkillName == skill)
                {
                    refundPoint += skillButton.RequiredPoint;
                    break;
                }
            }

            PlayerPrefs.SetInt(
                "Skill_" + skill,
                0
            );
        }

        int currentPoint = SkillPoint;

        currentPoint += refundPoint;

        PlayerPrefs.SetInt(
            SkillPointKey,
            currentPoint
        );

        PlayerPrefs.Save();

        // ÉÅÉÇÉäè„ÇÃâï˙èÛë‘Ç‡çXêV
        LoadUnlockedSkills();

        Debug.Log(
            "Skill Reset / Refund Point : " +
            refundPoint
        );
    }
}