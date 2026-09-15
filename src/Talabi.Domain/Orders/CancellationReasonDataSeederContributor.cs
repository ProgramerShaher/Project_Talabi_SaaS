using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Talabi.Orders;

/// <summary>
/// زارع بيانات أسباب إلغاء ورفض الطلبات المعيارية في النظام
/// </summary>
public class CancellationReasonDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<CancellationReason, Guid> _cancellationReasonRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CancellationReasonDataSeederContributor(
        IRepository<CancellationReason, Guid> cancellationReasonRepository,
        IGuidGenerator guidGenerator)
    {
        _cancellationReasonRepository = cancellationReasonRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _cancellationReasonRepository.GetCountAsync() > 0)
        {
            return;
        }

        // أسباب إلغاء العميل (Customer)
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "تم الطلب بالخطأ", CancellationTargetAudience.Customer));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "تأخر وقت التسليم المتوقع", CancellationTargetAudience.Customer));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "تغيير في خطة العميل", CancellationTargetAudience.Customer));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "أخرى", CancellationTargetAudience.Customer));

        // أسباب رفض المتجر (Store)
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "أحد الأصناف غير متوفر في المخزون", CancellationTargetAudience.Store));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "المتجر مغلق حالياً", CancellationTargetAudience.Store));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "ضغط طلبات كبير في المتجر", CancellationTargetAudience.Store));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "عنوان التوصيل خارج نطاق المتجر", CancellationTargetAudience.Store));
        await _cancellationReasonRepository.InsertAsync(new CancellationReason(_guidGenerator.Create(), "أخرى", CancellationTargetAudience.Store));
    }
}
