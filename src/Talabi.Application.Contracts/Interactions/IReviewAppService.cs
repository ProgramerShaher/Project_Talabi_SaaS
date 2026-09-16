using System;
using System.Threading.Tasks;
using Talabi.Interactions.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Interactions;

/// <summary>
/// واجهة خدمة التطبيق لإدارة تقييمات ومراجعات المتاجر
/// </summary>
public interface IReviewAppService : IApplicationService
{
    /// <summary>
    /// استرجاع بيانات تقييم محدد بواسطة المعرف
    /// </summary>
    /// <param name="id">معرف التقييم</param>
    /// <returns>بيانات التقييم</returns>
    Task<ReviewDto> GetAsync(Guid id);

    /// <summary>
    /// استرجاع قائمة تقييمات متجر محدد للعامة مع الترقيم والفرز
    /// </summary>
    /// <param name="storeId">معرف المتجر</param>
    /// <param name="input">معايير الفلترة والترقيم والفرز</param>
    /// <returns>قائمة التقييمات المرقمة للمتجر</returns>
    Task<PagedResultDto<ReviewDto>> GetStoreReviewsAsync(Guid storeId, GetReviewListInput input);

    /// <summary>
    /// استرجاع ملخص إحصائيات تقييمات المتجر وتوزيع النجوم والمتوسط الحسابي
    /// </summary>
    /// <param name="storeId">معرف المتجر</param>
    /// <returns>ملخص إحصائيات التقييمات</returns>
    Task<StoreReviewSummaryDto> GetStoreReviewSummaryAsync(Guid storeId);

    /// <summary>
    /// استرجاع قائمة جميع التقييمات الخاصة بالعميل المسجل حالياً
    /// </summary>
    /// <param name="input">معايير الترقيم والفرز</param>
    /// <returns>قائمة تقييمات العميل المرقمة</returns>
    Task<PagedResultDto<ReviewDto>> GetMyReviewsAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// استرجاع تقييم العميل الحالي لمتجر محدد إن وجد
    /// </summary>
    /// <param name="storeId">معرف المتجر</param>
    /// <returns>بيانات التقييم إن وجد أو null</returns>
    Task<ReviewDto?> GetMyReviewForStoreAsync(Guid storeId);

    /// <summary>
    /// إنشاء تقييم جديد لمتجر بواسطة العميل الحالي
    /// </summary>
    /// <param name="input">بيانات التقييم الجديد</param>
    /// <returns>التقييم المنشأ</returns>
    Task<ReviewDto> CreateAsync(CreateReviewInput input);

    /// <summary>
    /// تعديل تقييم سابق لمتجر بواسطة العميل صاحب التقييم حصراً
    /// </summary>
    /// <param name="id">معرف التقييم المراد تعديله</param>
    /// <param name="input">البيانات الجديدة للتقييم</param>
    /// <returns>التقييم بعد التعديل</returns>
    Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewInput input);

    /// <summary>
    /// حذف تقييم سابق بواسطة العميل صاحب التقييم حصراً
    /// </summary>
    /// <param name="id">معرف التقييم المراد حذفه</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// إضافة أو تعديل رد المتجر الرسمي على تقييم العميل (لصاحب المتجر أو الإدارة)
    /// </summary>
    /// <param name="id">معرف التقييم</param>
    /// <param name="input">نص الرد الرسمي</param>
    /// <returns>التقييم متضمناً الرد الرسمي</returns>
    Task<ReviewDto> ReplyAsync(Guid id, StoreReplyInput input);

    /// <summary>
    /// التحكم في إظهار أو إخفاء التقييم للعامة (للإدارة أو صاحب المتجر)
    /// </summary>
    /// <param name="id">معرف التقييم</param>
    /// <param name="isVisible">حالة الظهور</param>
    /// <returns>التقييم بعد تحديث حالة الظهور</returns>
    Task<ReviewDto> SetVisibilityAsync(Guid id, bool isVisible);
}
