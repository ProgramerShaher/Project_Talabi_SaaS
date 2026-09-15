using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Customers.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Talabi.Customers;

/// <summary>
/// خدمة التطبيق لإدارة عناوين التوصيل الخاصة بالعملاء
/// </summary>
[Authorize]
public class CustomerAddressAppService : ApplicationService, ICustomerAddressAppService
{
    private readonly IRepository<CustomerAddress, Guid> _customerAddressRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly CustomerMapper _mapper;

    /// <summary>
    /// منشئ الفئة لتهيئة الخدمة مع المستودعات المطلوبة
    /// </summary>
    public CustomerAddressAppService(
        IRepository<CustomerAddress, Guid> customerAddressRepository,
        IRepository<Customer, Guid> customerRepository)
    {
        _customerAddressRepository = customerAddressRepository;
        _customerRepository = customerRepository;
        _mapper = new CustomerMapper();
    }

    /// <summary>
    /// جلب معرف العميل المرتبط بالمستخدم الحالي
    /// </summary>
    private async Task<Guid> GetCurrentCustomerIdAsync()
    {
        var userId = CurrentUser.GetId();
        var customer = await _customerRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        
        if (customer == null)
        {
            throw new UserFriendlyException("لم يتم العثور على ملف العميل الخاص بك");
        }
        
        return customer.Id;
    }

    /// <summary>
    /// استرجاع قائمة جميع عناوين التوصيل الخاصة بالعميل الحالي
    /// </summary>
    public async Task<List<CustomerAddressDto>> GetMyAddressesAsync()
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var addresses = await _customerAddressRepository.GetListAsync(x => x.CustomerId == customerId);
        
        return _mapper.ToCustomerAddressDtoList(addresses);
    }

    /// <summary>
    /// استرجاع بيانات عنوان توصيل محدد بواسطة المعرف
    /// </summary>
    public async Task<CustomerAddressDto> GetAsync(Guid id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var address = await _customerAddressRepository.GetAsync(id);
        
        if (address.CustomerId != customerId)
        {
            throw new UserFriendlyException("غير مصرح لك بعرض هذا العنوان");
        }
        
        return _mapper.ToCustomerAddressDto(address);
    }

    /// <summary>
    /// إضافة عنوان توصيل جديد للعميل الحالي
    /// </summary>
    public async Task<CustomerAddressDto> CreateAsync(CreateUpdateCustomerAddressDto input)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        
        var address = new CustomerAddress(
            GuidGenerator.Create(),
            customerId,
            input.Title,
            input.Latitude,
            input.Longitude,
            "Yemen", // Country
            "", // Governorate
            "", // Region
            input.City,
            input.District,
            input.Street,
            CurrentTenant.Id
        )
        {
            Building = input.Building,
            Floor = input.Floor,
            Apartment = input.Apartment,
            AdditionalDetails = input.AdditionalDetails
        };

        if (input.IsDefault)
        {
            // Set all other addresses to non-default
            var existingAddresses = await _customerAddressRepository.GetListAsync(x => x.CustomerId == customerId && x.IsDefault);
            foreach (var existing in existingAddresses)
            {
                existing.IsDefault = false;
                await _customerAddressRepository.UpdateAsync(existing);
            }
            address.IsDefault = true;
        }

        await _customerAddressRepository.InsertAsync(address);
        
        return _mapper.ToCustomerAddressDto(address);
    }

    /// <summary>
    /// تحديث بيانات عنوان توصيل موجود
    /// </summary>
    public async Task<CustomerAddressDto> UpdateAsync(Guid id, CreateUpdateCustomerAddressDto input)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var address = await _customerAddressRepository.GetAsync(id);
        
        if (address.CustomerId != customerId)
        {
            throw new UserFriendlyException("غير مصرح لك بتعديل هذا العنوان");
        }
        
        _mapper.ApplyCreateUpdateDto(input, address);
        
        if (input.IsDefault && !address.IsDefault)
        {
            var existingAddresses = await _customerAddressRepository.GetListAsync(x => x.CustomerId == customerId && x.Id != id && x.IsDefault);
            foreach (var existing in existingAddresses)
            {
                existing.IsDefault = false;
                await _customerAddressRepository.UpdateAsync(existing);
            }
            address.IsDefault = true;
        }

        await _customerAddressRepository.UpdateAsync(address);
        
        return _mapper.ToCustomerAddressDto(address);
    }

    /// <summary>
    /// حذف عنوان توصيل للعميل الحالي
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var address = await _customerAddressRepository.GetAsync(id);
        
        if (address.CustomerId != customerId)
        {
            throw new UserFriendlyException("غير مصرح لك بحذف هذا العنوان");
        }
        
        await _customerAddressRepository.DeleteAsync(address);
    }

    /// <summary>
    /// تعيين عنوان محدد كعنوان التوصيل الافتراضي للعميل
    /// </summary>
    public async Task SetAsDefaultAsync(Guid id)
    {
        var customerId = await GetCurrentCustomerIdAsync();
        var address = await _customerAddressRepository.GetAsync(id);
        
        if (address.CustomerId != customerId)
        {
            throw new UserFriendlyException("غير مصرح لك بتعديل هذا العنوان");
        }
        
        var existingAddresses = await _customerAddressRepository.GetListAsync(x => x.CustomerId == customerId && x.Id != id && x.IsDefault);
        foreach (var existing in existingAddresses)
        {
            existing.IsDefault = false;
            await _customerAddressRepository.UpdateAsync(existing);
        }
        
        address.IsDefault = true;
        await _customerAddressRepository.UpdateAsync(address);
    }
}
