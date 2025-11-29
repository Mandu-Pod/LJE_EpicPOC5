using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 시스템 관리자
/// </summary>
public class CombatManager : SingletonObject<CombatManager>
{
    public static event Action OnCombatStart;
    public static event Action OnCombatEnd;
    public static event Action OnPlayerDeath;

    [Header("프리팹")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;

    [Header("스폰 설정")]
    [SerializeField] private Transform unitsParent;
    [SerializeField] private float unitSize = 0.5f;
    [SerializeField] private float minSpawnDistance = 2f; // 플레이어로부터 최소 거리
    [SerializeField] private int enemiesPerFold = 2; // 종이 접을 때마다 생성되는 적 수
    [SerializeField] private int maxSpawnRounds = 5; // 적이 스폰되는 최대 라운드 수 (0부터 시작)

    [Header("공격 범위 시각화")]
    [SerializeField] private GameObject attackRangeIndicatorPrefab;

    [Header("전투 순서 설정")]
    [SerializeField] private bool playerAttacksFirst = true; // true: 플레이어 첫 번째, false: 플레이어 마지막

    private Player player;
    private List<Unit> allEnemies = new List<Unit>();
    private List<GameObject> attackRangeIndicators = new List<GameObject>();

    private int foldCount = 0;
    private bool isInCombat = false;
    private bool isShowingAttackRanges = false;
    private bool isGameStarted = false; // 게임 시작 플래그

    public Player Player => player;
    public bool IsInCombat => isInCombat;

    private void Update()
    {
        // R키로 씬 재시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    private void OnEnable()
    {
        PaperController.OnPaperInitialized += OnPaperReady;
        PaperController.OnPaperFolded += OnPaperFolded;
        Unit.OnUnitDeath += OnUnitDeath;
    }

    private void OnDisable()
    {
        PaperController.OnPaperInitialized -= OnPaperReady;
        PaperController.OnPaperFolded -= OnPaperFolded;
        Unit.OnUnitDeath -= OnUnitDeath;
    }

    private void OnPaperReady()
    {
        StartNewRound();
    }

    /// <summary>
    /// 새로운 라운드 시작
    /// </summary>
    public void StartNewRound()
    {
        // 기존 유닛들 정리
        ClearAllUnits();

        foldCount = 0;
        isInCombat = false;
        isGameStarted = true; // 게임 시작 플래그 설정

        // 플레이어 스폰
        SpawnPlayer();

        // 초기 적 스폰
        SpawnEnemies(enemiesPerFold);

        // 모든 적의 공격 방향을 플레이어로 재설정
        UpdateAllEnemyDirections();

        // 공격 범위 표시
        ShowAttackRanges();
    }

    /// <summary>
    /// 플레이어 스폰
    /// </summary>
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            return;
        }

        Vector2 spawnPos = PaperController.Instance.GetRandomInternalPoint();

        GameObject playerObj = Instantiate(playerPrefab, unitsParent);
        playerObj.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);
        playerObj.transform.localScale = Vector3.one * unitSize;

        player = playerObj.GetComponent<Player>();
        if (player == null)
            player = playerObj.AddComponent<Player>();

        // originalPosition 초기화
        player.ConfirmPosition();
    }

    /// <summary>
    /// 적들 스폰 (플레이어로부터 일정 거리 이상)
    /// </summary>
    private void SpawnEnemies(int count)
    {
        if (player == null) return;

        for (int i = 0; i < count; i++)
        {
            // 랜덤하게 근접/원거리 적 선택
            bool isMelee = UnityEngine.Random.value > 0.5f;
            GameObject enemyPrefab = isMelee ? meleeEnemyPrefab : rangedEnemyPrefab;

            if (enemyPrefab == null)
            {
                continue;
            }

            // 플레이어로부터 충분히 떨어진 위치 찾기
            Vector2 spawnPos = FindValidEnemySpawnPosition();

            GameObject enemyObj = Instantiate(enemyPrefab, unitsParent);
            enemyObj.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);
            enemyObj.transform.localScale = Vector3.one * unitSize;

            Unit enemy = enemyObj.GetComponent<Unit>();
            if (enemy == null)
            {
                enemy = isMelee ?
                    enemyObj.AddComponent<MeleeEnemy>() :
                    enemyObj.AddComponent<RangedEnemy>();
            }

            // originalPosition 초기화
            enemy.ConfirmPosition();

            // RangedEnemy의 경우 플레이어 방향으로 화살 방향 고정
            if (enemy is RangedEnemy rangedEnemy)
            {
                rangedEnemy.LockDirectionToPlayer();
            }

            allEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// 플레이어로부터 일정 거리 이상 떨어진 유효한 스폰 위치 찾기
    /// </summary>
    private Vector2 FindValidEnemySpawnPosition()
    {
        int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 pos = PaperController.Instance.GetRandomInternalPoint();

            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(pos, player.GetPosition());

                if (distanceToPlayer >= minSpawnDistance)
                {
                    return pos;
                }
            }
        }

        // 적절한 위치를 못 찾은 경우 랜덤 위치 반환
        return PaperController.Instance.GetRandomInternalPoint();
    }

    /// <summary>
    /// 공격 범위 시각화 표시
    /// </summary>
    public void ShowAttackRanges()
    {
        if (isShowingAttackRanges) return;

        HideAttackRanges();

        // 플레이어 공격 범위
        if (player != null && player.IsAlive)
        {
            CreateAttackRangeIndicator(player);
        }

        // 모든 적 공격 범위
        foreach (Unit enemy in allEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                CreateAttackRangeIndicator(enemy);
            }
        }

        isShowingAttackRanges = true;

        // 공격 순서 표시 업데이트
        UpdateAttackOrderDisplay();
    }

    /// <summary>
    /// 공격 범위 숨기기
    /// </summary>
    public void HideAttackRanges()
    {
        foreach (GameObject indicator in attackRangeIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }

        attackRangeIndicators.Clear();
        isShowingAttackRanges = false;
    }

    /// <summary>
    /// 공격 범위 표시 오브젝트 생성
    /// </summary>
    private void CreateAttackRangeIndicator(Unit unit)
    {
        GameObject indicator = null;

        // 유닛 타입에 따라 다른 표시기 생성
        if (unit is Player)
        {
            // 플레이어: 원형 범위
            indicator = CreateCircleIndicator(unit);
        }
        else if (unit is MeleeEnemy meleeEnemy)
        {
            // 근접 적: 반원 범위
            indicator = CreateSemiCircleIndicator(meleeEnemy);
        }
        else if (unit is RangedEnemy rangedEnemy)
        {
            // 원거리 적: 일직선 화살 범위
            indicator = CreateArrowIndicator(rangedEnemy, false);
        }

        if (indicator != null)
        {
            attackRangeIndicators.Add(indicator);
        }
    }

    /// <summary>
    /// 전투 중 단일 유닛의 공격 범위 표시 (일회용)
    /// </summary>
    private GameObject CreateAttackRangeIndicatorForUnit(Unit unit)
    {
        GameObject indicator = null;

        if (unit is Player)
        {
            indicator = CreateCircleIndicator(unit);
        }
        else if (unit is MeleeEnemy meleeEnemy)
        {
            indicator = CreateSemiCircleIndicator(meleeEnemy);
        }
        else if (unit is RangedEnemy rangedEnemy)
        {
            // 원거리 적: 첫 번째 타겟까지만 표시
            indicator = CreateArrowIndicator(rangedEnemy, true);
        }

        return indicator;
    }

    /// <summary>
    /// 원형 범위 표시기 생성
    /// </summary>
    private GameObject CreateCircleIndicator(Unit unit)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = $"AttackRange_{unit.UnitType}";
        indicator.transform.position = new Vector3(unit.GetPosition().x, unit.GetPosition().y, -5f);
        indicator.transform.localScale = Vector3.one * unit.AttackRange * 2f;
        indicator.transform.SetParent(unit.transform);

        // 콜라이더 제거
        Destroy(indicator.GetComponent<Collider>());

        // 반투명 머티리얼 설정
        Renderer renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = unit.AttackRangeColor;
            renderer.material = mat;
            renderer.sortingOrder = 100; // 종이보다 위에 표시
        }

        return indicator;
    }

    /// <summary>
    /// 반원 범위 표시기 생성 (MeleeEnemy용)
    /// </summary>
    private GameObject CreateSemiCircleIndicator(MeleeEnemy enemy)
    {
        if (enemy == null) return null;

        GameObject indicator = new GameObject($"AttackRange_SemiCircle");
        indicator.transform.position = new Vector3(enemy.GetPosition().x, enemy.GetPosition().y, -5f);
        indicator.transform.SetParent(enemy.transform);

        MeshFilter meshFilter = indicator.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = indicator.AddComponent<MeshRenderer>();

        // 반원 메시 생성
        Mesh mesh = CreateSemiCircleMesh(enemy.AttackRange);
        meshFilter.mesh = mesh;

        // 머티리얼 설정
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = enemy.AttackRangeColor;
        meshRenderer.material = mat;
        meshRenderer.sortingOrder = 100; // 종이보다 위에 표시

        // 방향에 따라 회전 (반원이 0도에서 시작하므로 -90도 보정)
        Vector2 facingDir = enemy.FacingDirection;
        float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg - 90f;
        indicator.transform.localRotation = Quaternion.Euler(0, 0, angle);

        return indicator;
    }

    /// <summary>
    /// 반원 메시 생성
    /// </summary>
    private Mesh CreateSemiCircleMesh(float radius)
    {
        Mesh mesh = new Mesh();

        int segments = 20;
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // 중심점

        // 반원 vertices 생성 (0도에서 180도)
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI / segments; // 0 ~ π
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
        }

        // 삼각형 생성
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// 일직선 화살 범위 표시기 생성 (RangedEnemy용)
    /// </summary>
    private GameObject CreateArrowIndicator(RangedEnemy enemy, bool showOnlyToTarget)
    {
        if (enemy == null) return null;

        GameObject indicator = new GameObject($"AttackRange_Arrow");
        indicator.transform.position = new Vector3(enemy.GetPosition().x, enemy.GetPosition().y, -5f);
        indicator.transform.SetParent(enemy.transform);

        MeshFilter meshFilter = indicator.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = indicator.AddComponent<MeshRenderer>();

        float arrowLength = enemy.AttackRange;

        // 전투 중에는 첫 번째 타겟까지만 표시
        if (showOnlyToTarget)
        {
            Unit[] targets = enemy.FindTargetsInRange();
            if (targets.Length > 0)
            {
                Vector2 toTarget = targets[0].GetPosition() - enemy.GetPosition();
                arrowLength = toTarget.magnitude;
            }
        }

        // 직사각형 화살 메시 생성
        Mesh mesh = CreateRectangleMesh(arrowLength, enemy.ArrowWidth);
        meshFilter.mesh = mesh;

        // 머티리얼 설정
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = enemy.AttackRangeColor;
        meshRenderer.material = mat;
        meshRenderer.sortingOrder = 100; // 종이보다 위에 표시

        // 화살 방향으로 회전
        Vector2 arrowDir = enemy.ArrowDirection;
        float angle = Mathf.Atan2(arrowDir.y, arrowDir.x) * Mathf.Rad2Deg;
        indicator.transform.localRotation = Quaternion.Euler(0, 0, angle);

        return indicator;
    }

    /// <summary>
    /// 직사각형 메시 생성 (화살 경로 표시용)
    /// </summary>
    private Mesh CreateRectangleMesh(float length, float width)
    {
        Mesh mesh = new Mesh();

        float halfWidth = width * 0.5f;

        // Vertices: 단순한 직사각형
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, -halfWidth, 0),        // 왼쪽 아래
            new Vector3(length, -halfWidth, 0),   // 오른쪽 아래
            new Vector3(length, halfWidth, 0),    // 오른쪽 위
            new Vector3(0, halfWidth, 0)          // 왼쪽 위
        };

        // Triangles (2개의 삼각형으로 사각형 구성)
        int[] triangles = new int[]
        {
            0, 1, 2,  // 첫 번째 삼각형
            0, 2, 3   // 두 번째 삼각형
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// 종이 접힌 후 호출
    /// </summary>
    private void OnPaperFolded()
    {
        // 게임이 시작되지 않았으면 무시
        if (!isGameStarted)
        {
            return;
        }

        foldCount++;

        // 공격 범위 및 순서 표시 숨김
        HideAttackRanges();
        HideAllAttackOrders();

        // 전투 실행 후 다음 라운드 준비 (즉시 실행)
        ExecuteCombatAndNextRound();
    }

    /// <summary>
    /// 모든 적의 공격 방향을 플레이어로 재설정
    /// </summary>
    private void UpdateAllEnemyDirections()
    {
        foreach (Unit enemy in allEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                if (enemy is MeleeEnemy meleeEnemy)
                {
                    meleeEnemy.LockDirectionToPlayer();
                }
                else if (enemy is RangedEnemy rangedEnemy)
                {
                    rangedEnemy.LockDirectionToPlayer();
                }
            }
        }
    }

    /// <summary>
    /// 전투 실행 후 다음 라운드 준비 (즉시 실행)
    /// </summary>
    private void ExecuteCombatAndNextRound()
    {
        // 코루틴으로 전투 실행
        StartCoroutine(ExecuteCombatSequence());
    }

    /// <summary>
    /// 전투 실행 코루틴 - 각 유닛의 공격을 순차적으로 표시
    /// </summary>
    private System.Collections.IEnumerator ExecuteCombatSequence()
    {
        isInCombat = true;
        OnCombatStart?.Invoke();

        // 공격 순서대로 유닛 리스트 생성
        List<Unit> allUnits = GetAttackOrderList();

        // 각 유닛이 순서대로 공격
        foreach (Unit unit in allUnits)
        {
            if (unit != null && unit.IsAlive)
            {
                // 이 유닛의 공격 범위만 표시
                GameObject rangeIndicator = CreateAttackRangeIndicatorForUnit(unit);

                yield return new WaitForSeconds(0.3f); // 공격 범위 표시 시간

                // 공격 실행
                unit.PerformAttack();

                // 공격 범위 제거
                if (rangeIndicator != null)
                    Destroy(rangeIndicator);

                yield return new WaitForSeconds(0.2f); // 다음 공격까지 대기
            }
        }

        // 죽은 적 제거
        allEnemies.RemoveAll(e => e == null || !e.IsAlive);

        // 전투 종료
        isInCombat = false;
        OnCombatEnd?.Invoke();

        // 플레이어가 살아있으면 다음 라운드 준비
        if (player != null && player.IsAlive)
        {
            // 적 추가 스폰 (최대 라운드 이내일 때만)
            if (foldCount <= maxSpawnRounds)
            {
                SpawnEnemies(enemiesPerFold);
            }

            // 모든 적(기존 + 새로운 적)의 방향을 플레이어 쪽으로 재설정
            UpdateAllEnemyDirections();

            // 공격 범위 표시
            ShowAttackRanges();

            // 공격 순서 업데이트
            UpdateAttackOrderDisplay();
        }
    }

    /// <summary>
    /// 공격 순서대로 유닛 리스트 반환
    /// </summary>
    private List<Unit> GetAttackOrderList()
    {
        List<Unit> orderedUnits = new List<Unit>();

        if (playerAttacksFirst)
        {
            // 플레이어가 먼저 공격
            if (player != null && player.IsAlive)
                orderedUnits.Add(player);

            foreach (Unit enemy in allEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                    orderedUnits.Add(enemy);
            }
        }
        else
        {
            // 적들이 먼저 공격, 플레이어는 마지막
            foreach (Unit enemy in allEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                    orderedUnits.Add(enemy);
            }

            if (player != null && player.IsAlive)
                orderedUnits.Add(player);
        }

        return orderedUnits;
    }

    /// <summary>
    /// 모든 유닛의 공격 순서 표시 업데이트
    /// </summary>
    private void UpdateAttackOrderDisplay()
    {
        List<Unit> orderedUnits = GetAttackOrderList();
        int enemyOrder = 1; // 적의 순서는 항상 1부터 시작

        for (int i = 0; i < orderedUnits.Count; i++)
        {
            if (orderedUnits[i] != null && orderedUnits[i].IsAlive)
            {
                // 적만 순서 표시 (플레이어는 건너뜀)
                if (!(orderedUnits[i] is Player))
                {
                    orderedUnits[i].SetAttackOrder(enemyOrder);
                    enemyOrder++;
                }
                else
                {
                    orderedUnits[i].SetAttackOrder(i + 1); // 플레이어는 전체 순서 전달
                }
            }
        }
    }

    /// <summary>
    /// 모든 유닛의 공격 순서 표시 숨김
    /// </summary>
    private void HideAllAttackOrders()
    {
        if (player != null)
        {
            player.HideAttackOrder();
        }

        foreach (Unit enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.HideAttackOrder();
            }
        }
    }

    /// <summary>
    /// 유닛 사망 처리
    /// </summary>
    private void OnUnitDeath(Unit unit)
    {
        if (unit == player)
        {
            OnPlayerDeath?.Invoke();
            HideAttackRanges();
        }
        else
        {
            allEnemies.Remove(unit);
        }

        // 유닛 오브젝트 제거
        if (unit != null && unit.gameObject != null)
        {
            Destroy(unit.gameObject, 0.5f);
        }
    }

    /// <summary>
    /// 모든 유닛 정리
    /// </summary>
    private void ClearAllUnits()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        foreach (Unit enemy in allEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        allEnemies.Clear();
        HideAttackRanges();
    }

    /// <summary>
    /// 드래그 중 유닛 위치 업데이트
    /// </summary>
    public void UpdateUnitPositions()
    {
        List<Unit> allUnits = new List<Unit>();
        if (player != null && player.IsAlive)
            allUnits.Add(player);

        // 살아있는 적만 추가
        foreach (Unit enemy in allEnemies)
        {
            if (enemy != null && enemy.IsAlive)
                allUnits.Add(enemy);
        }

        foreach (Unit unit in allUnits)
        {
            if (unit == null) continue;

            Vector2 originalPos = unit.OriginalPosition;

            // 접히는 영역에 있는지 체크
            bool isInFoldingSource = PaperController.Instance.IsPointInsideFoldingSource(originalPos);

            if (isInFoldingSource)
            {
                // 반사 위치로 이동
                Vector2 reflectedPos = PaperController.Instance.GetReflectedPositionRealtime(originalPos);
                unit.MoveToFlippedPosition(reflectedPos);
            }
            else
            {
                // 원래 위치로 복원
                unit.RestoreOriginalPosition();
            }
        }

        // 공격 범위 업데이트
        if (isShowingAttackRanges)
        {
            UpdateAttackRangePositions();
        }
    }

    /// <summary>
    /// 공격 범위 표시 위치 업데이트
    /// </summary>
    private void UpdateAttackRangePositions()
    {
        // 기존 표시기 제거 후 재생성
        HideAttackRanges();
        ShowAttackRanges();
    }

    /// <summary>
    /// 드래그 취소 시 유닛 위치 복원
    /// </summary>
    public void RestoreUnitPositions()
    {
        List<Unit> allUnits = new List<Unit>();
        if (player != null) allUnits.Add(player);
        allUnits.AddRange(allEnemies);

        foreach (Unit unit in allUnits)
        {
            if (unit != null)
            {
                unit.RestoreOriginalPosition();
            }
        }

        // 공격 범위 업데이트
        if (isShowingAttackRanges)
        {
            UpdateAttackRangePositions();
        }
    }

    /// <summary>
    /// 접기 확정 시 유닛 위치 확정
    /// </summary>
    public void ConfirmUnitPositions()
    {
        List<Unit> allUnits = new List<Unit>();
        if (player != null) allUnits.Add(player);
        allUnits.AddRange(allEnemies);

        foreach (Unit unit in allUnits)
        {
            if (unit != null)
            {
                unit.ConfirmPosition();
            }
        }
    }

    /// <summary>
    /// 씬 재시작
    /// </summary>
    private void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
