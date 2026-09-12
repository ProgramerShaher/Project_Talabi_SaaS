using System;
using Talabi.Orders;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Deliveries;

/// <summary>
/// كيان تعيين توصيل الطلب لمندوب ومتابعة مراحل التوصيل
/// </summary>
public class DeliveryAssignment : FullAuditedEntity<Guid>
{
    /// <summary>
    /// معرف الطلب المرتبط (علاقة 1:1 فريدة)
    /// </summary>
    public virtual Guid OrderId { get; set; }

    /// <summary>
    /// معرف المندوب المعين
    /// </summary>
    public virtual Guid CourierId { get; set; }

    /// <summary>
    /// حالة مرحلة التوصيل (معين، مقبول، مستلم، في الطريق، مسلم، فاشل)
    /// </summary>
    public virtual DeliveryAssignmentStatus Status { get; set; } = DeliveryAssignmentStatus.Assigned;

    /// <summary>
    /// معرف المستخدم أو النظام الذي أسند الطلب للمندوب
    /// </summary>
    public virtual Guid AssignedByUserId { get; set; }

    /// <summary>
    /// تاريخ ووقت إسناد الطلب
    /// </summary>
    public virtual DateTime AssignedAt { get; set; }

    /// <summary>
    /// تاريخ ووقت قبول المندوب للطلب
    /// </summary>
    public virtual DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// تاريخ ووقت استلام الطلب من المتجر
    /// </summary>
    public virtual DateTime? PickupAt { get; set; }

    /// <summary>
    /// تاريخ ووقت تسليم الطلب النهائي للعميل
    /// </summary>
    public virtual DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// وقت الوصول التقديري المتوقع
    /// </summary>
    public virtual DateTime? EstimatedArrival { get; set; }

    /// <summary>
    /// المسافة التقديرية أو الفعلية بالكيلومتر
    /// </summary>
    public virtual decimal? DistanceKm { get; set; }

    /// <summary>
    /// رسوم التوصيل المحتسبة على الطلب (معطلة - المتجر هو المسؤول والنظام لا يتدخل في رسوم التوصيل)
    /// </summary>
    // public virtual decimal? DeliveryFee { get; set; }

    /// <summary>
    /// صافي حصة وأرباح المندوب من هذه التوصيلة (معطلة - النظام لا يتدخل في أرباح المشاوير)
    /// </summary>
    // public virtual decimal? CourierEarning { get; set; }

    /// <summary>
    /// الطلب المرتبط
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// المندوب المرتبط
    /// </summary>
    public virtual Courier? Courier { get; set; }

    /// <summary>
    /// سجل تأكيد التسليم المرتبط
    /// </summary>
    public virtual DeliveryConfirmation? Confirmation { get; set; }

    protected DeliveryAssignment()
    {
    }

    public DeliveryAssignment(
        Guid id,
        Guid orderId,
        Guid courierId,
        Guid assignedByUserId)
        : base(id)
    {
        OrderId = orderId;
        CourierId = courierId;
        AssignedByUserId = assignedByUserId;
        Status = DeliveryAssignmentStatus.Assigned;
        AssignedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// كيان توثيق وإثبات تأكيد تسليم الطلب للعميل
/// </summary>
public class DeliveryConfirmation : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف تعيين التوصيل المرتبط (علاقة 1:1 فريدة)
    /// </summary>
    public virtual Guid DeliveryAssignmentId { get; set; }

    /// <summary>
    /// الجهة المؤكدة للتسليم (العميل، المندوب، النظام)
    /// </summary>
    public virtual DeliveryConfirmedBy ConfirmedBy { get; set; }

    /// <summary>
    /// معرف المستخدم الذي أجرى التأكيد
    /// </summary>
    public virtual Guid ConfirmedByUserId { get; set; }

    /// <summary>
    /// كود التحقق السري المؤقت (OTP) في حال تم التسليم عبر رمز تحقق
    /// </summary>
    public virtual string? VerificationCode { get; set; }

    /// <summary>
    /// رابط صورة التوقيع الإلكتروني للعميل
    /// </summary>
    public virtual string? SignatureUrl { get; set; }

    /// <summary>
    /// رابط صورة إثبات التسليم (Photo Proof) عند الباب
    /// </summary>
    public virtual string? PhotoProofUrl { get; set; }

    /// <summary>
    /// ملاحظات إضافية حول عملية التسليم
    /// </summary>
    public virtual string? Notes { get; set; }

    /// <summary>
    /// تاريخ ووقت التأكيد
    /// </summary>
    public virtual DateTime ConfirmedAt { get; set; }

    /// <summary>
    /// تعيين التوصيل المرتبط
    /// </summary>
    public virtual DeliveryAssignment? DeliveryAssignment { get; set; }

    protected DeliveryConfirmation()
    {
    }

    public DeliveryConfirmation(
        Guid id,
        Guid deliveryAssignmentId,
        DeliveryConfirmedBy confirmedBy,
        Guid confirmedByUserId,
        string? verificationCode = null,
        string? signatureUrl = null,
        string? photoProofUrl = null,
        string? notes = null)
        : base(id)
    {
        DeliveryAssignmentId = deliveryAssignmentId;
        ConfirmedBy = confirmedBy;
        ConfirmedByUserId = confirmedByUserId;
        VerificationCode = verificationCode;
        SignatureUrl = signatureUrl;
        PhotoProofUrl = photoProofUrl;
        Notes = notes;
        ConfirmedAt = DateTime.UtcNow;
    }
}
