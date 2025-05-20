using Volo.Abp.Settings;

namespace TaskTracking.Settings;

public class TaskTrackingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(TaskTrackingSettings.MySetting1));
    }
}
