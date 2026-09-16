using System.ComponentModel.DataAnnotations;

namespace Talabi.Carts.Dtos;

/// <summary>
/// كائن تعديل كمية عنصر داخل سلة مشتريات العميل الحالي
/// </summary>
public class UpdateShoppingCartItemQuantityInput
{
    #region Properties

    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل. لإلغاء شراء المنتج، الرجاء حذفه من السلة.")]
    public int Quantity { get; set; }

    #endregion
}
