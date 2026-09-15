using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Talabi.Carts.Dtos;

/// <summary>
/// كائن عرض سلة مشتريات العميل الحالي
/// </summary>
public class ShoppingCartDto : EntityDto<Guid?>
{
    #region Properties

    public Guid CustomerId { get; set; }

    public Guid? StoreId { get; set; }

    public string StoreName { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal TotalDiscount { get; set; }

    public decimal FinalTotal { get; set; }

    public bool HasUnavailableItems { get; set; }

    public List<string> Warnings { get; set; } = new();

    public List<ShoppingCartItemDto> Items { get; set; } = new();

    #endregion
}
