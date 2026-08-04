using System.Reflection;
using AudioMgr;
using Il2Cpp;
using LongLargo.Model;
using ModSettings;
using UnityEngine;

namespace LongLargo;

internal class Settings : JsonModSettings 
{
    [Section("General Options")]

    [Name("Mod Enabled")]
    public bool ModEnabled = true;
    
    [Name("Custom volume enabled")]
    [Description("False if you want volume to be controlled by vanilla Music Volume.")]
    public bool BgmVolumeEnabled = false;
    
    [Name("Long Largo Volume")]
    [Description("Sets the music volume for modded tracks. Affected by MasterVolume, but not Music volume")]
    public uint BgmVolume = 100;

    [Name("Debug mode")]
    [Description("Write more info in logs. Useful when you want to report a problem with mod.")]
    public bool DebugMode = false;

    [Name("Load local soundtrack by NAudio")]
    [Description("Fix for loading local files by AudioManager. Will be removed when unnecessary.")]
    public bool EnableUglyLoad = true;
        
    [Name("Vanilla Music Chance")]
    [Description("Chance to play Vanilla soundtrack. 200 would make Vanilla play twice more often than modded.")]
    [Slider(0, 400)]
    public uint ModVanillaMusicChance = 100;
    
    
    [Name("Play next")]
    public KeyCode KeyPlayNext = KeyCode.None;
    
    [Name("Stop")]
    public KeyCode KeyStop = KeyCode.None;

    [Section("Exploration")]

    [Name("Suppress Exploration soundtracks")]
    [Description("If Yes, the exploration tracks would never play.")]
    public bool ExplorationSuppress = false;

    [Name("Vanilla only Exploration soundtracks")]
    [Description("If Yes, only vanilla exploration tracks would play.")]
    public bool ExplorationVanillaOnly = false;
    
    [Name("Exploration music delay")]
    [Description("Delay between Exploration soundtracks, percents. Vanilla is 100, recommended is 50.")]
    [Slider(1, 200)]
    public uint ExplorationDelay = 50;

    [Section("Weather")]
        
    [Name("Suppress Weather soundtracks")]
    [Description("If Yes, the weather tracks would never play.")]
    public bool WeatherSuppress = false;

    [Name("Vanilla only Exploration soundtracks")]
    [Description("If Yes, only vanilla exploration tracks would play.")]
    public bool WeatherVanillaOnly = false;

    [Section("Time of day")]
        
    [Name("Suppress Time of day soundtracks")]
    [Description("If Yes, a dusk and dawn tracks would never play.")]
    public bool TimeSuppress = false;

    [Name("Vanilla only Time soundtracks")]
    [Description("If Yes, only vanilla dusk and dawn tracks would play.")]
    public bool TimeVanillaOnly = false;

    [Section("Stalked")]
        
    [Name("Suppress Stalked soundtracks")]
    [Description("If Yes, you will be stalked silently.")]
    public bool StalkedSuppress = false;

    [Name("Vanilla only Stalked soundtracks")]
    [Description("If Yes, only vanilla tracks would play.")]
    public bool StalkedVanillaOnly = false;

    [Section("Timberwolf")]
        
    [Name("Suppress Timberwolf soundtracks")]
    [Description("If Yes, you will fight timberwolves without custom soundtracks.")]
    public bool TimberwolfSuppress = false;

    [Section("Condition")]
        
    [Name("Suppress Success/Sorrow soundtracks")]
    [Description("If Yes, success and sorrow tracks would never play.")]
    public bool ConditionSuppress = false;

    [Name("Vanilla only Success/Sorrow soundtracks")]
    [Description("If Yes, only vanilla tracks would play.")]
    public bool ConditionVanillaOnly = false;

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        if (field.Name is nameof(ModEnabled) or nameof(BgmVolumeEnabled)) 
        {
            Refresh();
        }
    }

    protected override void OnConfirm()
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

        if (StalkedSuppress)
        {
            Main.AudioPlayer.StopIfSituation(SituationType.Stalked);
        }

        if (TimberwolfSuppress)
        {
            Main.AudioPlayer.StopIfSituation(SituationType.Timberwolf);
        }

        base.OnConfirm();
    }

    internal void Refresh()
    {
        SetFieldVisible(nameof(ModVanillaMusicChance), ModEnabled);
        SetFieldVisible(nameof(BgmVolume), BgmVolumeEnabled && ModEnabled);
        SetFieldVisible(nameof(BgmVolumeEnabled), ModEnabled);
        SetFieldVisible(nameof(EnableUglyLoad), ModEnabled);
        SetFieldVisible(nameof(ExplorationSuppress), ModEnabled);
        SetFieldVisible(nameof(ExplorationVanillaOnly), ModEnabled);
        SetFieldVisible(nameof(ExplorationDelay), ModEnabled);
        SetFieldVisible(nameof(WeatherSuppress), ModEnabled);
        SetFieldVisible(nameof(WeatherVanillaOnly), ModEnabled);
        SetFieldVisible(nameof(TimeSuppress), ModEnabled);
        SetFieldVisible(nameof(TimeVanillaOnly), ModEnabled);
        SetFieldVisible(nameof(StalkedSuppress), ModEnabled);
        SetFieldVisible(nameof(StalkedVanillaOnly), ModEnabled);
        SetFieldVisible(nameof(TimberwolfSuppress), ModEnabled);
        SetFieldVisible(nameof(ConditionSuppress), ModEnabled);
        SetFieldVisible(nameof(ConditionVanillaOnly), ModEnabled);
    }
}