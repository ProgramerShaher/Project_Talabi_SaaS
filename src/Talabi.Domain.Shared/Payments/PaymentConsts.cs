namespace Talabi.Payments;

/// <summary>
/// ثوابت وقيود حقول طرق الدفع
/// </summary>
public static class PaymentMethodConsts
{
    public const int MaxNameLength = 50;
    public const int MaxDisplayNameLength = 100;
    public const int MaxIconUrlLength = 500;
}

/// <summary>
/// ثوابت وقيود حقول المدفوعات
/// </summary>
public static class PaymentConsts
{
    public const int MaxCurrencyLength = 10;
    public const string DefaultCurrency = "SAR";
    public const int MaxTransactionReferenceLength = 100;
}

/// <summary>
/// ثوابت وقيود حقول إيصالات الدفع
/// </summary>
public static class PaymentReceiptConsts
{
    public const int MaxWalletNameLength = 100;
    public const int MaxTransactionNumberLength = 100;
    public const int MaxRejectionReasonLength = 500;
}
