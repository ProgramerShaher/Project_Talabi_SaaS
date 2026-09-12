using System;
using System.Threading.Tasks;
using Talabi.Products.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Talabi.Products;

/// <summary>
/// واجهة خدمة إدارة المنتجات الخاصة بالمتاجر
/// </summary>
public interface IProductAppService : IApplicationService
{
    Task<ProductDto> GetAsync(Guid id);
    
    Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListInput input);
    
    Task<ProductDto> CreateAsync(CreateProductDto input);
    
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input);
    
    Task DeleteAsync(Guid id);

    /// <summary>
    /// استيراد المنتجات من ملف Excel
    /// </summary>
    Task<ImportProductResultDto> ImportFromExcelAsync(ImportProductFromExcelDto input);
}
