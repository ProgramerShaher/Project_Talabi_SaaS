using Talabi.Samples;
using Xunit;

namespace Talabi.EntityFrameworkCore.Domains;

[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<TalabiEntityFrameworkCoreTestModule>
{

}
