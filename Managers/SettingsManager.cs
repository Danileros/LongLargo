using ModSettings;

namespace LongLargo.Managers;

internal static class SettingsManager
{
    public static readonly Settings Settings = new Settings();

    public static void OnLoad()
    {
        Settings.AddToModSettings(BuildInfo.GuiName, MenuType.Both);
        Settings.Refresh();
    }
}