using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Categories.Dtos;

/// <summary>
/// كائن عرض تصنيف المتجر
/// </summary>
public class StoreCategoryDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid StoreId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? GlobalCategoryId { get; set; }
    public string? CustomName { get; set; }
    public string? CustomIconUrl { get; set; }
    public string? Description { get; set; }
    public bool IsHidden { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public List<StoreCategoryDto> Children { get; set; } = new();
    #endregion
}

/// <summary>
/// كائن إنشاء أو تعديل تصنيف متجر
/// </summary>
public class CreateUpdateStoreCategoryDto
{
    #region Properties
    [Required(ErrorMessage = "معرف المتجر مطلوب")]
    public Guid StoreId { get; set; }

    public Guid? ParentId { get; set; }
    public Guid? GlobalCategoryId { get; set; }

    [StringLength(StoreCategoryConsts.MaxCustomNameLength)]
    public string? CustomName { get; set; }

    [StringLength(StoreCategoryConsts.MaxCustomIconUrlLength)]
    public string? CustomIconUrl { get; set; }

    [StringLength(StoreCategoryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public bool IsHidden { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    #endregion
}
