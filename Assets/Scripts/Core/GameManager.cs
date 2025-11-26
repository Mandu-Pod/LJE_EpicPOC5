using UnityEngine;

public class GameManager : SingletonObject<GameManager>
{
    [Header("게임 상태")]
    [SerializeField] private bool isGameOver = false;
    
    public bool IsGameOver => isGameOver;
    
    private void OnEnable()
    {
        PopulationManager.OnGameOver += HandleGameOver;
        PaperController.OnPaperTokenized += HandlePaperTokenized;
    }
    
    private void OnDisable()
    {
        PopulationManager.OnGameOver -= HandleGameOver;
        PaperController.OnPaperTokenized -= HandlePaperTokenized;
    }
    
    private void HandleGameOver()
    {
        isGameOver = true;
        Debug.Log("========== 게임 오버 ==========");
        Debug.Log($"생존 일수: {TurnSystem.Instance?.CurrentDay}일");
    }
    
    private void HandlePaperTokenized()
    {
        Debug.Log("[게임] 종이가 토큰으로 변환되었습니다!");
        // TODO: 새 종이 제공 또는 게임 종료 처리
    }
}
