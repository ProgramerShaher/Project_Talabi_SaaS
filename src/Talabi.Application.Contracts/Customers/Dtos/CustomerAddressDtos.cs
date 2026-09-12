using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Talabi.Customers.Dtos;

/// <summary>
/// كائن عرض بيانات عنوان العميل
/// </summary>
public class CustomerAddressDto : FullAuditedEntityDto<Guid>
{
    #region Properties
    public Guid CustomerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string? AdditionalDetails { get; set; }
    public bool IsDefault { get; set; }
    #endregion
}

/// <summary>
/// كائن إضافة أو تعديل عنوان العميل
/// </summary>
public class CreateUpdateCustomerAddressDto
{
    #region Properties
    [Required(ErrorMessage = "عنوان التسمية مطلوب")]
    [StringLength(CustomerAddressConsts.MaxTitleLength, ErrorMessage = "تجاوزت الحد الأقصى للتسمية")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "خط العرض مطلوب")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "خط الطول مطلوب")]
    public decimal Longitude { get; set; }

    [Required(ErrorMessage = "اسم المدينة مطلوب")]
    [StringLength(CustomerAddressConsts.MaxCityLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم المدينة")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الحي مطلوب")]
    [StringLength(CustomerAddressConsts.MaxDistrictLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم الحي")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الشارع مطلوب")]
    [StringLength(CustomerAddressConsts.MaxStreetLength, ErrorMessage = "تجاوزت الحد الأقصى لاسم الشارع")]
    public string Street { get; set; } = string.Empty;

    [StringLength(CustomerAddressConsts.MaxBuildingLength, ErrorMessage = "تجاوزت الحد الأقصى لرقم المبنى")]
    public string? Building { get; set; }

    [StringLength(CustomerAddressConsts.MaxFloorLength, ErrorMessage = "تجاوزت الحد الأقصى لرقم الطابق")]
    public string? Floor { get; set; }

    [StringLength(CustomerAddressConsts.MaxApartmentLength, ErrorMessage = "تجاوزت الحد الأقصى لرقم الشقة")]
    public string? Apartment { get; set; }

    [StringLength(CustomerAddressConsts.MaxAdditionalDetailsLength, ErrorMessage = "تجاوزت الحد الأقصى للتفاصيل")]
    public string? AdditionalDetails { get; set; }

    public bool IsDefault { get; set; }
    #endregion
}
