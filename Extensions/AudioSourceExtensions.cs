using System;
using UnityEngine;
using System.Collections;

namespace LongLargo.Extensions;

public static class AudioSourceExtensions
{
    /// <summary>
    /// Stops audiosource with fade.
    /// </summary>
    /// <param name="audioSource">audiosource.</param>
    /// <param name="fadeTime">Time to fade sound completely.</param>
    /// <returns>Reason to use MelonCoroutines.</returns>
    public static IEnumerator FadeOut (this AudioSource audioSource, float fadeTime)
    {
        var startVolume = audioSource.volume;
        var startTime = Time.time;
        var time = 0f;
        
        // sounds a bit better than linear
        while (time < fadeTime)
        {
            time = Time.time - startTime;
            audioSource.volume = startVolume * (float)(Math.Cos(time / fadeTime * Math.PI) + 1) / 2.0f;
            yield return null;
        }
        
        //// linear
        // while (audioSource.volume > 0) {
        //     audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
        //     yield return null;
        // }
        
        audioSource.Stop ();
        audioSource.volume = startVolume;
    }
}