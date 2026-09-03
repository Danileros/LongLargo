using System.Reflection;
using AudioMgr;
using Il2Cpp;
using LongLargo.Models;
using ModSettings;
using UnityEngine;

namespace LongLargo;

internal class Settings : JsonModSettings
{
    private string[] _presetFields = new string[]
    {
        nameof(ExplorationMusicMode),
        nameof(ExplorationDelay),
        nameof(StalkedMode),
        nameof(TimberwolfSuppress),
        nameof(WeatherMode),
        nameof(TimeSuppress),
        nameof(ConditionSuppress),
    };
    
    [Name("Settings Version")]
    public ESettingsVersion SettingsVersion;
    
    [Section("General Options")]

    [Name("Mod Enabled")]
    public bool ModEnabled = true;
    
    [Name("Custom volume enabled")]
    [Description("False if you want volume to be controlled by vanilla Music Volume.")]
    public bool BgmVolumeEnabled = false;
    
    [Name("Long Largo Volume")]
    [Description("Sets the music volume for Long Largo tracks. Affected by MasterVolume, but not Music volume")]
    public uint BgmVolume = 100;

    [Name("Debug mode")]
    [Description("Write more info in logs. Useful when you want to report a problem with mod.")]
    public bool DebugMode = false;

    // [Name("Load local soundtrack by NAudio")]
    // [Description("Fix for loading local files by AudioManager. Will be removed when unnecessary.")]
    // public bool EnableUglyLoad = true;

    [Name("Disable Copyrighted Music")]
    [Description("Set it to Yes if you are streaming on Youtube.")]
    public bool DisableCopyrightedMusic = false;
    
    [Name("Play next")]
    public KeyCode KeyPlayNext = KeyCode.None;
    
    [Name("Stop")]
    public KeyCode KeyStop = KeyCode.None;
    
    [Name("Repeat last")]
    public KeyCode KeyPlayLast = KeyCode.None;
    
    [Name("Preset")]
    [Description("Recommended is the default preset set by the author. In this case, the list of music tracks " +
                 "is expanded, and the vanilla track becomes just one of many, with the same chance of playing " +
                 "as any other track, and the delay between tracks is halved. The Vanilla preset means the mod will " +
                 "essentially do nothing, but is a good starting point. The Balance preset means the mod will " +
                 "play the mod's music every other time, alternating it with the vanilla track, with a slight " +
                 "decrease in delay.")]
    [Choice(
        "Vanilla",
        "Vanilla, no stalking music",
        "Recommended",
        "Recommended, no stalking music",
        "Balanced (50/50)",
        "Balanced, no stalking music",
        "Custom")]
    public EPreset Preset = EPreset.Recommended;

    [Section("Exploration")]

    [Name("Vanilla music chance")]
    [Description("Default = vanilla chance equals to any custom soundtrack (one of the list)." +
                 "\nRight arrow to play vanilla music more often, left to play more often. Balanced means 50/50, Suppress means no exploration music at all.")]
    [Choice(
        "Suppress (no music)",
        "Custom (no Vanilla music)",
        "x0.5 (twice less often)",
        "Default (x1)",
        "x2 (slightly more often)",
        "x5 (more often)",
        "x10 (way more often)",
        "Balanced (50% Vanilla, 50% custom)",
        "Vanilla (no custom music)"
        )]
    public EExplorationMusicMode ExplorationMusicMode = EExplorationMusicMode.Default;
    
    [Name("Exploration music delay")]
    [Description("Delay between Exploration soundtracks, percents. Vanilla is 100, recommended is 50.")]
    [Slider(1, 200)]
    public uint ExplorationDelay = 50;

    [Section("Danger")]

    [Name("Vanilla Stalked music chance")]
    [Description("By default vanilla chance equals to any custom soundtrack)." +
                 "\nIf Suppress, you will be stalked silently.")]
    [Choice(
        "Suppress (no stalked music)",
        "Wintermute (or user added)",
        "Default (vanilla+wintermute)",
        "Vanilla"
    )]
    public EStalkedMode StalkedMode = EStalkedMode.Default;
        
    [Name("Disable Timberwolf combat music")]
    [Description("If Yes, you will fight timberwolves in silence (like in vanilla).")]
    public bool TimberwolfSuppress = false;

    [Section("Stringers")]

    [Name("Vanilla Weather stingers chance")]
    [Description("By default vanilla chance equals to any custom stinger (one of the list)." +
                 "\nIf Suppress, you will be stalked silently.")]
    [Choice(
        "Suppress (no stingers)",
        "Custom (no Vanilla stringers)",
        "Default",
        "Balanced (50% Vanilla, 50% custom)",
        "Vanilla (no custom stingers)"
    )]
    public EWeatherStingerMode WeatherMode = EWeatherStingerMode.Default; 
        
    [Name("Suppress Time of day stingers")]
    [Description("If Yes, a dusk and dawn tracks would never play.")]
    public bool TimeSuppress = false;
        
    [Name("Suppress Success/Sorrow stingers")]
    [Description("If Yes, success and sorrow tracks would never play.")]
    public bool ConditionSuppress = false;
    
    [Obsolete]
    [Name("Vanilla Music Chance")]
    [Description("Chance to play Vanilla soundtrack. 200 would make Vanilla play twice more often than modded.")]
    [Slider(0, 400)]
    public uint ModVanillaMusicChance = 100;
    
    [Obsolete]
    [Name("Suppress Exploration music")]
    [Description("If Yes, the exploration music would never play.")]
    public bool ExplorationSuppress = false;
    
    [Obsolete]
    [Name("Disable modded Exploration music")]
    [Description("If Yes, only vanilla exploration tracks would play.")]
    public bool ExplorationVanillaOnly = false;
    
    [Obsolete]
    [Name("Suppress Stalked sound")]
    [Description("If Yes, you will be stalked silently.")]
    public bool StalkedSuppress = false;
    
    [Obsolete]
    [Name("Suppress Weather stingers")]
    [Description("If Yes, the weather stingers would never play.")]
    public bool WeatherSuppress = false;

    [Obsolete]
    [Name("Disable modded Weather stingers")]
    [Description("If Yes, only vanilla weather stingers would play.")]
    public bool WeatherVanillaOnly = false;

    internal void Apply()
    {
        if (BgmVolumeEnabled)
        {
            var masterVolume = InterfaceManager.GetPanel<Panel_OptionsMenu>().State.m_MasterVolume;
            Main.AudioPlayer.SetVolume(masterVolume * BgmVolume / 100f);
        }
        else
        {
            Main.AudioPlayer.SetVolume(VolumeMaster.GetVolume(AudioMaster.SourceType.BGM));
        }

        if (StalkedMode == EStalkedMode.Suppress)
        {
            Main.AudioPlayer.StopIfSituation(FSituationType.Stalked);
        }

        if (TimberwolfSuppress)
        {
            Main.AudioPlayer.StopIfSituation(FSituationType.Timberwolf);
        }

        if (Main.AudioPlayer.IsPlaying && Main.AudioPlayer.LastSoundtrack.Copyright == true)
        {
            Main.AudioPlayer.Stop();
        }
    }

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        //Change evt
        if (field.Name == nameof(Preset))
        {
            UpdatePreset((EPreset)newValue);
            RefreshGUI();
        }
        else if(_presetFields.Contains(field.Name))
        {
            Preset = EPreset.Custom;
            RefreshGUI();
        }
        
        if (field.Name is nameof(ModEnabled) or nameof(BgmVolumeEnabled)) 
        {
            Refresh();
        }
    }

    private void UpdatePreset(EPreset newPreset)
    {
        switch (newPreset)
        {
            case EPreset.Vanilla:
                ExplorationMusicMode = EExplorationMusicMode.Vanilla;
                ExplorationDelay = 100;
                StalkedMode = EStalkedMode.Vanilla;
                TimberwolfSuppress = true;
                WeatherMode = EWeatherStingerMode.Vanilla;
                TimeSuppress = false;
                ConditionSuppress = false;
                break;
            case EPreset.VanillaNoStalkingWolf:
                ExplorationMusicMode = EExplorationMusicMode.Vanilla;
                ExplorationDelay = 100;
                StalkedMode = EStalkedMode.Suppress;
                TimberwolfSuppress = true;
                WeatherMode = EWeatherStingerMode.Vanilla;
                TimeSuppress = false;
                ConditionSuppress = false;
                break;
            case EPreset.Recommended:
                ExplorationMusicMode = EExplorationMusicMode.Default;
                ExplorationDelay = 50;
                StalkedMode = EStalkedMode.Default;
                TimberwolfSuppress = false;
                WeatherMode = EWeatherStingerMode.Default;
                TimeSuppress = false;
                ConditionSuppress = false;
                break;
            case EPreset.RecommendedNoStalkingWolf:
                ExplorationMusicMode = EExplorationMusicMode.Default;
                ExplorationDelay = 50;
                StalkedMode = EStalkedMode.Suppress;
                TimberwolfSuppress = false;
                WeatherMode = EWeatherStingerMode.Default;
                TimeSuppress = false;
                ConditionSuppress = false;
                break;
            case EPreset.Balanced:
                ExplorationMusicMode = EExplorationMusicMode.Balanced;
                ExplorationDelay = 75;
                StalkedMode = EStalkedMode.Default;
                TimberwolfSuppress = false;
                WeatherMode = EWeatherStingerMode.Balanced;
                TimeSuppress = false;
                ConditionSuppress = false;
                break;
            case EPreset.BalancedNoStalkingWolf:
                ExplorationMusicMode = EExplorationMusicMode.Balanced;
                ExplorationDelay = 75;
                StalkedMode = EStalkedMode.Suppress;
                TimberwolfSuppress = false;
                WeatherMode = EWeatherStingerMode.Balanced;
                TimeSuppress = false;
                ConditionSuppress = false;
                break;
            default:
                Preset = EPreset.Custom;
                break;
        }
    }

    protected override void OnConfirm()
    {
        Apply();

        base.OnConfirm();
    }

    internal void Refresh()
    {
        SetFieldVisible(nameof(SettingsVersion), false);
        SetFieldVisible(nameof(ModVanillaMusicChance), false);
        SetFieldVisible(nameof(ExplorationSuppress), false);
        SetFieldVisible(nameof(ExplorationVanillaOnly), false);
        SetFieldVisible(nameof(WeatherSuppress), false);
        SetFieldVisible(nameof(WeatherVanillaOnly), false);
        SetFieldVisible(nameof(StalkedSuppress), false);
        
        SetFieldVisible(nameof(Preset), ModEnabled);
        SetFieldVisible(nameof(BgmVolume), BgmVolumeEnabled && ModEnabled);
        SetFieldVisible(nameof(BgmVolumeEnabled), ModEnabled);
        SetFieldVisible(nameof(DebugMode), ModEnabled);
        SetFieldVisible(nameof(DisableCopyrightedMusic), ModEnabled);
        SetFieldVisible(nameof(ExplorationMusicMode), ModEnabled);
        SetFieldVisible(nameof(ExplorationDelay), ModEnabled);
        SetFieldVisible(nameof(StalkedMode), ModEnabled);
        SetFieldVisible(nameof(TimberwolfSuppress), ModEnabled);
        SetFieldVisible(nameof(WeatherMode), ModEnabled);
        SetFieldVisible(nameof(TimeSuppress), ModEnabled);
        SetFieldVisible(nameof(ConditionSuppress), ModEnabled);
        SetFieldVisible(nameof(KeyPlayNext), ModEnabled);
        SetFieldVisible(nameof(KeyStop), ModEnabled);
        SetFieldVisible(nameof(KeyPlayLast), ModEnabled);
    }
}