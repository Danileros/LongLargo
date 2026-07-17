// using System;
// using AudioMgr;
// using Il2Cpp;
// using Il2CppInterop.Runtime.Attributes;
// using Il2CppVLB;
// using LongLargo.Handlers;
// using LongLargo.Model;
// using MelonLoader;
// using UnityEngine;
//
// namespace LongLargo.PatchImplementations;
//
// public static class AkSoundEngineImpl
// {
//     /// <summary>
//     /// Decides whatever we play custom exploration clip and which exactly.
//     /// </summary>
//     /// <returns>false if we should suppress original music.</returns>
//     public static bool PostEvent(string name)
//     {
//         if (name == "Play_MusicWeather")
//         {
//             var stage = GameManager.GetWeatherComponent().GetWeatherStage();
//             SituationType situation;
//             switch (stage)
//             {
//                 case WeatherStage.LightSnow:
//                 case WeatherStage.HeavySnow:
//                     situation = SituationType.WeatherSnow;
//                     break;
//                 case WeatherStage.PartlyCloudy:
//                 case WeatherStage.Cloudy:
//                 case WeatherStage.ClearAurora:
//                 case WeatherStage.Clear:
//                 default:
//                     situation = SituationType.WeatherClear;
//                     break;
//                 case WeatherStage.Blizzard:
//                     situation = SituationType.WeatherBlizzard;
//                     break;
//                 case WeatherStage.LightFog:
//                 case WeatherStage.DenseFog:
//                 case WeatherStage.ToxicFog:
//                 case WeatherStage.ElectrostaticFog:
//                     situation = SituationType.WeatherFog;
//                     break;
//             }
//             
//             // Check for suppress music settings
//             if (MaybeSupressEvent(situation))
//             {
//                 return false;
//             }
//         
//             (var clip, var playVanilla) = LongLargoMain.QueueManager.GetSituationClip(situation);
//         
//             LLogger.Debug($"GameAudioManagerImpl plays {situation} clip {clip?.audioClip?.name ?? "ShortSilence"}");
//
//             return PlayCLip(playVanilla, situation, clip);
//         }
//
//         return true;
//     }
//
//     private static bool MaybeSupressEvent(SituationType situation)
//     {
//         if (Settings.settings.WeatherSuppress && (SituationType.WeatherClear | SituationType.WeatherBlizzard | 
//                                                   SituationType.WeatherSnow | SituationType.WeatherFog).HasFlag(situation)
//             || Settings.settings.TimeSuppress && (SituationType.TimeDawn | SituationType.TimeDusk).HasFlag(situation)
//             || Settings.settings.StalkedSuppress && (SituationType.Stalked).HasFlag(situation)
//             || Settings.settings.TimberwolfSuppress && (SituationType.Timberwolf).HasFlag(situation)
//             || Settings.settings.SuccessSuppress && (SituationType.Success | SituationType.Sorrow).HasFlag(situation))
//         {
//             return true;
//         }
//
//         // Check if we're already playing something custom
//         if (LongLargoMain.QueueManager.IsPlaying && situation != SituationType.Stalked && situation != SituationType.Timberwolf)
//         {
//             return true;
//         }
//
//         return false;
//     }
//
//     private static bool PlayCLip(bool playVanilla, SituationType situation, Clip clip)
//     {
//         if (situation.HasFlag(SituationType.Stalked) || situation.HasFlag(SituationType.Timberwolf))
//         {
//             LongLargoMain.QueueManager.PlayHard(clip, true);
//         }
//         else
//         {
//             LongLargoMain.QueueManager.PlaySoft(clip);
//         }
//         
//         return playVanilla;
//     }
// }