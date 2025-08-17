namespace TerrariumGardenTech.Common.Enums;

public static class CommonData
{
    public struct AccountStatus
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
    }
    public struct CartItemType
    {
        public const string SINGLE = "SINGLE";              // Mua lẻ
        public const string MAIN_ITEM = "MAIN_ITEM";        // Bể chính  
        public const string BUNDLE_ACCESSORY = "BUNDLE_ACCESSORY"; // Phụ kiện combo
        public const string COMBO = "COMBO"; // Phụ kiện combo
    }
}