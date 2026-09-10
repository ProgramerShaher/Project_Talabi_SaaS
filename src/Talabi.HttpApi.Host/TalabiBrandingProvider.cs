using Microsoft.Extensions.Localization;
using Talabi.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Talabi;

[Dependency(ReplaceServices = true)]
public class TalabiBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<TalabiResource> _localizer;

    public TalabiBrandingProvider(IStringLocalizer<TalabiResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
