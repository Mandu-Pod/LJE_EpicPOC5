using TMPro;
using UnityEngine;

public class SurvivalUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI foldCountText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI paperAreaText;
    
    private void OnEnable()
    {
        TurnSystem.OnDayPassed += UpdateUI;
        TurnSystem.OnFoldCountChanged += UpdateUI;
        PopulationManager.OnPopulationChanged += UpdateUI;
        PopulationManager.OnFoodChanged += UpdateUI;
        PaperController.OnPaperFolded += UpdateUI;
    }
    
    private void OnDisable()
    {
        TurnSystem.OnDayPassed -= UpdateUI;
        TurnSystem.OnFoldCountChanged -= UpdateUI;
        PopulationManager.OnPopulationChanged -= UpdateUI;
        PopulationManager.OnFoodChanged -= UpdateUI;
        PaperController.OnPaperFolded -= UpdateUI;
    }
    
    private void Start()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // 일수
        if (dayText != null && TurnSystem.Instance != null)
        {
            dayText.text = $"Day {TurnSystem.Instance.CurrentDay}";
        }
        
        // 유효 접기 카운트
        if (foldCountText != null && TurnSystem.Instance != null)
        {
            int current = TurnSystem.Instance.EffectiveFoldCount;
            int max = TurnSystem.Instance.FoldsPerDay;
            foldCountText.text = $"접기: {current}/{max}";
        }
        
        // 인구
        if (populationText != null && PopulationManager.Instance != null)
        {
            populationText.text = $"👥 {PopulationManager.Instance.CurrentPopulation}";
        }
        
        // 식량
        if (foodText != null && PopulationManager.Instance != null)
        {
            foodText.text = $"🍖 {PopulationManager.Instance.CurrentFood}";
        }
        
        // 종이 면적
        if (paperAreaText != null && PaperController.Instance != null)
        {
            float ratio = PaperController.Instance.AreaRatio * 100f;
            paperAreaText.text = $"📄 {ratio:F0}%";
        }
    }
}
