namespace Talabi.Payments;

/// <summary>
/// حالة معاملة الدفع
/// </summary>
public enum PaymentTransactionStatus
{
    /// <summary>
    /// قيد الانتظار
    /// </summary>
    Pending = 1,

    /// <summary>
    /// مكتملة بنجاح
    /// </summary>
    Completed = 2,

    /// <summary>
    /// فشلت العملية
    /// </summary>
    Failed = 3,

    /// <summary>
    /// تم استرجاع المبلغ
    /// </summary>
    Refunded = 4
}

/// <summary>
/// حالة التحقق من إيصال التحويل
/// </summary>
public enum ReceiptVerificationStatus
{
    /// <summary>
    /// بانتظار المراجعة
    /// </summary>
    Pending = 1,

    /// <summary>
    /// تم التحقق والقبول
    /// </summary>
    Verified = 2,

    /// <summary>
    /// تم الرفض
    /// </summary>
    Rejected = 3
}
