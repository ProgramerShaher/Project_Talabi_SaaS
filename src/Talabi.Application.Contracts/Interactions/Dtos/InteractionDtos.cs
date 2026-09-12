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
