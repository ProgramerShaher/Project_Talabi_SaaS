using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Talabi.Payments;

/// <summary>
/// Seed default payment methods (Cash on Delivery and External Transfer)
/// </summary>
public class PaymentMethodDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PaymentMethodDataSeederContributor(
        IRepository<PaymentMethod, Guid> paymentMethodRepository,
        IGuidGenerator guidGenerator)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _paymentMethodRepository.GetCountAsync() > 0)
        {
            return;
        }

        var cashOnDelivery = new PaymentMethod(
            _guidGenerator.Create(),
            "CashOnDelivery",
            "الدفع عند الاستلام",
            isOnline: false,
            requiresReceipt: false,
            isActive: true,
            iconUrl: "fas fa-money-bill"
        );

        var externalTransfer = new PaymentMethod(
            _guidGenerator.Create(),
            "ExternalTransfer",
            "تحويل بنكي / محفظة إلكترونية",
            isOnline: false,
            requiresReceipt: true, // This is crucial for manual payment!
            isActive: true,
            iconUrl: "fas fa-exchange-alt"
        );

        await _paymentMethodRepository.InsertAsync(cashOnDelivery);
        await _paymentMethodRepository.InsertAsync(externalTransfer);
    }
}
