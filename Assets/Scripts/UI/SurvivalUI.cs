using TMPro;
using UnityEngine;

public class SurvivalUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI foldCountText;
    [SerializeField] private TextMeshProUGUI totalPopulationText;  // 전체 인구
    [SerializeField] private TextMeshProUGUI activePopulationText; // 활동 인구
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI paperAreaText;

    private void OnEnable()
    {
        TurnSystem.OnDayPassed += UpdateUI;
        TurnSystem.OnFoldCountChanged += UpdateUI;
        PopulationManager.OnPopulationChanged += UpdateUI;
        InventorySystem.OnInventoryChanged += UpdateUI;
        PaperController.OnPaperFolded += UpdateUI;
    }

    private void OnDisable()
    {
        TurnSystem.OnDayPassed -= UpdateUI;
        TurnSystem.OnFoldCountChanged -= UpdateUI;
        PopulationManager.OnPopulationChanged -= UpdateUI;
        InventorySystem.OnInventoryChanged -= UpdateUI;
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
            dayText.text = $"일수 {TurnSystem.Instance.CurrentDay}";
        }

        // 유효 접기 카운트
        if (foldCountText != null && TurnSystem.Instance != null)
        {
            int current = TurnSystem.Instance.EffectiveFoldCount;
            int max = TurnSystem.Instance.FoldsPerDay;
            foldCountText.text = $"접기 : {current}/{max}";
        }

        // 전체 인구
        if (totalPopulationText != null && PopulationManager.Instance != null)
        {
            totalPopulationText.text = $"전체 인구: {PopulationManager.Instance.TotalPopulation}";
        }

        // 활동 인구
        if (activePopulationText != null && PopulationManager.Instance != null)
        {
            activePopulationText.text = $"활동 인구: {PopulationManager.Instance.ActivePopulation}";
        }

        // 식량
        if (foodText != null && InventorySystem.Instance != null)
        {
            foodText.text = $"식량 : {InventorySystem.Instance.GetItemCount(MarkType.Food)}";
        }

        // 종이 면적
        if (paperAreaText != null && PaperController.Instance != null)
        {
            float ratio = PaperController.Instance.AreaRatio * 100f;
            paperAreaText.text = $"종이 : {ratio:F0}%";
        }
    }
}