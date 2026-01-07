using UnityEngine;

public enum SkillType
{
    Attack,
    Move,
    Guard,
    Buff,
    Special
}

[CreateAssetMenu(menuName = "Battle/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Info")]
    public string skillName;
    [TextArea] public string description;
    public SkillType type;

    [Header("Distance")]
    public int distanceDelta;   // +1, -1 등

    [Header("Cost / Power")]
    public int staminaCost;
    public int baseDamage;

    [Header("Accuracy")]
    public float accuracyBonus; // +0.1 같은 추가 보정
}
