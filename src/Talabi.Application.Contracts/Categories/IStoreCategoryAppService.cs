using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talabi.Categories.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Categories;

/// <summary>
/// واجهة خدمة تصنيفات المتاجر المخصصة
/// </summary>
public interface IStoreCategoryAppService : IApplicationService
{
    /// <summary>
    /// استرجاع شجرة تصنيفات متجر محدد مرتبة هرمياً
    /// </summary>
    Task<List<StoreCategoryDto>> GetTreeAsync(Guid storeId);

    /// <summary>
    /// استرجاع قائمة تصنيفات المتجر مع الفلترة والباجيناشن
    /// </summary>
    Task<PagedResultDto<StoreCategoryDto>> GetListAsync(GetStoreCategoryListInput input);

    /// <summary>
    /// استرجاع تصنيف متجر واحد بمعرفه
    /// </summary>
    Task<StoreCategoryDto> GetAsync(Guid id);

    /// <summary>
    /// إنشاء تصنيف متجر جديد
    /// </summary>
    Task<StoreCategoryDto> CreateAsync(CreateUpdateStoreCategoryDto input);

    /// <summary>
    /// تعديل بيانات تصنيف متجر موجود
    /// </summary>
    Task<StoreCategoryDto> UpdateAsync(Guid id, CreateUpdateStoreCategoryDto input);

    /// <summary>
    /// حذف تصنيف متجر
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// استيراد تصنيفات المتجر من ملف Excel
    /// </summary>
    Task<ImportCategoryResultDto> ImportFromExcelAsync(ImportStoreCategoryFromExcelDto input);
}
