using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Stores;

/// <summary>
/// كيان حساب الدفع الخاص بالمتجر (مثل: رقم جوالي، النجم، حساب بنكي)
/// </summary>
public class StorePaymentAccount : FullAuditedEntity<Guid>
{
    /// <summary>
    /// معرف المتجر التابع له الحساب
    /// </summary>
    public virtual Guid StoreId { get; set; }

    /// <summary>
    /// اسم الجهة أو مزود الخدمة (مثل: جوالي، الكريمي، النجم، بنك اليمن والكويت)
    /// </summary>
    public virtual string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// رقم الحساب البنكي أو رقم الهاتف المحمول المربوط بالمحفظة
    /// </summary>
    public virtual string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// اسم صاحب الحساب (لمطابقة المستفيد عند التحويل)
    /// </summary>
    public virtual string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// هل الحساب نشط ومتاح للعملاء؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// ملاحظات إضافية يكتبها المتجر للعميل حول هذا الحساب
    /// </summary>
    public virtual string? Notes { get; set; }

    /// <summary>
    /// المتجر المرتبط
    /// </summary>
    public virtual Store? Store { get; set; }

    protected StorePaymentAccount()
    {
    }

    public StorePaymentAccount(
        Guid id,
        Guid storeId,
        string providerName,
        string accountNumber,
        string accountName,
        bool isActive = true,
        string? notes = null)
        : base(id)
    {
        StoreId = storeId;
        ProviderName = providerName;
        AccountNumber = accountNumber;
        AccountName = accountName;
        IsActive = isActive;
        Notes = notes;
    }
}
