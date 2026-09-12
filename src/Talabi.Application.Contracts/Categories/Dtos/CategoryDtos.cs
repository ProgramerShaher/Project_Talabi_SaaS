using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Categories.Dtos;

/// <summary>
/// كائن عرض التصنيف العام
/// </summary>
public class CategoryDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsGlobal { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; }
    public List<CategoryDto> Children { get; set; } = new();
    #endregion
}

/// <summary>
/// كائن إنشاء تصنيف عام جديد
/// </summary>
public class CreateCategoryDto
{
    #region Properties
    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [StringLength(CategoryConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرابط اللطيف مطلوب")]
    [StringLength(CategoryConsts.MaxSlugLength)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(CategoryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(CategoryConsts.MaxIconUrlLength)]
    public string? IconUrl { get; set; }

    [StringLength(CategoryConsts.MaxImageUrlLength)]
    public string? ImageUrl { get; set; }

    public bool IsGlobal { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    #endregion
}

/// <summary>
/// كائن تعديل تصنيف عام
/// </summary>
public class UpdateCategoryDto
{
    #region Properties
    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [StringLength(CategoryConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرابط اللطيف مطلوب")]
    [StringLength(CategoryConsts.MaxSlugLength)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(CategoryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(CategoryConsts.MaxIconUrlLength)]
    public string? IconUrl { get; set; }

    [StringLength(CategoryConsts.MaxImageUrlLength)]
    public string? ImageUrl { get; set; }

    public bool IsGlobal { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    #endregion
}

/// <summary>
/// كائن فلترة وبحث التصنيفات
/// </summary>
public class GetCategoryListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public string? Filter { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsGlobal { get; set; }
    public bool? IsActive { get; set; }
    #endregion
}
