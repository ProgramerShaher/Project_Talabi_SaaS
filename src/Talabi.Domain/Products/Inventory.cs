using System;
using Talabi.Stores;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Talabi.Products;

// /// <summary>
// /// كيان إدارة ومراقبة مخزون المنتجات في المتاجر (معطل بناءً على متطلبات النظام: النظام للعرض والطلب فقط ولا يتدخل في إدارة المخزون أو الكميات)
// /// </summary>
// public class Inventory : FullAuditedEntity<Guid>, IMultiTenant
// {
//     /// <summary>
//     /// معرف المستأجر في نظام SaaS
//     /// </summary>
//     public virtual Guid? TenantId { get; set; }
// 
//     /// <summary>
//     /// معرف المتجر
//     /// </summary>
//     public virtual Guid StoreId { get; set; }
// 
//     /// <summary>
//     /// معرف المنتج المرتبط بالمخزون
//     /// </summary>
//     public virtual Guid ProductId { get; set; }
// 
//     /// <summary>
//     /// الكمية الفعلية المتوفرة في المستودع
//     /// </summary>
//     public virtual int CurrentQuantity { get; set; }
// 
//     /// <summary>
//     /// الكمية المحجوزة لطلبات لم تُسلم بعد
//     /// </summary>
//     public virtual int ReservedQuantity { get; set; }
// 
//     /// <summary>
//     /// الكمية المتاحة للبيع الفعلي (الكمية الفعلية - الكمية المحجوزة)
//     /// </summary>
//     public virtual int AvailableQuantity => Math.Max(0, CurrentQuantity - ReservedQuantity);
// 
//     /// <summary>
//     /// الحد الأدنى للمخزون قبل إطلاق تنبيه بالنفاذ
//     /// </summary>
//     public virtual int MinStockLevel { get; set; } = InventoryConsts.DefaultMinStockLevel;
// 
//     /// <summary>
//     /// السعة القصوى للمخزون
//     /// </summary>
//     public virtual int MaxStockLevel { get; set; } = InventoryConsts.DefaultMaxStockLevel;
// 
//     /// <summary>
//     /// نقطة إعادة الطلب
//     /// </summary>
//     public virtual int ReorderLevel { get; set; } = InventoryConsts.DefaultReorderLevel;
// 
//     /// <summary>
//     /// مكان التخزين في المستودع (الرف، الممر...)
//     /// </summary>
//     public virtual string? WarehouseLocation { get; set; }
// 
//     /// <summary>
//     /// تاريخ ووقت آخر عملية إعادة توريد للمخزون
//     /// </summary>
//     public virtual DateTime? LastRestockedAt { get; set; }
// 
//     /// <summary>
//     /// هل المخزون خاضع للمتابعة والتتبع التلقائي؟
//     /// </summary>
//     public virtual bool IsTracked { get; set; } = true;
// 
//     /// <summary>
//     /// المتجر المرتبط
//     /// </summary>
//     public virtual Store? Store { get; set; }
// 
//     /// <summary>
//     /// المنتج المرتبط
//     /// </summary>
//     public virtual Product? Product { get; set; }
// 
//     protected Inventory()
//     {
//     }
// 
//     public Inventory(
//         Guid id,
//         Guid storeId,
//         Guid productId,
//         int currentQuantity = 0,
//         Guid? tenantId = null)
//         : base(id)
//     {
//         StoreId = storeId;
//         ProductId = productId;
//         CurrentQuantity = currentQuantity;
//         ReservedQuantity = 0;
//         TenantId = tenantId;
//         IsTracked = true;
//         MinStockLevel = InventoryConsts.DefaultMinStockLevel;
//         MaxStockLevel = InventoryConsts.DefaultMaxStockLevel;
//         ReorderLevel = InventoryConsts.DefaultReorderLevel;
//     }
// }
