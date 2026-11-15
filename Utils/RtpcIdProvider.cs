namespace LongLargo.Utils;

/// <summary>
/// Get RTPC name or id.
/// </summary>
public static class RtpcIdProvider
{
    static RtpcIdProvider()
    {
        foreach (var kvp in SoundbankEvents)
        {
            SoundbankEventsReversed.Add(kvp.Value.ToUpperInvariant(), kvp.Key);
        }
    }

    private static readonly Dictionary<string, uint> SoundbankEventsReversed = new Dictionary<string, uint>();
    
    // From SoundbanksInfo.xml
    private static readonly Dictionary<uint, string> SoundbankEvents = new Dictionary<uint, string>()
    {
        { 987886870U,	"SteamTrain_PSI" },
        { 1377735963U,	"WoozyFade" },
        { 3236975263U,	"WindIntensityBlend" },
        { 4133886675U,	"SteamTrain_Velocity" },
        { 419475361U,	"animalPtarmiganFlying" },
        { 899752953U,	"Temperature" },
        { 1599364659U,	"MusicDucksMeter" },
        { 2141048674U,	"Elevation" },
        { 2348125913U,	"_Distance" },
        { 2686616043U,	"InteriorExterior" },
        { 2692428577U,	"PredatorDistance" },
        { 3729505769U,	"TimeOfDay" },
        { 2713585638U,	"BearDistance" },
        { 3182076450U,	"BearAngerLevel" },
        { 5094053U,		"Travois_WeightRatio" },
        { 54191616U,	"ChemicalHazard_Counter" },
        { 69961555U,	"WeaponShellNumDropped" },
        { 88134581U,	"PlayerWindAngle" },
        { 93402251U,	"Flask_Volume" },
        { 135115684U,	"InventoryWeightMetal" },
        { 330491720U,	"InventoryWeightWood" },
        { 476134665U,	"WeaponFire" },
        { 640949982U,	"Speed" },
        { 642872282U,	"IceCrackingVolume" },
        { 841158053U,	"PlayerBowFatigue" },
        { 923990149U,	"FireBlendState" },
        { 924465545U,	"SteamTrain_FuelSputter" },
        { 991664965U,	"AuroraElectrolizer" },
        { 1103667651U,	"SteamTrain_DamagedTrackMeter" },
        { 1116036883U,	"Incremental" },
        { 1342856929U,	"SteamTrain_VolumeWater" },
        { 1372927986U,	"RTPC_HandheldShortwave_Strength" },
        { 1553757739U,	"RTPC_HandheldShortwave_ThreshholdAlertDelay" },
        { 1557571652U,	"RTPC_HandheldShortwave_Distance" },
        { 1579733230U,	"Trader_TrustLevel_rtpc" },
        { 1665458623U,	"PlayerVelocityTrue" },
        { 1668026760U,	"SpeechAutoDucker" },
        { 1721946080U,	"InventoryWeightWater" },
        { 1825515640U,	"LightsourceFlicker" },
        { 1844117411U,	"EntityFSDucker" },
        { 1922851440U,	"RopeVelocity" },
        { 1954039910U,	"CoughingIntensity" },
        { 1979406557U,	"ChemicalHazardRiskRTPC" },
        { 2055680380U,	"DarkwalkerBanishmentGlyph" },
        { 2064316281U,	"InventoryWeightGeneral" },
        { 2403673617U,	"SteamTrain_BoilerTemperature" },
        { 2497634674U,	"CookingState" },
        { 2551811045U,	"EarRingingVolume" },
        { 2603274823U,	"StoveDoors" },
        { 2615691938U,	"SteamTrain_DamagedTrack_State" },
        { 2720872602U,	"StimpackStrength" },
        { 2728861532U,	"EntityDistanceRTPC" },
        { 2777629753U,	"WindGustIntensity" },
        { 2886637823U,	"PlayerVelocity" },
        { 2985914446U,	"Travois_TurnAngle" },
        { 3174855862U,	"AuroraStrength" },
        { 3502797176U,	"SteamTrain_DamagedTrack" },
        { 3643818186U,	"WindActualSpeed" },
        { 3729329928U,	"ConditionHeartbeat" },
        { 3741502372U,	"PlayerIncline" },
        { 4007481307U,	"BreathLevel" },
        { 4070528516U,	"Timescale" },
        { 4256178795U,	"CougarAttackChanceCountdown" },
        { 4278817768U,	"SteamTrain_VolumeFuel" },
        { 262526082U,	"WolfAttackMute" },
        { 477512431U,	"LPFilterLevel" },
        { 600876260U,	"Respirator_Condition" },
        { 700645944U,	"CougarDistanceFromPlayer" },
        { 822163944U,	"SS_Air_RPM" },
        { 988953028U,	"SFXVolume" },
        { 1029930033U,	"SS_Air_Fury" },
        { 1219918067U,	"WindGustsSidechain" },
        { 1351367891U,	"SS_Air_Fear" },
        { 1444535918U,	"InteractiveDucksMeter" },
        { 1840270857U,	"_ListenerCone" },
        { 1876726300U,	"GameTimeElapsed" },
        { 1967737439U,	"FootstepWindDucker" },
        { 2346531308U,	"MusicVolume" },
        { 2496722626U,	"EntityDuckMeter" },
        { 2648548617U,	"SS_Air_Month" },
        { 2832409559U,	"_EmitterCone" },
        { 2918011349U,	"MasterVolume" },
        { 2935111770U,	"SPValve" },
        { 2950444590U,	"GUIWindLevelling" },
        { 3002758120U,	"SS_Air_Freefall" },
        { 3074696722U,	"SS_Air_Size" },
        { 3203397129U,	"SS_Air_TimeOfDay" },
        { 3325181466U,	"VOVolume" },
        { 3332483390U,	"InteractiveBusMeter" },
        { 3385215644U,	"PainPulse" },
        { 3546521921U,	"AmbientVolume" },
        { 3715662592U,	"SS_Air_Storm" },
        { 3731891983U,	"ItemCondition" },
        { 3770572768U,	"_Azimuth" },
        { 3847924954U,	"SS_Air_Presence" },
        { 3949411123U,	"EntityIntensity" },
        { 4008773032U,	"MusicFrontEndDuckMeter" },
        { 4025273213U,	"_Elevation" },
        { 4071000082U,	"GlobalVolume" },
        { 4075724360U,	"CougarEncroachmentWarningCountdown" },
        { 4160247818U,	"SS_Air_Turbulence" },
        { 4189837742U,	"ElectrostaticFogRtpc" },
    };

    public static string GetEventName(uint eventId)
    {
        if (SoundbankEvents.ContainsKey(eventId))
        {
            return SoundbankEvents[eventId];
        }
        else
        {
            return $"UnknownEventId_{eventId}";
        }
    }
        
    public static uint GetEventId(string eventName)
    {
        var name = eventName.ToUpperInvariant();
        if (SoundbankEventsReversed.ContainsKey(name))
        {
            return SoundbankEventsReversed[name];
        }
        else
        {
            return 0;
        }
    }
}