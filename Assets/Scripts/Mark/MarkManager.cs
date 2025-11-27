using System;
using System.Collections.Generic;
using UnityEngine;

public class MarkManager : SingletonObject<MarkManager>
{
    public static event Action OnResourceGenerated;

    [Header("프리팹")]
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject personPrefab;

    [Header("설정")]
    [SerializeField] private Transform markParentTransform;
    [SerializeField] private float markSize = 0.3f;
    [SerializeField] private float overlapDistance = 1f;

    [Header("레시피")]
    [SerializeField] private RecipeDatabase recipeDatabase;

    private List<Mark> allMarks = new List<Mark>();
    private bool isInitialized = false;
    private int deployedPopulationCount = 0;  // 현재 종이에 배치된 인구 수

    private void OnEnable()
    {
        PopulationSelectUI.OnPopulationConfirmed += OnPopulationConfirmed;
    }

    private void OnDisable()
    {
        PopulationSelectUI.OnPopulationConfirmed -= OnPopulationConfirmed;
    }



    /// <summary>
    /// PaperType에 따라 초기 마크 배치
    /// </summary>
    public void SpawnInitialMarksForPaper(PaperType paperType, bool isFirstPaper = false)
    {
        if (isInitialized) return;
        isInitialized = true;

        deployedPopulationCount = 0;  // 인구는 나중에 배치

        switch (paperType)
        {
            case PaperType.Forest:
                // 숲 - 나무(3-5), 돌(1-2), 식량(0-1)
                SpawnRandomMarks(treePrefab, MarkType.Tree, 3, 5);
                SpawnRandomMarks(stonePrefab, MarkType.Stone, 1, 2);
                SpawnRandomMarks(foodPrefab, MarkType.Food, 0, 1);
                Debug.Log($"[마크] 숲 타입 마크 생성 완료");
                break;

            case PaperType.Plains:
                // 평지 - 나무(0-1), 돌(1-2), 식량(3-5)
                SpawnRandomMarks(treePrefab, MarkType.Tree, 0, 1);
                SpawnRandomMarks(stonePrefab, MarkType.Stone, 1, 2);
                SpawnRandomMarks(foodPrefab, MarkType.Food, 3, 5);
                Debug.Log($"[마크] 평지 타입 마크 생성 완료");
                break;
        }

        if (isFirstPaper)
        {
            Debug.Log("[마크] 첫 종이 - 자동으로 1명 배치");
            OnPopulationConfirmed(1);
        }
    }

    /// <summary>
    /// 인구 선택 확정 시 호출
    /// </summary>
    private void OnPopulationConfirmed(int count)
    {
        Debug.Log($"[마크] {count}명 배치 시작");

        // 전체 인구에서 차감
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.DeployPopulation(count);
        }

        // Person 마크 배치
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(personPrefab, pos, MarkType.Person);
            deployedPopulationCount++;
        }

        Debug.Log($"[마크] {count}명 배치 완료");

        // 마크 표시 및 접기 활성화
        PaperController.Instance?.ShowMarks();
    }

    /// <summary>
    /// 랜덤 개수로 마크 생성
    /// </summary>
    private void SpawnRandomMarks(GameObject prefab, MarkType type, int min, int max)
    {
        int count = UnityEngine.Random.Range(min, max + 1);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(prefab, pos, type);
        }
    }

    private void SpawnMark(GameObject prefab, Vector2 position, MarkType type)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[마크] {type} 프리팹이 없습니다.");
            return;
        }

        GameObject markObj = Instantiate(prefab, markParentTransform);
        markObj.transform.localPosition = new Vector3(position.x, position.y, 0f);
        markObj.transform.localScale = Vector3.one * markSize;

        Mark mark = markObj.GetComponent<Mark>();
        if (mark == null)
            mark = markObj.AddComponent<Mark>();

        mark.Initialize(type, 0);
        allMarks.Add(mark);
    }

    /// <summary>
    /// 모든 마크 숨기기
    /// </summary>
    public void HideAllMarks()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
                mark.SetActive(false);
        }
        Debug.Log("[마크] 모든 마크 숨김");
    }

    /// <summary>
    /// 모든 마크 보이기
    /// </summary>
    public void ShowAllMarks()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
                mark.SetActive(true);
        }
        Debug.Log("[마크] 모든 마크 표시");
    }

    /// <summary>
    /// 현재 종이에 배치된 인구 수 반환
    /// </summary>
    public int GetDeployedPopulationCount()
    {
        return deployedPopulationCount;
    }

    /// <summary>
    /// 드래그 중 실시간 업데이트
    /// </summary>
    public void UpdateMarkVisibility()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
                mark.HideOutline();
        }

        List<Mark> foldedMarks = new List<Mark>();
        List<Mark> coveredMarks = new List<Mark>();
        List<Mark> remainingMarks = new List<Mark>();

        foreach (Mark mark in allMarks)
        {
            if (mark == null) continue;

            Vector2 originalPos = mark.OriginalPosition;

            bool isInFoldingSource = PaperController.Instance.IsPointInsideFoldingSource(originalPos);

            if (isInFoldingSource)
            {
                Vector2 reflectedPos = PaperController.Instance.GetReflectedPositionRealtime(originalPos);
                mark.MoveToFlippedPosition(reflectedPos);
                foldedMarks.Add(mark);
            }
            else
            {
                mark.RestoreOriginalPosition();

                bool isCovered = PaperController.Instance.IsPointInsideFlipedPolygons(originalPos);

                if (isCovered)
                    coveredMarks.Add(mark);
                else
                    remainingMarks.Add(mark);
            }
        }

        List<Mark> targetMarks = new List<Mark>();
        targetMarks.AddRange(coveredMarks);
        targetMarks.AddRange(remainingMarks);

        CheckOverlapsAndShowOutlines(foldedMarks, targetMarks);
    }

    private void CheckOverlapsAndShowOutlines(List<Mark> foldedMarks, List<Mark> targetMarks)
    {
        Dictionary<MarkType, int> expectedResources = new Dictionary<MarkType, int>();
        List<Mark> processedMarks = new List<Mark>();

        foreach (Mark folded in foldedMarks)
        {
            if (folded == null || processedMarks.Contains(folded)) continue;

            Vector2 foldedPos = folded.GetPosition();

            // [수정] 가장 가까운 타겟을 찾기 위한 변수 초기화
            Mark closestTarget = null;
            float minDistance = float.MaxValue;
            RecipeData bestRecipe = null;

            foreach (Mark target in targetMarks)
            {
                if (target == null || processedMarks.Contains(target)) continue;

                float distance = Vector2.Distance(foldedPos, target.GetPosition());

                // 범위 안에 들어오고, 지금까지 찾은 것보다 더 가까운 경우 갱신
                if (distance <= overlapDistance && distance < minDistance)
                {
                    RecipeData recipe = recipeDatabase?.FindRecipe(folded.Type, target.Type);

                    // 레시피가 유효한 경우만 타겟 후보로 등록
                    if (recipe != null)
                    {
                        minDistance = distance;
                        closestTarget = target;
                        bestRecipe = recipe;
                    }
                    else
                    {
                        // 레시피가 없으면 빨간불(Invalid) 처리를 위해 일단 체크할 수도 있지만,
                        // 보통은 유효한 조합을 우선으로 찾으므로 여기선 패스하거나
                        // 별도 로직으로 '가까운데 조합 불가'를 띄울 수 있습니다.
                        folded.ShowInvalidOutline();
                        target.ShowInvalidOutline();
                    }
                }
            }

            // [수정] 가장 가까운 타겟이 선정되었다면 처리
            if (closestTarget != null && bestRecipe != null)
            {
                folded.ShowValidOutline();
                closestTarget.ShowValidOutline();

                if (expectedResources.ContainsKey(bestRecipe.result))
                    expectedResources[bestRecipe.result]++;
                else
                    expectedResources[bestRecipe.result] = 1;

                processedMarks.Add(folded);
                processedMarks.Add(closestTarget);
            }
        }

        // UI 업데이트
        ExpectUI expectUI = FindFirstObjectByType<ExpectUI>();
        if (expectUI != null)
        {
            expectUI.UpdateExpectedResources(expectedResources);
        }
    }
    public void RestoreMarkVisibility()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
            {
                mark.RestoreOriginalPosition();
                mark.SetActive(true);
            }
        }

        ExpectUI expectUI = FindFirstObjectByType<ExpectUI>();
        if (expectUI != null)
        {
            expectUI.ClearExpectedResources();
        }
    }

    public void ProcessFold()
    {
        List<Mark> flippedMarks = new List<Mark>();
        List<Mark> remainingMarks = new List<Mark>();

        foreach (Mark mark in allMarks)
        {
            if (mark == null) continue;

            if (mark.IsFlipped)
                flippedMarks.Add(mark);
            else
                remainingMarks.Add(mark);
        }

        Debug.Log($"[마크] 접힌 마크: {flippedMarks.Count}, 남은 마크: {remainingMarks.Count}");

        bool anyResourceGenerated = ProcessOverlaps(flippedMarks, remainingMarks);

        foreach (Mark mark in allMarks)
        {
            if (mark != null)
            {
                mark.ConfirmPosition();
                mark.SetActive(true);
            }
        }

        ExpectUI expectUI = FindFirstObjectByType<ExpectUI>();
        if (expectUI != null)
        {
            expectUI.ClearExpectedResources();
        }

        if (anyResourceGenerated)
        {
            OnResourceGenerated?.Invoke();
            TurnSystem.Instance?.RegisterEffectiveFold();
        }
    }

    private bool ProcessOverlaps(List<Mark> flippedMarks, List<Mark> remainingMarks)
    {
        bool anyGenerated = false;
        List<Mark> marksToRemove = new List<Mark>();

        foreach (Mark flipped in flippedMarks)
        {
            if (flipped == null || marksToRemove.Contains(flipped)) continue;

            // [수정] 가장 가까운 타겟 찾기 변수
            Mark closestTarget = null;
            float minDistance = float.MaxValue;
            RecipeData bestRecipe = null;

            foreach (Mark remaining in remainingMarks)
            {
                if (remaining == null || marksToRemove.Contains(remaining)) continue;

                float distance = Vector2.Distance(flipped.GetPosition(), remaining.GetPosition());

                // 더 가까운 유효 타겟 찾기
                if (distance <= overlapDistance && distance < minDistance)
                {
                    RecipeData recipe = recipeDatabase?.FindRecipe(flipped.Type, remaining.Type);
                    if (recipe != null)
                    {
                        minDistance = distance;
                        closestTarget = remaining;
                        bestRecipe = recipe;
                    }
                }
            }

            // [수정] 찾은 최적의 타겟으로 로직 수행
            if (closestTarget != null && bestRecipe != null)
            {
                Debug.Log($"[조합] {flipped.Type} + {closestTarget.Type} = {bestRecipe.result} (거리: {minDistance:F2})");

                InventorySystem.Instance?.AddItem(bestRecipe.result);
                anyGenerated = true;

                // 처리된 마크들은 제거 목록에 추가 (재사용 방지)
                ProcessMarkAfterCombine(flipped, marksToRemove);
                ProcessMarkAfterCombine(closestTarget, marksToRemove);

                // 여기서 marksToRemove에 추가되었으므로, 다음 loop에서 이 타겟은 무시됨
            }
        }

        foreach (Mark mark in marksToRemove)
        {
            allMarks.Remove(mark);
            Destroy(mark.gameObject);
        }

        return anyGenerated;
    }
    private void ProcessMarkAfterCombine(Mark mark, List<Mark> marksToRemove)
    {
        // Person 마크는 소모되지 않음 (유지)
        if (mark.Type == MarkType.Person)
        {
            Debug.Log($"[마크] {mark.Type} 유지됨");
            return;
        }

        // 자원은 소모됨
        marksToRemove.Add(mark);
    }

    public void ClearAllMarks()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
                Destroy(mark.gameObject);
        }
        allMarks.Clear();

        deployedPopulationCount = 0;
        isInitialized = false;
    }
}