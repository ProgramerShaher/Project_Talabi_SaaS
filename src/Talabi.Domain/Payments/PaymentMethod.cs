using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Payments;

/// <summary>
/// كيان طرق ووسائل الدفع المتاحة في النظام
/// </summary>
public class PaymentMethod : FullAuditedEntity<Guid>
{
    /// <summary>
    /// الاسم البرمجي الفريد لطريقة الدفع (CashOnDelivery, OnlineCard, BankTransfer, Wallet)
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الظاهر للمستخدم (الدفع عند الاستلام، بطاقة مدى، تحويل بنكي...)
    /// </summary>
    public virtual string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// رابط الأيقونة أو الشعار
    /// </summary>
    public virtual string? IconUrl { get; set; }

    /// <summary>
    /// هل هي طريقة دفع إلكتروني عبر بوابة دفع؟
    /// </summary>
    public virtual bool IsOnline { get; set; }

    /// <summary>
    /// هل تتطلب رفع إيصال تحويل مالي للتحقق اليدوي؟
    /// </summary>
    public virtual bool RequiresReceipt { get; set; }

    /// <summary>
    /// هل طريقة الدفع مفعلة ومتاحة للعملاء؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    protected PaymentMethod()
    {
    }

    public PaymentMethod(
        Guid id,
        string name,
        string displayName,
        bool isOnline = false,
        bool requiresReceipt = false,
        bool isActive = true,
        string? iconUrl = null)
        : base(id)
    {
        Name = name;
        DisplayName = displayName;
        IsOnline = isOnline;
        RequiresReceipt = requiresReceipt;
        IsActive = isActive;
        IconUrl = iconUrl;
    }
}
