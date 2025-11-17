using UnityEngine;

public class GameManager : SingletonObject<GameManager>
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private int enemyMaxHealth = 100;

    private int currentRound = 1;  // 1라운드부터 시작
    private int enemyCurrentHealth;

    protected override void Awake()
    {
        base.Awake();
        enemyCurrentHealth = enemyMaxHealth;
    }

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
        // Awake에서 한번 호출되므로 1라운드가 아니면 로그 출력
        if (currentRound > 1)
        {
            Debug.Log($"=== 라운드 {currentRound}/{totalRounds} ===");
            Debug.Log($"남은 적 체력: {enemyCurrentHealth}/{enemyMaxHealth}");
        }

        currentRound++;
    }

    public void TakeDamageToEnemy(int damage)
    {
        enemyCurrentHealth -= damage;
        if (enemyCurrentHealth < 0)
            enemyCurrentHealth = 0;

        Debug.Log($"적에게 {damage} 데미지! 남은 적 체력: {enemyCurrentHealth}/{enemyMaxHealth}");

        // 데미지 후 UI 즉시 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }

        if (enemyCurrentHealth <= 0)
        {
            Debug.Log("적을 물리쳤습니다!");
        }
    }

    public int GetRemainingRounds()
    {
        return totalRounds - currentRound;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public int GetTotalRounds()
    {
        return totalRounds;
    }

    public int GetEnemyCurrentHealth()
    {
        return enemyCurrentHealth;
    }

    public int GetEnemyMaxHealth()
    {
        return enemyMaxHealth;
    }
}
