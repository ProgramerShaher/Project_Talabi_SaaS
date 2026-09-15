using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Orders.Dtos;

/// <summary>
/// كائن عرض بيانات الطلب للعميل أو المتجر
/// </summary>
public class OrderDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid? TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public Guid DeliveryAddressId { get; set; }
    public string DeliveryAddressSnapshot { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    // public decimal DeliveryFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public Guid OrderStatusId { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;
    public string? OrderStatusColor { get; set; }
    public OrderPaymentStatus PaymentStatus { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public Guid? CourierId { get; set; }
    public string? CourierName { get; set; }
    public string? CustomerNotes { get; set; }
    public string? StoreNotes { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? PaymentReceiptUrl { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    #endregion
}

/// <summary>
/// كائن عنصر من عناصر الطلب
/// </summary>
public class OrderItemDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    #endregion
}

/// <summary>
/// كائن إنشاء طلب جديد
/// </summary>
public class CreateOrderInput
{
    #region Properties
    [Required(ErrorMessage = "معرف المتجر مطلوب")]
    public Guid StoreId { get; set; }

    [Required(ErrorMessage = "معرف عنوان التوصيل مطلوب")]
    public Guid DeliveryAddressId { get; set; }

    [Required(ErrorMessage = "معرف طريقة الدفع مطلوب")]
    public Guid PaymentMethodId { get; set; }

    [StringLength(OrderConsts.MaxCustomerNotesLength)]
    public string? CustomerNotes { get; set; }

    [MaxLength(2048, ErrorMessage = "رابط صورة الإيصال طويل جداً")]
    public string? PaymentReceiptUrl { get; set; }

    [Required(ErrorMessage = "يجب تحديد عناصر الطلب")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
    #endregion
}

/// <summary>
/// كائن عنصر جديد عند إنشاء الطلب
/// </summary>
public class CreateOrderItemDto
{
    #region Properties
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; } = 1;

    [StringLength(OrderItemConsts.MaxNotesLength)]
    public string? Notes { get; set; }
    #endregion
}

/// <summary>
/// كائن طلب وفلترة قائمة الطلبات
/// </summary>
public class GetOrderListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public string? Filter { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? OrderStatusId { get; set; }
    public OrderPaymentStatus? PaymentStatus { get; set; }
    public Guid? CourierId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    #endregion
}

/// <summary>
/// كائن إتمام الطلب مباشرة من سلة المشتريات
/// </summary>
public class CheckoutCartInput
{
    #region Properties
    [Required(ErrorMessage = "معرف عنوان التوصيل مطلوب")]
    public Guid DeliveryAddressId { get; set; }

    [Required(ErrorMessage = "معرف طريقة الدفع مطلوب")]
    public Guid PaymentMethodId { get; set; }

    [StringLength(OrderConsts.MaxCustomerNotesLength)]
    public string? CustomerNotes { get; set; }

    [MaxLength(2048, ErrorMessage = "رابط صورة الإيصال طويل جداً")]
    public string? PaymentReceiptUrl { get; set; }
    #endregion
}
