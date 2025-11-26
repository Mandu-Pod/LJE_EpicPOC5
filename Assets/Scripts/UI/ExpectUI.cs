using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExpectUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI expectText;
    [SerializeField] private GameObject expectPanel;

    [Header("아이콘 설정")]
    [SerializeField] private string treeIcon = "🌲";
    [SerializeField] private string woodIcon = "🪵";
    [SerializeField] private string axeIcon = "🪓";
    [SerializeField] private string pickaxeIcon = "⛏️";
    [SerializeField] private string unknownIcon = "❓";

    private Dictionary<MarkType, int> expectedResources = new Dictionary<MarkType, int>();

    private void Start()
    {
        HideExpectation();
    }

    /// <summary>
    /// 예상 자원 목록 업데이트
    /// </summary>
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

    /// <summary>
    /// 예상 자원 초기화
    /// </summary>
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
            MarkType.Wood => woodIcon,
            MarkType.Axe => axeIcon,
            MarkType.Pickaxe => pickaxeIcon,
            _ => unknownIcon
        };
    }

    private string GetItemName(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => "나무",
            MarkType.Wood => "목재",
            MarkType.Axe => "도끼",
            MarkType.Pickaxe => "곡괭이",
            _ => "알 수 없음"
        };
    }
}