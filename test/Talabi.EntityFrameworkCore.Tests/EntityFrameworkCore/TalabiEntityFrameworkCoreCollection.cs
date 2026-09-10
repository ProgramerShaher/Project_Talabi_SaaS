using Xunit;

namespace Talabi.EntityFrameworkCore;

[CollectionDefinition(TalabiTestConsts.CollectionDefinitionName)]
public class TalabiEntityFrameworkCoreCollection : ICollectionFixture<TalabiEntityFrameworkCoreFixture>
{

}
