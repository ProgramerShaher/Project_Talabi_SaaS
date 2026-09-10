using System;
using System.Collections.Generic;
using System.Text;
using Talabi.Localization;
using Volo.Abp.Application.Services;

namespace Talabi;

/* Inherit your application services from this class.
 */
public abstract class TalabiAppService : ApplicationService
{
    protected TalabiAppService()
    {
        LocalizationResource = typeof(TalabiResource);
    }
}
