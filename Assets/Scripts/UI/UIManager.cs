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
        PopulationManager.OnGameOver += ShowGameOver;
        PaperController.OnPaperTokenized += ShowTokenized;
    }
    
    private void OnDisable()
    {
        PopulationManager.OnGameOver -= ShowGameOver;
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
            
            if (gameOverText != null && TurnSystem.Instance != null)
            {
                gameOverText.text = $"게임 오버\n\n생존 일수: {TurnSystem.Instance.CurrentDay}일";
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
