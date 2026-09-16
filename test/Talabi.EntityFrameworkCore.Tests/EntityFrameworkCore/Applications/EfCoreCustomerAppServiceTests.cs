using Talabi.Customers;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class EfCoreCustomerAppServiceTests : CustomerAppServiceTests<TalabiEntityFrameworkCoreTestModule>
{

}
