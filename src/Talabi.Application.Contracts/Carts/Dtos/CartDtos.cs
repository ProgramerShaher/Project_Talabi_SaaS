using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Carts.Dtos;

/// <summary>
/// كائن عرض بيانات سلة المشتريات
/// </summary>
public class CartDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid CustomerId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    #endregion
}

/// <summary>
/// كائن عرض عنصر أو منتج في السلة
/// </summary>
public class CartItemDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtAddition { get; set; }
    public decimal TotalPrice => UnitPriceAtAddition * Quantity;
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; }
    #endregion
}

/// <summary>
/// كائن إضافة منتج جديد إلى السلة
/// </summary>
public class AddToCartInput
{
    #region Properties
    [Required(ErrorMessage = "معرف المتجر مطلوب")]
    public Guid StoreId { get; set; }

    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "يجب أن تكون الكمية على الأقل 1")]
    public int Quantity { get; set; } = 1;

    [StringLength(CartConsts.MaxNotesLength)]
    public string? Notes { get; set; }
    #endregion
}

/// <summary>
/// كائن تعديل كمية منتج في السلة
/// </summary>
public class UpdateCartItemQuantityInput
{
    #region Properties
    [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من أو تساوي صفر")]
    public int Quantity { get; set; }
    #endregion
}
