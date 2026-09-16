using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Interactions.Dtos;

namespace Talabi.Interactions;

/// <summary>
/// محول الكائنات التلقائي لتقييمات ومراجعات المتاجر عبر Mapperly
/// </summary>
[Mapper]
public partial class ReviewMapper
{
    /// <summary>
    /// تحويل كيان التقييم إلى كائن نقل البيانات ReviewDto
    /// </summary>
    /// <param name="source">كيان التقييم المصدر</param>
    /// <returns>كائن نقل بيانات التقييم</returns>
    [MapperIgnoreTarget(nameof(ReviewDto.CustomerName))]
    [MapperIgnoreTarget(nameof(ReviewDto.StoreName))]
    public partial ReviewDto ToReviewDto(Review source);

    /// <summary>
    /// تحويل قائمة من كيانات التقييم إلى قائمة كائنات نقل البيانات ReviewDto
    /// </summary>
    /// <param name="source">قائمة كيانات التقييم المصدر</param>
    /// <returns>قائمة كائنات نقل بيانات التقييم</returns>
    [MapperIgnoreTarget(nameof(ReviewDto.CustomerName))]
    [MapperIgnoreTarget(nameof(ReviewDto.StoreName))]
    public partial List<ReviewDto> ToReviewDtoList(List<Review> source);
}
