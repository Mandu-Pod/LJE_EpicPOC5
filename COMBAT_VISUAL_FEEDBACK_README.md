# 전투 시각 피드백 시스템 구현 완료

## 구현된 기능

### 1. 전투 실행 시퀀스 개선 ✅

**파일**: `CombatManager.cs`

- **이전**: 모든 유닛이 동시에 공격하고, 공격 범위가 계속 표시됨
- **변경**:
  - 전투 시작 시 모든 공격 범위 숨김
  - 각 유닛이 순서대로 공격 (코루틴 사용)
  - 공격하는 유닛의 공격 범위만 짧게 표시 (0.3초)
  - 공격 후 0.2초 대기하여 다음 유닛 공격

```csharp
private System.Collections.IEnumerator ExecuteCombatSequence()
{
    // 각 유닛이 순서대로 공격
    foreach (Unit unit in allUnits)
    {
        // 공격 범위 표시
        GameObject rangeIndicator = CreateAttackRangeIndicatorForUnit(unit);
        yield return new WaitForSeconds(0.3f);

        // 공격 실행
        unit.PerformAttack();

        // 공격 범위 제거
        Destroy(rangeIndicator);
        yield return new WaitForSeconds(0.2f);
    }
}
```

### 2. 원거리 적 공격 범위 개선 ✅

**파일**: `CombatManager.cs`

- **이전**: 항상 최대 사거리까지 화살 범위 표시
- **변경**: 전투 중에는 첫 번째 타겟까지만 화살 범위 표시
  - 누가 맞는지 명확하게 표시
  - 단일 대상 공격임을 시각적으로 확인 가능

```csharp
private GameObject CreateArrowIndicator(RangedEnemy enemy, bool showOnlyToTarget)
{
    float arrowLength = enemy.AttackRange;

    if (showOnlyToTarget)
    {
        Unit[] targets = enemy.FindTargetsInRange();
        if (targets.Length > 0)
        {
            Vector2 toTarget = targets[0].GetPosition() - enemy.GetPosition();
            arrowLength = toTarget.magnitude; // 타겟까지의 거리만 표시
        }
    }
    // ...
}
```

### 3. 유닛 피격 시각 효과 ✅

**파일**: `Unit.cs`

- 데미지를 받을 때 유닛이 빨간색으로 깜빡임
- 0.15초 동안 hitFlashColor (빨간색)로 변경 후 원래 색상으로 복원
- SpriteRenderer 색상 변경 사용

```csharp
protected System.Collections.IEnumerator HitFlashEffect()
{
    if (spriteRenderer != null)
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }
}
```

### 4. 유닛 사망 파티클 효과 ✅

**파일**: `Unit.cs`

- 유닛이 죽을 때 파티클 시스템으로 사망 효과 생성
- 유닛 타입에 따라 다른 색상의 파티클 (각 유닛의 attackRangeColor 사용)
- 20개의 파티클이 원형으로 퍼지면서 페이드 아웃
- 파티클 지속 시간: 0.5초

```csharp
protected void CreateDeathParticles()
{
    // ParticleSystem 생성
    ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

    // 버스트로 20개 파티클 생성
    emission.SetBursts(new ParticleSystem.Burst[] {
        new ParticleSystem.Burst(0f, 20)
    });

    // 색상 그라데이션 (알파 페이드)
    colorOverLifetime.color = gradient;
}
```

### 5. 플레이어 피격 시 카메라 쉐이크 ✅

**파일**: `CameraShake.cs` (신규), `Player.cs`

- 싱글톤 패턴으로 카메라 쉐이크 매니저 생성
- 플레이어가 데미지를 받을 때 자동으로 카메라 흔들림
- 쉐이크 설정:
  - 지속 시간: 0.2초
  - 강도: 0.15
  - 랜덤한 방향으로 흔들림

```csharp
// Player.cs
public override void TakeDamage(int damage)
{
    base.TakeDamage(damage);
    CameraShake.Instance.ShakeDefault();
}

// CameraShake.cs
public void Shake(float duration, float magnitude)
{
    targetCamera.transform.localPosition =
        originalPosition + Random.insideUnitSphere * magnitude;
}
```

## 게임플레이 흐름 (업데이트)

1. **종이 접기 전**: 모든 유닛의 공격 범위 표시 (미리보기)
2. **종이 접기**: 유닛 위치 이동, 공격 범위 업데이트
3. **접기 확정**:
   - **모든 공격 범위 숨김**
   - 전투 시작
4. **전투 시퀀스**:
   - 플레이어 공격 → 공격 범위 표시 (0.3초) → 공격 실행 → 대기 (0.2초)
   - 적1 공격 → 공격 범위 표시 (0.3초) → 공격 실행 → 대기 (0.2초)
   - 적2 공격 → 공격 범위 표시 (0.3초) → 공격 실행 → 대기 (0.2초)
   - ...
5. **피격/사망 효과**:
   - 피격: 빨간색 깜빡임
   - 플레이어 피격: + 카메라 쉐이크
   - 사망: 파티클 효과 후 오브젝트 제거
6. **다음 라운드**: 적 추가 스폰, 공격 범위 다시 표시

## 시각적 피드백 효과

| 상황                     | 효과                    | 지속시간  |
| ------------------------ | ----------------------- | --------- |
| 공격 범위 표시 (전투 전) | 반투명 범위 표시        | 계속 표시 |
| 공격 실행 중             | 공격자의 범위만 표시    | 0.3초     |
| 원거리 적 공격           | 첫 타겟까지만 화살 표시 | 0.3초     |
| 유닛 피격                | 빨간색 깜빡임           | 0.15초    |
| 플레이어 피격            | 카메라 쉐이크           | 0.2초     |
| 유닛 사망                | 파티클 버스트           | 0.5초     |

## 주요 개선 사항

✅ **공격 순서 명확화**: 누가 언제 공격하는지 시각적으로 확인 가능  
✅ **피해 확인 용이**: 깜빡임 효과로 누가 맞았는지 즉시 확인  
✅ **사망 확인 용이**: 파티클 효과로 유닛 사망 명확히 표시  
✅ **플레이어 피드백 강화**: 카메라 쉐이크로 피격 감각 향상  
✅ **원거리 공격 명확화**: 화살이 누구를 맞추는지 시각적으로 표시

## 파일 변경 사항

### 수정된 파일

- `Assets/Scripts/Combat/CombatManager.cs`

  - ExecuteCombatSequence 코루틴 추가
  - CreateAttackRangeIndicatorForUnit 메서드 추가
  - CreateArrowIndicator에 showOnlyToTarget 파라미터 추가

- `Assets/Scripts/Combat/Unit.cs`

  - HitFlashEffect 코루틴 추가
  - CreateDeathParticles 메서드 추가
  - SpriteRenderer 및 색상 관리 추가

- `Assets/Scripts/Combat/Player.cs`
  - TakeDamage 오버라이드하여 카메라 쉐이크 추가

### 새로 생성된 파일

- `Assets/Scripts/Camera/CameraShake.cs`
  - 싱글톤 카메라 쉐이크 매니저
  - Shake 및 ShakeDefault 메서드

## 테스트 방법

1. Unity에서 씬 실행
2. 종이를 접어 적과 플레이어 배치
3. 접기 확정 후 전투 관찰:
   - 각 유닛이 순서대로 공격하는지 확인
   - 공격 범위가 짧게 표시되는지 확인
   - 원거리 적의 화살이 타겟까지만 표시되는지 확인
4. 피격 효과 확인:
   - 맞은 유닛이 빨간색으로 깜빡이는지 확인
   - 플레이어가 맞을 때 카메라가 흔들리는지 확인
5. 사망 효과 확인:
   - 유닛이 죽을 때 파티클이 나오는지 확인
   - 파티클 색상이 유닛 타입에 맞는지 확인

## 추가 조정 가능한 파라미터

### Unit.cs

```csharp
[SerializeField] protected Color hitFlashColor = Color.red;
[SerializeField] protected float hitFlashDuration = 0.15f;
```

### CameraShake.cs

```csharp
public void ShakeDefault()
{
    Shake(0.2f, 0.15f); // duration, magnitude 조정 가능
}
```

### CombatManager.cs (ExecuteCombatSequence)

```csharp
yield return new WaitForSeconds(0.3f); // 공격 범위 표시 시간
yield return new WaitForSeconds(0.2f); // 다음 공격까지 대기 시간
```
