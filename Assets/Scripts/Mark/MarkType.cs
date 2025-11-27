public enum MarkType
{
    // 자원 마크 (채집 대상)
    Tree,       // 나무
    Stone,      // 돌

    // 결과물 마크 (인벤토리 아이템)
    Wood,       // 목재
    Food,       // 식량

    // 인구 마크
    Person,     // 사람

    Coin,       // 화폐 (토큰)
}

public enum PaperType
{
    Forest,     // 숲 - 나무(3-5), 돌(1-2), 식량(0-1)
    Plains,     // 평지 - 나무(0-1), 돌(1-2), 식량(3-5)
}

public enum MarkCategory
{
    Resource,   // 자원 (나무, 돌 등)
    Product,    // 결과물 (목재, 식량 등)
    Person,     // 인구
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
            MarkType.Stone => MarkCategory.Resource,
            MarkType.Wood => MarkCategory.Product,
            MarkType.Food => MarkCategory.Product,
            MarkType.Person => MarkCategory.Person,
            MarkType.Coin => MarkCategory.Coin,
            _ => MarkCategory.Coin
        };
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

    /// <summary>
    /// 인구인지 여부
    /// </summary>
    public static bool IsPerson(this MarkType type)
    {
        return type.GetCategory() == MarkCategory.Person;
    }
}