using System;
using Volo.Abp.Application.Dtos;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن طلب وفلترة قائمة المتاجر
/// </summary>
public class GetStoreListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public string? Filter { get; set; }
    public Guid? StoreTypeId { get; set; }
    public StoreStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public decimal? MinRating { get; set; }
    #endregion
}

/// <summary>
/// كائن موظف المتجر
/// </summary>
public class StoreUserDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public StoreUserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    #endregion
}

/// <summary>
/// كائن إضافة موظف جديد لمتجر
/// </summary>
public class CreateStoreUserDto
{
    #region Properties
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public StoreUserRole Role { get; set; } = StoreUserRole.Employee;
    #endregion
}
