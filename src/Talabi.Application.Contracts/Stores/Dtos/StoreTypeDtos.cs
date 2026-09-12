using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن عرض بيانات نوع المتجر
/// </summary>
public class StoreTypeDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    #endregion
}

/// <summary>
/// كائن إنشاء وتعديل نوع المتجر
/// </summary>
public class CreateUpdateStoreTypeDto
{
    #region Properties
    [Required(ErrorMessage = "اسم نوع المتجر مطلوب")]
    [StringLength(StoreTypeConsts.MaxNameLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم نوع المتجر")]
    public string Name { get; set; } = string.Empty;

    [StringLength(StoreTypeConsts.MaxIconUrlLength, ErrorMessage = "تجاوزت الحد الأقصى لرابط الأيقونة")]
    public string? IconUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
    #endregion
}
