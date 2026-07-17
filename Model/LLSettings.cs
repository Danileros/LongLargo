using System.Reflection;
using ModSettings;

namespace LongLargo.Model;

internal class LongLargoSettings : JsonModSettings 
{
    [Section("General Options")]

    [Name("Mod Enabled")]
    public bool ModEnabled = true;

    [Name("Load local soundtrack by NAudio")]
    [Description("Fix for loading local files by AudioManager. Will be removed when unnecessary.")]
    public bool EnableUglyLoad = true;
        
    [Name("Vanilla Music Chance")]
    [Description("Chance to play Vanilla soundtrack. 200 would make Vanilla play twice more often than modded.")]
    [Slider(0, 400)]
    public uint ModVanillaMusicChance = 100;

    [Section("Exploration")]

    [Name("Suppress Exploration soundtracks")]
    [Description("If Yes, the exploration tracks would never play.")]
    public bool ExplorationSuppress = false;

    [Name("Vanilla only Exploration soundtracks")]
    [Description("If Yes, only vanilla exploration tracks would play.")]
    public bool ExplorationVanillaOnly = false;
    

    [Name("Exploration music delay")]
    [Description("Delay between Exploration soundtracks, percents.")]
    [Slider(1, 200)]
    public uint ExplorationDelay = 100;

    [Section("Weather")]
        
    [Name("Suppress Weather soundtracks")]
    [Description("If Yes, the weather tracks would never play.")]
    public bool WeatherSuppress = false;

    [Name("Vanilla only Exploration soundtracks")]
    [Description("If Yes, only vanilla exploration tracks would play.")]
    public bool WeatherVanillaOnly = false;

    [Section("Time")]
        
    [Name("Suppress Time soundtracks")]
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
    [Description("If Yes, you will fight timberwolves in silence.")]
    public bool TimberwolfSuppress = false;

    [Name("Vanilla only Timberwolf soundtracks")]
    [Description("If Yes, only vanilla tracks would play.")]
    public bool TimberwolfVanillaOnly = false;

    [Section("Success")]
        
    [Name("Suppress Success/Sorrow soundtracks")]
    [Description("If Yes, success and sorrow tracks would never play.")]
    public bool SuccessSuppress = false;

    [Name("Vanilla only Success/Sorrow soundtracks")]
    [Description("If Yes, only vanilla tracks would play.")]
    public bool SuccessVanillaOnly = false;

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        if (field.Name == nameof(ModEnabled)) 
        {
            Refresh();
        }
    }
    
    internal void Refresh()
    {
        SetFieldVisible(nameof(ModVanillaMusicChance), ModEnabled);
        SetFieldVisible(nameof(ExplorationSuppress), ModEnabled);
        SetFieldVisible(nameof(ExplorationDelay), ModEnabled);
        SetFieldVisible(nameof(WeatherSuppress), ModEnabled);
        SetFieldVisible(nameof(TimeSuppress), ModEnabled);
        SetFieldVisible(nameof(StalkedSuppress), ModEnabled);
        SetFieldVisible(nameof(TimberwolfSuppress), ModEnabled);
        SetFieldVisible(nameof(SuccessSuppress), ModEnabled);
    }
}

internal static class LLSettings
{
    public static LongLargoSettings settings = new LongLargoSettings();

    public static void OnLoad()
    {
        settings.AddToModSettings("Long Largo", MenuType.Both);
        settings.Refresh();
    }
}