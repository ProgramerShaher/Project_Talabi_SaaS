using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talabi.Categories.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Categories;

/// <summary>
/// واجهة خدمة التصنيفات العامة للمنصة
/// </summary>
public interface ICategoryAppService : IApplicationService
{
    /// <summary>
    /// استرجاع شجرة التصنيفات الكاملة مرتبة هرمياً
    /// </summary>
    Task<List<CategoryDto>> GetTreeAsync();

    /// <summary>
    /// استرجاع قائمة التصنيفات مع الفلترة والباجيناشن
    /// </summary>
    Task<PagedResultDto<CategoryDto>> GetListAsync(GetCategoryListInput input);

    /// <summary>
    /// استرجاع تصنيف واحد بمعرفه
    /// </summary>
    Task<CategoryDto> GetAsync(Guid id);

    /// <summary>
    /// إنشاء تصنيف جديد
    /// </summary>
    Task<CategoryDto> CreateAsync(CreateCategoryDto input);

    /// <summary>
    /// تعديل بيانات تصنيف موجود
    /// </summary>
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input);

    /// <summary>
    /// حذف تصنيف (يمنع الحذف إذا كان يحتوي على تصنيفات فرعية أو منتجات)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// استيراد التصنيفات من ملف Excel (يدعم التصنيفات الرئيسية والفرعية)
    /// </summary>
    Task<ImportCategoryResultDto> ImportFromExcelAsync(ImportCategoryFromExcelDto input);
}
