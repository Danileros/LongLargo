using AudioMgr;
using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Managers;
using LongLargo.Models;
using LongLargo.Utils;
using UnityEngine;

namespace LongLargo.Handlers;

public static class GameAudioManagerHandler
{
    /// <summary>
    /// Decides whatever we play custom clip and which exactly.
    /// </summary>
    /// <param name="soundName">Sound event name</param>
    /// <param name="go">Game object</param>
    /// <returns>false if we should suppress original music or sound.</returns>
    public static bool PlayMusic(string soundName, ref GameObject go)
    {
        if (Main.IsModDisabled())
        {
            return true;
        }
        
        var situationInfo = GetSituationByEvent(soundName, go);
        if (situationInfo.WorthLogging)
        {
            LLogger.Log($"[GameAudioManager] Hooking event {soundName}");
        }
        
        if (MaybeIgnoreEvent(situationInfo))
        {
            return true;
        }

        // Check for suppress music settings
        if (MaybeSupressEvent(situationInfo.Situation))
        {
            if (situationInfo.Situation.IsWeather() && !string.IsNullOrEmpty(situationInfo.StingerlessEvent))
            {
                LLogger.Debug($"[GameAudioManager] Replacing event with {situationInfo.StingerlessEvent}");
                GameAudioManager.PlaySound(situationInfo.StingerlessEvent, go);
            }

            return false;
        }
        
        SoundtrackInfo soundtrack;
        bool playVanilla;

        if (situationInfo.Situation.IsExploration())
        {
            (soundtrack, playVanilla) = Main.PlaylistManager.GetExplorationSoundtrack(situationInfo.Situation);
        }
        else
        {
            (soundtrack, playVanilla) = Main.PlaylistManager.GetSituationSoundtrack(situationInfo.Situation);
        }

        LLogger.Debug($"[GameAudioManager] Choosing {situationInfo.Situation} clip {soundtrack?.TrackName ?? "ShortSilence"}");
        
        return PlayCLip(playVanilla, situationInfo, soundtrack, go);
    }

    private static SituationInfo GetSituationByEvent(string soundName, GameObject go)
    {
        SituationInfo situationInfo = new SituationInfo();
        switch (soundName)
        {
            // Randomly plays in certain locations; let's pretend it is an Exploration track 
            case "Play_Caves":
            case "Play_SteamTunnels":
            // Not sure if it could be triggered here but just in case
            case "Play_SndMusicExploration1":
            case "Play_SndMusicExploration1Light":
            case "Play_SndMusicExploration2":
            case "Play_SndMusicExplorationLong":
            case "Play_SndMusicExplorationLongDay":
            case "Play_SndMusicExplorationLongNight":
                LLogger.Debug($"[GameAudioManager] Exploration event {soundName}");
                situationInfo.Situation = SituationTypeExtensions.GetExplorationSituation();
                break;
            case "Play_MusicClear":
                situationInfo.Situation = SituationType.WeatherClear;
                break;
            case "Play_Weather_Clear_withStinger45":
            case "Play_Weather_Clear_withStinger60":
                situationInfo.Situation = SituationType.WeatherClear;
                situationInfo.SetStinger("Play_Weather_Clear");
                break;
            case "Play_Weather_ClearAurora_withStinger05":
            case "Play_Weather_ClearAurora_withStinger60":
                situationInfo.Situation = SituationType.WeatherClear;
                situationInfo.SetStinger("Play_Weather_ClearAurora");
                break;
            case "Play_MusicFoggy":
                situationInfo.Situation = SituationType.WeatherFog;
                break;
            case "Play_Weather_LightFog_withStinger45":
            case "Play_Weather_LightFog_withStinger60":
                situationInfo.Situation = SituationType.WeatherFog;
                situationInfo.SetStinger("Play_Weather_LightFog");
                break;
            case "Play_Weather_DenseFog_withStinger30":
            case "Play_Weather_DenseFog_withStinger45":
            case "Play_Weather_DenseFog_withStinger60":
                situationInfo.Situation = SituationType.WeatherFog;
                situationInfo.SetStinger("Play_Weather_DenseFog");
                break;
            case "Play_Weather_ElectrostaticFog":
                situationInfo.Situation = SituationType.WeatherFog;
                break;
            case "Play_MusicHeavySnow":
                situationInfo.Situation = SituationType.WeatherSnow;
                break;
            case "Play_Weather_HeavySnow_withStinger30":
                situationInfo.Situation = SituationType.WeatherSnow;
                situationInfo.SetStinger("Play_Weather_HeavySnow");
                break;
            case "Play_MusicStorm":
                situationInfo.Situation = SituationType.WeatherBlizzard;
                break;
            case "Play_Weather_Blizzard_withStinger00":
            case "Play_Weather_Blizzard_withStinger05":
            case "Play_Weather_Blizzard_withStinger30":
            case "Play_Weather_Blizzard_withStinger60":
                situationInfo.Situation = SituationType.WeatherBlizzard;
                situationInfo.SetStinger("Play_Weather_Blizzard");
                break;
            case "Play_MusicWeather":
                var stage = GameManager.GetWeatherComponent().GetWeatherStage();
                switch (stage)
                {
                    case WeatherStage.LightSnow:
                    case WeatherStage.HeavySnow:
                        situationInfo.Situation = SituationType.WeatherSnow;
                        break;
                    case WeatherStage.PartlyCloudy:
                    case WeatherStage.Cloudy:
                    case WeatherStage.ClearAurora:
                    case WeatherStage.Clear:
                    default:
                        situationInfo.Situation = SituationType.WeatherClear;
                        break;
                    case WeatherStage.Blizzard:
                        situationInfo.Situation = SituationType.WeatherBlizzard;
                        break;
                    case WeatherStage.LightFog:
                    case WeatherStage.DenseFog:
                    case WeatherStage.ToxicFog:
                    case WeatherStage.ElectrostaticFog:
                        situationInfo.Situation = SituationType.WeatherFog;
                        break;
                }
                break;
            case "Play_musicMood_HappySuccess":
                situationInfo.Situation = SituationType.ConditionSuccess;
                break;
            case "Play_musicMood_Sorrow":
                situationInfo.Situation = SituationType.ConditionSorrow;
                break;
            case "Play_MusicTODDawn":
                situationInfo.Situation = SituationType.TimeDawn;
                break;
            case "Play_MusicTODDusk":
                situationInfo.Situation = SituationType.TimeDusk;
                break;
            // loop events, high priority
            case "Play_musicMood_AnimalStalking":
                situationInfo.Situation = SituationType.Stalked;
                break;
            case "Play_TimberwolfCombat":
                // situationInfo.Situation = SituationType.Timberwolf;
                situationInfo.Situation = Main.PackProximityManager.OnPlayCombat(go);
                break;
            // stop loop events
            case "Stop_musicMood_AnimalStalking":
                situationInfo.Situation = SituationType.Disabled;
                Main.AudioPlayer.StopIfSituation(SituationType.Stalked, 3f);
                break;
            case "Stop_TimberwolfCombat":
                situationInfo.Situation = Main.PackProximityManager.OnStopCombat();
                // situationInfo.Situation = SituationType.Disabled;
                // Main.AudioPlayer.StopIfSituation(SituationType.Timberwolf, 3f);
                break;
            case "Stop_Weather_Blizzard":
            case "Stop_Weather_Clear":
            case "Stop_Weather_ClearAurora":
            case "Stop_Weather_Cloudy":
            case "Stop_Weather_DenseFog":
            case "Stop_Weather_ElectrostaticFog":
            case "Stop_Weather_ElectrostaticFog_Actions":
            case "Stop_Weather_HeavySnow":
            case "Stop_Weather_LightFog":
            case "Stop_Weather_LightSnow":
            case "Stop_Weather_PartlyCloudy":
            case "Stop_Weather_ToxicFog":
                situationInfo.Situation = SituationType.Disabled;
                if (Main.AudioPlayer.LastSituation.IsWeather())
                {
                    Main.AudioPlayer.Stop(1f);
                }

                break;
                
            //// I did not need 
            // case "Play_Weather_Clear":
            // case "Play_Weather_ClearAurora":
            // case "Play_Weather_Cloudy":
            // case "Play_Weather_PartlyCloudy":
            // case "Play_Weather_LightSnow":
            // case "Play_Weather_HeavySnow":
            // case "Play_Weather_ToxicFog":
            // case "Play_Weather_DenseFog":
            // case "Play_Weather_LightFog":
            // case "Play_Weather_Blizzard":
                
            // reasons to play Silence to avoid overlap
            case "Play_musicMood_Creepy":
            case "Play_musicMood_Hope":
            case "Play_musicMood_Hope02":
            case "Play_musicMood_LifeAfterDeath":
            case "Play_musicMood_NearDeath":
            case "Play_musicMood_PlayerDeath":
            case "Play_musicMood_Suspense":
            case "Play_musicMood_SuspenseLowProbability":
            case "Play_musicMood_Vista":
            case "Play_musicMood_VistaLowProb":
            case "Play_musicTale02_RudigersChamber":
            case "Play_musicTales_Bunker_Exploration":
            case "Play_musicTales_Intro":
            case "Play_musicTales_Outro":
            case "Play_SndMusAreaTransition1":
            case "Play_SndMusNewLocation":
            case "Play_musicScene_AshCanyon_Enter":
            case "Play_musicScene_Hub_Enter":
                LLogger.Debug("[GameAudioManager] Playing Silence to avoid overlap");
                situationInfo.Situation = SituationType.Disabled;
                situationInfo.SilenceType = SituationInfo.SilenceLength.Long;
                break;
            case "Play_musicTales_Intro_Stinger":
            case "Play_musicTales_Objective_Intro_Stinger":
            case "Play_musicTales_Objective_Outro_Stinger":
            case "Play_musicTales_Outro_Stinger":
            case "Play_SndMusNewLocationShort":
            case "Play_musicTales_Bunker_Stinger":
                LLogger.Debug("[GameAudioManager] Playing short Silence to avoid overlap");
                situationInfo.Situation = SituationType.Disabled;
                situationInfo.SilenceType = SituationInfo.SilenceLength.Short;
                break;
            
            default:
                // Not for us
                situationInfo.Situation = SituationType.Disabled;
                situationInfo.WorthLogging = false;
                break;
        }

        if (situationInfo.WithStringer)
        {
            var lastDigits = soundName.Substring(Math.Max(0, soundName.Length - 2));
            if (float.TryParse(lastDigits, out var delay))
            {
                situationInfo.Delay = delay;
            }
        }
        
        return situationInfo;
    }

    private static bool MaybeIgnoreEvent(SituationInfo situationInfo)
    {
        if (situationInfo.Situation == SituationType.Disabled)
        {
            switch (situationInfo.SilenceType)
            {
                case SituationInfo.SilenceLength.None:
                    return true;
                case SituationInfo.SilenceLength.Short:
                    Main.AudioPlayer.PlayHard(Main.PlaylistManager.ShortSilence, situationInfo.Situation, 3f);
                    return true;
                case SituationInfo.SilenceLength.Long:
                    Main.AudioPlayer.PlayHard(Main.PlaylistManager.LongSilence, situationInfo.Situation, 3f);
                    return true;
            }
        }

        // since Timberwolf combat is silenced by Vanilla, I think suppressing here it is better
        if (SettingsManager.Settings.TimberwolfSuppress && (SituationType.Timberwolf).HasFlagSafe(situationInfo.Situation))
        {
            return true;
        }

        return false;
    }

    private static bool MaybeSupressEvent(SituationType situation)
    {
        if (SettingsManager.Settings.WeatherSuppress && situation.IsWeather()
            || SettingsManager.Settings.TimeSuppress && situation.IsTime()
            || SettingsManager.Settings.StalkedSuppress && (SituationType.Stalked).HasFlagSafe(situation)
            || SettingsManager.Settings.TimberwolfSuppress && (SituationType.Timberwolf).HasFlagSafe(situation)
            || SettingsManager.Settings.ConditionSuppress && situation.IsCondition())
        {
            return true;
        }

        // Check if we're already playing something custom
        if (Main.AudioPlayer.IsPlaying
            && situation != SituationType.Stalked && situation != SituationType.Timberwolf)
        {
            return true;
        }

        return false;
    }

    private static bool PlayCLip(bool playVanilla, SituationInfo situationInfo, SoundtrackInfo soundtrack, GameObject go)
    {
        if (!playVanilla && situationInfo.WithStringer)
        {
            // Play replaced stringer with delay, replace original event with stingerless
            LLogger.Debug($"[GameAudioManager] Replacing event with {situationInfo.StingerlessEvent}");
            Main.AudioPlayer.PlaySoftDelayed(soundtrack, situationInfo.Situation, situationInfo.Delay);
            GameAudioManager.PlaySound(situationInfo.StingerlessEvent, go);
            return false;
        }

        if (situationInfo.Situation.HasFlagSafe(SituationType.Stalked) || situationInfo.Situation.HasFlagSafe(SituationType.Timberwolf))
        {
            Main.AudioPlayer.PlayHard(soundtrack, situationInfo.Situation, true);
            if (!playVanilla)
            {
                // TODO: debug
                // <ActionPostEventEntry Id="2904596785" Name="musicMixer_StopSceneMusicFadeOut"/>
                // <ActionPostEventEntry Id="3267898453" Name="musicMixer_StopWeatherMusicFadeOut"/>
                GameAudioManager.PlaySound(2904596785U, go);
                GameAudioManager.PlaySound(3267898453U, go);
            }
        }
        else
        {
            Main.AudioPlayer.PlaySoft(soundtrack, situationInfo.Situation);
        }
        
        return playVanilla;
    }

    private class SituationInfo
    {
        public enum SilenceLength
        {
            None,
            Short,
            Long,
        }

        public void SetStinger(string stingerlessEvent)
        {
            WithStringer = true;
            StingerlessEvent = stingerlessEvent;
        }

        public SituationType Situation;
        public bool WithStringer;
        public string StingerlessEvent;
        public float Delay = 0;
        public SilenceLength SilenceType = SilenceLength.None;
        public bool WorthLogging = true;
    }
}