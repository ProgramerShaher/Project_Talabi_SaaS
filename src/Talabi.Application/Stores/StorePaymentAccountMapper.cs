using System.Collections.Generic;
using System.Linq;
using Talabi.Stores.Dtos;

namespace Talabi.Stores;

/// <summary>
/// كلاس Mapping لحسابات الدفع (تطبيق يدوي لتجنب أخطاء Mapperly)
/// </summary>
public class StorePaymentAccountMapper
{
    public StorePaymentAccountDto ToStorePaymentAccountDto(StorePaymentAccount source)
    {
        if (source == null) return null;

        return new StorePaymentAccountDto
        {
            Id = source.Id,
            StoreId = source.StoreId,
            ProviderName = source.ProviderName,
            AccountNumber = source.AccountNumber,
            AccountName = source.AccountName,
            IsActive = source.IsActive,
            Notes = source.Notes
        };
    }

    public List<StorePaymentAccountDto> ToStorePaymentAccountDtoList(List<StorePaymentAccount> source)
    {
        if (source == null) return null;
        return source.Select(ToStorePaymentAccountDto).ToList();
    }

    public void ApplyCreateUpdateDto(CreateStorePaymentAccountDto source, StorePaymentAccount target)
    {
        if (source == null || target == null) return;

        target.ProviderName = source.ProviderName;
        target.AccountNumber = source.AccountNumber;
        target.AccountName = source.AccountName;
        target.IsActive = source.IsActive;
        target.Notes = source.Notes;
    }
}
