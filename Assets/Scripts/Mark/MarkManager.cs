using System;
using System.Collections.Generic;
using UnityEngine;

public class MarkManager : SingletonObject<MarkManager>
{
    public static event Action OnResourceGenerated;

    [Header("프리팹")]
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject axePrefab;

    [Header("설정")]
    [SerializeField] private Transform markParentTransform;
    [SerializeField] private float markSize = 0.3f;
    [SerializeField] private float overlapDistance = 1f;
    [SerializeField] private int toolDurability = 3;

    [Header("레시피")]
    [SerializeField] private RecipeDatabase recipeDatabase;

    [Header("초기 마크 배치")]
    [SerializeField] private int initialTreeCount = 5;
    [SerializeField] private int initialAxeCount = 2;

    private List<Mark> allMarks = new List<Mark>();
    private bool isInitialized = false;

    private void OnEnable()
    {
        PaperController.OnPaperInitialized += OnPaperReady;
    }

    private void OnDisable()
    {
        PaperController.OnPaperInitialized -= OnPaperReady;
    }

    private void Start()
    {
        if (PaperController.Instance != null && PaperController.Instance.IsInitialized)
        {
            SpawnInitialMarks();
        }
    }

    private void OnPaperReady()
    {
        if (!isInitialized)
        {
            SpawnInitialMarks();
        }
    }

    private void SpawnInitialMarks()
    {
        if (isInitialized) return;
        isInitialized = true;

        for (int i = 0; i < initialTreeCount; i++)
        {
            Vector2 pos = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(treePrefab, pos, MarkType.Tree);
        }

        for (int i = 0; i < initialAxeCount; i++)
        {
            Vector2 pos = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(axePrefab, pos, MarkType.Axe);
        }
    }

    private void SpawnMark(GameObject prefab, Vector2 position, MarkType type)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject markObj = Instantiate(prefab, markParentTransform);
        markObj.transform.localPosition = new Vector3(position.x, position.y, 0f);
        markObj.transform.localScale = Vector3.one * markSize;

        Mark mark = markObj.GetComponent<Mark>();
        if (mark == null)
            mark = markObj.AddComponent<Mark>();

        int durability = type.IsTool() ? toolDurability : 0;
        mark.Initialize(type, durability);
        allMarks.Add(mark);
    }

    /// <summary>
    /// 드래그 중 실시간 업데이트
    /// </summary>
    public void UpdateMarkVisibility()
    {
        // 모든 아웃라인 초기화
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
                mark.HideOutline();
        }

        List<Mark> foldedMarks = new List<Mark>();    // 접는 선 넘어가서 반사 위치로 이동한 마크
        List<Mark> coveredMarks = new List<Mark>();   // 접힌 종이에 덮인 마크 (위치 유지)
        List<Mark> remainingMarks = new List<Mark>(); // 접히지 않은 영역의 마크

        foreach (Mark mark in allMarks)
        {
            if (mark == null) continue;

            Vector2 originalPos = mark.OriginalPosition;

            // 1. 접히는 원본 영역(polyB)에 있는지 체크
            bool isInFoldingSource = PaperController.Instance.IsPointInsideFoldingSource(originalPos);

            if (isInFoldingSource)
            {
                // 접는 선을 넘어간 마크 → 반사 위치로 이동
                Vector2 reflectedPos = PaperController.Instance.GetReflectedPositionRealtime(originalPos);
                mark.MoveToFlippedPosition(reflectedPos);
                foldedMarks.Add(mark);
            }
            else
            {
                // 접히지 않은 영역에 있음
                mark.RestoreOriginalPosition();

                // 2. 접힌 종이(flipedPolyB)에 덮이는지 체크
                bool isCovered = PaperController.Instance.IsPointInsideFlipedPolygons(originalPos);

                if (isCovered)
                    coveredMarks.Add(mark);
                else
                    remainingMarks.Add(mark);
            }
        }

        // 겹침 판정: 접혀서 이동한 마크 vs (덮인 마크 + 남은 마크)
        List<Mark> targetMarks = new List<Mark>();
        targetMarks.AddRange(coveredMarks);
        targetMarks.AddRange(remainingMarks);

        CheckOverlapsAndShowOutlines(foldedMarks, targetMarks);
    }


    /// <summary>
    /// 실시간 겹침 체크 및 아웃라인 표시 + 예상 자원 계산
    /// </summary>
    private void CheckOverlapsAndShowOutlines(List<Mark> foldedMarks, List<Mark> targetMarks)
    {
        Dictionary<MarkType, int> expectedResources = new Dictionary<MarkType, int>();
        List<Mark> processedMarks = new List<Mark>();

        foreach (Mark folded in foldedMarks)
        {
            if (folded == null || processedMarks.Contains(folded)) continue;

            // 이미 반사 위치로 이동했으므로 현재 위치 사용
            Vector2 foldedPos = folded.GetPosition();

            foreach (Mark target in targetMarks)
            {
                if (target == null || processedMarks.Contains(target)) continue;

                float distance = Vector2.Distance(foldedPos, target.GetPosition());

                if (distance <= overlapDistance)
                {
                    RecipeData recipe = recipeDatabase?.FindRecipe(folded.Type, target.Type);

                    if (recipe != null)
                    {
                        folded.ShowValidOutline();
                        target.ShowValidOutline();

                        // 예상 자원 추가
                        if (expectedResources.ContainsKey(recipe.result))
                            expectedResources[recipe.result]++;
                        else
                            expectedResources[recipe.result] = 1;

                        processedMarks.Add(folded);
                        processedMarks.Add(target);
                        break;
                    }
                    else
                    {
                        folded.ShowInvalidOutline();
                        target.ShowInvalidOutline();
                    }
                }
            }
        }

        // ExpectUI에 예상 자원 전달
        ExpectUI expectUI = FindFirstObjectByType<ExpectUI>();
        if (expectUI != null)
        {
            expectUI.UpdateExpectedResources(expectedResources);
        }
    }

    /// <summary>
    /// 드래그 취소 시 모든 마크 원래 위치로 복원
    /// </summary>
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

        // ExpectUI 초기화
        ExpectUI expectUI = FindFirstObjectByType<ExpectUI>();
        if (expectUI != null)
        {
            expectUI.ClearExpectedResources();
        }
    }

    /// <summary>
    /// 종이 접기 완료 시 호출
    /// </summary>
    public void ProcessFold()
    {
        List<Mark> flippedMarks = new List<Mark>();
        List<Mark> remainingMarks = new List<Mark>();

        // 현재 위치 기준으로 분류 (이미 이동된 상태)
        foreach (Mark mark in allMarks)
        {
            if (mark == null) continue;

            if (mark.IsFlipped)
                flippedMarks.Add(mark);
            else
                remainingMarks.Add(mark);
        }

        // 겹침 판정 및 조합 처리
        bool anyResourceGenerated = ProcessOverlaps(flippedMarks, remainingMarks);

        // 모든 마크 위치 확정 및 아웃라인 숨김
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
            {
                mark.ConfirmPosition();
                mark.SetActive(true);
            }
        }

        // ExpectUI 초기화
        ExpectUI expectUI = FindFirstObjectByType<ExpectUI>();
        if (expectUI != null)
        {
            expectUI.ClearExpectedResources();
        }

        if (anyResourceGenerated)
        {
            OnResourceGenerated?.Invoke();
            // 전투 시스템에서는 턴 시스템을 사용하지 않음
        }
    }

    private bool ProcessOverlaps(List<Mark> flippedMarks, List<Mark> remainingMarks)
    {
        bool anyGenerated = false;
        List<Mark> marksToRemove = new List<Mark>();

        foreach (Mark flipped in flippedMarks)
        {
            if (flipped == null || marksToRemove.Contains(flipped)) continue;

            foreach (Mark remaining in remainingMarks)
            {
                if (remaining == null || marksToRemove.Contains(remaining)) continue;

                // 현재 위치로 거리 계산 (이미 반사 위치로 이동됨)
                float distance = Vector2.Distance(flipped.GetPosition(), remaining.GetPosition());

                if (distance <= overlapDistance)
                {
                    RecipeData recipe = recipeDatabase?.FindRecipe(flipped.Type, remaining.Type);

                    if (recipe != null)
                    {
                        InventorySystem.Instance?.AddItem(recipe.result);
                        anyGenerated = true;

                        ProcessMarkAfterCombine(flipped, marksToRemove);
                        ProcessMarkAfterCombine(remaining, marksToRemove);

                        break;
                    }
                }
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
        if (mark.IsTool)
        {
            bool shouldDestroy = mark.UseTool();
            if (shouldDestroy)
            {
                marksToRemove.Add(mark);
            }
        }
        else
        {
            marksToRemove.Add(mark);
        }
    }

    public void ClearAllMarks()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark != null)
                Destroy(mark.gameObject);
        }
        allMarks.Clear();

        isInitialized = false;
    }
}