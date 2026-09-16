using Talabi.Orders;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class EfCoreOrderAppServiceTests : OrderAppServiceTests<TalabiEntityFrameworkCoreTestModule>
{

}
