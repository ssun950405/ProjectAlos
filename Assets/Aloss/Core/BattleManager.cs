using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    // ====== FootBar ======
    [Header("FootBar - Player")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image staFill;

    [Header("FootBar - Enemy")]
    [SerializeField] private Image enemyHpFill;

    // ====== FX (Attack/Hit) ======
    [Header("FX - Attack/Hit")]
    [SerializeField] private AttackFX playerAttackFx;
    [SerializeField] private HitFX playerHitFx;
    [SerializeField] private AttackFX enemyAttackFx;
    [SerializeField] private HitFX enemyHitFx;

    // ====== FX (Slash) ======
    [Header("FX - Slash")]
    [SerializeField] private GameObject slashFxPrefab;
    [SerializeField] private Vector3 slashOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float slashSecondDelay = 0.03f; // 2연타 간격

    [Header("FX Targets")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform enemyTarget;

    // ====== Camera Shake ======
    [Header("FX - Camera Shake")]
    [SerializeField] private Camera mainCam;          // Inspector에서 Main Camera 드래그
    [SerializeField] private float shakeDuration = 0.05f;
    [SerializeField] private float shakeStrength = 0.08f;

    private Vector3 camBasePos;
    private float shakeTimeLeft = 0f;

    // ====== Stats ======
    [Header("Stats")]
    [SerializeField] private int playerHP = 30;
    [SerializeField] private int playerMaxHP = 30;
    [SerializeField] private int enemyHP = 30;
    [SerializeField] private int enemyMaxHP = 30;
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
        ClampAll();
        RefreshUI();
        HideTooltip();

        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null) camBasePos = mainCam.transform.localPosition;
    }

    private void Update()
    {
        // 카메라 흔들림 업데이트(아주 짧게)
        if (shakeTimeLeft > 0f && mainCam != null)
        {
            shakeTimeLeft -= Time.deltaTime;

            Vector2 r = Random.insideUnitCircle * shakeStrength;
            mainCam.transform.localPosition = camBasePos + new Vector3(r.x, r.y, 0f);

            if (shakeTimeLeft <= 0f)
            {
                mainCam.transform.localPosition = camBasePos;
            }
        }
    }

    // =========================================================
    // ================== UI ===================================
    // =========================================================
    private void RefreshUI()
    {
        if (playerText)
            playerText.text = $"STATE {GetStateLabel(state)}\nHP {playerHP}/{playerMaxHP}\nSTA {stamina}/{maxStamina}";

        if (enemyText)
            enemyText.text = $"ENEMY HP\n{enemyHP}/{enemyMaxHP}";

        if (distanceText)
            distanceText.text = $"DIST {distance}";

        if (hpFill)
            hpFill.fillAmount = Mathf.Clamp01((float)playerHP / Mathf.Max(1, playerMaxHP));

        if (staFill)
            staFill.fillAmount = Mathf.Clamp01((float)stamina / Mathf.Max(1, maxStamina));

        if (enemyHpFill)
            enemyHpFill.fillAmount = Mathf.Clamp01((float)enemyHP / Mathf.Max(1, enemyMaxHP));
    }

    private static string GetStateLabel(BattleState s)
    {
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

        playerMaxHP = Mathf.Max(1, playerMaxHP);
        enemyMaxHP = Mathf.Max(1, enemyMaxHP);

        playerHP = Mathf.Clamp(playerHP, 0, playerMaxHP);
        enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);

        maxStamina = Mathf.Max(1, maxStamina);
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
    // ================== FX helpers ============================
    // =========================================================
  // Invoke용 임시 타겟 저장
private Transform _slashSecondTarget;

private void StartCameraShake()
{
    if (mainCam == null) return;

    // 기준 위치 갱신(혹시 다른 연출로 움직였을 수 있으니)
    camBasePos = mainCam.transform.localPosition;

    shakeTimeLeft = Mathf.Max(shakeTimeLeft, shakeDuration);
}

// 슬래시가 "무조건 보이게" 강제 세팅
private void ForceSlashVisible(GameObject go)
{
    if (go == null) return;

    // 1) Z 고정 (2D에서는 이게 가장 안전)
    Vector3 p = go.transform.position;
    p.z = 0f;
    go.transform.position = p;

    // 2) SpriteRenderer 정렬 강제 (뒤로 묻히는 문제 방지)
    //    - Sorting Layer "FX"가 있으면 그걸 쓰고,
    //    - 없으면 Default로라도 올림
    var sr = go.GetComponentInChildren<SpriteRenderer>();
    if (sr != null)
    {
        // ⚠️ "FX" 레이어가 없으면 Default로 바꿔도 됨
        sr.sortingLayerName = "FX";
        sr.sortingOrder = 999;

        // 혹시 알파가 0으로 들어가면 강제 복구
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
    }
}

private void SpawnSlashFX(Transform target)
{
    if (!slashFxPrefab || !target) return;

    // 기본 위치: target + offset
    Vector3 pos = target.position + slashOffset;

    // 가능하면 몸통 중앙(bounds.center)로 보정
    var targetSr = target.GetComponentInChildren<SpriteRenderer>();
    if (targetSr != null)
        pos = targetSr.bounds.center + slashOffset;

    // ✅ Z는 고정해두자 (캐릭이 0.1이어도 상관없고, 슬래시는 0이 안전)
    pos.z = 0f;

    // 각도 랜덤폭 넓혀서 "촥촥" 느낌
    Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(-60f, 60f) + 45f);

    // Instantiate 후 강제 노출 세팅
    GameObject go;
    if (slashFxPrefab is GameObject prefabGo)
    {
        go = Instantiate(prefabGo, pos, rot);
    }
    else
    {
        // slashFxPrefab이 Component/Transform 타입이어도 대응
        var spawned = Instantiate(slashFxPrefab, pos, rot);
        go = spawned.gameObject;
    }

    ForceSlashVisible(go);
}

private void SpawnSlashFX_Double(Transform target)
{
    if (target == null) return;

    // 타겟 먼저 저장(Invoke 타이밍 안정)
    _slashSecondTarget = target;

    // 1타
    SpawnSlashFX(target);

    // 2타(살짝 딜레이)
    if (slashSecondDelay > 0f)
        Invoke(nameof(_SpawnSlashSecond), slashSecondDelay);
    else
        _SpawnSlashSecond();
}

private void _SpawnSlashSecond()
{
    if (_slashSecondTarget == null) return;
    SpawnSlashFX(_slashSecondTarget);
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

        if (skill.staminaCost > 0 && stamina < skill.staminaCost)
        {
            Debug.Log("Not enough stamina");
            return;
        }

        stamina -= skill.staminaCost;

        float distMult = skill.GetDistanceMultiplier(distance);

        bool didSomethingPositive = false;

        switch (skill.type)
        {
            case SkillType.Attack:
            {
                int raw = baseAttackDamage + skill.power + nextAttackBonus;
                int dmg = Mathf.Max(0, Mathf.RoundToInt(raw * distMult));

                // (1) 공격 모션: 항상
                if (playerAttackFx) playerAttackFx.Play(+1);

                nextAttackBonus = 0;
                enemyHP -= dmg;

                Debug.Log($"Player Attack! -{dmg} (mult {distMult:0.00})");

                // (2) 피격 FX + 슬래시(2연타) + 카메라 흔들림: 데미지 들어갔을 때만
                if (dmg > 0)
                {
                    didSomethingPositive = true;

                    if (enemyHitFx) enemyHitFx.Play();
                    SpawnSlashFX_Double(enemyTarget);
                    StartCameraShake();
                }

                break;
            }

            case SkillType.Mobility:
            {
                int before = distance;
                distance += skill.distanceDelta;
                Debug.Log($"Move: dist {(skill.distanceDelta >= 0 ? "+" : "")}{skill.distanceDelta}");

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

        if (didSomethingPositive)
            ChangeState(+1, $"player {skill.type}");

        ClampAll();

        // “행동 1회 끝” 회복: consumesTurn 상관없이 1 회복(MVP)
        stamina = Mathf.Min(maxStamina, stamina + 1);

        if (skill.consumesTurn)
            EndPlayerAction();
        else
            RefreshUI();
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

        // (적 공격 모션: 항상)
        if (enemyAttackFx) enemyAttackFx.Play(-1);

        int reduced = Mathf.Min(dmg, nextDamageReduction);
        int finalDmg = dmg - reduced;
        nextDamageReduction = 0;

        playerHP -= finalDmg;
        Debug.Log($"Enemy Attack: -{finalDmg} (reduced {reduced})");

        if (finalDmg > 0)
        {
            if (playerHitFx) playerHitFx.Play();
            SpawnSlashFX_Double(playerTarget);
            StartCameraShake();

            ChangeState(-1, "hit");
        }
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
