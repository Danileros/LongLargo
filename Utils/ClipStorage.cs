using UnityEngine;

namespace LongLargo.Utils;

public class ClipStorage : Dictionary<string, AudioClip>
{
    public ClipStorage() : base()
    {
    }
    
    public ClipStorage(int capacity) : base(capacity)
    {
    }
    
    public static ClipStorage LoadFromBundle(AssetBundle assetBundle)
    {
        
        var clips = new ClipStorage(assetBundle.AllAssetNames().Length);
        foreach (string assetName in assetBundle.AllAssetNames())
        {
            if (assetName.Contains(".ogg") || assetName.Contains(".wav") || assetName.Contains(".mp3"))
            {
                var clip = assetBundle.LoadAsset<AudioClip>(assetName);
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
                clips.Add(
                    Path.GetFileNameWithoutExtension(assetName),
                    clip);
            }
        }
        
        return clips;
    }

    public AudioClip GetClip(string trackName)
    {
        return this[trackName];
    }

    public void LoadAudioclip(string trackName, AudioClip clip)
    {
        this.Add(trackName, clip);
        clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}