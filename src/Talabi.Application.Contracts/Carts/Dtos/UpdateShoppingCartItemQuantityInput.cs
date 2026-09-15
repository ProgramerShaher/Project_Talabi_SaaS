using System.ComponentModel.DataAnnotations;

namespace Talabi.Carts.Dtos;

/// <summary>
/// كائن تعديل كمية عنصر داخل سلة مشتريات العميل الحالي
/// </summary>
public class UpdateShoppingCartItemQuantityInput
{
    #region Properties

    [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من أو تساوي صفر")]
    public int Quantity { get; set; }

    #endregion
}
