using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text distanceText;

    [Header("Tooltip")]
    [SerializeField] private TMP_Text tooltipText;          // TooltipText(TMP)
    [SerializeField] private RectTransform tooltipRoot;     // TooltipBG(Image)의 RectTransform

    [Header("Stats")]
    [SerializeField] private int playerHP = 30;
    [SerializeField] private int enemyHP = 30;
    [SerializeField] private int stamina = 10;
    [SerializeField] private int maxStamina = 10;

    [Header("Distance / Accuracy")]
    [SerializeField] private int distance = 5;           
    [SerializeField] private int idealDistance = 5;      
    [SerializeField] private float maxHitChance = 0.90f; 
    [SerializeField] private float penaltyPerStep = 0.10f; 
    [SerializeField] private float minHitChance = 0.10f;   

    [Header("Damage")]
    [SerializeField] private int baseAttackDamage = 6;

    private int nextAttackBonus = 0;     
    private int nextDamageReduction = 0; 

    void Start()
    {
        Debug.Log("Battle Start");
        RefreshUI();
        HideTooltip();
    }

   private void RefreshUI()
{
    if (playerText)
        playerText.text = $"HP {playerHP}/30\nSTA {stamina}/{maxStamina}";

    if (enemyText)
        enemyText.text = $"ENEMY HP\n{enemyHP}/30";

    if (distanceText)
        distanceText.text = $"DIST {distance}";
}

    void ClampAll()
    {
        distance = Mathf.Clamp(distance, 0, 9);
        playerHP = Mathf.Clamp(playerHP, 0, 30);
        enemyHP = Mathf.Clamp(enemyHP, 0, 30);
    }

    // ====== Player Skills ======
    public void Skill_MovePlus1() { distance += 1; EndPlayerAction(); }
    public void Skill_MoveMinus1() { distance -= 1; EndPlayerAction(); }

    public void Skill_Attack()
    {
        bool hit = RollHit();

        if (hit)
        {
            int dmg = baseAttackDamage + nextAttackBonus;
            nextAttackBonus = 0;
            enemyHP -= dmg;
            Debug.Log($"Player Attack HIT! -{dmg} (dist {distance})");
        }
        else
        {
            Debug.Log($"Player Attack MISS (dist {distance})");
        }

        EndPlayerAction();
    }

    public void Skill_Backstep() { distance += 2; EndPlayerAction(); }

    public void Skill_Buff()
    {
        nextAttackBonus += 4;
        Debug.Log($"Buff: nextAttackBonus = {nextAttackBonus}");
        EndPlayerAction();
    }

    public void Skill_Guard()
    {
        nextDamageReduction = Mathf.Max(nextDamageReduction, 4);
        Debug.Log("Guard: next damage -4");
        EndPlayerAction();
    }

    // ====== Turn Flow ======
    void EndPlayerAction()
    {
        ClampAll();
        RefreshUI();

        if (enemyHP <= 0) { Debug.Log("Enemy Down!"); return; }

        EnemyAct();

        ClampAll();
        RefreshUI();

        if (playerHP <= 0) Debug.Log("Player Down!");
    }

    void EnemyAct()
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
    bool RollHit()
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

    // 배경 켜기
    if (tooltipRoot) tooltipRoot.gameObject.SetActive(true);

    int pct = Mathf.RoundToInt(GetHitChance() * 100f);
    tooltipText.text = $"HIT {pct}%   DMG {dmg}";
}

public void UseSkill(SkillData skill)
{
    Debug.Log($"UseSkill: {skill.skillName}");

    switch (skill.type)
    {
        case SkillType.Attack:
            enemyHP = Mathf.Max(0, enemyHP - skill.baseDamage);
            break;

        case SkillType.Move:
            distance += skill.distanceDelta;
            distance = Mathf.Clamp(distance, 0, 9);
            break;
    }

    RefreshUI();
}

public void HideTooltip()
{
    if (tooltipText) tooltipText.text = "";

    // 배경 끄기
    if (tooltipRoot) tooltipRoot.gameObject.SetActive(false);
}

    // 버튼 위로 툴팁 위치 이동
    public void SetTooltipPosition(RectTransform targetBtn)
    {
        if (!tooltipRoot || !targetBtn) return;

        Vector3[] corners = new Vector3[4];
        targetBtn.GetWorldCorners(corners);
        Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;

        tooltipRoot.position = topCenter + new Vector3(0, 20f, 0);
    }
}
