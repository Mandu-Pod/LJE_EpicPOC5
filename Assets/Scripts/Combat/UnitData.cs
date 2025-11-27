using UnityEngine;

/// <summary>
/// 유닛 타입 정의 (플레이어 및 적)
/// </summary>
public enum UnitType
{
    Player,
    MeleeEnemy,  // 반원 범위 공격 적
    RangedEnemy  // 일직선 화살 공격 적
}

/// <summary>
/// 공격 타입
/// </summary>
public enum AttackType
{
    Circle,      // 원형 범위 (플레이어)
    Single,      // 단일 대상 일직선 (원거리 적 - 화살)
    SemiCircle   // 반원 범위 (근접 적)
}

/// <summary>
/// 유닛 데이터 구조
/// </summary>
[System.Serializable]
public class UnitData
{
    public UnitType unitType;
    public int maxHP;
    public int attackPower;
    public float attackRange;
    public AttackType attackType;
    public Color attackRangeColor;
}
