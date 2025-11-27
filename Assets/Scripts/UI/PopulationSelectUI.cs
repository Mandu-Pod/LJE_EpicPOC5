using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopulationSelectUI : MonoBehaviour
{
	public static event Action<int> OnPopulationConfirmed;

	[Header("UI Elements")]
	[SerializeField] private GameObject panel;
	[SerializeField] private TextMeshProUGUI availablePopulationText;
	[SerializeField] private TextMeshProUGUI selectedCountText;
	[SerializeField] private Button decreaseButton;
	[SerializeField] private Button increaseButton;
	[SerializeField] private Button confirmButton;

	private int selectedCount = 1;
	private int availablePopulation = 0;

	private void Start()
	{
		Hide();

		if (decreaseButton != null)
			decreaseButton.onClick.AddListener(OnDecreaseClicked);

		if (increaseButton != null)
			increaseButton.onClick.AddListener(OnIncreaseClicked);

		if (confirmButton != null)
			confirmButton.onClick.AddListener(OnConfirmClicked);
	}

	public void Show(int available)
	{
		availablePopulation = available;
		selectedCount = Mathf.Clamp(1, 1, availablePopulation);

		UpdateUI();

		if (panel != null)
			panel.SetActive(true);
	}

	public void Hide()
	{
		if (panel != null)
			panel.SetActive(false);
	}

	private void OnDecreaseClicked()
	{
		selectedCount = Mathf.Max(1, selectedCount - 1);
		UpdateUI();
	}

	private void OnIncreaseClicked()
	{
		selectedCount = Mathf.Min(availablePopulation, selectedCount + 1);
		UpdateUI();
	}

	private void OnConfirmClicked()
	{
		OnPopulationConfirmed?.Invoke(selectedCount);
		Hide();
	}

	private void UpdateUI()
	{
		if (availablePopulationText != null)
		{
			availablePopulationText.text = $"사용 가능 인구: {availablePopulation}명";
		}

		if (selectedCountText != null)
		{
			selectedCountText.text = selectedCount.ToString();
		}

		// 버튼 활성화 상태
		if (decreaseButton != null)
			decreaseButton.interactable = selectedCount > 1;

		if (increaseButton != null)
			increaseButton.interactable = selectedCount < availablePopulation;

		if (confirmButton != null)
			confirmButton.interactable = selectedCount <= availablePopulation && selectedCount >= 1;
	}
}