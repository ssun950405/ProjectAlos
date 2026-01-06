using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum TooltipType { None, DamageSkill }

    [Header("What to show")]
    public TooltipType tooltipType = TooltipType.None;
    public int damage = 0;

    [Header("Refs")]
    public BattleManager battleManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleManager == null) return;

        if (tooltipType == TooltipType.DamageSkill)
        {
            battleManager.SetTooltipPosition(transform as RectTransform);
            battleManager.ShowDamageTooltip(damage);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleManager == null) return;
        battleManager.HideTooltip();
    }
}

