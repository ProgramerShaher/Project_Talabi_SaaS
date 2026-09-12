using System;
using Talabi.Customers;

namespace Talabi.Auth.Dtos;

/// <summary>
/// نموذج بيانات استكمال الملف الشخصي للعميل بعد تسجيل الدخول
/// </summary>
public class CompleteCustomerProfileDto
{
    /// <summary>
    /// تاريخ الميلاد
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// الجنس (ذكر / أنثى)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// الدولة
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// المحافظة
    /// </summary>
    public string Governorate { get; set; } = string.Empty;

    /// <summary>
    /// المنطقة
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// المدينة
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// الحي
    /// </summary>
    public string District { get; set; } = string.Empty;

    /// <summary>
    /// الشارع
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// العمارة / المبنى
    /// </summary>
    public string? Building { get; set; }

    /// <summary>
    /// رقم الشقة
    /// </summary>
    public string? Apartment { get; set; }

    /// <summary>
    /// رقم الطابق
    /// </summary>
    public string? Floor { get; set; }
}
