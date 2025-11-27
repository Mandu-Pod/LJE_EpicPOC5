using TMPro;
using UnityEngine;

public class UIManager : SingletonObject<UIManager>
{
    [Header("게임 오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("토큰화 UI")]
    [SerializeField] private GameObject tokenizedPanel;

    private void OnEnable()
    {
        CombatManager.OnPlayerDeath += ShowGameOver;
        PaperController.OnPaperTokenized += ShowTokenized;
    }

    private void OnDisable()
    {
        CombatManager.OnPlayerDeath -= ShowGameOver;
        PaperController.OnPaperTokenized -= ShowTokenized;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (tokenizedPanel != null)
            tokenizedPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverText != null)
            {
                gameOverText.text = "게임 오버\n\n플레이어가 사망했습니다!";
            }
        }
    }

    private void ShowTokenized()
    {
        if (tokenizedPanel != null)
        {
            tokenizedPanel.SetActive(true);
        }
    }
}
