using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Refs")]
    public SkillData skill;
    public BattleManager battleManager;

    public void OnClick()
    {
        if (skill == null || battleManager == null)
        {
            Debug.LogWarning("SkillButton: Missing reference");
            return;
        }

        battleManager.UseSkill(skill);
    }

    // ===== Tooltip =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skill == null || battleManager == null) return;

        battleManager.ShowDamageTooltip(skill);
        battleManager.SetTooltipPosition(transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleManager == null) return;

        battleManager.HideTooltip();
    }
}
