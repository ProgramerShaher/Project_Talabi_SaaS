using System.ComponentModel.DataAnnotations;

namespace Talabi.Stores.Dtos;

/// <summary>
/// كائن إرسال بيانات تعديل نوع متجر
/// </summary>
public class UpdateStoreTypeDto
{
    [Required(ErrorMessage = "اسم النوع مطلوب")]
    [StringLength(StoreTypeConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(StoreTypeConsts.MaxIconUrlLength)]
    public string? IconUrl { get; set; }

    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; }
}
