using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어 체력 UI 표시
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    
    private Player player;

    private void Start()
    {
        // CombatManager에서 플레이어 찾기
        if (CombatManager.Instance != null)
        {
            player = CombatManager.Instance.Player;
        }

        // TextMeshProUGUI 컴포넌트 자동 할당
        if (healthText == null)
        {
            healthText = GetComponent<TextMeshProUGUI>();
        }

        UpdateHealthDisplay();
    }

    private void Update()
    {
        // 플레이어가 없으면 찾기
        if (player == null && CombatManager.Instance != null)
        {
            player = CombatManager.Instance.Player;
        }

        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (healthText == null) return;

        if (player != null && player.IsAlive)
        {
            healthText.text = $"HP: {player.CurrentHP} / {player.MaxHP}";
        }
        else
        {
            healthText.text = "HP: 0 / 0";
        }
    }
}
