using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Interactions.Dtos;

/// <summary>
/// كائن عرض التقييم والمراجعة
/// </summary>
public class ReviewDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    // public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    // public Guid? CourierId { get; set; }
    public int StoreRating { get; set; }
    // public int? CourierRating { get; set; }
    public int? ProductQualityRating { get; set; }
    // public int? DeliverySpeedRating { get; set; }
    public string? Comment { get; set; }
    public string? StoreReply { get; set; }
    public DateTime? StoreRepliedAt { get; set; }
    public bool IsVisible { get; set; }
    #endregion
}

/// <summary>
/// كائن إرسال تقييم جديد للنشاط التجاري
/// </summary>
public class CreateReviewInput
{
    #region Properties
    // [Required(ErrorMessage = "معرف الطلب مطلوب")]
    // public Guid OrderId { get; set; }

    [Required(ErrorMessage = "معرف النشاط التجاري مطلوب")]
    public Guid StoreId { get; set; }

    [Range(ReviewConsts.MinRating, ReviewConsts.MaxRating, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    public int StoreRating { get; set; }

    // [Range(ReviewConsts.MinRating, ReviewConsts.MaxRating)]
    // public int? CourierRating { get; set; }

    [Range(ReviewConsts.MinRating, ReviewConsts.MaxRating)]
    public int? ProductQualityRating { get; set; }

    // [Range(ReviewConsts.MinRating, ReviewConsts.MaxRating)]
    // public int? DeliverySpeedRating { get; set; }

    [StringLength(ReviewConsts.MaxCommentLength)]
    public string? Comment { get; set; }
    #endregion
}

/// <summary>
/// كائن تعديل تقييم سابق للنشاط التجاري
/// </summary>
public class UpdateReviewInput
{
    #region Properties
    [Range(ReviewConsts.MinRating, ReviewConsts.MaxRating, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
    public int StoreRating { get; set; }

    [Range(ReviewConsts.MinRating, ReviewConsts.MaxRating)]
    public int? ProductQualityRating { get; set; }

    [StringLength(ReviewConsts.MaxCommentLength)]
    public string? Comment { get; set; }
    #endregion
}

/// <summary>
/// كائن إرسال رد المتجر الرسمي على التقييم
/// </summary>
public class StoreReplyInput
{
    #region Properties
    [Required(ErrorMessage = "نص رد المتجر إلزامي")]
    [StringLength(ReviewConsts.MaxStoreReplyLength, ErrorMessage = "لا يمكن أن يتجاوز رد المتجر 1000 حرف")]
    public string Reply { get; set; } = string.Empty;
    #endregion
}

/// <summary>
/// كائن ملخص إحصائيات تقييمات المتجر وتوزيع النجوم
/// </summary>
public class StoreReviewSummaryDto
{
    #region Properties
    /// <summary>
    /// معرف المتجر
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// اسم المتجر
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// متوسط التقييم الإجمالي (من 0 إلى 5)
    /// </summary>
    public decimal AverageRating { get; set; }

    /// <summary>
    /// إجمالي عدد التقييمات
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// عدد تقييمات 5 نجوم
    /// </summary>
    public int FiveStarCount { get; set; }

    /// <summary>
    /// عدد تقييمات 4 نجوم
    /// </summary>
    public int FourStarCount { get; set; }

    /// <summary>
    /// عدد تقييمات 3 نجوم
    /// </summary>
    public int ThreeStarCount { get; set; }

    /// <summary>
    /// عدد تقييمات نجمتين
    /// </summary>
    public int TwoStarCount { get; set; }

    /// <summary>
    /// عدد تقييمات نجمة واحدة
    /// </summary>
    public int OneStarCount { get; set; }
    #endregion
}

/// <summary>
/// كائن طلب وفلترة التقييمات
/// </summary>
public class GetReviewListInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public Guid? StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    public int? MinRating { get; set; }
    #endregion
}

/// <summary>
/// كائن عرض العنصر المفضل
/// </summary>
public class FavoriteDto : CreationAuditedEntityDto<Guid>
{
    #region Properties
    public Guid CustomerId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? EntityImageUrl { get; set; }
    #endregion
}

/// <summary>
/// كائن إضافة أو إزالة عنصر من المفضلة
/// </summary>
public class ToggleFavoriteInput
{
    #region Properties
    [Required(ErrorMessage = "نوع الكيان مطلوب")]
    [StringLength(FavoriteConsts.MaxEntityTypeLength)]
    public string EntityType { get; set; } = string.Empty;

    [Required(ErrorMessage = "معرف الكيان مطلوب")]
    public Guid EntityId { get; set; }
    #endregion
}

/// <summary>
/// كائن نتيجة تبديل حالة المفضلة
/// </summary>
public class ToggleFavoriteResultDto
{
    #region Properties
    public bool IsFavorite { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Message { get; set; } = string.Empty;
    #endregion
}

/// <summary>
/// كائن عرض المتجر المفضل مع تفاصيله الكاملة
/// </summary>
public class FavoriteStoreDto
{
    #region Properties
    public Guid FavoriteId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? StoreLogoUrl { get; set; }
    public string? StoreCoverImageUrl { get; set; }
    public string? Address { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public DateTime AddedAt { get; set; }
    #endregion
}

/// <summary>
/// كائن عرض المنتج المفضل مع تفاصيله الكاملة
/// </summary>
public class FavoriteProductDto
{
    #region Properties
    public Guid FavoriteId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime AddedAt { get; set; }
    #endregion
}


/// <summary>
/// كائن طلب واستعلام عناصر المفضلة مع التقسيم والفلترة
/// </summary>
public class GetMyFavoritesInput : PagedAndSortedResultRequestDto
{
    #region Properties
    public string? EntityType { get; set; }
    #endregion
}

