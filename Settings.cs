using System.Reflection;
using AudioMgr;
using Il2Cpp;
using LongLargo.Models;
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

    [Name("Disable Copyrighted Music")]
    [Description("Set it to Yes if you are streaming on Youtube.")]
    public bool DisableCopyrightedMusic = false;
    
    [Name("Play next")]
    public KeyCode KeyPlayNext = KeyCode.None;
    
    [Name("Stop")]
    public KeyCode KeyStop = KeyCode.None;
    
    [Name("Repeat last")]
    public KeyCode KeyPlayLast = KeyCode.None;

    [Section("Exploration")]

    [Name("Suppress Exploration music")]
    [Description("If Yes, the exploration music would never play.")]
    public bool ExplorationSuppress = false;

    [Name("Disable modded Exploration music")]
    [Description("If Yes, only vanilla exploration tracks would play.")]
    public bool ExplorationVanillaOnly = false;
    
    [Name("Exploration music delay")]
    [Description("Delay between Exploration soundtracks, percents. Vanilla is 100, recommended is 50.")]
    [Slider(1, 200)]
    public uint ExplorationDelay = 50;

    [Section("Danger")]
        
    [Name("Suppress Stalked sound")]
    [Description("If Yes, you will be stalked silently.")]
    public bool StalkedSuppress = false;
        
    [Name("Suppress Timberwolf combat music")]
    [Description("If Yes, you will fight timberwolves in silence (like in vanilla).")]
    public bool TimberwolfSuppress = false;

    [Name("Combat range (obsolete)")]
    [Description("If wolves are out of combat range, music will fade. Make it shorter if the music plays too intensely.")]
    [Choice("Very short", "Short", "Medium", "Default", "Long", "Very long")]
    public PackProximityRange ProximityRange = PackProximityRange.Default;

    [Section("Stringers")]
        
    [Name("Suppress Weather stingers")]
    [Description("If Yes, the weather stingers would never play.")]
    public bool WeatherSuppress = false;

    [Name("Disable modded Weather stingers")]
    [Description("If Yes, only vanilla weather stingers would play.")]
    public bool WeatherVanillaOnly = false;
        
    [Name("Suppress Time of day stingers")]
    [Description("If Yes, a dusk and dawn tracks would never play.")]
    public bool TimeSuppress = false;
        
    [Name("Suppress Success/Sorrow stingers")]
    [Description("If Yes, success and sorrow tracks would never play.")]
    public bool ConditionSuppress = false;

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

        if (StalkedSuppress)
        {
            Main.AudioPlayer.StopIfSituation(SituationType.Stalked);
        }

        if (TimberwolfSuppress)
        {
            Main.AudioPlayer.StopIfSituation(SituationType.Timberwolf);
        }
        
        Main.PackCombatManager.SelectSettings(ProximityRange);

        if (Main.AudioPlayer.IsPlaying && Main.AudioPlayer.LastSoundtrack.Copyright == true)
        {
            Main.AudioPlayer.Stop();
        }
    }

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        if (field.Name is nameof(ModEnabled) or nameof(BgmVolumeEnabled)) 
        {
            Refresh();
        }
    }

    protected override void OnConfirm()
    {
        Apply();

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
        SetFieldVisible(nameof(StalkedSuppress), ModEnabled);
        SetFieldVisible(nameof(TimberwolfSuppress), ModEnabled);
        SetFieldVisible(nameof(ProximityRange), ModEnabled);
        SetFieldVisible(nameof(ConditionSuppress), ModEnabled);
        SetFieldVisible(nameof(KeyPlayNext), ModEnabled);
        SetFieldVisible(nameof(KeyStop), ModEnabled);
        SetFieldVisible(nameof(KeyPlayLast), ModEnabled);
    }
}