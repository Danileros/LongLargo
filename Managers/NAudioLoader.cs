using System.Reflection;
using AudioMgr;
using LongLargo.Utils;
using UnityEngine;

namespace LongLargo.Managers;

public class NAudioLoader
{
    private Assembly _naudio;
    private Type _audioFileReaderType;

    public void LoadLibraries(string folderPath)
    {
        try
        {
            _naudio = Assembly.LoadFrom(Path.Combine(folderPath, "NAudio.dll"));
            _audioFileReaderType = _naudio.GetType("NAudio.Wave.AudioFileReader");
            LLogger.Log("Loaded NAudio");
        }
        catch (Exception e)
        {
            _naudio = null;
            LLogger.Error("Failed to load NAudio, user music feature is now disabled. Ensure Directory " +
                          "'Mods/LongLargo' exists and has NAudio dlls. This error is not critical, " +
                          "main Long Largo features are still enabled");
        }
    }
    
    public void LoadUgly(string[] soundtrackPaths, ClipManager clipManager)
    {
        try
        {
            if (soundtrackPaths.Length == 0)
            {
                return;
            }

            foreach (var path in soundtrackPaths)
            {
                var (pcmData, channels, sampleRate) = LoadAudioAsFloatEvenUglier(path);
                var name = PlaylistHelper.GetTrackName(path);
                var audioClip = LoadFromRawPCM(name, pcmData, channels, sampleRate);
                clipManager.LoadAudioclip(name, audioClip);
            }
        }
        catch (Exception e)
        {
            LLogger.Error(e.ToString());
        }
    }

    // // TODO: remove when Melon or AudioManager will get an update
    // private (float[],int, int) LoadAudioAsFloat(string filePath)
    // {
    //     using (var reader = new AudioFileReader(filePath))
    //     {
    //         // Total number of samples = length in bytes / bytes per sample (4 for float)
    //         var sampleCount = (int)(reader.Length / 4);
    //         var buffer = new float[sampleCount];
    //
    //         // Read all samples into the array
    //         var read = reader.Read(buffer, 0, buffer.Length);
    //         var channels = reader.WaveFormat.Channels;
    //         var sampleRate = reader.WaveFormat.SampleRate;
    //         return (buffer, channels, sampleRate);
    //     }
    // }
    
    private (float[],int, int) LoadAudioAsFloatEvenUglier(string filePath)
    {
        using (var reader = GetReader(filePath))
        {
            dynamic readerDynamic = reader;
            // Total number of samples = length in bytes / bytes per sample (4 for float)
            var sampleCount = (int)(readerDynamic.Length / 4);
            var buffer = new float[sampleCount];
    
            // Read all samples into the array
            var read = readerDynamic.Read(buffer, 0, buffer.Length);
            var channels = readerDynamic.WaveFormat.Channels;
            var sampleRate = readerDynamic.WaveFormat.SampleRate;
            return (buffer, channels, sampleRate);
        }
    }

    private IDisposable GetReader(string filePath)
    {
        return Activator.CreateInstance(_audioFileReaderType, filePath) as IDisposable;
    }


    private AudioClip LoadFromRawPCM(string name, float[] pcmData, int channels, int sampleRate)
    {
        var clip = AudioClip.Create(name, pcmData.Length / channels, channels, sampleRate, false);
        clip.SetData(pcmData, 0);
    
        return clip;
    }
}