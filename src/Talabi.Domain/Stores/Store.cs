using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Talabi.Stores;

/// <summary>
/// كيان المتجر - يمثل المتجر الفعلي في منصة SaaS المتعددة المستأجرين
/// </summary>
public class Store : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// معرف المستأجر التابع له المتجر في نظام SaaS
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// معرف مالك المتجر الأساسي في جدول المستخدمين
    /// </summary>
    public virtual Guid OwnerId { get; set; }

    /// <summary>
    /// معرف نوع ونشاط المتجر
    /// </summary>
    public virtual Guid StoreTypeId { get; set; }

    /// <summary>
    /// اسم المتجر التجاري
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// الرابط اللطيف الفريد للمتجر (Slug URL)
    /// </summary>
    public virtual string Slug { get; set; } = string.Empty;

    /// <summary>
    /// وصف تعريفي بالمتجر وخدماته
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// رابط شعار المتجر (Logo)
    /// </summary>
    public virtual string? LogoUrl { get; set; }

    /// <summary>
    /// رابط صورة الغلاف (Banner)
    /// </summary>
    public virtual string? CoverImageUrl { get; set; }

    /// <summary>
    /// رقم الهاتف للتواصل
    /// </summary>
    public virtual string Phone { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني للمتجر
    /// </summary>
    public virtual string? Email { get; set; }

    /// <summary>
    /// العنوان النصي للمتجر
    /// </summary>
    public virtual string Address { get; set; } = string.Empty;

    /// <summary>
    /// إحداثي خط العرض الجغرافي
    /// </summary>
    public virtual decimal Latitude { get; set; }

    /// <summary>
    /// إحداثي خط الطول الجغرافي
    /// </summary>
    public virtual decimal Longitude { get; set; }

    /// <summary>
    /// ساعات وأيام العمل بتنسيق JSON
    /// </summary>
    public virtual string? WorkingHoursJson { get; set; }

    /// <summary>
    /// الحد الأدنى لقيمة الطلب
    /// </summary>
    public virtual decimal MinimumOrderAmount { get; set; }

    /// <summary>
    /// رسوم التوصيل الأساسية (معطلة - المتجر هو المسؤول والنظام لا يتدخل في رسوم التوصيل)
    /// </summary>
    // public virtual decimal DeliveryFee { get; set; }

    /// <summary>
    /// متوسط وقت التوصيل التقديري بالدقائق
    /// </summary>
    public virtual int? AverageDeliveryTime { get; set; }

    /// <summary>
    /// حالة المتجر التشغيلية (نشط، موقوف، بانتظار الموافقة)
    /// </summary>
    public virtual StoreStatus Status { get; set; } = StoreStatus.PendingApproval;

    /// <summary>
    /// هل المتجر نشط حالياً؟
    /// </summary>
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// هل المتجر مميز في الصفحة الرئيسية؟
    /// </summary>
    public virtual bool IsFeatured { get; set; }

    /// <summary>
    /// متوسط تقييم المتجر (من 0 إلى 5)
    /// </summary>
    public virtual decimal Rating { get; set; }

    /// <summary>
    /// إجمالي عدد التقييمات المستلمة
    /// </summary>
    public virtual int TotalReviews { get; set; }

    /// <summary>
    /// إجمالي عدد الطلبات المنفذة
    /// </summary>
    public virtual int TotalOrders { get; set; }

    /// <summary>
    /// نوع المتجر المرتبط
    /// </summary>
    public virtual StoreType? StoreType { get; set; }

    /// <summary>
    /// قائمة موظفي ومسؤولي المتجر
    /// </summary>
    public virtual ICollection<StoreUser> StoreUsers { get; protected set; } = new List<StoreUser>();

    /// <summary>
    /// قائمة حسابات الدفع والتحويل الخاصة بالمتجر
    /// </summary>
    public virtual ICollection<StorePaymentAccount> PaymentAccounts { get; protected set; } = new List<StorePaymentAccount>();

    protected Store()
    {
    }

    public Store(
        Guid id,
        Guid ownerId,
        Guid storeTypeId,
        string name,
        string slug,
        string phone,
        string address,
        decimal latitude,
        decimal longitude,
        Guid? tenantId = null)
        : base(id)
    {
        OwnerId = ownerId;
        StoreTypeId = storeTypeId;
        Name = name;
        Slug = slug;
        Phone = phone;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        TenantId = tenantId;
        Status = StoreStatus.PendingApproval;
        IsActive = true;
    }

    /// <summary>
    /// اعتماد المتجر وتنشيطه في المنصة من قِبل إدارة النظام
    /// </summary>
    public void Approve()
    {
        Status = StoreStatus.Active;
        IsActive = true;
    }

    /// <summary>
    /// تعليق المتجر وإيقاف نشاطه مؤقتاً من قِبل إدارة النظام
    /// </summary>
    public void Suspend()
    {
        Status = StoreStatus.Suspended;
        IsActive = false;
    }

    /// <summary>
    /// تبديل حالة الفتح والإغلاق التشغيلي للمتجر من قِبل التاجر
    /// </summary>
    /// <returns>الحالة الجديدة للمتجر بعد التبديل</returns>
    public bool ToggleOpenClose()
    {
        IsActive = !IsActive;
        return IsActive;
    }

    /// <summary>
    /// تعيين حالة الفتح والإغلاق التشغيلي للمتجر بشكل مباشر
    /// </summary>
    public void SetOpenClose(bool isOpen)
    {
        IsActive = isOpen;
    }
}
