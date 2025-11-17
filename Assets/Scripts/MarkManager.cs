using UnityEngine;

public class MarkManager : SingletonObject<MarkManager>
{
    [SerializeField] private GameObject oMarkPrefab;
    [SerializeField] private GameObject xMarkPrefab;
    [SerializeField] private Transform markParentTransform;
    [SerializeField] private float markSize = 0.2f;
    [SerializeField] private int oMarkPerRound = 1;
    [SerializeField] private int xMarkPerRound = 2;

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
        for (int i = 0; i < oMarkPerRound; i++)
        {
            Vector2 randomPoint = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(oMarkPrefab, randomPoint);
        }
        for (int i = 0; i < xMarkPerRound; i++)
        {
            Vector2 randomPoint = PaperController.Instance.GetRandomInternalPoint();
            SpawnMark(xMarkPrefab, randomPoint);
        }
    }

    private void SpawnMark(GameObject markPrefab, Vector2 position)
    {
        GameObject mark = Instantiate(markPrefab, markParentTransform);
        mark.transform.localPosition = new Vector3(position.x, position.y, 0f);
        mark.transform.localScale = Vector3.one * markSize;
    }
}
