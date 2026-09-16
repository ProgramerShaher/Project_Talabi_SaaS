using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Notifications;
using Talabi.Permissions;
using Talabi.Stores.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Talabi.Stores;

/// <summary>
/// خدمة إدارة وتشغيل المتاجر وساعات العمل
/// </summary>
[Authorize]
public class StoreAppService : ApplicationService, IStoreAppService
{
    private static readonly TimeZoneInfo RiyadhTimeZone = GetRiyadhTimeZone();

    private readonly IRepository<Store, Guid> _storeRepository;
    private readonly IRepository<StorePaymentAccount, Guid> _paymentAccountRepository;
    private readonly INotificationSender _notificationSender;
    private readonly StoreMapper _mapper;

    public StoreAppService(
        IRepository<Store, Guid> storeRepository,
        IRepository<StorePaymentAccount, Guid> paymentAccountRepository,
        INotificationSender notificationSender)
    {
        _storeRepository = storeRepository;
        _paymentAccountRepository = paymentAccountRepository;
        _notificationSender = notificationSender;
        _mapper = new StoreMapper();
    }

    /// <summary>
    /// استرجاع بيانات متجر محدد بواسطة المعرف
    /// </summary>
    public async Task<StoreDto> GetAsync(Guid id)
    {
        var store = await _storeRepository.GetAsync(id);
        var dto = _mapper.ToStoreDto(store);
        dto.IsOpenNow = CalculateStoreIsOpen(store);
        return dto;
    }

    /// <summary>
    /// استرجاع قائمة المتاجر المصفاة مع دعم الترقيم والبحث
    /// </summary>
    public async Task<PagedResultDto<StoreDto>> GetListAsync(GetStoreListInput input)
    {
        var queryable = await _storeRepository.GetQueryableAsync();

        var filteredQuery = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.Slug.Contains(input.Filter!))
            .WhereIf(input.StoreTypeId.HasValue, x => x.StoreTypeId == input.StoreTypeId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.MinRating.HasValue, x => x.Rating >= input.MinRating);

        var totalCount = await AsyncExecuter.CountAsync(filteredQuery);

        var items = await AsyncExecuter.ToListAsync(
            filteredQuery
                .OrderByDescending(x => x.CreationTime)
                .PageBy(input)
        );

        var dtos = _mapper.ToStoreDtoList(items);
        for (int i = 0; i < items.Count; i++)
        {
            dtos[i].IsOpenNow = CalculateStoreIsOpen(items[i]);
        }

        return new PagedResultDto<StoreDto>(totalCount, dtos);
    }

    /// <summary>
    /// إنشاء متجر جديد في المنصة
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Create)]
    public async Task<StoreDto> CreateAsync(CreateStoreDto input)
    {
        // التحقق من تكرار الرابط اللطيف (Slug) في النظام
        var slugExists = await _storeRepository.AnyAsync(x => x.Slug == input.Slug);
        if (slugExists)
        {
            throw new UserFriendlyException($"الرابط اللطيف ({input.Slug}) مسجل مسبقاً لمتجر آخر، يرجى اختيار رابط مختلف.");
        }

        var store = new Store(
            GuidGenerator.Create(),
            input.OwnerId,
            input.StoreTypeId,
            input.Name,
            input.Slug,
            input.Phone,
            input.Address,
            input.Latitude,
            input.Longitude,
            CurrentTenant.Id
        );
        _mapper.ApplyCreateDto(input, store);
        
        // تعيين القيم الافتراضية
        store.Status = StoreStatus.PendingApproval;
        store.IsActive = true;
        store.TenantId = CurrentTenant.Id;

        await _storeRepository.InsertAsync(store);

        Logger.LogInformation("Store {StoreId} ({StoreName}) created by user {UserId}", store.Id, store.Name, CurrentUser.Id);

        var dto = _mapper.ToStoreDto(store);
        dto.IsOpenNow = CalculateStoreIsOpen(store);
        return dto;
    }

    /// <summary>
    /// تحديث بيانات متجر قائم
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Edit)]
    public async Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto input)
    {
        var store = await _storeRepository.GetAsync(id);

        _mapper.ApplyUpdateDto(input, store);

        await _storeRepository.UpdateAsync(store);

        Logger.LogInformation("Store {StoreId} ({StoreName}) updated by user {UserId}", store.Id, store.Name, CurrentUser.Id);

        var dto = _mapper.ToStoreDto(store);
        dto.IsOpenNow = CalculateStoreIsOpen(store);
        return dto;
    }

    /// <summary>
    /// حذف متجر من النظام
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var store = await _storeRepository.GetAsync(id);
        await _storeRepository.DeleteAsync(id);
        Logger.LogInformation("Store {StoreId} ({StoreName}) deleted by user {UserId}", store.Id, store.Name, CurrentUser.Id);
    }

    /// <summary>
    /// استرجاع حسابات الدفع والتحويل البنكي الخاصة بمتجر
    /// </summary>
    public async Task<List<StorePaymentAccountDto>> GetPaymentAccountsAsync(Guid storeId)
    {
        var accounts = await _paymentAccountRepository.GetListAsync(x => x.StoreId == storeId);
        
        return accounts.Select(a => new StorePaymentAccountDto
        {
            Id = a.Id,
            StoreId = a.StoreId,
            ProviderName = a.ProviderName,
            AccountNumber = a.AccountNumber,
            AccountName = a.AccountName,
            IsActive = a.IsActive,
            Notes = a.Notes
        }).ToList();
    }

    /// <summary>
    /// إضافة حساب دفع وتحويل مالي لمتجر
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Edit)]
    public async Task<StorePaymentAccountDto> AddPaymentAccountAsync(CreateStorePaymentAccountDto input)
    {
        var account = new StorePaymentAccount(
            GuidGenerator.Create(),
            input.StoreId,
            input.ProviderName,
            input.AccountNumber,
            input.AccountName,
            input.IsActive,
            input.Notes
        );

        await _paymentAccountRepository.InsertAsync(account);

        Logger.LogInformation("Payment account {AccountId} added to store {StoreId}", account.Id, account.StoreId);

        return new StorePaymentAccountDto
        {
            Id = account.Id,
            StoreId = account.StoreId,
            ProviderName = account.ProviderName,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            IsActive = account.IsActive,
            Notes = account.Notes
        };
    }

    /// <summary>
    /// حذف حساب دفع من المتجر
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Edit)]
    public async Task DeletePaymentAccountAsync(Guid storeId, Guid accountId)
    {
        var account = await _paymentAccountRepository.GetAsync(accountId);
        if (account.StoreId != storeId)
        {
            throw new UserFriendlyException("هذا الحساب لا يتبع لهذا المتجر");
        }

        await _paymentAccountRepository.DeleteAsync(accountId);
        Logger.LogInformation("Payment account {AccountId} deleted from store {StoreId}", accountId, storeId);
    }

    /// <summary>
    /// زر الفتح والإغلاق السريع للمتجر من قِبل التاجر
    /// </summary>
    public async Task<StoreDto> ToggleStoreOpenCloseAsync(Guid id)
    {
        var store = await _storeRepository.GetAsync(id);

        // التحقق من الصلاحية: مالك المتجر فقط
        if (CurrentUser.Id == null || CurrentUser.Id != store.OwnerId)
        {
            throw new AbpAuthorizationException("غير مصرح لك بتغيير حالة تشغيل وفتح/إغلاق هذا المتجر. هذه العملية خاصة بمالك المتجر.");
        }

        var newActiveStatus = store.ToggleOpenClose();
        await _storeRepository.UpdateAsync(store, autoSave: true);

        Logger.LogInformation(
            "Store {StoreId} ({StoreName}) open/close operational status toggled to {IsActive} by merchant {UserId}",
            store.Id, store.Name, newActiveStatus, CurrentUser.Id);

        var dto = _mapper.ToStoreDto(store);
        dto.IsOpenNow = CalculateStoreIsOpen(store);
        return dto;
    }

    /// <summary>
    /// فحص حالة المتجر اللحظية هل هو مفتوح ومتاح لاستقبال الطلبات الآن
    /// </summary>
    [AllowAnonymous]
    public async Task<StoreOpenStatusDto> CheckIsOpenAsync(Guid id)
    {
        var store = await _storeRepository.GetAsync(id);
        var result = new StoreOpenStatusDto
        {
            StoreId = store.Id,
            StoreName = store.Name,
            IsActive = store.IsActive,
            Status = store.Status
        };

        if (store.Status != StoreStatus.Active)
        {
            result.IsOpenNow = false;
            result.StatusMessage = "المتجر غير معتمد أو معلق مؤقتاً من قِبل إدارة المنصة.";
            return result;
        }

        if (!store.IsActive)
        {
            result.IsOpenNow = false;
            result.StatusMessage = "المتجر مغلق حالياً من قِبل الإدارة أو التاجر.";
            return result;
        }

        if (string.IsNullOrWhiteSpace(store.WorkingHoursJson))
        {
            result.IsOpenNow = true;
            result.StatusMessage = "المتجر مفتوح ومتاح لاستقبال الطلبات على مدار الساعة.";
            return result;
        }

        try
        {
            var schedule = JsonSerializer.Deserialize<List<StoreWorkingDayDto>>(store.WorkingHoursJson);
            if (schedule == null || !schedule.Any())
            {
                result.IsOpenNow = true;
                result.StatusMessage = "المتجر مفتوح ومتاح لاستقبال الطلبات.";
                return result;
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, RiyadhTimeZone);
            var today = schedule.FirstOrDefault(d => d.DayOfWeek == now.DayOfWeek);

            if (today == null || !today.IsOpen)
            {
                result.IsOpenNow = false;
                result.StatusMessage = "المتجر في عطلة أسبوعية اليوم.";
                result.NextOpenTime = FindNextOpenTime(schedule, now);
                return result;
            }

            if (today.Shifts == null || !today.Shifts.Any())
            {
                result.IsOpenNow = true;
                result.StatusMessage = "المتجر مفتوح ومتاح لاستقبال الطلبات طوال اليوم.";
                return result;
            }

            var currentTime = now.TimeOfDay;
            var isInsideShift = today.Shifts.Any(shift =>
            {
                if (TimeSpan.TryParse(shift.OpeningTime, out var open) && TimeSpan.TryParse(shift.ClosingTime, out var close))
                {
                    return close >= open
                        ? currentTime >= open && currentTime <= close
                        : currentTime >= open || currentTime <= close;
                }
                return false;
            });

            if (isInsideShift)
            {
                result.IsOpenNow = true;
                result.StatusMessage = "المتجر مفتوح ومتاح لاستقبال الطلبات حالياً.";
            }
            else
            {
                result.IsOpenNow = false;
                result.StatusMessage = "المتجر مغلق حالياً خارج فترات العمل المقررة.";
                result.NextOpenTime = FindNextOpenTime(schedule, now);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to parse working hours JSON for store {StoreId}", store.Id);
            result.IsOpenNow = true;
            result.StatusMessage = "المتجر مفتوح ومتاح لاستقبال الطلبات.";
        }

        return result;
    }

    /// <summary>
    /// استرجاع جدول ساعات وأيام العمل الأسبوعية للمتجر وحالته اللحظية
    /// </summary>
    public async Task<StoreWorkingHoursDto> GetWorkingHoursAsync(Guid storeId)
    {
        var store = await _storeRepository.GetAsync(storeId);
        List<StoreWorkingDayDto> schedule;

        if (string.IsNullOrWhiteSpace(store.WorkingHoursJson))
        {
            schedule = GetDefaultWorkingSchedule();
        }
        else
        {
            try
            {
                schedule = JsonSerializer.Deserialize<List<StoreWorkingDayDto>>(store.WorkingHoursJson)
                           ?? GetDefaultWorkingSchedule();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error parsing working hours for store {StoreId}. Returning default schedule.", store.Id);
                schedule = GetDefaultWorkingSchedule();
            }
        }

        // إكمال أسماء الأيام بالعربية إن لم تكن متوفرة
        foreach (var day in schedule)
        {
            if (string.IsNullOrWhiteSpace(day.DayName))
            {
                day.DayName = GetArabicDayName(day.DayOfWeek);
            }
        }

        var openStatus = await CheckIsOpenAsync(storeId);

        return new StoreWorkingHoursDto
        {
            StoreId = store.Id,
            StoreName = store.Name,
            Days = schedule,
            TimeZone = "Asia/Riyadh",
            IsOpenNow = openStatus.IsOpenNow,
            StatusMessage = openStatus.StatusMessage
        };
    }

    /// <summary>
    /// تحديد وتحديث جدول مواعيد وساعات وأيام العمل الأسبوعية للمتجر
    /// </summary>
    public async Task<StoreWorkingHoursDto> SetWorkingHoursAsync(Guid storeId, SetStoreWorkingHoursInput input)
    {
        var store = await _storeRepository.GetAsync(storeId);

        // التحقق من الصلاحية: مالك المتجر
        if (CurrentUser.Id == null || CurrentUser.Id != store.OwnerId)
        {
            throw new AbpAuthorizationException("غير مصرح لك بتعديل ساعات العمل لهذا المتجر. هذه العملية خاصة بمالك المتجر.");
        }

        if (input.Days == null || !input.Days.Any())
        {
            throw new UserFriendlyException("يجب تقديم جدول أيام العمل الأسبوعية للمتجر.");
        }

        // التحقق وتنسيق أسماء الأيام
        foreach (var day in input.Days)
        {
            if (string.IsNullOrWhiteSpace(day.DayName))
            {
                day.DayName = GetArabicDayName(day.DayOfWeek);
            }
        }

        store.WorkingHoursJson = JsonSerializer.Serialize(input.Days);
        await _storeRepository.UpdateAsync(store, autoSave: true);

        Logger.LogInformation(
            "Working hours updated for store {StoreId} ({StoreName}) by user {UserId}",
            store.Id, store.Name, CurrentUser.Id);

        return await GetWorkingHoursAsync(storeId);
    }

    /// <summary>
    /// اعتماد المتجر وتنشيطه في المنصة من قِبل إدارة النظام
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Approve)]
    public async Task<StoreDto> ApproveStoreAsync(Guid id)
    {
        var store = await _storeRepository.GetAsync(id);

        store.Approve();
        await _storeRepository.UpdateAsync(store, autoSave: true);

        Logger.LogInformation(
            "Store {StoreId} ({StoreName}) was approved by platform admin {UserId}",
            store.Id, store.Name, CurrentUser.Id);

        // إشعار فوري لمالك المتجر
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "StoreApproved",
            title: "تهانينا! تم اعتماد متجرك",
            message: $"تمت مراجعة واعتماد متجرك \"{store.Name}\" بنجاح، وأصبح نشطاً ومتاحاً لاستقبال طلبات العملاء.",
            relatedEntityName: nameof(Store),
            relatedEntityId: store.Id,
            actionUrl: $"/stores/{store.Id}"
        );

        var dto = _mapper.ToStoreDto(store);
        dto.IsOpenNow = CalculateStoreIsOpen(store);
        return dto;
    }

    /// <summary>
    /// تعليق المتجر وإيقاف نشاطه مؤقتاً من قِبل إدارة النظام
    /// </summary>
    [Authorize(TalabiPermissions.Stores.Suspend)]
    public async Task<StoreDto> SuspendStoreAsync(Guid id, SuspendStoreInput input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Reason))
        {
            throw new UserFriendlyException("يرجى تقديم سبب واضح لتعليق نشاط المتجر.");
        }

        var store = await _storeRepository.GetAsync(id);

        store.Suspend();
        await _storeRepository.UpdateAsync(store, autoSave: true);

        Logger.LogWarning(
            "Store {StoreId} ({StoreName}) was suspended by admin {UserId}. Reason: {Reason}",
            store.Id, store.Name, CurrentUser.Id, input.Reason);

        // إشعار فوري لمالك المتجر بسبب التعليق
        await _notificationSender.SendToUserAsync(
            recipientUserId: store.OwnerId,
            notificationTypeName: "StoreSuspended",
            title: "تنبيه: تم تعليق نشاط متجرك",
            message: $"تم تعليق نشاط متجرك \"{store.Name}\" مؤقتاً من قِبل إدارة المنصة. سبب التعليق: {input.Reason}",
            relatedEntityName: nameof(Store),
            relatedEntityId: store.Id,
            actionUrl: $"/stores/{store.Id}"
        );

        var dto = _mapper.ToStoreDto(store);
        dto.IsOpenNow = CalculateStoreIsOpen(store);
        return dto;
    }

    #region Helper Methods

    /// <summary>
    /// فحص داخلي لحساب حالة فتح المتجر بناءً على حالته وساعات العمل
    /// </summary>
    private bool CalculateStoreIsOpen(Store store)
    {
        if (store.Status != StoreStatus.Active || !store.IsActive)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(store.WorkingHoursJson))
        {
            return true;
        }

        try
        {
            var schedule = JsonSerializer.Deserialize<List<StoreWorkingDayDto>>(store.WorkingHoursJson);
            if (schedule == null || !schedule.Any())
            {
                return true;
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, RiyadhTimeZone);
            var today = schedule.FirstOrDefault(d => d.DayOfWeek == now.DayOfWeek);
            if (today == null || !today.IsOpen)
            {
                return false;
            }

            if (today.Shifts == null || !today.Shifts.Any())
            {
                return true;
            }

            var currentTime = now.TimeOfDay;
            return today.Shifts.Any(shift =>
            {
                if (TimeSpan.TryParse(shift.OpeningTime, out var open) && TimeSpan.TryParse(shift.ClosingTime, out var close))
                {
                    return close >= open
                        ? currentTime >= open && currentTime <= close
                        : currentTime >= open || currentTime <= close;
                }
                return false;
            });
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// حساب الموعد القادم لفتح المتجر
    /// </summary>
    private static string? FindNextOpenTime(List<StoreWorkingDayDto> schedule, DateTime now)
    {
        var currentTime = now.TimeOfDay;

        // 1. التحقق من وجود وردية لاحقة اليوم
        var today = schedule.FirstOrDefault(d => d.DayOfWeek == now.DayOfWeek);
        if (today != null && today.IsOpen && today.Shifts != null)
        {
            var upcomingShift = today.Shifts
                .Where(s => TimeSpan.TryParse(s.OpeningTime, out var open) && open > currentTime)
                .OrderBy(s => TimeSpan.Parse(s.OpeningTime))
                .FirstOrDefault();

            if (upcomingShift != null)
            {
                return $"اليوم في تمام الساعة {upcomingShift.OpeningTime}";
            }
        }

        // 2. التحقق من الأيام القادمة في الأسبوع
        for (int i = 1; i <= 7; i++)
        {
            var nextDate = now.AddDays(i);
            var nextDay = schedule.FirstOrDefault(d => d.DayOfWeek == nextDate.DayOfWeek);
            if (nextDay != null && nextDay.IsOpen && nextDay.Shifts != null && nextDay.Shifts.Any())
            {
                var firstShift = nextDay.Shifts
                    .Where(s => TimeSpan.TryParse(s.OpeningTime, out _))
                    .OrderBy(s => TimeSpan.Parse(s.OpeningTime))
                    .FirstOrDefault();

                if (firstShift != null)
                {
                    var dayName = string.IsNullOrWhiteSpace(nextDay.DayName) ? GetArabicDayName(nextDate.DayOfWeek) : nextDay.DayName;
                    return i == 1
                        ? $"غداً ({dayName}) في تمام الساعة {firstShift.OpeningTime}"
                        : $"يوم {dayName} في تمام الساعة {firstShift.OpeningTime}";
                }
            }
        }

        return null;
    }

    /// <summary>
    /// استرجاع اسم اليوم باللغة العربية
    /// </summary>
    private static string GetArabicDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الإثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            DayOfWeek.Saturday => "السبت",
            _ => dayOfWeek.ToString()
        };
    }

    /// <summary>
    /// توليد جدول ساعات عمل أسبوعي افتراضي (يعمل طوال الأسبوع من 8 صباحاً حتى 11 مساءً)
    /// </summary>
    private static List<StoreWorkingDayDto> GetDefaultWorkingSchedule()
    {
        var days = new List<StoreWorkingDayDto>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            days.Add(new StoreWorkingDayDto
            {
                DayOfWeek = day,
                DayName = GetArabicDayName(day),
                IsOpen = true,
                Shifts = new List<WorkingHoursShiftDto>
                {
                    new WorkingHoursShiftDto
                    {
                        OpeningTime = "08:00",
                        ClosingTime = "23:00"
                    }
                }
            });
        }
        return days;
    }

    /// <summary>
    /// الحصول على كائن المنطقة الزمنية للرياض/السعودية المتوافق مع أنظمة التشغيل المختلفة
    /// </summary>
    private static TimeZoneInfo GetRiyadhTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");
            }
            catch
            {
                return TimeZoneInfo.CreateCustomTimeZone("Asia/Riyadh", TimeSpan.FromHours(3), "Riyadh Time", "Riyadh Time");
            }
        }
    }

    #endregion
}
