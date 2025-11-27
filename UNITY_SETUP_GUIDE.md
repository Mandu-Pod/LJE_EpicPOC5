# 전투 시스템 Unity 설정 가이드

## 1. 프리팹 생성

### 플레이어 프리팹 만들기

1. Hierarchy에서 빈 GameObject 생성 → 이름: `Player`
2. `SpriteRenderer` 컴포넌트 추가
   - Sprite: 플레이어 이미지 할당
   - Sorting Order: 100
3. `Player` 스크립트 추가 (Assets/Scripts/Combat/Player.cs)
4. (선택) `UnitHealthBar` 스크립트 추가
5. Prefab으로 저장: `Assets/Prefabs/Combat/Player.prefab`

### 근접 적 프리팹 만들기

1. Hierarchy에서 빈 GameObject 생성 → 이름: `MeleeEnemy`
2. `SpriteRenderer` 컴포넌트 추가
   - Sprite: 근접 적 이미지 할당 (반원 범위 공격)
   - Sorting Order: 100
3. `MeleeEnemy` 스크립트 추가 (Assets/Scripts/Combat/Enemies.cs)
4. (선택) `UnitHealthBar` 스크립트 추가
5. Prefab으로 저장: `Assets/Prefabs/Combat/MeleeEnemy.prefab`

### 원거리 적 프리팹 만들기

1. Hierarchy에서 빈 GameObject 생성 → 이름: `RangedEnemy`
2. `SpriteRenderer` 컴포넌트 추가
   - Sprite: 원거리 적 이미지 할당 (화살 공격)
   - Sorting Order: 100
3. `RangedEnemy` 스크립트 추가 (Assets/Scripts/Combat/Enemies.cs)
4. (선택) `UnitHealthBar` 스크립트 추가
5. Prefab으로 저장: `Assets/Prefabs/Combat/RangedEnemy.prefab`

## 2. CombatManager 설정

### 씬에 추가

1. Hierarchy에서 빈 GameObject 생성 → 이름: `CombatManager`
2. `CombatManager` 스크립트 추가

### Inspector 설정

```
CombatManager 컴포넌트:

[프리팹]
- Player Prefab: Player.prefab 드래그
- Melee Enemy Prefab: MeleeEnemy.prefab 드래그
- Ranged Enemy Prefab: RangedEnemy.prefab 드래그

[스폰 설정]
- Units Parent: 유닛들을 담을 빈 GameObject (예: "Units" 오브젝트)
- Unit Size: 0.5 (유닛 표시 크기)
- Min Spawn Distance: 2.0 (플레이어로부터 최소 거리)
- Enemies Per Fold: 2 (접기당 생성되는 적 수)

[공격 범위 시각화]
- Attack Range Indicator Prefab: (비워둠 - 코드에서 자동 생성)
```

## 3. 기존 매니저 비활성화

### PopulationManager

- Inspector에서 체크박스 해제하여 비활성화
- 또는 게임오브젝트 자체를 비활성화

### TurnSystem

- Inspector에서 체크박스 해제하여 비활성화
- 또는 게임오브젝트 자체를 비활성화

### MarkManager (선택)

- 전투 시스템에서는 마크를 사용하지 않으므로 비활성화 가능
- 또는 그대로 두고 마크 생성만 안 되도록 설정

## 4. 씬 구조 (정확한 설정)

### 필수 GameObject들

```
Hierarchy:
├── Main Camera
│   └── Camera 컴포넌트 (Orthographic)
│
├── === 매니저들 ===
├── GameManager (빈 GameObject)
│   └── GameManager.cs 스크립트
│
├── CombatManager (빈 GameObject)
│   ├── CombatManager.cs 스크립트
│   └── Inspector 설정:
│       ├── Player Prefab: Player.prefab 할당
│       ├── Melee Enemy Prefab: MeleeEnemy.prefab 할당
│       ├── Ranged Enemy Prefab: RangedEnemy.prefab 할당
│       └── Units Parent: Units GameObject 할당
│
├── InventorySystem (빈 GameObject)
│   └── InventorySystem.cs 스크립트
│
├── MarkManager (빈 GameObject) [선택사항 - 비활성화 가능]
│   ├── MarkManager.cs 스크립트
│   └── Inspector 설정:
│       ├── Tree Prefab: (마크 사용시)
│       ├── Axe Prefab: (마크 사용시)
│       └── Recipe Database: (마크 사용시)
│
├── === 종이 시스템 ===
├── Paper (빈 GameObject)
│   ├── PaperController.cs 스크립트
│   ├── MeshTransform (자식 GameObject)
│   │   └── (종이 메시들이 여기에 동적 생성됨)
│   └── Inspector 설정:
│       ├── Paper Size: 5
│       ├── Paper Front Material: 밝은 색 머티리얼
│       ├── Paper Back Material: 어두운 색 머티리얼
│       ├── Mesh Transform: MeshTransform 할당
│       ├── Paper Front Color: (255, 255, 255)
│       └── Paper Back Color: (180, 180, 180)
│
├── === 유닛 컨테이너 ===
├── Units (빈 GameObject)
│   └── (플레이어와 적들이 런타임에 여기에 스폰됨)
│
├── === UI 시스템 ===
└── Canvas
    ├── Canvas 컴포넌트
    │   ├── Render Mode: Screen Space - Overlay
    │   └── Canvas Scaler 컴포넌트
    ├── UIManager.cs 스크립트
    │
    ├── GameOverPanel (빈 GameObject) [비활성화 상태로 시작]
    │   ├── Image 컴포넌트 (반투명 검은색 배경)
    │   └── GameOverText (TextMeshProUGUI)
    │       └── Text: "게임 오버"
    │
    ├── TokenizedPanel (빈 GameObject) [비활성화 상태로 시작]
    │   ├── Image 컴포넌트 (배경)
    │   └── Text (TextMeshProUGUI)
    │       └── Text: "종이가 토큰으로 변환되었습니다!"
    │
    ├── InventoryPanel (우측 상단)
    │   ├── RectTransform: Anchor (1, 1), Pivot (1, 1), Pos (-10, -10)
    │   ├── Image 컴포넌트 (배경)
    │   ├── InventoryUI.cs 스크립트
    │   └── InventoryText (TextMeshProUGUI)
    │       └── Text: "인벤토리\n(비어있음)"
    │
    ├── ExpectPanel (중앙 하단)
    │   ├── RectTransform: Anchor (0.5, 0), Pivot (0.5, 0)
    │   ├── Image 컴포넌트 (배경)
    │   ├── ExpectUI.cs 스크립트
    │   └── ExpectText (TextMeshProUGUI)
    │       └── Text: "예상 자원:"
    │
    └── CombatInfoPanel (좌측 상단) [선택사항]
        ├── RectTransform: Anchor (0, 1), Pivot (0, 1)
        ├── Image 컴포넌트 (배경)
        └── InfoText (TextMeshProUGUI)
            └── Text: "HP: 100\n적: 0"
```

### Inspector 연결 필수 사항

#### UIManager (Canvas에 붙음)

- `Game Over Panel`: GameOverPanel GameObject 할당
- `Game Over Text`: GameOverText (TextMeshProUGUI) 할당
- `Tokenized Panel`: TokenizedPanel GameObject 할당

#### InventoryUI (InventoryPanel에 붙음)

- `Inventory Text`: InventoryText (TextMeshProUGUI) 할당

#### ExpectUI (ExpectPanel에 붙음)

- `Expect Text`: ExpectText (TextMeshProUGUI) 할당

#### PaperController (Paper에 붙음)

- `Mesh Transform`: Paper → MeshTransform GameObject 할당
- `Paper Front Material`: 밝은 색 머티리얼
- `Paper Back Material`: 어두운 색 머티리얼

#### CombatManager

- `Player Prefab`: Assets/Prefabs/Combat/Player.prefab
- `Melee Enemy Prefab`: Assets/Prefabs/Combat/MeleeEnemy.prefab
- `Ranged Enemy Prefab`: Assets/Prefabs/Combat/RangedEnemy.prefab
- `Units Parent`: Hierarchy의 Units GameObject

## 5. 단계별 씬 설정 가이드

### Step 1: 매니저 오브젝트들 생성

1. **GameManager**

   - Hierarchy 우클릭 → Create Empty
   - 이름: `GameManager`
   - Add Component → `GameManager` 스크립트

2. **CombatManager**

   - Hierarchy 우클릭 → Create Empty
   - 이름: `CombatManager`
   - Add Component → `CombatManager` 스크립트
   - Inspector에서 프리팹들 할당 (나중에)

3. **InventorySystem**

   - Hierarchy 우클릭 → Create Empty
   - 이름: `InventorySystem`
   - Add Component → `InventorySystem` 스크립트

4. **MarkManager** (선택사항)
   - 비활성화해도 됨 (전투 시스템에서는 사용 안 함)

### Step 2: 종이 오브젝트 생성

1. **Paper 루트 생성**

   - Hierarchy 우클릭 → Create Empty
   - 이름: `Paper`
   - Add Component → `PaperController` 스크립트

2. **MeshTransform 자식 생성**

   - Paper 우클릭 → Create Empty
   - 이름: `MeshTransform`

3. **PaperController 설정**
   - Paper 선택
   - Inspector에서:
     - Paper Size: `5`
     - Mesh Transform: `MeshTransform` 드래그
     - Paper Front Color: 흰색 (255, 255, 255)
     - Paper Back Color: 회색 (180, 180, 180)
     - 머티리얼은 기본 Sprites/Default 사용

### Step 3: Units 컨테이너 생성

1. Hierarchy 우클릭 → Create Empty
2. 이름: `Units`
3. Position: (0, 0, 0)
4. 이 오브젝트는 비워둠 (런타임에 유닛들이 자동 생성됨)

### Step 4: UI 시스템 생성

1. **Canvas 생성**

   - Hierarchy 우클릭 → UI → Canvas
   - 이름: `Canvas`
   - Canvas Scaler 확인 (자동 생성됨)

2. **UIManager 추가**

   - Canvas 선택
   - Add Component → `UIManager` 스크립트

3. **GameOverPanel 생성**

   - Canvas 우클릭 → UI → Panel
   - 이름: `GameOverPanel`
   - Inspector에서 비활성화 (체크박스 해제)
   - Image 컴포넌트 색상: 검은색, Alpha 200
   - 자식으로 TextMeshProUGUI 추가:
     - 이름: `GameOverText`
     - Text: "게임 오버"
     - Font Size: 48
     - Alignment: Center

4. **TokenizedPanel 생성**

   - Canvas 우클릭 → UI → Panel
   - 이름: `TokenizedPanel`
   - Inspector에서 비활성화
   - 자식으로 TextMeshProUGUI 추가
     - Text: "종이가 토큰으로 변환되었습니다!"

5. **InventoryPanel 생성**

   - Canvas 우클릭 → Create Empty
   - 이름: `InventoryPanel`
   - RectTransform 설정:
     - Anchor: Right-Top (1, 1)
     - Pivot: (1, 1)
     - Pos X: -10, Pos Y: -10
     - Width: 200, Height: 150
   - Add Component → Image (배경)
   - Add Component → `InventoryUI` 스크립트
   - 자식으로 TextMeshProUGUI 추가:
     - 이름: `InventoryText`
     - Text: "인벤토리\n(비어있음)"

6. **ExpectPanel 생성**
   - Canvas 우클릭 → Create Empty
   - 이름: `ExpectPanel`
   - RectTransform 설정:
     - Anchor: Bottom-Center (0.5, 0)
     - Pivot: (0.5, 0)
     - Pos X: 0, Pos Y: 10
     - Width: 300, Height: 100
   - Add Component → Image (배경)
   - Add Component → `ExpectUI` 스크립트
   - 자식으로 TextMeshProUGUI 추가:
     - 이름: `ExpectText`
     - Text: "예상 자원:"

### Step 5: Inspector 연결

1. **UIManager (Canvas)**

   - Canvas 선택
   - UIManager 컴포넌트에서:
     - Game Over Panel → `GameOverPanel` 드래그
     - Game Over Text → `GameOverPanel/GameOverText` 드래그
     - Tokenized Panel → `TokenizedPanel` 드래그

2. **InventoryUI (InventoryPanel)**

   - InventoryPanel 선택
   - InventoryUI 컴포넌트에서:
     - Inventory Text → `InventoryText` 드래그

3. **ExpectUI (ExpectPanel)**

   - ExpectPanel 선택
   - ExpectUI 컴포넌트에서:
     - Expect Text → `ExpectText` 드래그

4. **CombatManager**
   - CombatManager 선택
   - CombatManager 컴포넌트에서:
     - Player Prefab → `Assets/Prefabs/Combat/Player.prefab` 드래그
     - Melee Enemy Prefab → `Assets/Prefabs/Combat/MeleeEnemy.prefab` 드래그
     - Ranged Enemy Prefab → `Assets/Prefabs/Combat/RangedEnemy.prefab` 드래그
     - Units Parent → Hierarchy의 `Units` GameObject 드래그
     - Unit Size: 0.5
     - Min Spawn Distance: 2.0
     - Enemies Per Fold: 2

### Step 6: 카메라 설정

1. Main Camera 선택
2. Camera 컴포넌트:
   - Projection: `Orthographic`
   - Size: `6` (종이가 잘 보이도록)
   - Position: (0, 0, -10)

완료! 이제 Play 버튼을 누르면 게임이 작동합니다.

## 6. 테스트 방법

1. Play 버튼 클릭
2. 종이가 생성되고 플레이어가 랜덤 위치에 스폰됨
3. 적 2마리가 플레이어로부터 떨어진 위치에 스폰됨
4. 모든 유닛의 공격 범위가 반투명 원/반원으로 표시됨
5. 마우스로 종이를 접음 (좌클릭 드래그)
   - 유닛들이 반사 위치로 이동하는 것을 확인
6. 첫 번째 접기 확정
   - 적 2마리 추가 스폰
   - 공격 범위 다시 표시
7. 두 번째 접기 확정
   - 전투 시작
   - 모든 유닛이 공격
   - 콘솔에서 전투 로그 확인

## 6. 테스트 방법

1. Play 버튼 클릭
2. 종이가 생성되고 플레이어가 랜덤 위치에 스폰됨
3. 마우스로 종이를 접음 (좌클릭 드래그)
   - 유닛들이 반사 위치로 이동하는 것을 확인
4. 접기 확정 (마우스 버튼 놓기)
   - 적 2마리 스폰
   - 공격 범위가 반투명 원/반원으로 표시됨
   - 자동으로 전투 시작
5. 전투 결과 확인
   - 콘솔에서 전투 로그 확인
   - 플레이어/적의 HP 변화 확인
6. 플레이어가 살아있으면 다시 종이 접기 반복

## 7. 디버깅 팁

### 콘솔 로그 확인

```
[전투] 새로운 라운드 시작
[전투] 플레이어 스폰: (x, y)
[전투] 근접 적 스폰: (x, y)
[전투] 원거리 적 스폰: (x, y)
[전투] 공격 범위 표시
[전투] 전투 시작!
[전투] Player이(가) MeleeEnemy을(를) 공격!
[유닛] MeleeEnemy 데미지 20 받음 (HP: 30/50)
...
```

### Scene 뷰에서 확인할 것

- 유닛들이 종이 위에 제대로 스폰되는지
- 공격 범위가 올바르게 표시되는지
- 종이를 접을 때 유닛들이 반사 위치로 이동하는지
- 반원 범위가 올바른 방향을 향하는지

## 7. 디버깅 팁

### 콘솔 로그 확인

```
[전투] 새로운 라운드 시작
[전투] 플레이어 스폰: (x, y)
[전투] 종이를 접으면 적이 스폰됩니다!
(종이 접기)
[전투] 근접 적 스폰: (x, y)
[전투] 원거리 적 스폰: (x, y)
[전투] 공격 범위 표시
[전투] 전투 시작!
[전투] Player이(가) MeleeEnemy을(를) 공격!
[유닛] MeleeEnemy 데미지 20 받음 (HP: 30/50)
...
[전투] 전투 종료! 남은 적: 1
```

### 흔한 문제들

1. **"NullReferenceException: PaperController.Instance"**

   - Paper GameObject가 씬에 있는지 확인
   - PaperController 스크립트가 붙어있는지 확인

2. **"플레이어/적이 스폰 안 됨"**

   - CombatManager의 Units Parent가 할당되었는지 확인
   - 프리팹들이 제대로 할당되었는지 확인

3. **"공격 범위가 안 보임"**

   - 카메라가 Orthographic인지 확인
   - 유닛들이 종이 위에 있는지 확인 (Z 좌표 0 근처)

4. **"UI가 안 보임"**
   - Canvas가 Screen Space - Overlay인지 확인
   - UI 오브젝트들이 활성화되어 있는지 확인
   - UIManager의 참조가 제대로 연결되었는지 확인

## 8. 커스터마이징

### 유닛 스탯 조정

`Player.cs`, `MeleeEnemy`, `RangedEnemy` 클래스의 Awake() 메서드에서:

```csharp
unitData = new UnitData
{
    unitType = UnitType.Player,
    maxHP = 100,        // ← HP 조정
    attackPower = 20,   // ← 공격력 조정
    attackRange = 2f,   // ← 범위 조정
    attackType = AttackType.Circle,
    attackRangeColor = new Color(0f, 0.5f, 1f, 0.3f) // ← 색상 조정
};
```

### 난이도 조정

`CombatManager.cs`의 Inspector에서:

- `Min Spawn Distance`: 더 크게 → 쉬움, 더 작게 → 어려움
- `Enemies Per Fold`: 더 많이 → 어려움, 더 적게 → 쉬움

### 공격 범위 색상

`UnitData.cs`에서 `attackRangeColor` 값 조정

## 8. 커스터마이징

### 유닛 스탯 조정

`Player.cs`, `MeleeEnemy`, `RangedEnemy` 클래스의 Awake() 메서드에서:

```csharp
unitData = new UnitData
{
    unitType = UnitType.Player,
    maxHP = 100,        // ← HP 조정
    attackPower = 20,   // ← 공격력 조정
    attackRange = 2f,   // ← 범위 조정
    attackType = AttackType.Circle,
    attackRangeColor = new Color(0f, 0.5f, 1f, 0.3f) // ← 색상 조정
};
```

### 난이도 조정

`CombatManager.cs`의 Inspector에서:

- `Min Spawn Distance`: 더 크게 → 쉬움, 더 작게 → 어려움
- `Enemies Per Fold`: 더 많이 → 어려움, 더 적게 → 쉬움

### 공격 범위 색상

`UnitData.cs`에서 `attackRangeColor` 값 조정

## 9. 추가 개선 아이디어

- **데미지 텍스트**: TextMeshPro로 데미지 숫자 표시
- **공격 이펙트**: 파티클 시스템 추가
- **사운드**: 공격/피격 효과음 추가
- **적 AI**: 더 똑똑한 이동 패턴
- **레벨 시스템**: 라운드가 진행될수록 적이 강해짐
- **파워업**: 종이 위에 특수 아이템 스폰

## 10. 최종 체크리스트

씬 설정 완료 전에 확인:

- [ ] Main Camera가 Orthographic으로 설정됨
- [ ] GameManager GameObject 존재
- [ ] CombatManager GameObject 존재 및 모든 참조 연결됨
- [ ] InventorySystem GameObject 존재
- [ ] Paper GameObject 존재 및 PaperController 설정 완료
- [ ] Units GameObject (빈 오브젝트) 존재
- [ ] Canvas 존재 및 UIManager 붙어있음
- [ ] GameOverPanel 생성 및 비활성화됨
- [ ] TokenizedPanel 생성 및 비활성화됨
- [ ] InventoryPanel 생성 및 InventoryUI 설정 완료
- [ ] ExpectPanel 생성 및 ExpectUI 설정 완료
- [ ] 플레이어 프리팹 생성 완료
- [ ] 근접 적 프리팹 생성 완료
- [ ] 원거리 적 프리팹 생성 완료
- [ ] CombatManager에 모든 프리팹 할당 완료
- [ ] Play 버튼으로 테스트 완료

모든 항목이 체크되었다면 게임을 즐기세요! 🎮
