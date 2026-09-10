using Talabi.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Talabi.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TalabiController : AbpControllerBase
{
    protected TalabiController()
    {
        LocalizationResource = typeof(TalabiResource);
    }
}
