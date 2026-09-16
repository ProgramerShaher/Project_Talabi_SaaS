namespace Talabi.Products;

/// <summary>
/// ثوابت وقيود حقول وحدات البيع
/// </summary>
public static class SalesUnitConsts
{
    /// <summary>
    /// الحد الأقصى لطول اسم وحدة البيع (مثال: كيلو، كيس، لتر، حبة)
    /// </summary>
    public const int MaxNameLength = 50;

    /// <summary>
    /// الحد الأقصى لطول الرمز البرمجي للوحدة (مثال: KG, BAG, LTR, PCS)
    /// </summary>
    public const int MaxCodeLength = 20;

    /// <summary>
    /// الحد الأقصى لوصف وحدة البيع
    /// </summary>
    public const int MaxDescriptionLength = 200;
}
