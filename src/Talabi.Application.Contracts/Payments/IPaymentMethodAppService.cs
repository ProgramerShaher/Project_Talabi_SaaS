using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talabi.Payments.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Payments;

/// <summary>
/// واجهة خدمة طرق الدفع
/// </summary>
public interface IPaymentMethodAppService : IApplicationService
{
    Task<List<PaymentMethodDto>> GetListAsync();
}
