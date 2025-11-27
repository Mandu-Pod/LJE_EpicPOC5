using UnityEngine;

public enum MarketItemType
{
    NewPaperForest,     // 새로운 숲 종이 생성
    NewPaperPlains,     // 새로운 평지 종이 생성
}

[CreateAssetMenu(fileName = "MarketItem", menuName = "Foldlands/MarketItem")]
public class MarketItemSO : ScriptableObject
{
    [Header("아이템 정보")]
    public string itemName;
    public string description;

    [Header("비용")]
    public int cost;

    [Header("아이템 타입")]
    public MarketItemType itemType;
}