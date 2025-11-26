public enum MarkType
{
    // 자원 마크 (채집 대상)
    Tree,       // 나무
    
    // 결과물 마크 (인벤토리 아이템)
    Wood,       // 목재
    
    // 도구 마크
    Axe,        // 도끼
    Pickaxe,    // 곡괭이 (확장용)

    //Coin
    Token,      // 토큰 (화폐)

}

public enum MarkCategory
{
    Resource,   // 자원 (나무, 바위 등)
    Product,    // 결과물 (목재, 돌 등)
    Tool,       // 도구 (도끼, 곡괭이 등)
    Coin,       // 화폐 (토큰 등)
}

public static class MarkTypeExtensions
{
    /// <summary>
    /// 마크 타입의 카테고리 반환
    /// </summary>
    public static MarkCategory GetCategory(this MarkType type)
    {
        return type switch
        {
            MarkType.Tree => MarkCategory.Resource,
            MarkType.Wood => MarkCategory.Product,
            MarkType.Axe => MarkCategory.Tool,
            MarkType.Pickaxe => MarkCategory.Tool,
            _ => MarkCategory.Coin
        };
    }
    
    /// <summary>
    /// 도구인지 여부
    /// </summary>
    public static bool IsTool(this MarkType type)
    {
        return type.GetCategory() == MarkCategory.Tool;
    }
    
    /// <summary>
    /// 자원인지 여부
    /// </summary>
    public static bool IsResource(this MarkType type)
    {
        return type.GetCategory() == MarkCategory.Resource;
    }
    
    /// <summary>
    /// 결과물인지 여부
    /// </summary>
    public static bool IsProduct(this MarkType type)
    {
        return type.GetCategory() == MarkCategory.Product;
    }
}
