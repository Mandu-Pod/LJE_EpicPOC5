using System;
using UnityEngine;

/// <summary>
/// 플레이어와 적의 공통 베이스 클래스
/// </summary>
public abstract class Unit : MonoBehaviour
{
    [Header("유닛 기본 정보")]
    [SerializeField] protected UnitData unitData;

    [Header("시각 효과 설정")]
    [SerializeField] protected Color hitFlashColor = Color.red;
    [SerializeField] protected float hitFlashDuration = 0.15f;

    protected int currentHP;
    protected Vector2 originalPosition;
    protected bool isFlipped = false;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

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

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public virtual void Initialize(UnitData data)
    {
        unitData = data;
        currentHP = data.maxHP;
        originalPosition = transform.position;

    }

    protected virtual void Start()
    {
        originalPosition = transform.position;

    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        currentHP -= damage;

        // 피격 효과
        StartCoroutine(HitFlashEffect());

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
        // 사망 파티클 효과
        CreateDeathParticles();

        OnUnitDeath?.Invoke(this);
    }

    /// <summary>
    /// 피격 시 깜빡임 효과
    /// </summary>
    protected System.Collections.IEnumerator HitFlashEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// 사망 파티클 생성
    /// </summary>
    protected void CreateDeathParticles()
    {
        GameObject particleObj = new GameObject("DeathParticles");
        particleObj.transform.position = transform.position;

        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        // 유닛 타입에 따라 색상 설정
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        Color particleColor = unitData != null ? unitData.attackRangeColor : Color.white;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(particleColor, 0f), new GradientColorKey(particleColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        ps.Play();

        // 파틴클 종료 후 제거
        Destroy(particleObj, main.startLifetime.constant + 0.1f);
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
                target.TakeDamage(AttackPower);
            }
        }
    }

    /// <summary>
    /// 종이 접힐 때 반사 위치로 이동
    /// </summary>
    public virtual void MoveToFlippedPosition(Vector2 reflectedPosition)
    {
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
