using TMPro;
using UnityEngine;

public class UIManager : SingletonObject<UIManager>
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        PaperController.OnPaperFolded += UpdateUI;
    }

    private void OnDisable()
    {
        PaperController.OnPaperFolded -= UpdateUI;
    }

    protected override void Awake()
    {
        base.Awake();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (GameManager.Instance == null)
            return;

        // 라운드 텍스트 업데이트: 현재 라운드 / 총 라운드
        if (roundText != null)
        {
            int currentRound = GameManager.Instance.GetCurrentRound();
            int totalRounds = GameManager.Instance.GetTotalRounds();
            roundText.text = $"현재 라운드: {currentRound} / {totalRounds}";
        }

        // 체력 텍스트 업데이트
        if (healthText != null)
        {
            int currentHealth = GameManager.Instance.GetEnemyCurrentHealth();
            int maxHealth = GameManager.Instance.GetEnemyMaxHealth();
            healthText.text = $"적 체력: {currentHealth} / {maxHealth}";
        }
    }
}