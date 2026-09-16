using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Talabi.Categories;
using Talabi.Permissions;
using Talabi.Products.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Talabi.Products;

[Authorize]
public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<StoreCategory, Guid> _storeCategoryRepository;
    private readonly IRepository<SalesUnit, Guid> _salesUnitRepository;
    private readonly IRepository<ProductSalesUnit, Guid> _productSalesUnitRepository;
    private readonly ProductMapper _mapper;
    private readonly SalesUnitMapper _salesUnitMapper;

    public ProductAppService(
        IRepository<Product, Guid> productRepository,
        IRepository<StoreCategory, Guid> storeCategoryRepository,
        IRepository<SalesUnit, Guid> salesUnitRepository,
        IRepository<ProductSalesUnit, Guid> productSalesUnitRepository)
    {
        _productRepository = productRepository;
        _storeCategoryRepository = storeCategoryRepository;
        _salesUnitRepository = salesUnitRepository;
        _productSalesUnitRepository = productSalesUnitRepository;
        _mapper = new ProductMapper();
        _salesUnitMapper = new SalesUnitMapper();
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var query = await _productRepository.WithDetailsAsync(x => x.Images, x => x.SalesUnits);
        var product = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (product == null)
        {
            throw new EntityNotFoundException(typeof(Product), id);
        }
        
        // جلب التصنيف إن وجد للـ DTO
        if (product.StoreCategoryId.HasValue)
        {
            var category = await _storeCategoryRepository.FindAsync(product.StoreCategoryId.Value);
            product.StoreCategory = category;
        }

        var dto = _mapper.ToProductDto(product);
        if (product.SalesUnits?.Count > 0)
        {
            dto.SalesUnits = _salesUnitMapper.ToProductSalesUnitDtoList(product.SalesUnits.OrderBy(u => u.DisplayOrder).ToList());
        }

        return dto;
    }

    public async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListInput input)
    {
        var queryable = await _productRepository.WithDetailsAsync(x => x.Images, x => x.SalesUnits);

        var query = queryable
            .WhereIf(input.StoreId.HasValue, x => x.StoreId == input.StoreId)
            .WhereIf(input.StoreCategoryId.HasValue, x => x.StoreCategoryId == input.StoreCategoryId)
            .WhereIf(input.IsAvailable.HasValue, x => x.IsAvailable == input.IsAvailable)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!) || x.SKU.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var products = await AsyncExecuter.ToListAsync(
            query.OrderBy(input.Sorting ?? nameof(Product.Name))
                 .PageBy(input.SkipCount, input.MaxResultCount)
        );

        // تحميل أسماء التصنيفات
        var categoryIds = products.Where(p => p.StoreCategoryId.HasValue).Select(p => p.StoreCategoryId!.Value).Distinct().ToList();
        var categories = await _storeCategoryRepository.GetListAsync(c => categoryIds.Contains(c.Id));
        
        foreach(var product in products.Where(p => p.StoreCategoryId.HasValue))
        {
            product.StoreCategory = categories.FirstOrDefault(c => c.Id == product.StoreCategoryId);
        }

        var dtoList = new List<ProductDto>();
        foreach (var product in products)
        {
            var pDto = _mapper.ToProductDto(product);
            if (product.SalesUnits?.Count > 0)
            {
                pDto.SalesUnits = _salesUnitMapper.ToProductSalesUnitDtoList(product.SalesUnits.OrderBy(u => u.DisplayOrder).ToList());
            }
            dtoList.Add(pDto);
        }

        return new PagedResultDto<ProductDto>(
            totalCount,
            dtoList
        );
    }

    [Authorize(TalabiPermissions.Products.Create)]
    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        // التحقق من أن SKU غير مكرر في نفس المتجر
        var exists = await _productRepository.AnyAsync(x => x.StoreId == input.StoreId && x.SKU == input.SKU);
        if (exists)
        {
            throw new UserFriendlyException($"رمز SKU ({input.SKU}) موجود مسبقاً في هذا المتجر.");
        }

        var product = new Product(
            GuidGenerator.Create(),
            input.StoreId,
            input.Name,
            input.SKU,
            input.Price,
            input.Unit,
            input.StoreCategoryId,
            input.Discount,
            input.DiscountType,
            CurrentTenant.Id
        );

        _mapper.ApplyCreateDto(input, product);
        
        // يعاد حساب السعر النهائي بعد تطبيق القيم من الـ Mapper في حال وجود خصم
        product.CalculateFinalPrice();

        // معالجة وحدات البيع الاختيارية إن وُجدت
        if (input.SalesUnits != null && input.SalesUnits.Count > 0)
        {
            var defaultCount = input.SalesUnits.Count(u => u.IsDefault);
            if (defaultCount > 1)
            {
                throw new UserFriendlyException("يمكن تحديد وحدة بيع افتراضية واحدة فقط للمنتج.");
            }

            foreach (var uInput in input.SalesUnits)
            {
                var salesUnit = await _salesUnitRepository.GetAsync(uInput.SalesUnitId);
                var unitName = !string.IsNullOrWhiteSpace(uInput.UnitName) ? uInput.UnitName : salesUnit.Name;

                product.SalesUnits.Add(new ProductSalesUnit(
                    GuidGenerator.Create(),
                    product.Id,
                    salesUnit.Id,
                    unitName,
                    uInput.Price,
                    uInput.IsDefault,
                    uInput.IsActive,
                    uInput.DisplayOrder
                ));
            }
        }

        await _productRepository.InsertAsync(product, autoSave: true);
        return await GetAsync(product.Id);
    }

    [Authorize(TalabiPermissions.Products.Edit)]
    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        var query = await _productRepository.WithDetailsAsync(x => x.SalesUnits);
        var product = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        if (product == null)
        {
            throw new EntityNotFoundException(typeof(Product), id);
        }

        // التحقق من تكرار SKU في نفس المتجر مع استثناء المنتج الحالي
        var exists = await _productRepository.AnyAsync(x => x.Id != id && x.StoreId == product.StoreId && x.SKU == input.SKU);
        if (exists)
        {
            throw new UserFriendlyException($"رمز SKU ({input.SKU}) موجود مسبقاً في هذا المتجر.");
        }

        _mapper.ApplyUpdateDto(input, product);
        product.CalculateFinalPrice();

        // تحديث وحدات البيع المخصصة إذا تم تمريرها (اختيارية)
        if (input.SalesUnits != null)
        {
            var defaultCount = input.SalesUnits.Count(u => u.IsDefault);
            if (defaultCount > 1)
            {
                throw new UserFriendlyException("يمكن تحديد وحدة بيع افتراضية واحدة فقط للمنتج.");
            }

            product.SalesUnits.Clear();
            foreach (var uInput in input.SalesUnits)
            {
                var salesUnit = await _salesUnitRepository.GetAsync(uInput.SalesUnitId);
                var unitName = !string.IsNullOrWhiteSpace(uInput.UnitName) ? uInput.UnitName : salesUnit.Name;

                product.SalesUnits.Add(new ProductSalesUnit(
                    uInput.Id ?? GuidGenerator.Create(),
                    product.Id,
                    salesUnit.Id,
                    unitName,
                    uInput.Price,
                    uInput.IsDefault,
                    uInput.IsActive,
                    uInput.DisplayOrder
                ));
            }
        }

        await _productRepository.UpdateAsync(product, autoSave: true);
        return await GetAsync(product.Id);
    }

    [Authorize(TalabiPermissions.Products.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _productRepository.DeleteAsync(id);
    }

    [Authorize(TalabiPermissions.Products.Import)]
    public async Task<ImportProductResultDto> ImportFromExcelAsync(ImportProductFromExcelDto input)
    {
        if (input.File == null)
            throw new UserFriendlyException("الرجاء رفع ملف Excel صالح.");

        var fileName = input.File.FileName ?? string.Empty;
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".xlsx", ".xls" };
        if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension))
            throw new UserFriendlyException("يجب أن يكون الملف بصيغة .xlsx أو .xls");

        var rows = ParseExcelRows(input.File);
        var result = new ImportProductResultDto();

        // إحضار تصنيفات المتجر الحالي للبحث عن التصنيف بالاسم
        var storeCategories = await _storeCategoryRepository.GetListAsync(c => c.StoreId == input.StoreId);

        foreach (var row in rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Name))
                    throw new UserFriendlyException("حقل اسم المنتج مطلوب.");

                Guid? categoryId = null;
                if (!string.IsNullOrWhiteSpace(row.CategoryName))
                {
                    var cat = storeCategories.FirstOrDefault(c => c.CustomName.Equals(row.CategoryName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (cat != null)
                        categoryId = cat.Id;
                    else
                        throw new UserFriendlyException($"التصنيف '{row.CategoryName}' غير موجود في متجرك.");
                }

                // إنشاء SKU عشوائي فريد إذا لم يكن موجوداً
                var newSku = $"PRD-{DateTime.UtcNow.Ticks}";

                var product = new Product(
                    GuidGenerator.Create(),
                    input.StoreId,
                    row.Name,
                    newSku,
                    row.Price,
                    "حبة", // قيمة افتراضية
                    categoryId,
                    tenantId: CurrentTenant.Id
                );
                
                if (!string.IsNullOrWhiteSpace(row.Description))
                {
                    product.Description = row.Description;
                }

                await _productRepository.InsertAsync(product);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                var errorMessage = ex is UserFriendlyException || ex is BusinessException ? ex.Message : "خطأ غير متوقع.";
                result.Errors.Add(new ImportProductRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    ProductName = row.Name ?? "غير محدد",
                    Reason = errorMessage
                });
                Logger.LogWarning(ex, "فشل استيراد المنتج صف {RowNumber}: {Reason}", row.RowNumber, errorMessage);
            }
        }

        return result;
    }

    private static List<ImportProductRowDto> ParseExcelRows(Volo.Abp.Content.IRemoteStreamContent file)
    {
        var rows = new List<ImportProductRowDto>();
        using var stream = file.GetStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int rowNum = 2; rowNum <= lastRow; rowNum++)
        {
            var row = worksheet.Row(rowNum);
            
            var name = row.Cell(1).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var description = row.Cell(2).GetString()?.Trim();
            
            decimal price = 0;
            var priceStr = row.Cell(3).GetString()?.Trim();
            decimal.TryParse(priceStr, out price);
            
            var categoryName = row.Cell(4).GetString()?.Trim();

            rows.Add(new ImportProductRowDto
            {
                RowNumber = rowNum,
                Name = name,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                Price = price,
                CategoryName = string.IsNullOrWhiteSpace(categoryName) ? null : categoryName
            });
        }
        return rows;
    }
}
