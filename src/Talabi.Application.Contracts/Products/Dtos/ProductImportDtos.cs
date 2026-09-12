using System;
using System.Collections.Generic;
using Volo.Abp.Content;

namespace Talabi.Products.Dtos;

/// <summary>
/// كائن استيراد المنتجات لمتجر محدد من ملف Excel
/// </summary>
public class ImportProductFromExcelDto
{
    public Guid StoreId { get; set; }
    public IRemoteStreamContent File { get; set; } = null!;
}

/// <summary>
/// صف مقروء من ملف إكسل للمنتجات
/// </summary>
public class ImportProductRowDto
{
    public int RowNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// نتيجة استيراد المنتجات
/// </summary>
public class ImportProductResultDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ImportProductRowErrorDto> Errors { get; set; } = new();
}

public class ImportProductRowErrorDto
{
    public int RowNumber { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
