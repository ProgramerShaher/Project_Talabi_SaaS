using System;
using Volo.Abp.Application.Dtos;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن إرجاع بيانات نوع المتجر
/// </summary>
public class StoreTypeDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
