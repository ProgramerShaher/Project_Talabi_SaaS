using System.ComponentModel.DataAnnotations;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن إرسال بيانات إنشاء نوع متجر جديد
/// </summary>
public class CreateStoreTypeDto
{
    [Required(ErrorMessage = "اسم النوع مطلوب")]
    [StringLength(StoreTypeConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(StoreTypeConsts.MaxIconUrlLength)]
    public string? IconUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
}
