using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Foldlands/RecipeDatabase")]
public class RecipeDatabase : ScriptableObject
{
    public List<RecipeData> recipes = new List<RecipeData>();
    
    /// <summary>
    /// 두 마크 타입으로 만들 수 있는 레시피 찾기
    /// </summary>
    public RecipeData FindRecipe(MarkType a, MarkType b)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.Matches(a, b))
                return recipe;
        }
        return null;
    }
}
