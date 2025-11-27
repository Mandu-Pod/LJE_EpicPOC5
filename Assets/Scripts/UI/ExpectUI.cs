using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExpectUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI expectText;
    [SerializeField] private GameObject expectPanel;

    [Header("아이콘 설정")]
    [SerializeField] private string treeIcon = "T";
    [SerializeField] private string stoneIcon = "S";
    [SerializeField] private string woodIcon = "W";
    [SerializeField] private string foodIcon = "F";
    [SerializeField] private string personIcon = "P";
    [SerializeField] private string unknownIcon = "?";

    private Dictionary<MarkType, int> expectedResources = new Dictionary<MarkType, int>();

    private void Start()
    {
        HideExpectation();
    }

    public void UpdateExpectedResources(Dictionary<MarkType, int> resources)
    {
        expectedResources = new Dictionary<MarkType, int>(resources);

        if (expectedResources.Count > 0)
        {
            ShowExpectation();
            UpdateUI();
        }
        else
        {
            HideExpectation();
        }
    }

    public void ClearExpectedResources()
    {
        expectedResources.Clear();
        HideExpectation();
    }

    private void UpdateUI()
    {
        if (expectText == null) return;

        string text = "<b>예상 획득 자원</b>\n";

        foreach (var resource in expectedResources)
        {
            text += $"{GetItemIcon(resource.Key)} {GetItemName(resource.Key)} +{resource.Value}\n";
        }

        expectText.text = text;
    }

    private void ShowExpectation()
    {
        if (expectPanel != null)
            expectPanel.SetActive(true);
    }

    private void HideExpectation()
    {
        if (expectPanel != null)
            expectPanel.SetActive(false);
    }

    private string GetItemIcon(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => treeIcon,
            MarkType.Stone => stoneIcon,
            MarkType.Wood => woodIcon,
            MarkType.Food => foodIcon,
            MarkType.Person => personIcon,
            _ => unknownIcon
        };
    }

    private string GetItemName(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => "나무",
            MarkType.Stone => "돌",
            MarkType.Wood => "목재",
            MarkType.Food => "식량",
            MarkType.Person => "인구",
            _ => "알 수 없음"
        };
    }
}