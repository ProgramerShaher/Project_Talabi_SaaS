using System;
using Volo.Abp.Application.Dtos;

namespace Talabi.Customers.Dtos;

/// <summary>
/// كائن طلب قائمة العملاء مع الترقيم والفلترة
/// </summary>
public class GetCustomerListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    /// <summary>
    /// البحث العام (في اسم المستخدم أو البريد)
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// فلتر بالجنس
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// الحد الأدنى لنقاط الولاء
    /// </summary>
    public int? MinLoyaltyPoints { get; set; }
    #endregion
}
