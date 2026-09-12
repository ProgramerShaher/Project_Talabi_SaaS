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
/// خدمة إدارة تصنيفات المتاجر المخصصة
/// </summary>
[Authorize(TalabiPermissions.StoreCategories.Default)]
public class StoreCategoryAppService : ApplicationService, IStoreCategoryAppService
{
    #region Fields

    private readonly IRepository<StoreCategory, Guid> _storeCategoryRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// مُنشئ الخدمة مع حقن التبعيات
    /// </summary>
    public StoreCategoryAppService(IRepository<StoreCategory, Guid> storeCategoryRepository)
    {
        _storeCategoryRepository = storeCategoryRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// استرجاع شجرة تصنيفات متجر محدد مرتبة هرمياً
    /// </summary>
    public async Task<List<StoreCategoryDto>> GetTreeAsync(Guid storeId)
    {
        var queryable = await _storeCategoryRepository.GetQueryableAsync();

        var all = queryable
            .Where(c => c.StoreId == storeId && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.CustomName)
            .ToList();

        var roots = all.Where(c => c.ParentId == null).ToList();
        var result = BuildStoreCategoryTree(roots, all);

        Logger.LogInformation("تم استرجاع شجرة تصنيفات المتجر: {StoreId} - {Count} تصنيف رئيسي", storeId, result.Count);
        return result;
    }

    /// <summary>
    /// استرجاع قائمة تصنيفات المتجر مع الفلترة والباجيناشن
    /// </summary>
    public async Task<PagedResultDto<StoreCategoryDto>> GetListAsync(GetStoreCategoryListInput input)
    {
        var queryable = await _storeCategoryRepository.GetQueryableAsync();

        var query = queryable
            .Where(c => c.StoreId == input.StoreId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                c => c.CustomName != null && c.CustomName.Contains(input.Filter!))
            .WhereIf(input.ParentId.HasValue, c => c.ParentId == input.ParentId)
            .WhereIf(input.IsActive.HasValue, c => c.IsActive == input.IsActive);

        var totalCount = query.Count();
        var items = query
            .OrderBy(c => c.SortOrder)
            .PageBy(input)
            .ToList();

        return new PagedResultDto<StoreCategoryDto>(
            totalCount,
            ObjectMapper.Map<List<StoreCategory>, List<StoreCategoryDto>>(items));
    }

    /// <summary>
    /// استرجاع تصنيف متجر واحد بمعرفه
    /// </summary>
    public async Task<StoreCategoryDto> GetAsync(Guid id)
    {
        var category = await _storeCategoryRepository.GetAsync(id);
        return ObjectMapper.Map<StoreCategory, StoreCategoryDto>(category);
    }

    /// <summary>
    /// إنشاء تصنيف متجر جديد مع التحقق من صحة الأب
    /// </summary>
    [Authorize(TalabiPermissions.StoreCategories.Create)]
    public async Task<StoreCategoryDto> CreateAsync(CreateUpdateStoreCategoryDto input)
    {
        if (input.StoreId == Guid.Empty)
        {
            throw new UserFriendlyException("معرف المتجر مطلوب ولا يمكن أن يكون فارغاً.");
        }

        // التحقق من صحة الأب إذا وُجد
        if (input.ParentId.HasValue)
        {
            var parent = await _storeCategoryRepository.FindAsync(input.ParentId.Value)
                ?? throw new UserFriendlyException($"التصنيف الأب بالمعرف {input.ParentId} غير موجود.");

            if (parent.StoreId != input.StoreId)
            {
                throw new UserFriendlyException("التصنيف الأب لا ينتمي إلى نفس المتجر.");
            }
        }

        var category = new StoreCategory(
            GuidGenerator.Create(),
            input.StoreId,
            input.CustomName,
            input.ParentId,
            input.GlobalCategoryId,
            input.SortOrder)
        {
            Description = input.Description,
            CustomIconUrl = input.CustomIconUrl,
            IsHidden = input.IsHidden,
            IsActive = input.IsActive
        };

        await _storeCategoryRepository.InsertAsync(category);
        Logger.LogInformation("تم إنشاء تصنيف متجر جديد: {Name} للمتجر: {StoreId}", category.CustomName, category.StoreId);
        return ObjectMapper.Map<StoreCategory, StoreCategoryDto>(category);
    }

    /// <summary>
    /// تعديل بيانات تصنيف متجر موجود
    /// </summary>
    [Authorize(TalabiPermissions.StoreCategories.Edit)]
    public async Task<StoreCategoryDto> UpdateAsync(Guid id, CreateUpdateStoreCategoryDto input)
    {
        var category = await _storeCategoryRepository.GetAsync(id);

        if (input.ParentId.HasValue && input.ParentId.Value == id)
        {
            throw new UserFriendlyException("لا يمكن تعيين التصنيف كأب لنفسه.");
        }

        category.CustomName = input.CustomName;
        category.ParentId = input.ParentId;
        category.GlobalCategoryId = input.GlobalCategoryId;
        category.CustomIconUrl = input.CustomIconUrl;
        category.Description = input.Description;
        category.IsHidden = input.IsHidden;
        category.IsActive = input.IsActive;
        category.SortOrder = input.SortOrder;

        await _storeCategoryRepository.UpdateAsync(category);
        Logger.LogInformation("تم تعديل تصنيف المتجر: {Id}", id);
        return ObjectMapper.Map<StoreCategory, StoreCategoryDto>(category);
    }

    /// <summary>
    /// حذف تصنيف متجر مع منع الحذف إذا كان يحتوي على تصنيفات فرعية
    /// </summary>
    [Authorize(TalabiPermissions.StoreCategories.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var queryable = await _storeCategoryRepository.GetQueryableAsync();
        var hasChildren = queryable.Any(c => c.ParentId == id);

        if (hasChildren)
        {
            throw new UserFriendlyException("لا يمكن حذف هذا التصنيف لأنه يحتوي على تصنيفات فرعية. يرجى حذف التصنيفات الفرعية أولاً.");
        }

        await _storeCategoryRepository.DeleteAsync(id);
        Logger.LogInformation("تم حذف تصنيف المتجر: {Id}", id);
    }

    /// <summary>
    /// استيراد تصنيفات المتجر من ملف Excel
    /// الأعمدة: اسم_التصنيف | اسم_التصنيف_الأب | الوصف | مفعل
    /// </summary>
    [Authorize(TalabiPermissions.StoreCategories.Import)]
    public async Task<ImportCategoryResultDto> ImportFromExcelAsync(ImportStoreCategoryFromExcelDto input)
    {
        if (input.StoreId == Guid.Empty)
        {
            throw new UserFriendlyException("معرف المتجر مطلوب.");
        }

        if (input.File == null)
        {
            throw new UserFriendlyException("الرجاء رفع ملف Excel صالح.");
        }

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileName = input.File.FileName ?? string.Empty;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException("نوع الملف غير مدعوم. يجب أن يكون بصيغة .xlsx أو .xls");
        }

        var rows = ParseExcelRows(input.File);
        if (!rows.Any())
        {
            throw new UserFriendlyException("الملف فارغ أو لا يحتوي على بيانات.");
        }

        var result = new ImportCategoryResultDto();
        var createdInThisSession = new Dictionary<string, StoreCategory>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    result.FailureCount++;
                    result.Errors.Add(new ImportRowErrorDto { RowNumber = row.RowNumber, CategoryName = "(فارغ)", Reason = "اسم التصنيف مطلوب." });
                    continue;
                }

                Guid? parentId = null;

                if (!string.IsNullOrWhiteSpace(row.ParentName))
                {
                    if (createdInThisSession.TryGetValue(row.ParentName.Trim(), out var sessionParent))
                    {
                        parentId = sessionParent.Id;
                    }
                    else
                    {
                        var queryable = await _storeCategoryRepository.GetQueryableAsync();
                        var dbParent = queryable.FirstOrDefault(c => c.StoreId == input.StoreId && c.CustomName == row.ParentName.Trim());
                        if (dbParent == null)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new ImportRowErrorDto
                            {
                                RowNumber = row.RowNumber,
                                CategoryName = row.Name,
                                Reason = $"التصنيف الأب '{row.ParentName}' غير موجود. تأكد من ترتيب الصفوف (الأب قبل الابن)."
                            });
                            continue;
                        }
                        parentId = dbParent.Id;
                    }
                }

                if (createdInThisSession.ContainsKey(row.Name.Trim()))
                {
                    result.FailureCount++;
                    result.Errors.Add(new ImportRowErrorDto { RowNumber = row.RowNumber, CategoryName = row.Name, Reason = "اسم التصنيف مكرر في نفس الملف." });
                    continue;
                }

                var category = new StoreCategory(
                    GuidGenerator.Create(),
                    input.StoreId,
                    row.Name.Trim(),
                    parentId)
                {
                    Description = row.Description,
                    IsActive = row.IsActive
                };

                await _storeCategoryRepository.InsertAsync(category);
                createdInThisSession[row.Name.Trim()] = category;
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(new ImportRowErrorDto { RowNumber = row.RowNumber, CategoryName = row.Name, Reason = ex.Message });
            }
        }

        Logger.LogInformation("اكتمل استيراد تصنيفات المتجر {StoreId}: {Success} ناجح, {Fail} فاشل",
            input.StoreId, result.SuccessCount, result.FailureCount);

        return result;
    }

    #endregion

    #region Private Helpers

    private List<StoreCategoryDto> BuildStoreCategoryTree(List<StoreCategory> roots, List<StoreCategory> all)
    {
        var result = new List<StoreCategoryDto>();
        foreach (var root in roots)
        {
            var dto = ObjectMapper.Map<StoreCategory, StoreCategoryDto>(root);
            var children = all.Where(c => c.ParentId == root.Id).OrderBy(c => c.SortOrder).ToList();
            dto.Children = BuildStoreCategoryTree(children, all);
            result.Add(dto);
        }
        return result;
    }

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

    #endregion
}
