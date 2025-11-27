using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 근접 적 - 반원 범위 공격
/// </summary>
public class MeleeEnemy : Unit
{
    [Header("반원 방향")]
    [SerializeField] private Vector2 facingDirection = Vector2.right;
    private bool isDirectionLocked = false;

    protected override void Awake()
    {
        base.Awake();

        // 기본 근접 적 데이터 설정
        if (unitData == null)
        {
            unitData = new UnitData
            {
                unitType = UnitType.MeleeEnemy,
                maxHP = 50,
                attackPower = 15,
                attackRange = 1.5f,
                attackType = AttackType.SemiCircle,
                attackRangeColor = new Color(1f, 0f, 0f, 0.3f) // 빨간색
            };
            currentHP = unitData.maxHP;
        }
    }

    private void Start()
    {
        // 스폰 시 플레이어를 향해 방향 설정
        LockDirectionToPlayer();
    }

    /// <summary>
    /// 플레이어를 향해 방향 고정
    /// </summary>
    public void LockDirectionToPlayer()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null && player.IsAlive)
        {
            Vector2 toPlayer = (player.GetPosition() - GetPosition()).normalized;
            if (toPlayer != Vector2.zero)
            {
                facingDirection = toPlayer;
                isDirectionLocked = true;
                Debug.Log($"[MeleeEnemy] 공격 방향 고정: {facingDirection}");
            }
        }
    }

    /// <summary>
    /// 반원 범위 내의 모든 유닛 공격
    /// </summary>
    public override Unit[] FindTargetsInRange()
    {
        List<Unit> targets = new List<Unit>();

        // 모든 유닛 검색
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        // 방향이 고정되지 않았으면 플레이어 방향으로 설정
        if (!isDirectionLocked)
        {
            LockDirectionToPlayer();
        }

        foreach (Unit unit in allUnits)
        {
            if (unit == this || !unit.IsAlive) continue;

            Vector2 toTarget = unit.GetPosition() - GetPosition();
            float distance = toTarget.magnitude;

            // 범위 내에 있는지 확인
            if (distance <= AttackRange)
            {
                // 반원 판정: 방향 벡터와의 내적이 0 이상이면 앞쪽 반원
                Vector2 directionToTarget = toTarget.normalized;
                float dotProduct = Vector2.Dot(facingDirection.normalized, directionToTarget);

                if (dotProduct >= 0) // 180도 범위 (반원)
                {
                    targets.Add(unit);
                }
            }
        }

        return targets.ToArray();
    }

    public Vector2 FacingDirection => facingDirection;

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            facingDirection = direction.normalized;
        }
    }
}
