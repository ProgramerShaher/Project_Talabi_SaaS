using System;
using System.ComponentModel.DataAnnotations;

namespace Talabi.Carts.Dtos;

/// <summary>
/// كائن إضافة منتج إلى سلة مشتريات العميل الحالي
/// </summary>
public class AddShoppingCartItemInput
{
    #region Properties

    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية على الأقل 1")]
    public int Quantity { get; set; } = 1;

    [StringLength(CartConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    #endregion
}
