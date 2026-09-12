using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Customers;

/// <summary>
/// كيان عنوان توصيل العميل
/// </summary>
public class CustomerAddress : FullAuditedEntity<Guid>
{
    /// <summary>
    /// معرف العميل التابع له العنوان
    /// </summary>
    public virtual Guid CustomerId { get; set; }

    /// <summary>
    /// عنوان التسمية (مثل: المنزل، العمل)
    /// </summary>
    public virtual string Title { get; set; } = string.Empty;

    /// <summary>
    /// خط العرض الجغرافي (Latitude)
    /// </summary>
    public virtual decimal Latitude { get; set; }

    /// <summary>
    /// خط الطول الجغرافي (Longitude)
    /// </summary>
    public virtual decimal Longitude { get; set; }

    /// <summary>
    /// الدولة
    /// </summary>
    public virtual string Country { get; set; } = string.Empty;

    /// <summary>
    /// المحافظة
    /// </summary>
    public virtual string Governorate { get; set; } = string.Empty;

    /// <summary>
    /// المنطقة
    /// </summary>
    public virtual string Region { get; set; } = string.Empty;

    /// <summary>
    /// اسم المدينة
    /// </summary>
    public virtual string City { get; set; } = string.Empty;

    /// <summary>
    /// اسم الحي
    /// </summary>
    public virtual string District { get; set; } = string.Empty;

    /// <summary>
    /// اسم الشارع
    /// </summary>
    public virtual string Street { get; set; } = string.Empty;

    /// <summary>
    /// رقم أو اسم المبنى
    /// </summary>
    public virtual string? Building { get; set; }

    /// <summary>
    /// رقم الطابق
    /// </summary>
    public virtual string? Floor { get; set; }

    /// <summary>
    /// رقم الشقة أو المكتب
    /// </summary>
    public virtual string? Apartment { get; set; }

    /// <summary>
    /// تفاصيل أو إرشادات إضافية للوصول
    /// </summary>
    public virtual string? AdditionalDetails { get; set; }

    /// <summary>
    /// هل هو العنوان الافتراضي للتوصيل؟
    /// </summary>
    public virtual bool IsDefault { get; set; }

    /// <summary>
    /// كائن العميل المرتبط
    /// </summary>
    public virtual Customer? Customer { get; set; }

    protected CustomerAddress()
    {
    }

    public CustomerAddress(
        Guid id,
        Guid customerId,
        string title,
        decimal latitude,
        decimal longitude,
        string country,
        string governorate,
        string region,
        string city,
        string district,
        string street,
        bool isDefault = false)
        : base(id)
    {
        CustomerId = customerId;
        Title = title;
        Latitude = latitude;
        Longitude = longitude;
        Country = country;
        Governorate = governorate;
        Region = region;
        City = city;
        District = district;
        Street = street;
        IsDefault = isDefault;
    }
}
