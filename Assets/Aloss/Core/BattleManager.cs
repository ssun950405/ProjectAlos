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

    // ====== Distance / Accuracy ======
    [Header("Distance / Accuracy")]
    [SerializeField] private int distance = 5;
    [SerializeField] private int idealDistance = 5;
    [SerializeField] private float maxHitChance = 0.90f;
    [SerializeField] private float penaltyPerStep = 0.10f;
    [SerializeField] private float minHitChance = 0.10f;

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
        Debug.Log($"UseSkill: {skill.skillName}");

        switch (skill.type)
        {
            case SkillType.Attack:
            {
                bool hit = RollHit();
                if (hit)
                {
                    int dmg = baseAttackDamage + skill.baseDamage + nextAttackBonus;
                    nextAttackBonus = 0;
                    enemyHP -= dmg;
                    Debug.Log($"Player Attack HIT! -{dmg}");
                }
                else
                {
                    Debug.Log("Player Attack MISS");
                }
                break;
            }

            case SkillType.Move:
            {
                distance += skill.distanceDelta;
                break;
            }

            case SkillType.Guard:
            {
                nextDamageReduction = Mathf.Max(nextDamageReduction, 4);
                Debug.Log("Guard: next damage reduced");
                break;
            }

            case SkillType.Buff:
            {
                nextAttackBonus += 4;
                Debug.Log("Buff: next attack boosted");
                break;
            }
        }

        EndPlayerAction();
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

    // ====== Accuracy ======
    private bool RollHit()
    {
        return Random.value < GetHitChance();
    }

    public float GetHitChance()
    {
        int diff = Mathf.Abs(distance - idealDistance);
        float chance = maxHitChance - penaltyPerStep * diff;
        return Mathf.Clamp(chance, minHitChance, maxHitChance);
    }

    // ====== Tooltip ======
    public void ShowDamageTooltip(int dmg)
    {
        if (!tooltipText) return;

        if (tooltipRoot)
            tooltipRoot.gameObject.SetActive(true);

        int pct = Mathf.RoundToInt(GetHitChance() * 100f);
        tooltipText.text = $"HIT {pct}%   DMG {dmg}";
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
