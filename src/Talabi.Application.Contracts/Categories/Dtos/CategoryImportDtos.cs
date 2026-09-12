using System;
using System.Collections.Generic;
using Volo.Abp.Content;

namespace Talabi.Categories.Dtos;

/// <summary>
/// كائن استيراد التصنيفات العامة من ملف Excel
/// </summary>
public class ImportCategoryFromExcelDto
{
    /// <summary>
    /// ملف Excel المرفوع (يجب أن يكون بصيغة .xlsx أو .xls)
    /// </summary>
    public IRemoteStreamContent File { get; set; } = null!;
}

/// <summary>
/// كائن استيراد تصنيفات متجر من ملف Excel
/// </summary>
public class ImportStoreCategoryFromExcelDto
{
    /// <summary>
    /// معرف المتجر الذي سيتم استيراد التصنيفات إليه
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// ملف Excel المرفوع
    /// الأعمدة المطلوبة: اسم_التصنيف | اسم_التصنيف_الأب | الوصف | مفعل
    /// </summary>
    public IRemoteStreamContent File { get; set; } = null!;
}

/// <summary>
/// كائن يمثل صف واحد مُقرأ من ملف Excel المستورَد
/// </summary>
public class ImportCategoryRowDto
{
    /// <summary>
    /// اسم التصنيف
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اسم التصنيف الأب (فارغ إذا كان التصنيف رئيسياً)
    /// </summary>
    public string? ParentName { get; set; }

    /// <summary>
    /// وصف التصنيف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// هل التصنيف مفعل
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// رقم الصف في ملف Excel (لعرضه عند الخطأ)
    /// </summary>
    public int RowNumber { get; set; }
}

/// <summary>
/// نتيجة عملية الاستيراد من ملف Excel
/// </summary>
public class ImportCategoryResultDto
{
    /// <summary>
    /// عدد التصنيفات التي تم إنشاؤها بنجاح
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// عدد الصفوف التي فشل استيرادها
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// قائمة أخطاء الصفوف الفاشلة مع رقم الصف وسبب الخطأ
    /// </summary>
    public List<ImportRowErrorDto> Errors { get; set; } = new();
}

/// <summary>
/// كائن يصف خطأ في صف محدد عند الاستيراد
/// </summary>
public class ImportRowErrorDto
{
    /// <summary>
    /// رقم الصف في ملف Excel
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// اسم التصنيف في هذا الصف
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// سبب الفشل
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// كائن فلترة قائمة تصنيفات المتاجر مع الباجيناشن
/// </summary>
public class GetStoreCategoryListInput : Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
{
    /// <summary>
    /// معرف المتجر (مطلوب)
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// فلترة بالاسم
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// فلترة بمعرف التصنيف الأب
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// فلترة حسب حالة التفعيل
    /// </summary>
    public bool? IsActive { get; set; }
}
