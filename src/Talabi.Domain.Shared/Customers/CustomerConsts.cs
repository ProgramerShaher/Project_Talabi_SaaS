namespace Talabi.Customers;

/// <summary>
/// ثوابت وقيود حقول العملاء
/// </summary>
public static class CustomerConsts
{
    /// <summary>
    /// أقصى طول للغة المفضلة (مثل "ar", "en")
    /// </summary>
    public const int MaxPreferredLanguageLength = 10;

    /// <summary>
    /// اللغة الافتراضية
    /// </summary>
    public const string DefaultPreferredLanguage = "ar";
}

/// <summary>
/// ثوابت وقيود حقول عناوين العملاء
/// </summary>
public static class CustomerAddressConsts
{
    /// <summary>
    /// أقصى طول لعنوان التسمية (المنزل، العمل)
    /// </summary>
    public const int MaxTitleLength = 50;

    /// <summary>
    /// أقصى طول لاسم المدينة
    /// </summary>
    public const int MaxCityLength = 100;

    /// <summary>
    /// أقصى طول لاسم الحي
    /// </summary>
    public const int MaxDistrictLength = 100;

    /// <summary>
    /// أقصى طول لاسم الشارع
    /// </summary>
    public const int MaxStreetLength = 200;

    /// <summary>
    /// أقصى طول لرقم المبنى
    /// </summary>
    public const int MaxBuildingLength = 50;

    /// <summary>
    /// أقصى طول لرقم الطابق
    /// </summary>
    public const int MaxFloorLength = 20;

    /// <summary>
    /// أقصى طول لرقم الشقة
    /// </summary>
    public const int MaxApartmentLength = 20;

    /// <summary>
    /// أقصى طول للتفاصيل الإضافية
    /// </summary>
    public const int MaxAdditionalDetailsLength = 500;
}
