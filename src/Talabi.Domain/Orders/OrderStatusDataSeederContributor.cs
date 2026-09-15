using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Talabi.Orders;

public class OrderStatusDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<OrderStatus, Guid> _orderStatusRepository;
    private readonly IGuidGenerator _guidGenerator;

    public OrderStatusDataSeederContributor(
        IRepository<OrderStatus, Guid> orderStatusRepository,
        IGuidGenerator guidGenerator)
    {
        _orderStatusRepository = orderStatusRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _orderStatusRepository.GetCountAsync() > 0)
        {
            return;
        }

        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "Pending", "قيد الانتظار", 1, false, "#FFC107", "clock"));
        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "Accepted", "تم القبول", 2, false, "#17A2B8", "check-circle"));
        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "Preparing", "قيد التجهيز", 3, false, "#007BFF", "box"));
        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "InTransit", "في الطريق", 4, false, "#6F42C1", "truck"));
        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "Delivered", "تم التسليم", 5, true, "#28A745", "check-all"));
        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "Cancelled", "ملغي (من العميل)", 6, true, "#6C757D", "cancel"));
        await _orderStatusRepository.InsertAsync(new OrderStatus(_guidGenerator.Create(), "Rejected", "مرفوض (من المتجر)", 7, true, "#DC3545", "close"));
    }
}
