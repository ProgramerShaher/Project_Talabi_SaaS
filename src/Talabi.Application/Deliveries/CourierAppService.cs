using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Deliveries.Dtos;
using Talabi.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Talabi.Deliveries;

/// <summary>
/// خدمة التطبيق لإدارة مناديب التوصيل، تسجيلهم وتحديث بياناتهم وحالاتهم ومواقعهم الجغرافية اللحظية
/// </summary>
[Authorize]
public class CourierAppService : ApplicationService, ICourierAppService
{
    #region Fields & Dependencies
    private readonly IRepository<Courier, Guid> _courierRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly DeliveryMapper _mapper;
    #endregion

    #region Constructors
    /// <summary>
    /// منشئ خدمة إدارة المناديب وحقن الاعتماديات المطلوبة
    /// </summary>
    public CourierAppService(
        IRepository<Courier, Guid> courierRepository,
        IIdentityUserRepository identityUserRepository)
    {
        _courierRepository = courierRepository;
        _identityUserRepository = identityUserRepository;
        _mapper = new DeliveryMapper();
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// جلب بيانات مندوب محدد بواسطة المعرف
    /// </summary>
    public async Task<CourierDto> GetAsync(Guid id)
    {
        var courier = await _courierRepository.GetAsync(id);
        return _mapper.ToCourierDto(courier);
    }

    /// <summary>
    /// جلب بيانات المندوب بواسطة معرف المستخدم المرتبط في النظام
    /// </summary>
    public async Task<CourierDto> GetByUserIdAsync(Guid userId)
    {
        var courier = await _courierRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (courier == null)
        {
            throw new UserFriendlyException("لم يتم العثور على حساب مندوب توصيل مرتبط بهذا المستخدم.");
        }

        return _mapper.ToCourierDto(courier);
    }

    /// <summary>
    /// جلب الملف الشخصي للمندوب الحالي المسجل دخوله
    /// </summary>
    public async Task<CourierDto> GetMyProfileAsync()
    {
        var currentUserId = CurrentUser.GetId();
        return await GetByUserIdAsync(currentUserId);
    }

    /// <summary>
    /// جلب قائمة المناديب مع إمكانية الفلترة والتقسيم
    /// </summary>
    [Authorize(TalabiPermissions.Deliveries.Track)]
    public async Task<PagedResultDto<CourierDto>> GetListAsync(GetCourierListInput input)
    {
        var query = await _courierRepository.GetQueryableAsync();

        query = query
            .WhereIf(input.IsAvailable.HasValue, x => x.IsAvailable == input.IsAvailable!.Value)
            .WhereIf(input.IsOnline.HasValue, x => x.IsOnline == input.IsOnline!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.VehicleType), x => x.VehicleType == input.VehicleType)
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                x => (x.VehicleNumber != null && x.VehicleNumber.Contains(input.Filter!)) ||
                     (x.LicenseNumber != null && x.LicenseNumber.Contains(input.Filter!)) ||
                     (x.VehicleType != null && x.VehicleType.Contains(input.Filter!))
            );

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Courier.CreationTime)} desc"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).PageBy(input)
        );


        var dtoList = _mapper.ToCourierDtoList(items);
        return new PagedResultDto<CourierDto>(totalCount, dtoList);
    }

    /// <summary>
    /// تسجيل مندوب توصيل جديد في المنصة
    /// </summary>
    [Authorize(TalabiPermissions.Deliveries.ManageCouriers)]
    public async Task<CourierDto> CreateAsync(CreateCourierDto input)
    {
        var user = await _identityUserRepository.FindAsync(input.UserId);
        if (user == null)
        {
            throw new UserFriendlyException("المستخدم المحدد غير موجود في النظام.");
        }

        var existingCourier = await _courierRepository.FirstOrDefaultAsync(x => x.UserId == input.UserId);
        if (existingCourier != null)
        {
            throw new UserFriendlyException("هذا المستخدم مسجل بالفعل كمندوب توصيل في النظام.");
        }

        var courier = new Courier(
            GuidGenerator.Create(),
            input.UserId,
            input.VehicleType,
            input.VehicleNumber
        );

        courier.LicenseNumber = input.LicenseNumber;
        courier.IsAvailable = true;
        courier.IsOnline = false;

        await _courierRepository.InsertAsync(courier, autoSave: true);

        Logger.LogInformation(
            "تم تسجيل مندوب توصيل جديد بنجاح: {CourierId} للمستخدم: {UserId} برقم لوحة: {VehicleNumber}",
            courier.Id, courier.UserId, courier.VehicleNumber);

        return _mapper.ToCourierDto(courier);
    }

    /// <summary>
    /// تعديل بيانات مندوب التوصيل
    /// </summary>
    public async Task<CourierDto> UpdateAsync(Guid id, UpdateCourierDto input)
    {
        var courier = await _courierRepository.GetAsync(id);

        // التحقق من الصلاحية: إما يملك صلاحية الإدارة أو أنه المندوب نفسه
        if (!CurrentUser.IsInRole("admin") &&
            !await AuthorizationService.IsGrantedAsync(TalabiPermissions.Deliveries.ManageCouriers))
        {
            var currentUserId = CurrentUser.GetId();
            if (courier.UserId != currentUserId)
            {
                throw new UserFriendlyException("غير مصرح لك بتعديل بيانات مندوب آخر.");
            }
        }

        if (input.VehicleType != null)
        {
            courier.VehicleType = input.VehicleType;
        }

        if (input.VehicleNumber != null)
        {
            courier.VehicleNumber = input.VehicleNumber;
        }

        if (input.LicenseNumber != null)
        {
            courier.LicenseNumber = input.LicenseNumber;
        }

        if (input.IsAvailable.HasValue)
        {
            courier.IsAvailable = input.IsAvailable.Value;
        }

        await _courierRepository.UpdateAsync(courier, autoSave: true);

        Logger.LogInformation("تم تحديث بيانات المندوب بنجاح: {CourierId}", courier.Id);
        return _mapper.ToCourierDto(courier);
    }

    /// <summary>
    /// تحديث حالة جاهزية وتوافر المندوب الحالي لاستقبال مشاوير جديدة
    /// </summary>
    public async Task<CourierDto> SetAvailabilityAsync(bool isAvailable)
    {
        var courier = await GetCurrentCourierEntityAsync();
        courier.IsAvailable = isAvailable;

        await _courierRepository.UpdateAsync(courier, autoSave: true);

        Logger.LogInformation(
            "تم تحديث حالة توافر المندوب {CourierId} إلى: {IsAvailable}",
            courier.Id, isAvailable);

        return _mapper.ToCourierDto(courier);
    }

    /// <summary>
    /// تحديث حالة اتصال المندوب بالتطبيق (Online/Offline)
    /// </summary>
    public async Task<CourierDto> SetOnlineStatusAsync(bool isOnline)
    {
        var courier = await GetCurrentCourierEntityAsync();
        courier.IsOnline = isOnline;

        // إذا أصبح غير متصل، يتم جعله تلقائياً غير متاح
        if (!isOnline)
        {
            courier.IsAvailable = false;
        }

        await _courierRepository.UpdateAsync(courier, autoSave: true);

        Logger.LogInformation(
            "تم تحديث حالة اتصال المندوب {CourierId} إلى: {IsOnline}",
            courier.Id, isOnline);

        return _mapper.ToCourierDto(courier);
    }

    /// <summary>
    /// تحديث الموقع الجغرافي اللحظي للمندوب عبر إحداثيات GPS
    /// </summary>
    public async Task<CourierDto> UpdateLocationAsync(UpdateCourierLocationInput input)
    {
        var courier = await GetCurrentCourierEntityAsync();

        courier.CurrentLatitude = input.Latitude;
        courier.CurrentLongitude = input.Longitude;
        courier.LastLocationUpdate = Clock.Now;

        await _courierRepository.UpdateAsync(courier, autoSave: true);

        return _mapper.ToCourierDto(courier);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// استرجاع كيان المندوب الخاص بالمستخدم الحالي المسجل دخوله مع التحقق
    /// </summary>
    private async Task<Courier> GetCurrentCourierEntityAsync()
    {
        var currentUserId = CurrentUser.GetId();
        var courier = await _courierRepository.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (courier == null)
        {
            throw new UserFriendlyException("الحساب الحالي غير مسجل كمندوب توصيل في المنصة.");
        }

        return courier;
    }

    #endregion
}
