using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Talabi.Products;

/// <summary>
/// كيان صور المنتج الإضافية والمعرض
/// </summary>
public class ProductImage : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// معرف المنتج التابع له الصورة
    /// </summary>
    public virtual Guid ProductId { get; set; }

    /// <summary>
    /// رابط الصورة الكاملة
    /// </summary>
    public virtual string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// رابط الصورة المصغرة (Thumbnail)
    /// </summary>
    public virtual string? ThumbnailUrl { get; set; }

    /// <summary>
    /// نص وصفي بديل للصورة (Alt Text)
    /// </summary>
    public virtual string? AltText { get; set; }

    /// <summary>
    /// ترتيب عرض الصورة في السلايدر
    /// </summary>
    public virtual int DisplayOrder { get; set; }

    /// <summary>
    /// هل هذه هي الصورة الأساسية للمنتج؟
    /// </summary>
    public virtual bool IsPrimary { get; set; }

    /// <summary>
    /// كائن المنتج المرتبط
    /// </summary>
    public virtual Product? Product { get; set; }

    protected ProductImage()
    {
    }

    public ProductImage(
        Guid id,
        Guid productId,
        string imageUrl,
        int displayOrder = 0,
        bool isPrimary = false,
        string? thumbnailUrl = null,
        string? altText = null)
        : base(id)
    {
        ProductId = productId;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        ThumbnailUrl = thumbnailUrl;
        AltText = altText;
    }
}
