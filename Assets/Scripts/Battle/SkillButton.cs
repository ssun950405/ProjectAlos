using UnityEngine;

public class SkillButton : MonoBehaviour
{
    public SkillData skill;
    public BattleManager battleManager;

    public void OnClick()
    {
        battleManager.UseSkill(skill);
    }
}
