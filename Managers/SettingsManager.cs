using LongLargo.Models;
using ModSettings;

namespace LongLargo.Managers;

internal static class SettingsManager
{
    private const ESettingsVersion CurrentVersion = ESettingsVersion.v1_1_4;
    
    public static readonly Settings Settings = new Settings();

    public static void OnLoad()
    {
        Settings.AddToModSettings(BuildInfo.GuiName, MenuType.Both);
        MaybeMigrate();
        Settings.Refresh();
    }

    private static void MaybeMigrate()
    {
        switch (Settings.SettingsVersion)
        {
            case CurrentVersion:
                return;
            
            case ESettingsVersion.v1_1_3: // 1.1.3 or lower
                MigrateTo113();
                break;
            default:
                return;
        }
        
        Settings.SettingsVersion = CurrentVersion;
        Settings.Save();
    }

    private static void MigrateTo113()
    {
        Settings.Preset = EPreset.Custom;
        if (Settings.ExplorationSuppress)
        {
            Settings.ExplorationMusicMode = EExplorationMusicMode.Suppress;
        }
        else if (Settings.ExplorationVanillaOnly)
        {
            Settings.ExplorationMusicMode = EExplorationMusicMode.Vanilla;
        }
        else
        {
            Settings.ExplorationMusicMode = Settings.ModVanillaMusicChance switch
            {
                <= 0 => EExplorationMusicMode.CustomOnly,
                <= 74 => EExplorationMusicMode.X05,
                <= 149 => EExplorationMusicMode.Default,
                <= 299 => EExplorationMusicMode.X2,
                <= 399 => EExplorationMusicMode.X5,
                _ => EExplorationMusicMode.Balanced,
            };
        }

        if (Settings.StalkedSuppress)
        {
            Settings.StalkedMode = EStalkedMode.Suppress;
        }

        if (Settings.WeatherSuppress)
        {
            Settings.WeatherMode = EWeatherStingerMode.Suppress;
        }
        else if (Settings.WeatherVanillaOnly)
        {
            Settings.WeatherMode = EWeatherStingerMode.Vanilla;
        }
    }
}