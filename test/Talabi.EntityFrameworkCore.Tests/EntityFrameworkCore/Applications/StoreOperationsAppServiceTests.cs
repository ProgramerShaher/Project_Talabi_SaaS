using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Talabi.Stores;
using Talabi.Stores.Dtos;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

/// <summary>
/// اختبارات وحدة وتكامل إدارة وتشغيل المتاجر وساعات العمل واعتمادها
/// Store Operations & Working Hours Tests
/// </summary>
[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class StoreOperationsAppServiceTests : TalabiEntityFrameworkCoreTestBase
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public StoreOperationsAppServiceTests()
    {
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private async Task WithUowAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(new AbpUnitOfWorkOptions());
        await action(scope.ServiceProvider);
        await uow.CompleteAsync();
    }

    private async Task<TResult> WithUowAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(new AbpUnitOfWorkOptions());
        var result = await action(scope.ServiceProvider);
        await uow.CompleteAsync();
        return result;
    }

    private IDisposable ChangeUser(Guid userId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "merchant_" + userId.ToString("N")[..6]),
            new Claim(AbpClaimTypes.Email, "merchant@test.com")
        }));
        return _currentPrincipalAccessor.Change(principal);
    }

    private async Task<Store> CreateTestStoreAsync(Guid ownerId, StoreStatus status = StoreStatus.Active, bool isActive = true)
    {
        return await WithUowAsync(async sp =>
        {
            var storeTypeRepository = sp.GetRequiredService<IRepository<StoreType, Guid>>();
            var storeRepository = sp.GetRequiredService<IRepository<Store, Guid>>();

            var storeType = new StoreType(_guidGenerator.Create(), "مطاعم وتغذية", 1, true);
            await storeTypeRepository.InsertAsync(storeType, autoSave: true);

            var store = new Store(
                _guidGenerator.Create(),
                ownerId,
                storeType.Id,
                "متجر السعادة التجاري",
                "happiness-store-" + Guid.NewGuid().ToString("N")[..8],
                "771122334",
                "صنعاء - الدائري",
                15.35M,
                44.20M
            );
            store.Status = status;
            store.IsActive = isActive;
            await storeRepository.InsertAsync(store, autoSave: true);

            return store;
        });
    }

    [Fact]
    public async Task ToggleStoreOpenCloseAsync_Owner_Should_Toggle_Status_Successfully()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: true);

        // Act 1: Toggle close
        StoreDto closedDto;
        using (ChangeUser(ownerId))
        {
            closedDto = await WithUowAsync(async sp =>
            {
                var storeAppService = sp.GetRequiredService<IStoreAppService>();
                return await storeAppService.ToggleStoreOpenCloseAsync(store.Id);
            });
        }

        // Assert 1
        closedDto.IsActive.ShouldBeFalse();
        closedDto.IsOpenNow.ShouldBeFalse();

        // Act 2: Toggle open again
        StoreDto reopenedDto;
        using (ChangeUser(ownerId))
        {
            reopenedDto = await WithUowAsync(async sp =>
            {
                var storeAppService = sp.GetRequiredService<IStoreAppService>();
                return await storeAppService.ToggleStoreOpenCloseAsync(store.Id);
            });
        }

        // Assert 2
        reopenedDto.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task ToggleStoreOpenCloseAsync_NonOwner_Without_Permission_Should_Throw_AuthorizationException()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var strangerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: true);

        // Act & Assert
        using (ChangeUser(strangerId))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(async () =>
            {
                await WithUowAsync(async sp =>
                {
                    var storeAppService = sp.GetRequiredService<IStoreAppService>();
                    await storeAppService.ToggleStoreOpenCloseAsync(store.Id);
                });
            });
        }
    }

    [Fact]
    public async Task SetWorkingHoursAsync_And_GetWorkingHoursAsync_Should_Persist_Schedule()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: true);

        var daysSchedule = new List<StoreWorkingDayDto>
        {
            new StoreWorkingDayDto
            {
                DayOfWeek = DayOfWeek.Sunday,
                DayName = "الأحد",
                IsOpen = true,
                Shifts = new List<WorkingHoursShiftDto>
                {
                    new WorkingHoursShiftDto { OpeningTime = "09:00", ClosingTime = "14:00" },
                    new WorkingHoursShiftDto { OpeningTime = "16:00", ClosingTime = "23:00" }
                }
            },
            new StoreWorkingDayDto
            {
                DayOfWeek = DayOfWeek.Friday,
                DayName = "الجمعة",
                IsOpen = false,
                Shifts = new List<WorkingHoursShiftDto>()
            }
        };

        var input = new SetStoreWorkingHoursInput
        {
            StoreId = store.Id,
            Days = daysSchedule,
            TimeZone = "Asia/Riyadh"
        };

        // Act: Set
        StoreWorkingHoursDto savedResult;
        using (ChangeUser(ownerId))
        {
            savedResult = await WithUowAsync(async sp =>
            {
                var storeAppService = sp.GetRequiredService<IStoreAppService>();
                return await storeAppService.SetWorkingHoursAsync(store.Id, input);
            });
        }

        // Assert Set
        savedResult.ShouldNotBeNull();
        savedResult.Days.Count.ShouldBe(2);
        savedResult.Days[0].DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        savedResult.Days[0].Shifts.Count.ShouldBe(2);
        savedResult.Days[1].DayOfWeek.ShouldBe(DayOfWeek.Friday);
        savedResult.Days[1].IsOpen.ShouldBeFalse();

        // Act: Get
        var fetchedResult = await WithUowAsync(async sp =>
        {
            var storeAppService = sp.GetRequiredService<IStoreAppService>();
            return await storeAppService.GetWorkingHoursAsync(store.Id);
        });

        // Assert Get
        fetchedResult.StoreId.ShouldBe(store.Id);
        fetchedResult.Days.Count.ShouldBe(2);
        fetchedResult.Days[0].Shifts[0].OpeningTime.ShouldBe("09:00");
    }

    [Fact]
    public async Task CheckIsOpenAsync_When_Store_Is_Inactive_Should_Return_Closed()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: false);

        // Act
        var status = await WithUowAsync(async sp =>
        {
            var storeAppService = sp.GetRequiredService<IStoreAppService>();
            return await storeAppService.CheckIsOpenAsync(store.Id);
        });

        // Assert
        status.IsOpenNow.ShouldBeFalse();
        status.IsActive.ShouldBeFalse();
        status.StatusMessage.ShouldContain("مغلق حالياً");
    }

    [Fact]
    public async Task CheckIsOpenAsync_When_Store_Is_Suspended_Should_Return_Closed()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Suspended, isActive: false);

        // Act
        var status = await WithUowAsync(async sp =>
        {
            var storeAppService = sp.GetRequiredService<IStoreAppService>();
            return await storeAppService.CheckIsOpenAsync(store.Id);
        });

        // Assert
        status.IsOpenNow.ShouldBeFalse();
        status.Status.ShouldBe(StoreStatus.Suspended);
        status.StatusMessage.ShouldContain("معلق");
    }

    [Fact]
    public async Task CheckIsOpenAsync_When_Open_All_Day_Should_Return_Open()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: true);

        var daysSchedule = new List<StoreWorkingDayDto>();
        foreach (DayOfWeek d in Enum.GetValues(typeof(DayOfWeek)))
        {
            daysSchedule.Add(new StoreWorkingDayDto
            {
                DayOfWeek = d,
                IsOpen = true,
                Shifts = new List<WorkingHoursShiftDto>
                {
                    new WorkingHoursShiftDto { OpeningTime = "00:00", ClosingTime = "23:59" }
                }
            });
        }

        using (ChangeUser(ownerId))
        {
            await WithUowAsync(async sp =>
            {
                var storeAppService = sp.GetRequiredService<IStoreAppService>();
                await storeAppService.SetWorkingHoursAsync(store.Id, new SetStoreWorkingHoursInput
                {
                    StoreId = store.Id,
                    Days = daysSchedule
                });
            });
        }

        // Act
        var status = await WithUowAsync(async sp =>
        {
            var storeAppService = sp.GetRequiredService<IStoreAppService>();
            return await storeAppService.CheckIsOpenAsync(store.Id);
        });

        // Assert
        status.IsOpenNow.ShouldBeTrue();
        status.StatusMessage.ShouldContain("مفتوح");
    }

    [Fact]
    public async Task ApproveStoreAsync_Should_Activate_Store_And_Update_Status()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.PendingApproval, isActive: false);

        // Act
        var approvedDto = await WithUowAsync(async sp =>
        {
            var storeAppService = sp.GetRequiredService<IStoreAppService>();
            return await storeAppService.ApproveStoreAsync(store.Id);
        });

        // Assert
        approvedDto.Status.ShouldBe(StoreStatus.Active);
        approvedDto.IsActive.ShouldBeTrue();

        // Verify in DB
        await WithUowAsync(async sp =>
        {
            var storeRepository = sp.GetRequiredService<IRepository<Store, Guid>>();
            var dbStore = await storeRepository.GetAsync(store.Id);
            dbStore.Status.ShouldBe(StoreStatus.Active);
            dbStore.IsActive.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task SuspendStoreAsync_Should_Deactivate_Store_And_Update_Status()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: true);

        // Act
        var suspendInput = new SuspendStoreInput
        {
            Reason = "تكرار إلغاء الطلبات ومخالفة سياسة المنصة"
        };

        var suspendedDto = await WithUowAsync(async sp =>
        {
            var storeAppService = sp.GetRequiredService<IStoreAppService>();
            return await storeAppService.SuspendStoreAsync(store.Id, suspendInput);
        });

        // Assert
        suspendedDto.Status.ShouldBe(StoreStatus.Suspended);
        suspendedDto.IsActive.ShouldBeFalse();

        // Verify in DB
        await WithUowAsync(async sp =>
        {
            var storeRepository = sp.GetRequiredService<IRepository<Store, Guid>>();
            var dbStore = await storeRepository.GetAsync(store.Id);
            dbStore.Status.ShouldBe(StoreStatus.Suspended);
            dbStore.IsActive.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task SuspendStoreAsync_Without_Reason_Should_Throw_ValidationException()
    {
        // Arrange
        var ownerId = _guidGenerator.Create();
        var store = await CreateTestStoreAsync(ownerId, StoreStatus.Active, isActive: true);

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(async () =>
        {
            await WithUowAsync(async sp =>
            {
                var storeAppService = sp.GetRequiredService<IStoreAppService>();
                await storeAppService.SuspendStoreAsync(store.Id, new SuspendStoreInput { Reason = "" });
            });
        });
    }
}
