using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Categories.Dtos;
using Talabi.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace Talabi.Categories;

/// <summary>
/// خدمة إدارة التصنيفات العامة للمنصة
/// </summary>
[Authorize(TalabiPermissions.Categories.Default)]
public class CategoryAppService : ApplicationService, ICategoryAppService
{
    #region Fields

    private readonly IRepository<Category, Guid> _categoryRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// مُنشئ الخدمة مع حقن التبعيات
    /// </summary>
    public CategoryAppService(IRepository<Category, Guid> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// استرجاع شجرة التصنيفات الكاملة مرتبة هرمياً (التصنيفات الجذرية مع أبنائها)
    /// </summary>
    public async Task<List<CategoryDto>> GetTreeAsync()
    {
        var queryable = await _categoryRepository.GetQueryableAsync();

        var allCategories = queryable
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToList();

        var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
        var result = BuildTree(rootCategories, allCategories);

        Logger.LogInformation("تم استرجاع شجرة التصنيفات: {Count} تصنيف رئيسي", result.Count);
        return result;
    }

    /// <summary>
    /// استرجاع قائمة التصنيفات مع الفلترة والباجيناشن
    /// </summary>
    public async Task<PagedResultDto<CategoryDto>> GetListAsync(GetCategoryListInput input)
    {
        var queryable = await _categoryRepository.GetQueryableAsync();

        var query = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                c => c.Name.Contains(input.Filter!) || (c.Description != null && c.Description.Contains(input.Filter!)))
            .WhereIf(input.ParentId.HasValue, c => c.ParentId == input.ParentId)
            .WhereIf(input.IsGlobal.HasValue, c => c.IsGlobal == input.IsGlobal)
            .WhereIf(input.IsActive.HasValue, c => c.IsActive == input.IsActive);

        var totalCount = query.Count();
        var items = query
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .PageBy(input)
            .ToList();

        Logger.LogInformation("استرجاع قائمة التصنيفات: {Total} تصنيف", totalCount);
        return new PagedResultDto<CategoryDto>(totalCount, ObjectMapper.Map<List<Category>, List<CategoryDto>>(items));
    }

    /// <summary>
    /// استرجاع تصنيف واحد بمعرفه
    /// </summary>
    public async Task<CategoryDto> GetAsync(Guid id)
    {
        var category = await _categoryRepository.GetAsync(id);
        return ObjectMapper.Map<Category, CategoryDto>(category);
    }

    /// <summary>
    /// إنشاء تصنيف جديد مع التحقق من عدم تكرار الـ Slug وصحة الأب
    /// </summary>
    [Authorize(TalabiPermissions.Categories.Create)]
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto input)
    {
        // التحقق من عدم تكرار الـ Slug
        await EnsureSlugIsUniqueAsync(input.Slug);

        // التحقق من صحة التصنيف الأب إذا وُجد
        int level = 0;
        string? path = null;
        if (input.ParentId.HasValue)
        {
            var parent = await _categoryRepository.FindAsync(input.ParentId.Value)
                ?? throw new UserFriendlyException($"التصنيف الأب بالمعرف {input.ParentId} غير موجود.");
            level = parent.Level + 1;
            path = $"{parent.Path}/{input.ParentId}";
        }

        var category = new Category(
            GuidGenerator.Create(),
            input.Name,
            input.Slug,
            input.ParentId,
            input.SortOrder,
            level,
            input.IsGlobal,
            input.IsActive)
        {
            Description = input.Description,
            IconUrl = input.IconUrl,
            ImageUrl = input.ImageUrl,
            Path = path
        };

        await _categoryRepository.InsertAsync(category);
        Logger.LogInformation("تم إنشاء تصنيف جديد: {Name} بالمستوى {Level}", category.Name, category.Level);
        return ObjectMapper.Map<Category, CategoryDto>(category);
    }

    /// <summary>
    /// تعديل بيانات تصنيف موجود
    /// </summary>
    [Authorize(TalabiPermissions.Categories.Edit)]
    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input)
    {
        var category = await _categoryRepository.GetAsync(id);

        // التحقق من عدم تكرار الـ Slug إذا تغير
        if (!string.Equals(category.Slug, input.Slug, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureSlugIsUniqueAsync(input.Slug, id);
        }

        // منع التصنيف من أن يكون ابناً لنفسه أو لأحد أبنائه
        if (input.ParentId.HasValue && input.ParentId.Value == id)
        {
            throw new UserFriendlyException("لا يمكن تعيين التصنيف كأب لنفسه.");
        }

        category.Name = input.Name;
        category.Slug = input.Slug;
        category.ParentId = input.ParentId;
        category.Description = input.Description;
        category.IconUrl = input.IconUrl;
        category.ImageUrl = input.ImageUrl;
        category.IsGlobal = input.IsGlobal;
        category.IsActive = input.IsActive;
        category.SortOrder = input.SortOrder;

        await _categoryRepository.UpdateAsync(category);
        Logger.LogInformation("تم تعديل التصنيف: {Name} (Id: {Id})", category.Name, category.Id);
        return ObjectMapper.Map<Category, CategoryDto>(category);
    }

    /// <summary>
    /// حذف تصنيف مع منع الحذف إذا كان يحتوي على تصنيفات فرعية
    /// </summary>
    [Authorize(TalabiPermissions.Categories.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var queryable = await _categoryRepository.GetQueryableAsync();
        var hasChildren = queryable.Any(c => c.ParentId == id);

        if (hasChildren)
        {
            throw new UserFriendlyException("لا يمكن حذف هذا التصنيف لأنه يحتوي على تصنيفات فرعية. يرجى حذف التصنيفات الفرعية أولاً.");
        }

        await _categoryRepository.DeleteAsync(id);
        Logger.LogInformation("تم حذف التصنيف: {Id}", id);
    }

    /// <summary>
    /// استيراد التصنيفات من ملف Excel بدعم الهيكل الشجري
    /// الأعمدة المطلوبة: اسم_التصنيف | اسم_التصنيف_الأب | الوصف | مفعل
    /// </summary>
    [Authorize(TalabiPermissions.Categories.Import)]
    public async Task<ImportCategoryResultDto> ImportFromExcelAsync(ImportCategoryFromExcelDto input)
    {
        if (input.File == null)
        {
            throw new UserFriendlyException("الرجاء رفع ملف Excel صالح.");
        }

        // التحقق من نوع الملف
        var fileName = input.File.FileName ?? string.Empty;
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".xlsx", ".xls" };
        if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException("نوع الملف غير مدعوم. يجب أن يكون الملف بصيغة .xlsx أو .xls");
        }

        // قراءة الصفوف من الملف
        var rows = ParseExcelRows(input.File);
        if (!rows.Any())
        {
            throw new UserFriendlyException("الملف فارغ أو لا يحتوي على بيانات.");
        }

        var result = new ImportCategoryResultDto();

        // قاموس لتتبع الأصناف التي تم إنشاؤها خلال نفس عملية الاستيراد (الاسم → Category)
        var createdInThisSession = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                // التحقق من وجود الاسم
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    result.FailureCount++;
                    result.Errors.Add(new ImportRowErrorDto
                    {
                        RowNumber = row.RowNumber,
                        CategoryName = "(فارغ)",
                        Reason = "اسم التصنيف مطلوب ولا يمكن أن يكون فارغاً."
                    });
                    continue;
                }

                // تحديد الأب
                Guid? parentId = null;
                int level = 0;
                string? path = null;

                if (!string.IsNullOrWhiteSpace(row.ParentName))
                {
                    // البحث عن الأب في الصفوف المستوردة في نفس الجلسة أولاً
                    if (createdInThisSession.TryGetValue(row.ParentName.Trim(), out var sessionParent))
                    {
                        parentId = sessionParent.Id;
                        level = sessionParent.Level + 1;
                        path = $"{sessionParent.Path}/{sessionParent.Id}";
                    }
                    else
                    {
                        // البحث في قاعدة البيانات
                        var queryable = await _categoryRepository.GetQueryableAsync();
                        var dbParent = queryable.FirstOrDefault(c => c.Name == row.ParentName.Trim());
                        if (dbParent == null)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new ImportRowErrorDto
                            {
                                RowNumber = row.RowNumber,
                                CategoryName = row.Name,
                                Reason = $"التصنيف الأب '{row.ParentName}' غير موجود في النظام ولم يتم إنشاؤه في هذه الدفعة. تأكد من ترتيب الصفوف (الأب قبل الابن)."
                            });
                            continue;
                        }

                        parentId = dbParent.Id;
                        level = dbParent.Level + 1;
                        path = $"{dbParent.Path}/{dbParent.Id}";
                    }
                }

                // توليد Slug تلقائي من الاسم
                var slug = GenerateSlug(row.Name);

                // التحقق من تكرار الاسم في نفس الجلسة
                if (createdInThisSession.ContainsKey(row.Name.Trim()))
                {
                    result.FailureCount++;
                    result.Errors.Add(new ImportRowErrorDto
                    {
                        RowNumber = row.RowNumber,
                        CategoryName = row.Name,
                        Reason = "اسم التصنيف مكرر في نفس الملف."
                    });
                    continue;
                }

                var category = new Category(
                    GuidGenerator.Create(),
                    row.Name.Trim(),
                    slug,
                    parentId,
                    sortOrder: 0,
                    level: level,
                    isGlobal: true,
                    isActive: row.IsActive)
                {
                    Description = row.Description,
                    Path = path
                };

                await _categoryRepository.InsertAsync(category);
                createdInThisSession[row.Name.Trim()] = category;
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(new ImportRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    CategoryName = row.Name,
                    Reason = ex.Message
                });
            }
        }

        Logger.LogInformation("اكتمل استيراد التصنيفات: {Success} ناجح, {Fail} فاشل",
            result.SuccessCount, result.FailureCount);

        return result;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// بناء الشجرة الهرمية للتصنيفات بشكل تكراري
    /// </summary>
    private List<CategoryDto> BuildTree(List<Category> roots, List<Category> all)
    {
        var result = new List<CategoryDto>();
        foreach (var root in roots)
        {
            var dto = ObjectMapper.Map<Category, CategoryDto>(root);
            var children = all.Where(c => c.ParentId == root.Id).OrderBy(c => c.SortOrder).ToList();
            dto.Children = BuildTree(children, all);
            result.Add(dto);
        }
        return result;
    }

    /// <summary>
    /// التحقق من أن الـ Slug غير مكرر في قاعدة البيانات
    /// </summary>
    private async Task EnsureSlugIsUniqueAsync(string slug, Guid? excludeId = null)
    {
        var queryable = await _categoryRepository.GetQueryableAsync();
        var exists = queryable
            .WhereIf(excludeId.HasValue, c => c.Id != excludeId!.Value)
            .Any(c => c.Slug == slug.ToLowerInvariant().Trim());

        if (exists)
        {
            throw new UserFriendlyException($"الرابط اللطيف (Slug) '{slug}' مستخدم بالفعل في تصنيف آخر.");
        }
    }

    /// <summary>
    /// قراءة صفوف ملف Excel وتحويلها إلى قائمة كائنات
    /// </summary>
    private static List<ImportCategoryRowDto> ParseExcelRows(IRemoteStreamContent file)
    {
        var rows = new List<ImportCategoryRowDto>();
        using var stream = file.GetStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int rowNum = 2; rowNum <= lastRow; rowNum++)
        {
            var row = worksheet.Row(rowNum);
            var name = row.Cell(1).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var parentName = row.Cell(2).GetString()?.Trim();
            var description = row.Cell(3).GetString()?.Trim();
            var isActiveStr = row.Cell(4).GetString()?.Trim().ToLowerInvariant();
            var isActive = isActiveStr != "لا" && isActiveStr != "no" && isActiveStr != "false" && isActiveStr != "0";

            rows.Add(new ImportCategoryRowDto
            {
                RowNumber = rowNum,
                Name = name,
                ParentName = string.IsNullOrWhiteSpace(parentName) ? null : parentName,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                IsActive = isActive
            });
        }

        return rows;
    }

    /// <summary>
    /// توليد Slug لطيف تلقائياً من اسم التصنيف
    /// </summary>
    private static string GenerateSlug(string name)
    {
        return name.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            + "-" + Guid.NewGuid().ToString("N")[..6];
    }

    #endregion
}
