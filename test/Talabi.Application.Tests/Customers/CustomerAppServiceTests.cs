using Shouldly;
using System;
using System.Threading.Tasks;
using Talabi.Customers.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

using Volo.Abp.Modularity;

namespace Talabi.Customers;

public abstract class CustomerAppServiceTests<TStartupModule> : TalabiApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ICustomerAppService _customerAppService;
    private readonly ICustomerAddressAppService _customerAddressAppService;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly Volo.Abp.Users.ICurrentUser _currentUser;

    protected CustomerAppServiceTests()
    {
        _customerAppService = GetRequiredService<ICustomerAppService>();
        _customerAddressAppService = GetRequiredService<ICustomerAddressAppService>();
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentUser = GetRequiredService<Volo.Abp.Users.ICurrentUser>();
    }

    [Fact]
    public async Task Should_Get_And_Update_Customer_Profile()
    {
        // 1. Arrange (The CurrentUser is mocked, but we need a Customer entity for that UserId in the DB)
        var currentUserId = _currentUser.Id ?? Guid.Empty;
        
        var customer = new Customer(
            _guidGenerator.Create(),
            currentUserId,
            null,
            "ar"
        );
        customer.Gender = Gender.Male;
        customer.DateOfBirth = new DateTime(1990, 1, 1);
        await _customerRepository.InsertAsync(customer, autoSave: true);

        // 2. Act: Get Profile
        var profile = await _customerAppService.GetProfileAsync();

        // 3. Assert Get
        profile.ShouldNotBeNull();
        profile.UserId.ShouldBe(currentUserId);
        profile.Gender.ShouldBe(Gender.Male);
        profile.PreferredLanguage.ShouldBe("ar");

        // 4. Act: Update Profile
        var updateInput = new UpdateCustomerDto
        {
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1995, 1, 1),
            PreferredLanguage = "en"
        };
        var updatedProfile = await _customerAppService.UpdateProfileAsync(updateInput);

        // 5. Assert Update
        updatedProfile.Gender.ShouldBe(Gender.Female);
        updatedProfile.PreferredLanguage.ShouldBe("en");
        updatedProfile.DateOfBirth.Value.Year.ShouldBe(1995);
    }

    [Fact]
    public async Task Should_Create_And_Get_Address()
    {
        var currentUserId = _currentUser.Id ?? Guid.Empty;
        var customer = new Customer(
            _guidGenerator.Create(),
            currentUserId,
            null,
            "ar"
        );
        await _customerRepository.InsertAsync(customer, autoSave: true);

        // Act: Create Address
        var addressInput = new CreateUpdateCustomerAddressDto
        {
            Title = "Home",
            Latitude = 15.3M,
            Longitude = 44.2M,
            City = "Sanaa",
            District = "Hadda",
            Street = "Main St",
            IsDefault = true
        };
        var newAddress = await _customerAddressAppService.CreateAsync(addressInput);

        // Assert Create
        newAddress.ShouldNotBeNull();
        newAddress.Title.ShouldBe("Home");
        newAddress.IsDefault.ShouldBeTrue();

        // Act: Get Addresses
        var addresses = await _customerAddressAppService.GetMyAddressesAsync();
        addresses.Count.ShouldBeGreaterThan(0);
        addresses.ShouldContain(a => a.Title == "Home");
    }
}
