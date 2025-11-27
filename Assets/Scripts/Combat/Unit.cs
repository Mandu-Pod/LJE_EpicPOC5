using System;
using UnityEngine;

/// <summary>
/// 플레이어와 적의 공통 베이스 클래스
/// </summary>
public abstract class Unit : MonoBehaviour
{
    [Header("유닛 기본 정보")]
    [SerializeField] protected UnitData unitData;

    protected int currentHP;
    protected Vector2 originalPosition;
    protected bool isFlipped = false;

    public UnitType UnitType => unitData.unitType;
    public int CurrentHP => currentHP;
    public int MaxHP => unitData.maxHP;
    public int AttackPower => unitData.attackPower;
    public float AttackRange => unitData.attackRange;
    public AttackType AttackType => unitData.attackType;
    public Color AttackRangeColor => unitData.attackRangeColor;
    public bool IsAlive => currentHP > 0;
    public bool IsFlipped => isFlipped;
    public Vector2 OriginalPosition => originalPosition;

    public static event Action<Unit> OnUnitDeath;

    protected virtual void Awake()
    {
        if (unitData != null)
        {
            currentHP = unitData.maxHP;
        }
    }

    public virtual void Initialize(UnitData data)
    {
        unitData = data;
        currentHP = data.maxHP;
        originalPosition = transform.position;
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        currentHP -= damage;
        Debug.Log($"[유닛] {unitData.unitType} 데미지 {damage} 받음 (HP: {currentHP}/{unitData.maxHP})");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"[유닛] {unitData.unitType} 사망");
        OnUnitDeath?.Invoke(this);
    }

    /// <summary>
    /// 공격 범위 내의 타겟 찾기
    /// </summary>
    public abstract Unit[] FindTargetsInRange();

    /// <summary>
    /// 공격 실행
    /// </summary>
    public virtual void PerformAttack()
    {
        if (!IsAlive) return;

        Unit[] targets = FindTargetsInRange();

        foreach (Unit target in targets)
        {
            if (target != null && target.IsAlive)
            {
                Debug.Log($"[전투] {unitData.unitType}이(가) {target.UnitType}을(를) 공격!");
                target.TakeDamage(AttackPower);
            }
        }
    }

    /// <summary>
    /// 종이 접힐 때 반사 위치로 이동
    /// </summary>
    public virtual void MoveToFlippedPosition(Vector2 reflectedPosition)
    {
        if (!isFlipped)
        {
            originalPosition = transform.position;
        }
        isFlipped = true;
        transform.position = new Vector3(reflectedPosition.x, reflectedPosition.y, transform.position.z);
    }

    /// <summary>
    /// 접기 취소 시 원래 위치로 복원
    /// </summary>
    public virtual void RestoreOriginalPosition()
    {
        if (isFlipped)
        {
            transform.position = originalPosition;
            isFlipped = false;
        }
    }

    /// <summary>
    /// 접기 확정 시 위치 확정
    /// </summary>
    public virtual void ConfirmPosition()
    {
        originalPosition = transform.position;
        isFlipped = false;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }
}
