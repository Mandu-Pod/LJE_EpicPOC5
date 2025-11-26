using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Foldlands/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("재료 (순서 무관)")]
    public MarkType ingredient1;
    public MarkType ingredient2;
    
    [Header("결과물")]
    public MarkType result;
    
    [Header("설명")]
    public string description;
    
    /// <summary>
    /// 두 마크 타입이 이 레시피와 일치하는지 확인 (순서 무관)
    /// </summary>
    public bool Matches(MarkType a, MarkType b)
    {
        return (a == ingredient1 && b == ingredient2) ||
               (a == ingredient2 && b == ingredient1);
    }
}
