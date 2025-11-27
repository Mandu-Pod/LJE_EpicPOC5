using UnityEngine;

public class GameManager : SingletonObject<GameManager>
{
    [Header("게임 상태")]
    [SerializeField] private bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    private void OnEnable()
    {
        // 전투 시스템으로 변경
        CombatManager.OnPlayerDeath += HandlePlayerDeath;
        PaperController.OnPaperTokenized += HandlePaperTokenized;
    }

    private void OnDisable()
    {
        CombatManager.OnPlayerDeath -= HandlePlayerDeath;
        PaperController.OnPaperTokenized -= HandlePaperTokenized;
    }

    private void HandlePlayerDeath()
    {
        isGameOver = true;
        // TODO: 게임 오버 UI 표시
    }

    private void HandlePaperTokenized()
    {
        // TODO: 새 종이 제공 또는 게임 종료 처리

        InventorySystem.Instance.AddItem(MarkType.Token, 1);
    }
}
