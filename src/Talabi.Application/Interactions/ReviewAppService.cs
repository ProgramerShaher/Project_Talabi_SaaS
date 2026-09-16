using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Customers;
using Talabi.Interactions.Dtos;
using Talabi.Permissions;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Talabi.Interactions;

/// <summary>
/// خدمة التطبيق لإدارة تقييمات ومراجعات المتاجر
/// توفر عمليات تقييم المتاجر من قبل العملاء، والتحقق الصارم من الملكية، وتحديث إحصائيات المتاجر تلقائياً
/// </summary>
public class ReviewAppService : ApplicationService, IReviewAppService
{
    #region Fields & Dependencies
    private readonly IRepository<Review, Guid> _reviewRepository;
    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly ReviewMapper _mapper;
    #endregion

    #region Constructors
    /// <summary>
    /// منشئ خدمة التقييمات وحقن الاعتماديات المطلوبة
    /// </summary>
    public ReviewAppService(
        IRepository<Review, Guid> reviewRepository,
        IRepository<Store, Guid> storeRepository,
        IRepository<Customer, Guid> customerRepository,
        IIdentityUserRepository identityUserRepository)
    {
        _reviewRepository = reviewRepository;
        _storeRepository = storeRepository;
        _customerRepository = customerRepository;
        _identityUserRepository = identityUserRepository;
        _mapper = new ReviewMapper();
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// استرجاع بيانات تقييم محدد بواسطة المعرف
    /// </summary>
    public async Task<ReviewDto> GetAsync(Guid id)
    {
        var review = await _reviewRepository.GetAsync(id);
        var dto = _mapper.ToReviewDto(review);
        return await PopulateDtoNamesAsync(review, dto);
    }

    /// <summary>
    /// استرجاع تقييمات متجر محدد للعامة مع الترقيم والفرز والتصفية
    /// </summary>
    public async Task<PagedResultDto<ReviewDto>> GetStoreReviewsAsync(Guid storeId, GetReviewListInput input)
    {
        var queryable = await _reviewRepository.GetQueryableAsync();

        var filtered = queryable
            .Where(r => r.StoreId == storeId && r.IsVisible)
            .WhereIf(input.MinRating.HasValue, r => r.StoreRating >= input.MinRating!.Value)
            .WhereIf(input.CustomerId.HasValue, r => r.CustomerId == input.CustomerId!.Value);

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var items = await AsyncExecuter.ToListAsync(
            filtered
                .OrderByDescending(r => r.CreationTime)
                .PageBy(input)
        );

        var dtos = await PopulateDtosNamesAsync(items);

        return new PagedResultDto<ReviewDto>(totalCount, dtos);
    }

    /// <summary>
    /// استرجاع ملخص إحصائيات تقييمات المتجر وتوزيع النجوم والمتوسط الحسابي
    /// </summary>
    public async Task<StoreReviewSummaryDto> GetStoreReviewSummaryAsync(Guid storeId)
    {
        var store = await _storeRepository.FindAsync(storeId);
        if (store == null)
        {
            throw new UserFriendlyException("المتجر المطلوب غير موجود في النظام.");
        }

        var queryable = await _reviewRepository.GetQueryableAsync();
        var storeReviews = queryable.Where(r => r.StoreId == storeId && r.IsVisible);

        var ratings = await AsyncExecuter.ToListAsync(storeReviews.Select(r => r.StoreRating));
        var total = ratings.Count;

        var summary = new StoreReviewSummaryDto
        {
            StoreId = storeId,
            StoreName = store.Name,
            TotalReviews = total,
            AverageRating = total > 0 ? Math.Round((decimal)ratings.Average(), 1) : 0,
            FiveStarCount = ratings.Count(r => r == 5),
            FourStarCount = ratings.Count(r => r == 4),
            ThreeStarCount = ratings.Count(r => r == 3),
            TwoStarCount = ratings.Count(r => r == 2),
            OneStarCount = ratings.Count(r => r == 1)
        };

        return summary;
    }

    /// <summary>
    /// استرجاع قائمة جميع التقييمات الخاصة بالعميل المسجل حالياً
    /// </summary>
    [Authorize]
    public async Task<PagedResultDto<ReviewDto>> GetMyReviewsAsync(PagedAndSortedResultRequestDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var queryable = await _reviewRepository.GetQueryableAsync();

        var filtered = queryable.Where(r => r.CustomerId == customer.Id);
        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var items = await AsyncExecuter.ToListAsync(
            filtered
                .OrderByDescending(r => r.CreationTime)
                .PageBy(input)
        );

        var dtos = await PopulateDtosNamesAsync(items);

        return new PagedResultDto<ReviewDto>(totalCount, dtos);
    }

    /// <summary>
    /// استرجاع تقييم العميل الحالي لمتجر محدد إن وجد
    /// </summary>
    [Authorize]
    public async Task<ReviewDto?> GetMyReviewForStoreAsync(Guid storeId)
    {
        var customer = await GetCurrentCustomerAsync();
        var review = await _reviewRepository.FirstOrDefaultAsync(r => r.StoreId == storeId && r.CustomerId == customer.Id);

        if (review == null)
        {
            return null;
        }

        var dto = _mapper.ToReviewDto(review);
        return await PopulateDtoNamesAsync(review, dto);
    }

    /// <summary>
    /// إنشاء تقييم جديد للمتجر بواسطة العميل الحالي
    /// </summary>
    [Authorize]
    public async Task<ReviewDto> CreateAsync(CreateReviewInput input)
    {
        var customer = await GetCurrentCustomerAsync();

        var store = await _storeRepository.FindAsync(input.StoreId);
        if (store == null)
        {
            throw new UserFriendlyException("المتجر المطلوب تقييمه غير موجود في النظام.");
        }

        var existingReview = await _reviewRepository.FirstOrDefaultAsync(
            r => r.StoreId == input.StoreId && r.CustomerId == customer.Id);

        if (existingReview != null)
        {
            throw new UserFriendlyException("لقد قمت بتقييم هذا المتجر مسبقاً، يمكنك تعديل تقييمك الحالي بدلاً من إنشاء تقييم جديد.");
        }

        var review = new Review(
            GuidGenerator.Create(),
            customer.Id,
            input.StoreId,
            input.StoreRating,
            input.Comment,
            input.ProductQualityRating
        );

        await _reviewRepository.InsertAsync(review, autoSave: true);

        Logger.LogInformation(
            "تم إنشاء تقييم جديد للمتجر: {StoreId} بواسطة العميل: {CustomerId}، التقييم: {Rating}",
            input.StoreId,
            customer.Id,
            input.StoreRating);

        await UpdateStoreRatingStatisticsAsync(input.StoreId);

        var dto = _mapper.ToReviewDto(review);
        return await PopulateDtoNamesAsync(review, dto);
    }

    /// <summary>
    /// تعديل تقييم سابق بواسطة العميل صاحب التقييم حصراً
    /// </summary>
    [Authorize]
    public async Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewInput input)
    {
        var customer = await GetCurrentCustomerAsync();
        var review = await _reviewRepository.GetAsync(id);

        if (review.CustomerId != customer.Id)
        {
            throw new UserFriendlyException("غير مصرح لك بتعديل هذا التقييم، يمكنك فقط تعديل تقييماتك الخاصة.");
        }

        review.StoreRating = input.StoreRating;
        review.ProductQualityRating = input.ProductQualityRating;
        review.Comment = input.Comment;

        await _reviewRepository.UpdateAsync(review, autoSave: true);

        Logger.LogInformation(
            "تم تحديث التقييم: {ReviewId} للمتجر: {StoreId} بواسطة العميل: {CustomerId}، التقييم الجديد: {Rating}",
            id,
            review.StoreId,
            customer.Id,
            input.StoreRating);

        await UpdateStoreRatingStatisticsAsync(review.StoreId);

        var dto = _mapper.ToReviewDto(review);
        return await PopulateDtoNamesAsync(review, dto);
    }

    /// <summary>
    /// حذف تقييم سابق بواسطة العميل صاحب التقييم حصراً
    /// </summary>
    [Authorize]
    public async Task DeleteAsync(Guid id)
    {
        var customer = await GetCurrentCustomerAsync();
        var review = await _reviewRepository.GetAsync(id);

        if (review.CustomerId != customer.Id)
        {
            throw new UserFriendlyException("غير مصرح لك بحذف هذا التقييم، يمكنك فقط حذف تقييماتك الخاصة.");
        }

        var storeId = review.StoreId;
        await _reviewRepository.DeleteAsync(review, autoSave: true);

        Logger.LogInformation(
            "تم حذف التقييم: {ReviewId} للمتجر: {StoreId} بواسطة العميل: {CustomerId}",
            id,
            storeId,
            customer.Id);

        await UpdateStoreRatingStatisticsAsync(storeId);
    }

    /// <summary>
    /// إضافة أو تعديل رد المتجر الرسمي على تقييم العميل
    /// </summary>
    [Authorize]
    public async Task<ReviewDto> ReplyAsync(Guid id, StoreReplyInput input)
    {
        var review = await _reviewRepository.GetAsync(id);
        var store = await _storeRepository.GetAsync(review.StoreId);

        var currentUserId = CurrentUser.GetId();
        var isOwner = store.OwnerId == currentUserId;
        var hasReplyPermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Reviews.Reply);
        var hasManagePermission = await AuthorizationService.IsGrantedAsync(TalabiPermissions.Reviews.Manage);

        if (!isOwner && !hasReplyPermission && !hasManagePermission)
        {
            throw new UserFriendlyException("غير مصرح لك بالرد على هذا التقييم، الرد متاح لمالك المتجر أو للإدارة فقط.");
        }

        review.StoreReply = input.Reply;
        review.StoreRepliedAt = Clock.Now;

        await _reviewRepository.UpdateAsync(review, autoSave: true);

        Logger.LogInformation(
            "تم الرد على التقييم: {ReviewId} من قبل المتجر: {StoreId}",
            id,
            review.StoreId);

        var dto = _mapper.ToReviewDto(review);
        return await PopulateDtoNamesAsync(review, dto);
    }

    /// <summary>
    /// التحكم في إظهار أو إخفاء التقييم للعامة في شاشة المتجر
    /// </summary>
    [Authorize(TalabiPermissions.Reviews.Manage)]
    public async Task<ReviewDto> SetVisibilityAsync(Guid id, bool isVisible)
    {
        var review = await _reviewRepository.GetAsync(id);
        review.IsVisible = isVisible;

        await _reviewRepository.UpdateAsync(review, autoSave: true);

        Logger.LogInformation(
            "تم تعديل حالة ظهور التقييم: {ReviewId} إلى: {IsVisible}",
            id,
            isVisible);

        await UpdateStoreRatingStatisticsAsync(review.StoreId);

        var dto = _mapper.ToReviewDto(review);
        return await PopulateDtoNamesAsync(review, dto);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// جلب سجل العميل المرتبط بالمستخدم الحالي والتحقق من وجوده
    /// </summary>
    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var userId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("لم يتم العثور على ملف العميل الخاص بك، يرجى إنشاء ملف العميل أولاً.");
        }

        return customer;
    }

    /// <summary>
    /// إعادة احتساب متوسط التقييم وإجمالي التقييمات للمتجر وتحديث سجله في قاعدة البيانات
    /// </summary>
    private async Task UpdateStoreRatingStatisticsAsync(Guid storeId)
    {
        var store = await _storeRepository.FindAsync(storeId);
        if (store == null) return;

        var queryable = await _reviewRepository.GetQueryableAsync();
        var visibleReviews = queryable.Where(r => r.StoreId == storeId && r.IsVisible);

        var totalCount = await AsyncExecuter.CountAsync(visibleReviews);
        if (totalCount > 0)
        {
            var average = await AsyncExecuter.AverageAsync(visibleReviews.Select(r => (decimal)r.StoreRating));
            store.Rating = Math.Round(average, 1);
            store.TotalReviews = totalCount;
        }
        else
        {
            store.Rating = 0;
            store.TotalReviews = 0;
        }

        await _storeRepository.UpdateAsync(store, autoSave: true);

        Logger.LogInformation(
            "تم تحديث إحصائيات تقييم المتجر: {StoreId}، المتوسط: {Rating}، إجمالي التقييمات: {TotalReviews}",
            storeId,
            store.Rating,
            store.TotalReviews);
    }

    /// <summary>
    /// تعبئة اسم العميل واسم المتجر في كائن DTO المنفرد
    /// </summary>
    private async Task<ReviewDto> PopulateDtoNamesAsync(Review review, ReviewDto dto)
    {
        var customer = review.Customer ?? await _customerRepository.FindAsync(review.CustomerId);
        if (customer != null)
        {
            var user = await _identityUserRepository.FindAsync(customer.UserId);
            if (user != null)
            {
                var name = $"{user.Name} {user.Surname}".Trim();
                dto.CustomerName = string.IsNullOrWhiteSpace(name) ? user.UserName : name;
            }
        }

        if (string.IsNullOrWhiteSpace(dto.CustomerName))
        {
            dto.CustomerName = "عميل طلبي";
        }

        var store = review.Store ?? await _storeRepository.FindAsync(review.StoreId);
        if (store != null)
        {
            dto.StoreName = store.Name;
        }

        return dto;
    }

    /// <summary>
    /// تعبئة أسماء العملاء والمتاجر لقائمة من كائنات DTO دفعة واحدة وبأداء عالٍ
    /// </summary>
    private async Task<List<ReviewDto>> PopulateDtosNamesAsync(List<Review> reviews)
    {
        var dtos = _mapper.ToReviewDtoList(reviews);
        if (dtos.Count == 0) return dtos;

        var customerIds = reviews.Select(r => r.CustomerId).Distinct().ToList();
        var storeIds = reviews.Select(r => r.StoreId).Distinct().ToList();

        var customers = await _customerRepository.GetListAsync(c => customerIds.Contains(c.Id));
        var userIds = customers.Select(c => c.UserId).Distinct().ToList();

        var userNames = new Dictionary<Guid, string>();
        foreach (var userId in userIds)
        {
            var u = await _identityUserRepository.FindAsync(userId);
            if (u != null)
            {
                var name = $"{u.Name} {u.Surname}".Trim();
                userNames[userId] = string.IsNullOrWhiteSpace(name) ? u.UserName : name;
            }
        }

        var customerUserMap = customers.ToDictionary(c => c.Id, c => c.UserId);

        var stores = (await _storeRepository.GetListAsync(s => storeIds.Contains(s.Id)))
            .ToDictionary(s => s.Id, s => s.Name);

        for (int i = 0; i < reviews.Count; i++)
        {
            var review = reviews[i];
            var dto = dtos[i];

            if (customerUserMap.TryGetValue(review.CustomerId, out var userId) &&
                userNames.TryGetValue(userId, out var userName))
            {
                dto.CustomerName = userName;
            }
            else
            {
                dto.CustomerName = "عميل طلبي";
            }

            if (stores.TryGetValue(review.StoreId, out var storeName))
            {
                dto.StoreName = storeName;
            }
        }

        return dtos;
    }

    #endregion
}
