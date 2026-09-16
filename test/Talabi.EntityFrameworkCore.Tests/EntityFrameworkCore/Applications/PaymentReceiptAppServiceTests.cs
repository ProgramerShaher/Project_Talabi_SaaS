using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Talabi.Customers;
using Talabi.MediaFiles;
using Talabi.Orders;
using Talabi.Payments;
using Talabi.Payments.Dtos;
using Talabi.Stores;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

/// <summary>
/// اختبارات وحدة وتكامل خدمة مراجعة والتحقق من إيصالات الدفع PaymentReceiptAppService
/// </summary>
[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class PaymentReceiptAppServiceTests : TalabiEntityFrameworkCoreTestBase
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public PaymentReceiptAppServiceTests()
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

    private IDisposable ChangeUser(Guid userId, string userName = "test_user")
    {
        var claims = new System.Collections.Generic.List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, userName)
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        return _currentPrincipalAccessor.Change(principal);
    }

    private async Task<(IdentityUser customerUser, Customer customer, IdentityUser storeOwnerUser, Store store, Order order, MediaFile mediaFile)> CreateTestScenarioAsync()
    {
        return await WithUowAsync(async sp =>
        {
            var userRepo = sp.GetRequiredService<IRepository<IdentityUser, Guid>>();
            var customerRepo = sp.GetRequiredService<IRepository<Customer, Guid>>();
            var storeTypeRepo = sp.GetRequiredService<IRepository<StoreType, Guid>>();
            var storeRepo = sp.GetRequiredService<IRepository<Store, Guid>>();
            var statusRepo = sp.GetRequiredService<IRepository<OrderStatus, Guid>>();
            var paymentMethodRepo = sp.GetRequiredService<IRepository<PaymentMethod, Guid>>();
            var orderRepo = sp.GetRequiredService<IRepository<Order, Guid>>();
            var mediaRepo = sp.GetRequiredService<IRepository<MediaFile, Guid>>();

            // مستخدم العميل
            var custUser = new IdentityUser(_guidGenerator.Create(), "cust_" + Guid.NewGuid().ToString("N")[..6], "cust@talabi.com");
            custUser.Name = "محمد";
            custUser.Surname = "أحمد";
            await userRepo.InsertAsync(custUser, autoSave: true);

            var customer = new Customer(_guidGenerator.Create(), custUser.Id, null, "ar");
            await customerRepo.InsertAsync(customer, autoSave: true);

            // مستخدم مالك المتجر
            var ownerUser = new IdentityUser(_guidGenerator.Create(), "owner_" + Guid.NewGuid().ToString("N")[..6], "owner@talabi.com");
            await userRepo.InsertAsync(ownerUser, autoSave: true);

            var storeType = new StoreType(_guidGenerator.Create(), "مطاعم", 1, true);
            await storeTypeRepo.InsertAsync(storeType, autoSave: true);

            var store = new Store(
                _guidGenerator.Create(),
                ownerUser.Id,
                storeType.Id,
                "مطعم البركة",
                "albaraka-" + Guid.NewGuid().ToString("N")[..6],
                "777123456",
                "صنعاء",
                15.35M,
                44.20M
            );
            await storeRepo.InsertAsync(store, autoSave: true);

            var pendingStatus = await statusRepo.FirstOrDefaultAsync(x => x.Name == "Pending");
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus(_guidGenerator.Create(), "Pending", "قيد الانتظار", 1);
                await statusRepo.InsertAsync(pendingStatus, autoSave: true);
            }

            var bankMethod = await paymentMethodRepo.FirstOrDefaultAsync(x => x.Name == "BankTransfer");
            if (bankMethod == null)
            {
                bankMethod = new PaymentMethod(_guidGenerator.Create(), "BankTransfer", "تحويل بنكي", false, true, true);
                await paymentMethodRepo.InsertAsync(bankMethod, autoSave: true);
            }

            var order = new Order(
                _guidGenerator.Create(),
                "ORD-" + Random.Shared.Next(10000, 99999),
                customer.Id,
                store.Id,
                _guidGenerator.Create(),
                "صنعاء - شارع حدة",
                pendingStatus.Id,
                bankMethod.Id,
                15000,
                15000
            );
            await orderRepo.InsertAsync(order, autoSave: true);

            var mediaFile = new MediaFile(
                _guidGenerator.Create(),
                "receipt_1.png",
                "image/png",
                204800,
                "Local",
                "receipts",
                "orders/receipt_1.png",
                "https://talabi.storage/receipts/receipt_1.png"
            );
            await mediaRepo.InsertAsync(mediaFile, autoSave: true);

            return (custUser, customer, ownerUser, store, order, mediaFile);
        });
    }

    [Fact]
    public async Task SubmitReceiptAsync_ShouldCreateReceiptAndSetStatusPending()
    {
        var data = await CreateTestScenarioAsync();

        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var result = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id,
                    WalletName = "الكريمي للتمويل الأصغر",
                    TransactionNumber = "TRX-88776655",
                    Amount = 15000
                });
            });

            result.ShouldNotBeNull();
            result.OrderId.ShouldBe(data.order.Id);
            result.VerificationStatus.ShouldBe(ReceiptVerificationStatus.Pending);
            result.WalletName.ShouldBe("الكريمي للتمويل الأصغر");
            result.TransactionNumber.ShouldBe("TRX-88776655");
            result.Amount.ShouldBe(15000);
            result.StoreName.ShouldBe(data.store.Name);
            result.CustomerName.ShouldBe("محمد أحمد");
            result.ReceiptFileUrl.ShouldBe(data.mediaFile.PublicUrl);
        }
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnReceiptDetails()
    {
        var data = await CreateTestScenarioAsync();

        // 1. العميل يرفع الإيصال
        Guid receiptId;
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var submitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id,
                    WalletName = "STC Pay",
                    TransactionNumber = "STC-12345"
                });
            });
            receiptId = submitted.Id;
        }

        // 2. الاستعلام بواسطة معرف الطلب
        using (ChangeUser(data.storeOwnerUser.Id, data.storeOwnerUser.UserName))
        {
            var details = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.GetByOrderIdAsync(data.order.Id);
            });

            details.ShouldNotBeNull();
            details.Id.ShouldBe(receiptId);
            details.OrderId.ShouldBe(data.order.Id);
            details.WalletName.ShouldBe("STC Pay");
            details.TransactionNumber.ShouldBe("STC-12345");
        }
    }

    [Fact]
    public async Task GetListAsync_StoreOwner_ShouldReturnOnlyStoreReceipts()
    {
        var data = await CreateTestScenarioAsync();

        // رفع إيصال للطلب
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id,
                    WalletName = "بنك التضامن",
                    TransactionNumber = "TAD-998877"
                });
            });
        }

        // استعلام مالك المتجر
        using (ChangeUser(data.storeOwnerUser.Id, data.storeOwnerUser.UserName))
        {
            var pagedResult = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.GetListAsync(new GetPaymentReceiptListInput
                {
                    StoreId = data.store.Id,
                    Status = ReceiptVerificationStatus.Pending
                });
            });

            pagedResult.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
            pagedResult.Items.ShouldContain(x => x.OrderId == data.order.Id);
        }
    }

    [Fact]
    public async Task VerifyReceiptAsync_Approved_ShouldUpdateStatusesAndMarkPaid()
    {
        var data = await CreateTestScenarioAsync();

        Guid receiptId;
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var submitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id,
                    TransactionNumber = "APPROVAL-TEST"
                });
            });
            receiptId = submitted.Id;
        }

        // مالك المتجر يعتمد ويقبل الإيصال
        using (ChangeUser(data.storeOwnerUser.Id, data.storeOwnerUser.UserName))
        {
            var verified = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.VerifyReceiptAsync(new VerifyPaymentReceiptInput
                {
                    ReceiptId = receiptId,
                    IsApproved = true
                });
            });

            verified.VerificationStatus.ShouldBe(ReceiptVerificationStatus.Verified);
            verified.VerifiedByUserId.ShouldBe(data.storeOwnerUser.Id);
            verified.VerifiedAt.ShouldNotBeNull();
            verified.RejectionReason.ShouldBeNull();

            // فحص قاعدة البيانات للطلب والمعاملة
            await WithUowAsync(async sp =>
            {
                var orderRepo = sp.GetRequiredService<IRepository<Order, Guid>>();
                var paymentRepo = sp.GetRequiredService<IRepository<Payment, Guid>>();

                var order = await orderRepo.GetAsync(data.order.Id);
                order.PaymentStatus.ShouldBe(OrderPaymentStatus.Paid);

                var payment = await paymentRepo.FirstOrDefaultAsync(x => x.OrderId == data.order.Id);
                payment.ShouldNotBeNull();
                payment.Status.ShouldBe(PaymentTransactionStatus.Completed);
                payment.PaidAt.ShouldNotBeNull();
            });
        }
    }

    [Fact]
    public async Task VerifyReceiptAsync_Rejected_WithoutReason_ShouldThrowValidationException()
    {
        var data = await CreateTestScenarioAsync();

        Guid receiptId;
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var submitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id
                });
            });
            receiptId = submitted.Id;
        }

        using (ChangeUser(data.storeOwnerUser.Id, data.storeOwnerUser.UserName))
        {
            await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(async () =>
            {
                await WithUowAsync(async sp =>
                {
                    var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                    return await receiptAppService.VerifyReceiptAsync(new VerifyPaymentReceiptInput
                    {
                        ReceiptId = receiptId,
                        IsApproved = false,
                        RejectionReason = "   " // فارغ
                    });
                });
            });
        }
    }

    [Fact]
    public async Task VerifyReceiptAsync_Rejected_WithReason_ShouldUpdateStatusesAndRecordReason()
    {
        var data = await CreateTestScenarioAsync();

        Guid receiptId;
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var submitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id,
                    TransactionNumber = "REJECT-TEST"
                });
            });
            receiptId = submitted.Id;
        }

        using (ChangeUser(data.storeOwnerUser.Id, data.storeOwnerUser.UserName))
        {
            var rejected = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.VerifyReceiptAsync(new VerifyPaymentReceiptInput
                {
                    ReceiptId = receiptId,
                    IsApproved = false,
                    RejectionReason = "المبلغ المحول غير مطابق لقيمة الفاتورة"
                });
            });

            rejected.VerificationStatus.ShouldBe(ReceiptVerificationStatus.Rejected);
            rejected.RejectionReason.ShouldBe("المبلغ المحول غير مطابق لقيمة الفاتورة");
            rejected.VerifiedByUserId.ShouldBe(data.storeOwnerUser.Id);

            await WithUowAsync(async sp =>
            {
                var paymentRepo = sp.GetRequiredService<IRepository<Payment, Guid>>();
                var payment = await paymentRepo.FirstOrDefaultAsync(x => x.OrderId == data.order.Id);
                payment.ShouldNotBeNull();
                payment.Status.ShouldBe(PaymentTransactionStatus.Failed);
            });
        }
    }

    [Fact]
    public async Task SubmitReceiptAsync_ResubmitAfterRejection_ShouldResetStatusToPending()
    {
        var data = await CreateTestScenarioAsync();

        Guid receiptId;
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var submitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id
                });
            });
            receiptId = submitted.Id;
        }

        // رفض الإيصال
        using (ChangeUser(data.storeOwnerUser.Id, data.storeOwnerUser.UserName))
        {
            await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.VerifyReceiptAsync(new VerifyPaymentReceiptInput
                {
                    ReceiptId = receiptId,
                    IsApproved = false,
                    RejectionReason = "الصورة غير واضحة"
                });
            });
        }

        // العميل يعيد رفع الإيصال الصحيح
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var resubmitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id,
                    WalletName = "الكريمي",
                    TransactionNumber = "NEW-TRX-2026",
                    Amount = 15000
                });
            });

            resubmitted.Id.ShouldBe(receiptId);
            resubmitted.VerificationStatus.ShouldBe(ReceiptVerificationStatus.Pending);
            resubmitted.RejectionReason.ShouldBeNull();
            resubmitted.TransactionNumber.ShouldBe("NEW-TRX-2026");
        }
    }

    [Fact]
    public async Task VerifyReceiptAsync_CustomerCannotVerifyOwnReceipt_ShouldThrowUserFriendlyException()
    {
        var data = await CreateTestScenarioAsync();

        Guid receiptId;
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            var submitted = await WithUowAsync(async sp =>
            {
                var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                return await receiptAppService.SubmitReceiptAsync(new SubmitPaymentReceiptInput
                {
                    OrderId = data.order.Id,
                    MediaFileId = data.mediaFile.Id
                });
            });
            receiptId = submitted.Id;
        }

        // العميل يحاول اعتماد إيصاله بنفسه
        using (ChangeUser(data.customerUser.Id, data.customerUser.UserName))
        {
            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await WithUowAsync(async sp =>
                {
                    var receiptAppService = sp.GetRequiredService<IPaymentReceiptAppService>();
                    return await receiptAppService.VerifyReceiptAsync(new VerifyPaymentReceiptInput
                    {
                        ReceiptId = receiptId,
                        IsApproved = true
                    });
                });
            });
        }
    }
}
