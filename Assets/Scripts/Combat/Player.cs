using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어 유닛 - 원형 범위 공격
/// </summary>
public class Player : Unit
{
    protected override void Awake()
    {
        base.Awake();

        // 기본 플레이어 데이터 설정
        if (unitData == null)
        {
            unitData = new UnitData
            {
                unitType = UnitType.Player,
                maxHP = 100,
                attackPower = 20,
                attackRange = 2f,
                attackType = AttackType.Circle,
                attackRangeColor = new Color(0f, 0.5f, 1f, 0.3f) // 파란색
            };
            currentHP = unitData.maxHP;
        }
    }

    /// <summary>
    /// 플레이어가 데미지를 받을 때 카메라 쉐이크 추가
    /// </summary>
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // 카메라 쉐이크 효과
        if (IsAlive || currentHP == 0) // 피해를 받았을 때
        {
            CameraShake.Instance.ShakeDefault();
        }
    }

    /// <summary>
    /// 원형 범위 내의 모든 적 찾기
    /// </summary>
    public override Unit[] FindTargetsInRange()
    {
        List<Unit> targets = new List<Unit>();

        // 모든 유닛 검색
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        foreach (Unit unit in allUnits)
        {
            if (unit == this || !unit.IsAlive) continue;

            float distance = Vector2.Distance(GetPosition(), unit.GetPosition());

            // 원형 범위 내에 있으면 타겟에 추가
            if (distance <= AttackRange)
            {
                targets.Add(unit);
            }
        }

        return targets.ToArray();
    }
}
