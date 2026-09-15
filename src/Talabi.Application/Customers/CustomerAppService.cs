using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Customers.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Talabi.Customers;

/// <summary>
/// خدمة التطبيق لإدارة حسابات وملفات العملاء الشخصية
/// تجمع بيانات ABP Identity مع بيانات النطاق الخاصة بالعملاء
/// </summary>
[Authorize]
public class CustomerAppService : ApplicationService, ICustomerAppService
{
    #region 1. Fields & Dependencies

    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly CustomerMapper _mapper;

    #endregion

    #region 2. Constructors

    /// <summary>
    /// منشئ الفئة لتهيئة الخدمة مع المستودعات المطلوبة
    /// </summary>
    public CustomerAppService(
        IRepository<Customer, Guid> customerRepository,
        IIdentityUserRepository identityUserRepository)
    {
        _customerRepository = customerRepository;
        _identityUserRepository = identityUserRepository;
        _mapper = new CustomerMapper();
    }

    #endregion

    #region 3. Public Methods / Actions

    /// <summary>
    /// إنشاء ملف شخصي جديد للعميل الحالي
    /// </summary>
    public async Task<CustomerDto> CreateProfileAsync(UpdateCustomerDto input)
    {
        var userId = CurrentUser.GetId();

        var existingCustomer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (existingCustomer != null)
        {
            throw new UserFriendlyException("لقد قمت بإنشاء ملف شخصي مسبقاً.");
        }

        var customer = new Customer(
            GuidGenerator.Create(),
            userId,
            CurrentTenant.Id,
            input.PreferredLanguage ?? CustomerConsts.DefaultPreferredLanguage
        );

        _mapper.ApplyUpdateDto(input, customer);

        await _customerRepository.InsertAsync(customer);

        Logger.LogInformation(
            "تم إنشاء ملف شخصي جديد للعميل: {UserId}",
            userId);

        return await BuildCustomerDtoAsync(customer);
    }

    /// <summary>
    /// استرجاع بيانات الملف الشخصي للعميل الحالي
    /// </summary>
    public async Task<CustomerDto> GetProfileAsync()
    {
        var userId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("لم يتم العثور على ملف العميل الخاص بك");
        }

        return await BuildCustomerDtoAsync(customer);
    }

    /// <summary>
    /// تحديث بيانات الملف الشخصي للعميل الحالي
    /// </summary>
    public async Task<CustomerDto> UpdateProfileAsync(UpdateCustomerDto input)
    {
        var userId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("لم يتم العثور على ملف العميل الخاص بك");
        }

        _mapper.ApplyUpdateDto(input, customer);
        await _customerRepository.UpdateAsync(customer);

        Logger.LogInformation(
            "تم تحديث الملف الشخصي للعميل: {UserId}",
            userId);

        return await BuildCustomerDtoAsync(customer);
    }

    /// <summary>
    /// تحديث الصورة الشخصية للعميل الحالي
    /// </summary>
    public async Task<CustomerDto> UpdateAvatarAsync(UpdateCustomerAvatarDto input)
    {
        var userId = CurrentUser.GetId();
        
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        
        if (customer == null)
        {
            throw new UserFriendlyException("لم يتم العثور على ملف العميل الخاص بك");
        }

        customer.AvatarUrl = input.AvatarUrl;
        
        await _customerRepository.UpdateAsync(customer);

        return await BuildCustomerDtoAsync(customer);
    }

    #endregion

    #region 4. Private Helper Methods

    /// <summary>
    /// بناء كائن CustomerDto بدمج بيانات AbpUser مع بيانات العميل
    /// </summary>
    private async Task<CustomerDto> BuildCustomerDtoAsync(Customer customer)
    {
        var dto = _mapper.ToCustomerDto(customer);

        var identityUser = await _identityUserRepository.FindAsync(customer.UserId);
        if (identityUser != null)
        {
            dto.FullName    = (identityUser.Name + " " + identityUser.Surname).Trim();
            dto.Email       = identityUser.Email ?? string.Empty;
            dto.PhoneNumber = identityUser.PhoneNumber;
        }

        return dto;
    }

    #endregion
}