using System.Collections.Generic;
using UnityEngine;

public class MarkManager : SingletonObject<MarkManager>
{
    [SerializeField] private GameObject oMarkPrefab;
    [SerializeField] private GameObject xMarkPrefab;
    [SerializeField] private Transform markParentTransform;
    [SerializeField] private float markSize = 0.2f;
    [SerializeField] private int oMarkPerRound = 1;
    [SerializeField] private int xMarkPerRound = 2;

    private List<Mark> allMarks = new List<Mark>();

    private void OnEnable()
    {
        PaperController.OnPaperFolded += HandlePaperFolded;
    }

    private void OnDisable()
    {
        PaperController.OnPaperFolded -= HandlePaperFolded;
    }

    private void HandlePaperFolded()
    {
        // 겹친 마크 제거 및 개수 카운트
        (int removedO, int removedX) = RemoveOverlappingMarks();
        Debug.Log($"제거된 마크 - O: {removedO}, X: {removedX}");

        // 남은 O마크 개수 계산
        int remainingOMarks = GetRemainingOMarksCount();

        // 데미지 계산: 남은 O마크 * 제거된 X마크
        int damage = remainingOMarks * removedX;

        if (damage > 0)
        {
            GameManager.Instance?.TakeDamageToEnemy(damage);
        }

        // 새 마크 생성
        for (int i = 0; i < oMarkPerRound; i++)
        {
            Vector2 randomPoint = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(oMarkPrefab, randomPoint, MarkType.O);
        }
        for (int i = 0; i < xMarkPerRound; i++)
        {
            Vector2 randomPoint = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(xMarkPrefab, randomPoint, MarkType.X);
        }
    }

    private void SpawnMark(GameObject markPrefab, Vector2 position, MarkType type)
    {
        GameObject markObj = Instantiate(markPrefab, markParentTransform);
        markObj.transform.localPosition = new Vector3(position.x, position.y, 0f);
        markObj.transform.localScale = Vector3.one * markSize;

        Mark mark = markObj.GetComponent<Mark>();
        if (mark == null)
        {
            mark = markObj.AddComponent<Mark>();
        }
        mark.Initialize(type);
        allMarks.Add(mark);
    }

    /// <summary>
    /// 드래그 중 겹친 마크를 임시로 비활성화
    /// </summary>
    public void UpdateMarkVisibility()
    {
        foreach (Mark mark in allMarks)
        {
            if (mark == null) continue;

            Vector2 markPos = mark.GetPosition();
            bool isOverlapping = PaperController.Instance.IsPointInsideFlipedPolygons(markPos);

            mark.SetActive(!isOverlapping);
        }
    }

    /// <summary>
    /// 종이 접기 완료 시 겹친 마크를 영구 제거하고 개수 반환
    /// </summary>
    private (int removedO, int removedX) RemoveOverlappingMarks()
    {
        int removedO = 0;
        int removedX = 0;

        for (int i = allMarks.Count - 1; i >= 0; i--)
        {
            Mark mark = allMarks[i];
            if (mark == null)
            {
                allMarks.RemoveAt(i);
                continue;
            }

            Vector2 markPos = mark.GetPosition();
            bool isOverlapping = PaperController.Instance.IsPointInsideFlipedPolygons(markPos);

            if (isOverlapping)
            {
                if (mark.Type == MarkType.O)
                    removedO++;
                else if (mark.Type == MarkType.X)
                    removedX++;

                Destroy(mark.gameObject);
                allMarks.RemoveAt(i);
            }
            else
            {
                // 겹치지 않은 마크는 다시 활성화
                mark.SetActive(true);
            }
        }

        return (removedO, removedX);
    }

    /// <summary>
    /// 현재 남아있는 O마크 개수 반환
    /// </summary>
    private int GetRemainingOMarksCount()
    {
        int count = 0;
        foreach (Mark mark in allMarks)
        {
            if (mark != null && mark.Type == MarkType.O)
            {
                count++;
            }
        }
        return count;
    }
}
