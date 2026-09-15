using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Talabi.Customers.Dtos;

namespace Talabi.Customers;

[Mapper]
public partial class CustomerMapper
{
    public partial CustomerDto ToCustomerDto(Customer source);
    public partial List<CustomerDto> ToCustomerDtoList(List<Customer> source);

    [MapperIgnoreTarget(nameof(Customer.Id))]
    [MapperIgnoreTarget(nameof(Customer.UserId))]
    [MapperIgnoreTarget(nameof(Customer.TenantId))]
    [MapperIgnoreTarget(nameof(Customer.LoyaltyPoints))]
    public partial void ApplyUpdateDto(UpdateCustomerDto source, Customer target);

    public partial CustomerAddressDto ToCustomerAddressDto(CustomerAddress source);
    public partial List<CustomerAddressDto> ToCustomerAddressDtoList(List<CustomerAddress> source);

    [MapperIgnoreTarget(nameof(CustomerAddress.Id))]
    [MapperIgnoreTarget(nameof(CustomerAddress.CustomerId))]
    [MapperIgnoreTarget(nameof(CustomerAddress.TenantId))]
    public partial void ApplyCreateUpdateDto(CreateUpdateCustomerAddressDto source, CustomerAddress target);
}
