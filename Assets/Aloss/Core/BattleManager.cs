using UnityEngine;
using TMPro;

public enum BattleState
{
    Fear = 0,        // 공포
    Shaken = 1,      // 동요
    Distracted = 2,  // 산만
    Calm = 3,        // 평정 (기본)
    Focused = 4,     // 집중
    Immersed = 5,    // 몰입
    Extreme = 6      // 극한
}

public class BattleManager : MonoBehaviour
{
    // ====== Battle State ======
    [Header("Battle State")]
    [SerializeField] private BattleState state = BattleState.Calm;

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
    [SerializeField] private int distance = 5; // 0~9

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

    // =========================================================
    // ================== UI ===================================
    // =========================================================
    private void RefreshUI()
    {
        if (playerText)
            playerText.text = $"STATE {GetStateLabel(state)}\nHP {playerHP}/30\nSTA {stamina}/{maxStamina}";

        if (enemyText)
            enemyText.text = $"ENEMY HP\n{enemyHP}/30";

        if (distanceText)
            distanceText.text = $"DIST {distance}";
    }

    private static string GetStateLabel(BattleState s)
    {
        // 한글 표시(폰트는 이미 해결됐다고 봄)
        switch (s)
        {
            case BattleState.Fear: return "공포";
            case BattleState.Shaken: return "동요";
            case BattleState.Distracted: return "산만";
            case BattleState.Calm: return "평정";
            case BattleState.Focused: return "집중";
            case BattleState.Immersed: return "몰입";
            case BattleState.Extreme: return "극한";
            default: return s.ToString();
        }
    }

    private void ClampAll()
    {
        distance = Mathf.Clamp(distance, 0, 9);
        playerHP = Mathf.Clamp(playerHP, 0, 30);
        enemyHP = Mathf.Clamp(enemyHP, 0, 30);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    // =========================================================
    // ================== STATE MVP =============================
    // =========================================================
    private void ChangeState(int delta, string reason)
    {
        if (delta == 0) return;

        int before = (int)state;
        int after = Mathf.Clamp(before + delta, (int)BattleState.Fear, (int)BattleState.Extreme);
        if (after == before) return;

        state = (BattleState)after;
        Debug.Log($"STATE: {GetStateLabel((BattleState)before)} -> {GetStateLabel(state)} ({reason})");
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

        // 스태미나 체크 (staminaCost가 양수면 소모)
        if (skill.staminaCost > 0 && stamina < skill.staminaCost)
        {
            Debug.Log("Not enough stamina");
            return;
        }

        // 비용 적용 (음수면 회복)
        stamina -= skill.staminaCost;

        // 거리 배율(ideal+falloff, no cutoff)
        float distMult = skill.GetDistanceMultiplier(distance);

        bool didSomethingPositive = false; // 상태 +1 트리거용

        switch (skill.type)
        {
            case SkillType.Attack:
            {
                int raw = baseAttackDamage + skill.power + nextAttackBonus;
                int dmg = Mathf.Max(0, Mathf.RoundToInt(raw * distMult));

                nextAttackBonus = 0;
                enemyHP -= dmg;

                Debug.Log($"Player Attack! -{dmg} (mult {distMult:0.00})");

                if (dmg > 0)
                {
                    didSomethingPositive = true; // “공격 성공”
                }
                break;
            }

            case SkillType.Mobility:
            {
                int before = distance;
                distance += skill.distanceDelta;
                Debug.Log($"Move: dist {(skill.distanceDelta >= 0 ? "+" : "")}{skill.distanceDelta}");

                // 실제로 변화가 있었으면 성공
                if (before != distance) didSomethingPositive = true;
                break;
            }

            case SkillType.Guard:
            {
                nextDamageReduction = Mathf.Max(nextDamageReduction, 4);
                Debug.Log("Guard: next damage reduced");
                didSomethingPositive = true;
                break;
            }

            case SkillType.Control:
            {
                Debug.Log("Control: (TODO) implement");
                // 컨트롤은 아직이라 상태 변화는 보류
                break;
            }

            case SkillType.Focus:
            {
                int add = Mathf.Max(0, skill.nextAttackBonus);
                if (add <= 0) add = 4;
                nextAttackBonus += add;

                Debug.Log($"Focus: next attack boosted (+{add})");
                didSomethingPositive = true;
                break;
            }
        }

        // 플레이어 행동 결과로 상태 상승(MVP)
        if (didSomethingPositive)
            ChangeState(+1, $"player {skill.type}");

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
        // MVP: 턴 종료 시 스태미나 1 회복
        stamina = Mathf.Min(maxStamina, stamina + 1);

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
        // 멀면 따라오기
        if (distance >= 6)
        {
            distance -= 1;
            Debug.Log("Enemy moves closer (-1 dist)");
            return;
        }

        // 공격
        int dmg = 6;
        int reduced = Mathf.Min(dmg, nextDamageReduction);
        int finalDmg = dmg - reduced;
        nextDamageReduction = 0;

        playerHP -= finalDmg;
        Debug.Log($"Enemy Attack: -{finalDmg} (reduced {reduced})");

        // 맞았으면 상태 하락(MVP)
        if (finalDmg > 0)
            ChangeState(-1, "hit");
    }

    // =========================================================
    // ================== Tooltip ==============================
    // =========================================================
    public void ShowDamageTooltip(SkillData skill)
    {
        if (!tooltipText || skill == null) return;

        if (tooltipRoot)
            tooltipRoot.gameObject.SetActive(true);

        float mult = skill.GetDistanceMultiplier(distance);

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
