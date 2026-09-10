using Volo.Abp.Settings;

namespace Talabi.Settings;

public class TalabiSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(TalabiSettings.MySetting1));
    }
}
