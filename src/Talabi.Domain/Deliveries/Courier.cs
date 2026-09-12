using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Deliveries;

/// <summary>
/// كيان مندوب التوصيل في النظام (Courier Aggregate Root)
/// </summary>
public class Courier : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// معرف المستخدم المرتبط في جدول المستخدمين الأساسي (علاقة 1:1 فريدة)
    /// </summary>
    public virtual Guid UserId { get; set; }

    /// <summary>
    /// نوع وسيلة النقل (سيارة، دراجة نارية، شاحنة)
    /// </summary>
    public virtual string? VehicleType { get; set; }

    /// <summary>
    /// رقم لوحة المركبة
    /// </summary>
    public virtual string? VehicleNumber { get; set; }

    /// <summary>
    /// رقم رخصة القيادة
    /// </summary>
    public virtual string? LicenseNumber { get; set; }

    /// <summary>
    /// هل المندوب متاح وجاهز لاستقبال طلبات جديدة؟
    /// </summary>
    public virtual bool IsAvailable { get; set; } = true;

    /// <summary>
    /// هل تطبيق المندوب متصل بالإنترنت حالياً؟
    /// </summary>
    public virtual bool IsOnline { get; set; }

    /// <summary>
    /// خط العرض للموقع الجغرافي اللحظي الأخير للمندوب
    /// </summary>
    public virtual decimal? CurrentLatitude { get; set; }

    /// <summary>
    /// خط الطول للموقع الجغرافي اللحظي الأخير للمندوب
    /// </summary>
    public virtual decimal? CurrentLongitude { get; set; }

    /// <summary>
    /// تاريخ ووقت آخر تحديث للموقع الجغرافي
    /// </summary>
    public virtual DateTime? LastLocationUpdate { get; set; }

    /// <summary>
    /// متوسط تقييم أداء المندوب (معطل - التقييم مخصص للنشاط التجاري/المتجر فقط)
    /// </summary>
    // public virtual decimal Rating { get; set; }

    /// <summary>
    /// إجمالي عدد الطلبات التي سلمها المندوب بنجاح
    /// </summary>
    public virtual int TotalDeliveries { get; set; }

    /// <summary>
    /// إجمالي أرباح ومستحقات المندوب المالية (معطلة - النظام لا يتدخل في أرباح التوصيل)
    /// </summary>
    // public virtual decimal TotalEarnings { get; set; }

    /// <summary>
    /// قائمة عمليات تعيين التوصيل المسندة لهذا المندوب
    /// </summary>
    public virtual ICollection<DeliveryAssignment> Assignments { get; protected set; } = new List<DeliveryAssignment>();

    protected Courier()
    {
    }

    public Courier(Guid id, Guid userId, string? vehicleType = null, string? vehicleNumber = null)
        : base(id)
    {
        UserId = userId;
        VehicleType = vehicleType;
        VehicleNumber = vehicleNumber;
        IsAvailable = true;
        IsOnline = false;
        // Rating = 0;
        TotalDeliveries = 0;
        // TotalEarnings = 0;
    }
}
