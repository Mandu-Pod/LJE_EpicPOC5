using System;
using UnityEngine;

// [비활성화] 전투 시스템으로 전환으로 인해 비활성화됨
/*
public class TurnSystem : SingletonObject<TurnSystem>
{
    public static event Action OnDayPassed;
    public static event Action OnFoldCountChanged;
    
    [Header("설정")]
    [SerializeField] private int foldsPerDay = 3;  // 유효 접기 3회 = 1일
    
    private int currentDay = 1;
    private int effectiveFoldCount = 0;
    
    public int CurrentDay => currentDay;
    public int EffectiveFoldCount => effectiveFoldCount;
    public int FoldsPerDay => foldsPerDay;
    
    /// <summary>
    /// 유효 접기 발생 시 호출 (자원 생성 시에만)
    /// </summary>
    public void RegisterEffectiveFold()
    {
        effectiveFoldCount++;
        Debug.Log($"[턴] 유효 접기 {effectiveFoldCount}/{foldsPerDay}");
        OnFoldCountChanged?.Invoke();
        
        if (effectiveFoldCount >= foldsPerDay)
        {
            effectiveFoldCount = 0;
            AdvanceDay();
        }
    }
    
    private void AdvanceDay()
    {
        currentDay++;
        Debug.Log($"[턴] === {currentDay}일차 시작 ===");
        OnDayPassed?.Invoke();
    }
}
*/
