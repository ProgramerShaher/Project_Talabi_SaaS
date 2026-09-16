using System;
using System.Threading.Tasks;
using Talabi.Products.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Products;

/// <summary>
/// واجهة خدمة تطبيق إدارة وحدات البيع في النظام
/// </summary>
public interface ISalesUnitAppService : IApplicationService
{
    /// <summary>
    /// جلب وحدة بيع عبر معرفها
    /// </summary>
    Task<SalesUnitDto> GetAsync(Guid id);

    /// <summary>
    /// جلب قائمة وحدات البيع مع الفلترة والصفحات
    /// </summary>
    Task<PagedResultDto<SalesUnitDto>> GetListAsync(GetSalesUnitListInput input);

    /// <summary>
    /// إضافة وحدة بيع جديدة في النظام (مثل: كيلو، كيس، لتر، حبة)
    /// </summary>
    Task<SalesUnitDto> CreateAsync(CreateSalesUnitDto input);

    /// <summary>
    /// تعديل بيانات وحدة بيع
    /// </summary>
    Task<SalesUnitDto> UpdateAsync(Guid id, UpdateSalesUnitDto input);

    /// <summary>
    /// حذف وحدة بيع
    /// </summary>
    Task DeleteAsync(Guid id);
}
