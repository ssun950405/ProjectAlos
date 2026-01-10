using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    // ====== UI ======
    [Header("UI")]
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text distanceText;

    // ====== Tooltip ======
    [Header("Tooltip")]
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private RectTransform tooltipRoot;

    // ====== Stats ======
    [Header("Stats")]
    [SerializeField] private int playerHP = 30;
    [SerializeField] private int enemyHP = 30;
    [SerializeField] private int stamina = 10;
    [SerializeField] private int maxStamina = 10;

    // ====== Distance ======
    [Header("Distance")]
    [SerializeField] private int distance = 5;

    // ====== Damage ======
    [Header("Damage")]
    [SerializeField] private int baseAttackDamage = 6;

    // ====== Temporary Buff States ======
    private int nextAttackBonus = 0;
    private int nextDamageReduction = 0;

    // ====== Lifecycle ======
    private void Start()
    {
        Debug.Log("Battle Start");
        RefreshUI();
        HideTooltip();
    }

    // ====== UI ======
    private void RefreshUI()
    {
        if (playerText)
            playerText.text = $"HP {playerHP}/30\nSTA {stamina}/{maxStamina}";

        if (enemyText)
            enemyText.text = $"ENEMY HP\n{enemyHP}/30";

        if (distanceText)
            distanceText.text = $"DIST {distance}";
    }

    private void ClampAll()
    {
        distance = Mathf.Clamp(distance, 0, 9);
        playerHP = Mathf.Clamp(playerHP, 0, 30);
        enemyHP = Mathf.Clamp(enemyHP, 0, 30);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    // =========================================================
    // ================== SKILL ENTRY POINT ====================
    // =========================================================
    public void UseSkill(SkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("UseSkill: skill is null");
            return;
        }

        Debug.Log($"UseSkill: {skill.displayName}");

        // 스태미나 체크 (staminaCost가 음수면 회복 스킬도 가능)
        if (skill.staminaCost > 0 && stamina < skill.staminaCost)
        {
            Debug.Log("Not enough stamina");
            return;
        }

        // 비용 적용
        stamina -= skill.staminaCost;

        // 거리 배율(ideal+falloff, no cutoff)
        float distMult = skill.GetDistanceMultiplier(distance);

        switch (skill.type)
        {
            case SkillType.Attack:
            {
                // 공격은 거리 배율로 "명중/피해" 둘 중 하나에 적용해야 함
                // 지금은 MVP로 피해에만 적용(명중 시스템은 나중에 정교화 가능)
                int raw = baseAttackDamage + skill.power + nextAttackBonus;
                int dmg = Mathf.Max(0, Mathf.RoundToInt(raw * distMult));

                nextAttackBonus = 0;
                enemyHP -= dmg;

                Debug.Log($"Player Attack! -{dmg} (mult {distMult:0.00})");
                break;
            }

            case SkillType.Mobility:
            {
                distance += skill.distanceDelta;
                Debug.Log($"Move: dist {(skill.distanceDelta >= 0 ? "+" : "")}{skill.distanceDelta}");
                break;
            }

            case SkillType.Guard:
            {
                // 예시: Guard는 고정 감쇄 4
                nextDamageReduction = Mathf.Max(nextDamageReduction, 4);
                Debug.Log("Guard: next damage reduced");
                break;
            }

            case SkillType.Control:
            {
                // 아직 컨트롤 효과 정의 안 했으니 MVP로 로그만
                Debug.Log("Control: (TODO) implement");
                break;
            }

            case SkillType.Focus:
            {
                // 보라색: 다음 공격 증폭 예시
                nextAttackBonus += Mathf.Max(0, skill.nextAttackBonus);
                if (skill.nextAttackBonus <= 0) nextAttackBonus += 4; // 에셋 값 없으면 기본 +4
                Debug.Log($"Focus: next attack boosted (+{nextAttackBonus})");
                break;
            }
        }

        // 턴 소모 여부 반영 (지금은 대부분 true일 거고, 나중에 기동/반격류를 false로 쓸 수 있음)
        if (skill.consumesTurn)
            EndPlayerAction();
        else
        {
            ClampAll();
            RefreshUI();
        }
    }

    // =========================================================
    // ================== TURN FLOW ============================
    // =========================================================
    private void EndPlayerAction()
    {
        ClampAll();
        RefreshUI();

        if (enemyHP <= 0)
        {
            Debug.Log("Enemy Down!");
            return;
        }

        EnemyAct();

        ClampAll();
        RefreshUI();

        if (playerHP <= 0)
            Debug.Log("Player Down!");
    }

    private void EnemyAct()
    {
        if (distance >= 6)
        {
            distance -= 1;
            Debug.Log("Enemy moves closer (-1 dist)");
            return;
        }

        int dmg = 6;
        int reduced = Mathf.Min(dmg, nextDamageReduction);
        int finalDmg = dmg - reduced;
        nextDamageReduction = 0;

        playerHP -= finalDmg;
        Debug.Log($"Enemy Attack: -{finalDmg} (reduced {reduced})");
    }

    // ====== Tooltip ======
    public void ShowDamageTooltip(SkillData skill)
    {
        if (!tooltipText || skill == null) return;

        if (tooltipRoot)
            tooltipRoot.gameObject.SetActive(true);

        float mult = skill.GetDistanceMultiplier(distance);

        // 공격툴팁: 예상 피해 표시(명중 확률은 아직 스킬데이터 기반으로 안 만듦)
        int raw = baseAttackDamage + skill.power + nextAttackBonus;
        int dmg = Mathf.Max(0, Mathf.RoundToInt(raw * mult));

        tooltipText.text = $"DIST x{mult:0.00}   DMG {dmg}";
    }

    public void HideTooltip()
    {
        if (tooltipText)
            tooltipText.text = "";

        if (tooltipRoot)
            tooltipRoot.gameObject.SetActive(false);
    }

    public void SetTooltipPosition(RectTransform targetBtn)
    {
        if (!tooltipRoot || !targetBtn) return;

        Vector3[] corners = new Vector3[4];
        targetBtn.GetWorldCorners(corners);
        Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;

        tooltipRoot.position = topCenter + new Vector3(0, 20f, 0);
    }
}
