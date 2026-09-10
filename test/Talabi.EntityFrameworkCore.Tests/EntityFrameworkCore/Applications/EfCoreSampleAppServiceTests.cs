using Talabi.Samples;
using Xunit;

namespace Talabi.EntityFrameworkCore.Applications;

[Collection(TalabiTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<TalabiEntityFrameworkCoreTestModule>
{

}
