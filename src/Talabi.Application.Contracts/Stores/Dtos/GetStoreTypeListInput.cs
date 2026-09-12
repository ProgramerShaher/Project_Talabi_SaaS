using Volo.Abp.Application.Dtos;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن طلب وفلترة قائمة أنواع المتاجر
/// </summary>
public class GetStoreTypeListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    
    public bool? IsActive { get; set; }
}
