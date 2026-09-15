using System.ComponentModel.DataAnnotations;

namespace Talabi.Customers.Dtos;

/// <summary>
/// كائن تحديث رابط الصورة الشخصية للعميل
/// </summary>
public class UpdateCustomerAvatarDto
{
    [Required(ErrorMessage = "رابط الصورة مطلوب")]
    [StringLength(CustomerConsts.MaxAvatarUrlLength, ErrorMessage = "تجاوزت الحد الأقصى لطول الرابط")]
    public string AvatarUrl { get; set; } = string.Empty;
}
