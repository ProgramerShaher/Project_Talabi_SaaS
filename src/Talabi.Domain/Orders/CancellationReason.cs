using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Orders;

/// <summary>
/// كيان أسباب رفض أو إلغاء الطلبات المعيارية
/// </summary>
public class CancellationReason : FullAuditedEntity<Guid>
{
    /// <summary>
    /// نص السبب
    /// </summary>
    public virtual string Reason { get; set; } = string.Empty;

    /// <summary>
    /// الفئة المستهدفة أو المعنية بالسبب (عميل، متجر، نظام)
    /// </summary>
    public virtual CancellationTargetAudience TargetAudience { get; set; }

    /// <summary>
    /// هل السبب نشط ومتاح للاختيار؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    protected CancellationReason()
    {
    }

    public CancellationReason(Guid id, string reason, CancellationTargetAudience targetAudience, bool isActive = true)
        : base(id)
    {
        Reason = reason;
        TargetAudience = targetAudience;
        IsActive = isActive;
    }
}
