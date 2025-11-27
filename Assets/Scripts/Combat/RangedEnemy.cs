using UnityEngine;

/// <summary>
/// 원거리 적 - 일직선 화살 공격 (종이 접기 전에 방향 고정)
/// </summary>
public class RangedEnemy : Unit
{
    [Header("화살 방향 (종이 접기 전 고정)")]
    [SerializeField] private Vector2 arrowDirection = Vector2.right;
    private bool isDirectionLocked = false;

    [Header("화살 설정")]
    [SerializeField] private float arrowWidth = 0.5f; // 화살 너비 (두께)

    protected override void Awake()
    {
        base.Awake();

        // 기본 원거리 적 데이터 설정
        if (unitData == null)
        {
            unitData = new UnitData
            {
                unitType = UnitType.RangedEnemy,
                maxHP = 30,
                attackPower = 10,
                attackRange = 3.5f,
                attackType = AttackType.Single, // 일직선 단일 대상
                attackRangeColor = new Color(1f, 0.5f, 0f, 0.3f) // 주황색
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
                arrowDirection = toPlayer;
                isDirectionLocked = true;
            }
        }
    }

    /// <summary>
    /// 일직선 상에서 가장 가까운 유닛 찾기
    /// </summary>
    public override Unit[] FindTargetsInRange()
    {
        // 방향이 고정되지 않았으면 플레이어 방향으로 설정
        if (!isDirectionLocked)
        {
            LockDirectionToPlayer();
        }

        // 모든 유닛 검색
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        Unit closestInLine = null;
        float closestDistance = float.MaxValue;

        Vector2 myPos = GetPosition();
        float halfWidth = arrowWidth * 0.5f;

        foreach (Unit unit in allUnits)
        {
            if (unit == this || !unit.IsAlive) continue;

            Vector2 toTarget = unit.GetPosition() - myPos;
            float distance = toTarget.magnitude;

            // 범위 내에 있는지 확인
            if (distance > AttackRange || distance < 0.1f) continue;

            // 직사각형 범위 판정
            // 1. 화살 방향으로의 거리 (전진 방향)
            float forwardDistance = Vector2.Dot(toTarget, arrowDirection);

            // 2. 화살 방향에 수직인 거리 (좌우 거리)
            Vector2 perpendicular = new Vector2(-arrowDirection.y, arrowDirection.x);
            float sidewaysDistance = Mathf.Abs(Vector2.Dot(toTarget, perpendicular));

            // 직사각형 범위 내에 있는지 확인
            // - 전진 방향: 0 ~ AttackRange
            // - 좌우 방향: -halfWidth ~ +halfWidth
            if (forwardDistance >= 0 && forwardDistance <= AttackRange && sidewaysDistance <= halfWidth)
            {
                // 가장 가까운 유닛만 저장 (처음 맞은 것)
                if (forwardDistance < closestDistance)
                {
                    closestInLine = unit;
                    closestDistance = forwardDistance;
                }
            }
        }

        if (closestInLine != null)
        {
            return new Unit[] { closestInLine };
        }

        return new Unit[0];
    }

    public Vector2 ArrowDirection => arrowDirection;
    public float ArrowWidth => arrowWidth; // 화살 두께 접근자 추가

    /// <summary>
    /// 종이 접기로 위치가 이동해도 방향은 유지
    /// </summary>
    public override void MoveToFlippedPosition(Vector2 reflectedPosition)
    {
        base.MoveToFlippedPosition(reflectedPosition);
        // 방향은 그대로 유지 (화살은 원래 조준한 방향으로 날아감)
    }

    /// <summary>
    /// 접기 확정 시에도 방향 유지
    /// </summary>
    public override void ConfirmPosition()
    {
        base.ConfirmPosition();
        // 방향은 계속 유지
    }
}
