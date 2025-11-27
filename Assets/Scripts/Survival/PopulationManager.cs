using System;
using UnityEngine;

public class PopulationManager : SingletonObject<PopulationManager>
{
    public static event Action OnPopulationChanged;
    public static event Action OnGameOver;

    [Header("초기 설정")]
    [SerializeField] private int initialPopulation = 10;
    [SerializeField] private int initialFood = 30;
    [SerializeField] private int foodPerPerson = 1;

    private int totalPopulation;      // 전체 인구 (종이에 미배치된 인구 포함)
    private int activePopulation;     // 현재 활동 중인 인구 (종이에 배치된 인구)

    public int TotalPopulation => totalPopulation;
    public int ActivePopulation => activePopulation;
    public int FoodPerPerson => foodPerPerson;

    protected override void Awake()
    {
        base.Awake();
        totalPopulation = initialPopulation;
        activePopulation = 0;

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(MarkType.Food, initialFood);
        }
    }

    private void OnEnable()
    {
        TurnSystem.OnDayPassed += ConsumeFood;
    }

    private void OnDisable()
    {
        TurnSystem.OnDayPassed -= ConsumeFood;
    }

    /// <summary>
    /// 종이에 인구 배치 (전체 인구 풀에서 차감)
    /// </summary>
    public void DeployPopulation(int count)
    {
        if (count > totalPopulation)
        {
            Debug.LogWarning($"[인구] 배치 실패 - 요청: {count}, 가능: {totalPopulation}");
            return;
        }

        totalPopulation -= count;
        activePopulation += count;

        Debug.Log($"[인구] {count}명 배치 - 전체: {totalPopulation}, 활동: {activePopulation}");
        OnPopulationChanged?.Invoke();
    }

    /// <summary>
    /// 종이 토큰화 시 인구 회수 (전체 인구 풀로 복귀)
    /// </summary>
    public void ReturnPopulation(int count)
    {
        totalPopulation += count;
        activePopulation -= count;

        Debug.Log($"[인구] {count}명 회수 - 전체: {totalPopulation}, 활동: {activePopulation}");
        OnPopulationChanged?.Invoke();
    }

    /// <summary>
    /// 일일 식량 소모 (활동 중인 인구만)
    /// </summary>
    private void ConsumeFood()
    {
        int required = activePopulation * foodPerPerson;
        int currentFood = InventorySystem.Instance.GetItemCount(MarkType.Food);

        if (currentFood >= required)
        {
            InventorySystem.Instance.RemoveItem(MarkType.Food, required);
            Debug.Log($"[생존] 식량 -{required} 소모 (남은 식량: {InventorySystem.Instance.GetItemCount(MarkType.Food)})");
        }
        else
        {
            // 식량 부족 - 활동 인구 감소
            int shortage = required - currentFood;
            int deaths = Mathf.CeilToInt((float)shortage / foodPerPerson);
            deaths = Mathf.Min(deaths, activePopulation);

            if (currentFood > 0)
            {
                InventorySystem.Instance.RemoveItem(MarkType.Food, currentFood);
            }

            activePopulation -= deaths;
            // 전체 인구도 감소 (사망)
            // totalPopulation -= deaths;  // 선택사항: 전체 인구도 줄일지 결정

            Debug.Log($"[생존] 식량 부족! {deaths}명 사망 (활동 인구: {activePopulation})");
            OnPopulationChanged?.Invoke();

            if (activePopulation <= 0 && totalPopulation <= 0)
            {
                Debug.Log("[생존] 게임 오버 - 모든 인구 사망");
                OnGameOver?.Invoke();
            }
        }
    }
}