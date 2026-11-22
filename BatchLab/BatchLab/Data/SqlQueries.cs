public static class SqlQueries
{
    public const string SelectProductForUpdate =
        "SELECT * FROM `Products` WHERE `Id` = {0} FOR UPDATE";

    public const string SelectProductsBelowStock =
        "SELECT * FROM `Products` WHERE `StockQuantity` < {0}";

    public const string CountProductsBySkuPrefix =
        "SELECT COUNT(*) FROM `Products` WHERE `Sku` LIKE {0}";
}