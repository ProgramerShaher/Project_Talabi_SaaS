using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talabi.Auth.Dtos;
using Talabi.Customers;
using Talabi.Deliveries;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
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
    private readonly IRepository<CustomerAddress, Guid> _customerAddressRepository;
    private readonly IEmailSender _emailSender;

    public AuthAppService(
        IdentityUserManager userManager,
        IRepository<Customer, Guid> customerRepository,
        IRepository<StoreUser, Guid> storeUserRepository,
        IRepository<Courier, Guid> courierRepository,
        IRepository<CustomerAddress, Guid> customerAddressRepository,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _customerRepository = customerRepository;
        _storeUserRepository = storeUserRepository;
        _courierRepository = courierRepository;
        _customerAddressRepository = customerAddressRepository;
        _emailSender = emailSender;
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

    /// <summary>
    /// تسجيل حساب عميل جديد (الخطوة الأولى) وإرسال كود التحقق
    /// </summary>
    [AllowAnonymous]
    public async Task RegisterAsync(RegisterCustomerDto input)
    {
        // التحقق من أن البريد الإلكتروني غير مستخدم مسبقاً
        var existingUser = await _userManager.FindByEmailAsync(input.Email);
        if (existingUser != null)
        {
            throw new UserFriendlyException("البريد الإلكتروني مستخدم بالفعل.");
        }

        var user = new IdentityUser(GuidGenerator.Create(), input.Email, input.Email, CurrentTenant.Id)
        {
            Name = input.Name,
            Surname = input.Surname
        };
        user.SetPhoneNumber(input.PhoneNumber, false);

        var result = await _userManager.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            throw new UserFriendlyException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // إنشاء كود OTP من 6 أرقام للإيميل
        var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        
        var emailBody = $"مرحباً {input.Name}،\n\nكود التحقق الخاص بك هو: {token}\n\nيرجى إدخال هذا الكود لتفعيل حسابك.";
        
        await _emailSender.SendAsync(
            input.Email,
            "كود التحقق لتفعيل حسابك في طلبي",
            emailBody
        );

        Logger.LogInformation("تم إنشاء حساب جديد: {Email} وإرسال كود التحقق.", input.Email);
    }

    /// <summary>
    /// التحقق من البريد الإلكتروني باستخدام الكود المرسل
    /// </summary>
    [AllowAnonymous]
    public async Task VerifyEmailAsync(VerifyEmailDto input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            throw new UserFriendlyException("المستخدم غير موجود.");
        }

        if (user.EmailConfirmed)
        {
            throw new UserFriendlyException("البريد الإلكتروني مفعل مسبقاً.");
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, input.Code);
        if (!isValid)
        {
            throw new UserFriendlyException("كود التحقق غير صحيح أو منتهي الصلاحية.");
        }

        user.SetEmailConfirmed(true);
        await _userManager.UpdateAsync(user);

        Logger.LogInformation("تم تأكيد الإيميل للمستخدم: {Email} بنجاح.", input.Email);
    }

    /// <summary>
    /// استكمال بيانات الملف الشخصي للعميل (الخطوة الثانية) بعد تسجيل الدخول
    /// </summary>
    public async Task CompleteProfileAsync(CompleteCustomerProfileDto input)
    {
        var userId = CurrentUser.GetId();

        // التحقق من أن العميل غير مسجل مسبقاً في جدول Customers
        var existingCustomer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == userId);
        if (existingCustomer != null)
        {
            throw new UserFriendlyException("لقد قمت باستكمال ملفك الشخصي كعميل مسبقاً.");
        }

        // إنشاء كيان العميل
        var customerId = GuidGenerator.Create();
        var customer = new Customer(customerId, userId)
        {
            Gender = input.Gender,
            DateOfBirth = input.DateOfBirth
        };

        await _customerRepository.InsertAsync(customer);

        // إنشاء كيان العنوان
        var address = new CustomerAddress(
            GuidGenerator.Create(),
            customerId,
            "المنزل", // كعنوان افتراضي أولي
            0, // Latitude مؤقت
            0, // Longitude مؤقت
            input.Country,
            input.Governorate,
            input.Region,
            input.City,
            input.District,
            input.Street,
            isDefault: true)
        {
            Building = input.Building,
            Apartment = input.Apartment,
            Floor = input.Floor
        };

        await _customerAddressRepository.InsertAsync(address);

        Logger.LogInformation("تم استكمال بيانات العميل بنجاح للمستخدم: {UserId}", userId);
    }
}
