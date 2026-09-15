using System;
using Volo.Abp.Application.Dtos;

namespace Talabi.Carts.Dtos;

/// <summary>
/// كائن عرض عنصر داخل سلة مشتريات العميل
/// </summary>
public class ShoppingCartItemDto : EntityDto<Guid>
{
    #region Properties

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ProductImageUrl { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal OriginalUnitPrice { get; set; }

    public decimal UnitDiscount { get; set; }

    public decimal TotalPrice { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public int MinOrderQuantity { get; set; }

    public int? MaxOrderQuantity { get; set; }

    public string? Notes { get; set; }

    public string? WarningCode { get; set; }

    public string? WarningMessage { get; set; }

    #endregion
}
