using System;
using AudioMgr;
using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Handlers;
using LongLargo.Model;
using UnityEngine;

namespace LongLargo.PatchImplementations;

public static class GameAudioManagerImpl
{
    /// <summary>
    /// Decides whatever we play custom clip and which exactly.
    /// </summary>
    /// <param name="soundName">Sound event name</param>
    /// <param name="go">Game object</param>
    /// <returns>false if we should suppress original music or sound.</returns>
    public static bool PlayMusic(string soundName, ref GameObject go)
    {
        if (ShouldSkip())
        {
            return true;
        }
        
        var situationInfo = GetSituationByEvent(soundName);
        if (MaybeIgnoreEvent(situationInfo))
        {
            return true;
        }

        // Check for suppress music settings
        if (MaybeSupressEvent(situationInfo.Situation))
        {
            return false;
        }
        
        Clip clip;
        bool playVanilla;

        if (situationInfo.Situation.IsExploration())
        {
            (clip, playVanilla) = LongLargoMain.QueueManager.GetExplorationClip();
        }
        else
        {
            (clip, playVanilla) = LongLargoMain.QueueManager.GetSituationClip(situationInfo.Situation);
        }

        LLogger.Debug($"[GameAudioManagerImpl] plays {situationInfo.Situation} clip {clip?.audioClip?.name ?? "ShortSilence"}");
        
        return PlayCLip(playVanilla, situationInfo, clip, go);
    }

    private static bool ShouldSkip()
    {
        if (!LLSettings.settings.ModEnabled)
        {
            return true;
        }

        var scene = GameManager.m_ActiveScene;
        if (scene == null || scene.Contains("Menu") || scene.Contains("Boot")
            || scene.Contains("Bunker") || scene == "MiningRegionMine") // Don't mess with tales
        {
            return true;
        }

        return false;
    }

    private static SituationInfo GetSituationByEvent(string soundName)
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
                situationInfo.Situation = SituationType.Timberwolf;
                break;
            // stop loop events
            case "Stop_musicMood_AnimalStalking":
                situationInfo.Situation = SituationType.Disabled;
                if (LongLargoMain.QueueManager.LastSituation == SituationType.Stalked)
                {
                    LongLargoMain.QueueManager.Stop(3f);
                }
                break;
            case "Stop_TimberwolfCombat":
                situationInfo.Situation = SituationType.Disabled;
                if (LongLargoMain.QueueManager.LastSituation == SituationType.Timberwolf)
                {
                    LongLargoMain.QueueManager.Stop(3f);
                }
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
                if (LongLargoMain.QueueManager.LastSituation.IsWeather())
                {
                    LongLargoMain.QueueManager.Stop();
                }

                LongLargoMain.QueueManager.ResetLastSoundtrack();
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
                LLogger.Debug("Playing Silence to avoid overlap");
                situationInfo.Situation = SituationType.Disabled;
                situationInfo.SilenceType = SituationInfo.SilenceLength.Long;
                break;
            case "Play_musicTales_Intro_Stinger":
            case "Play_musicTales_Objective_Intro_Stinger":
            case "Play_musicTales_Objective_Outro_Stinger":
            case "Play_musicTales_Outro_Stinger":
            case "Play_SndMusNewLocationShort":
            case "Play_musicTales_Bunker_Stinger":
                LLogger.Debug("Playing short Silence to avoid overlap");
                situationInfo.Situation = SituationType.Disabled;
                situationInfo.SilenceType = SituationInfo.SilenceLength.Short;
                break;
            
            default:
                // Not for us
                situationInfo.Situation = SituationType.Disabled;
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
                    LongLargoMain.QueueManager.PlayHard(LongLargoMain.PlaylistProvider.LongSilence, 3f);
                    return true;
                case SituationInfo.SilenceLength.Long:
                    LongLargoMain.QueueManager.PlayHard(LongLargoMain.PlaylistProvider.ShortSilence, 3f);
                    return true;
            }
        }

        // since Timberwolf combat is silenced by Vanilla, I think suppressing here it is better
        if (LLSettings.settings.TimberwolfSuppress && (SituationType.Timberwolf).HasFlag(situationInfo.Situation))
        {
            return true;
        }

        return false;
    }

    private static bool MaybeSupressEvent(SituationType situation)
    {
        if (LLSettings.settings.WeatherSuppress && (SituationType.WeatherClear | SituationType.WeatherBlizzard | 
                                                  SituationType.WeatherSnow | SituationType.WeatherFog).HasFlag(situation)
            || LLSettings.settings.TimeSuppress && (SituationType.TimeDawn | SituationType.TimeDusk).HasFlag(situation)
            || LLSettings.settings.StalkedSuppress && (SituationType.Stalked).HasFlag(situation)
            || LLSettings.settings.TimberwolfSuppress && (SituationType.Timberwolf).HasFlag(situation)
            || LLSettings.settings.ConditionSuppress && (SituationType.ConditionSuccess | SituationType.ConditionSorrow).HasFlag(situation))
        {
            return true;
        }

        // Check if we're already playing something custom
        if (LongLargoMain.QueueManager.IsPlaying
            && situation != SituationType.Stalked && situation != SituationType.Timberwolf)
        {
            return true;
        }

        return false;
    }

    private static bool PlayCLip(bool playVanilla, SituationInfo situationInfo, Clip clip, GameObject go)
    {
        if (!playVanilla && situationInfo.WithStringer)
        {
            // Play replaced stringer with delay, replace original event with stingerless
            LLogger.Debug($"[GameAudioManagerImpl] replaces with stringerless {situationInfo.StingerlessEvent}");
            LongLargoMain.QueueManager.PlaySoftDelayed(clip, situationInfo.Delay);
            GameAudioManager.PlaySound(situationInfo.StingerlessEvent, go);
            return false;
        }

        if (situationInfo.Situation.HasFlag(SituationType.Stalked) || situationInfo.Situation.HasFlag(SituationType.Timberwolf))
        {
            LongLargoMain.QueueManager.PlayHard(clip, true);
        }
        else
        {
            LongLargoMain.QueueManager.PlaySoft(clip);
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
    }
}