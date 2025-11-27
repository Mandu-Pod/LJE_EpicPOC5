using System;
using UnityEngine;

// [비활성화] 전투 시스템으로 전환으로 인해 비활성화됨
/*
public class PopulationManager : SingletonObject<PopulationManager>
{
    public static event Action OnPopulationChanged;
    public static event Action OnFoodChanged;
    public static event Action OnGameOver;
    
    [Header("초기 설정")]
    [SerializeField] private int initialPopulation = 3;
    [SerializeField] private int initialFood = 30;
    [SerializeField] private int foodPerPerson = 1;  // 1인당 일일 식량 소모
    
    private int currentPopulation;
    private int currentFood;
    
    public int CurrentPopulation => currentPopulation;
    public int CurrentFood => currentFood;
    
    protected override void Awake()
    {
        base.Awake();
        currentPopulation = initialPopulation;
        currentFood = initialFood;
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
    /// 식량 추가
    /// </summary>
    public void AddFood(int amount)
    {
        currentFood += amount;
        OnFoodChanged?.Invoke();
    }
    
    /// <summary>
    /// 일일 식량 소모
    /// </summary>
    private void ConsumeFood()
    {
        int required = currentPopulation * foodPerPerson;
        
        if (currentFood >= required)
        {
            currentFood -= required;
        }
        else
        {
            // 식량 부족 - 인구 감소
            int shortage = required - currentFood;
            int deaths = Mathf.CeilToInt((float)shortage / foodPerPerson);
            deaths = Mathf.Min(deaths, currentPopulation);
            
            currentFood = 0;
            currentPopulation -= deaths;
            
            OnPopulationChanged?.Invoke();
            
            if (currentPopulation <= 0)
            {
                OnGameOver?.Invoke();
            }
        }
        
        OnFoodChanged?.Invoke();
    }
}
*/
