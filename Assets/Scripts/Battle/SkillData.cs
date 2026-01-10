using UnityEngine;
using UnityEngine.Serialization;

public enum SkillType
{
    Attack,
    Mobility,
    Guard,
    Control,
    Focus, // 보라색: 턴 소모 + 즉시 효과 + 다음 공격 증폭/상태 트리거
}

[CreateAssetMenu(menuName = "ProjectAlos/Skill Data", fileName = "SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Core")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;
    public SkillType type;

    [Header("Cost / Turn")]
    public int staminaCost = 0;          // +면 소모, -면 회복도 가능
    public bool consumesTurn = true;     // Focus(보라색)은 보통 true 고정 추천

    [Header("Distance Profile (No Cutoff)")]
    [Tooltip("이상 거리. 이 거리에서 효율/명중/계수 최대.")]
    public int idealDistance = 5;

    [Tooltip("ideal에서 멀어질수록 감소하는 정도(거리 1당 감소). 0.05면 1칸당 5% 감소.")]
    [Range(0f, 1f)]
    public float falloffPerStep = 0.08f;

    [Tooltip("효율 하한. 컷오프는 없지만 바닥은 필요(0이면 완전 무의미).")]
    [Range(0f, 1f)]
    public float minEffectMultiplier = 0.25f;

    [Header("Effect Numbers (Example)")]
    public int power = 10;               // 공격/효과 기본치(타입별 해석)
    public int distanceDelta = 0;        // 기동 스킬: 사용 시 보폭 변화(+) 멀어짐, (-) 가까워짐
    public int nextAttackBonus = 0;      // Focus류에서 "다음 공격 증폭" 값 등으로 사용

    // -------------------------
    // Legacy fields (깨짐 방지용)
    // -------------------------
    [Header("Legacy (Do not edit)")]
    [FormerlySerializedAs("minDistance")] [SerializeField] private int legacyMinDistance = 0;
    [FormerlySerializedAs("maxDistance")] [SerializeField] private int legacyMaxDistance = 0;
    [FormerlySerializedAs("accuracy")]    [SerializeField] private int legacyAccuracy = 0;
    [SerializeField] private bool legacyMigrated = false;

    /// <summary>
    /// 거리 기반 최종 배율(ideal에서 1.0, 멀어질수록 falloff, minEffectMultiplier까지 감소).
    /// 컷오프 없음.
    /// </summary>
    public float GetDistanceMultiplier(int currentDistance)
    {
        int delta = Mathf.Abs(currentDistance - idealDistance);
        float mult = 1f - (delta * falloffPerStep);
        return Mathf.Clamp(mult, minEffectMultiplier, 1f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 기존 에셋이 있다면, 구 필드를 신 필드로 1회만 옮겨서 “자동 생존”시키기
        if (legacyMigrated) return;

        // legacyMin/max가 의미있게 들어있던 프로젝트라면 ideal을 중간값으로 추정
        if (legacyMaxDistance > 0 || legacyMinDistance > 0)
        {
            int min = Mathf.Max(0, legacyMinDistance);
            int max = Mathf.Max(min, legacyMaxDistance);
            idealDistance = (min + max) / 2;
        }

        // legacyAccuracy 같은 게 있었다면 falloff로 대충 매핑(프로젝트 상황에 맞게 조정 가능)
        // 예: legacyAccuracy가 70~100 같은 형태였다면, 정확도가 낮을수록 falloff 증가
        if (legacyAccuracy > 0)
        {
            float acc01 = Mathf.Clamp01(legacyAccuracy / 100f);
            falloffPerStep = Mathf.Lerp(0.12f, 0.04f, acc01); // 정확도 높을수록 falloff 완만
            minEffectMultiplier = Mathf.Lerp(0.20f, 0.35f, acc01);
        }

        legacyMigrated = true;
    }
#endif
}
