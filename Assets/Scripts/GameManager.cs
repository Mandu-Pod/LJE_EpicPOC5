using UnityEngine;

public class GameManager : SingletonObject<GameManager>
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private int enemyMaxHealth = 100;

    private int currentRound = 0;
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
        // Awake에서 한번 호출되므로 0라운드는 카운트 안함
        if (currentRound > 0)
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

    public int GetEnemyCurrentHealth()
    {
        return enemyCurrentHealth;
    }
}
