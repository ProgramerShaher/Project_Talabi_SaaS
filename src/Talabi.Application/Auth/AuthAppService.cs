using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Auth.Dtos;
using Talabi.Customers;
using Talabi.Deliveries;
using Talabi.Stores;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Talabi.Auth;

/// <summary>
/// تطبيق خدمة المصادقة والملف الشخصي لتطبيق الموبايل
/// </summary>
[Authorize]
public class AuthAppService : TalabiAppService, IAuthAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<StoreUser, Guid> _storeUserRepository;
    private readonly IRepository<Courier, Guid> _courierRepository;

    public AuthAppService(
        IdentityUserManager userManager,
        IRepository<Customer, Guid> customerRepository,
        IRepository<StoreUser, Guid> storeUserRepository,
        IRepository<Courier, Guid> courierRepository)
    {
        _userManager = userManager;
        _customerRepository = customerRepository;
        _storeUserRepository = storeUserRepository;
        _courierRepository = courierRepository;
    }

    /// <summary>
    /// جلب بيانات الملف الشخصي للمستخدم الحالي بعد تسجيل الدخول (بعد الحصول على التوكن)
    /// </summary>
    /// <returns>بيانات المستخدم المخصصة لتطبيق الموبايل</returns>
    public async Task<MobileProfileDto> GetMobileProfileAsync()
    {
        var userId = CurrentUser.GetId();
        
        Logger.LogInformation("بدء جلب بيانات الملف الشخصي للمستخدم: {UserId}", userId);

        var user = await _userManager.GetByIdAsync(userId);
        var roles = await _userManager.GetRolesAsync(user);

        var profileDto = new MobileProfileDto
        {
            UserId = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList()
        };

        // التحقق مما إذا كان المستخدم مسجلاً كعميل
        var customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer != null)
        {
            profileDto.CustomerId = customer.Id;
        }

        // التحقق مما إذا كان المستخدم يدير متجراً أو موظف فيه
        var storeUser = await _storeUserRepository.FirstOrDefaultAsync(su => su.UserId == userId);
        if (storeUser != null)
        {
            profileDto.StoreId = storeUser.StoreId;
        }

        // التحقق مما إذا كان المستخدم مندوب توصيل
        var courier = await _courierRepository.FirstOrDefaultAsync(c => c.UserId == userId);
        if (courier != null)
        {
            profileDto.CourierId = courier.Id;
        }

        Logger.LogInformation("تم جلب بيانات الملف الشخصي للمستخدم: {UserId} بنجاح", userId);

        return profileDto;
    }
}
