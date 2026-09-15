using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Talabi.Payments.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Talabi.Payments;

/// <summary>
/// خدمة الحصول على طرق الدفع
/// </summary>
[Authorize]
public class PaymentMethodAppService : ApplicationService, IPaymentMethodAppService
{
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;

    public PaymentMethodAppService(IRepository<PaymentMethod, Guid> paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<List<PaymentMethodDto>> GetListAsync()
    {
        var methods = await _paymentMethodRepository.GetListAsync(x => x.IsActive);
        
        var dtoList = new List<PaymentMethodDto>();
        foreach (var method in methods)
        {
            dtoList.Add(new PaymentMethodDto
            {
                Id = method.Id,
                Name = method.Name,
                DisplayName = method.DisplayName,
                IconUrl = method.IconUrl,
                IsOnline = method.IsOnline,
                RequiresReceipt = method.RequiresReceipt,
                IsActive = method.IsActive
            });
        }
        
        return dtoList;
    }
}
