using LongLargo.Models;

namespace LongLargo.Handlers;

public static class AkSoundEngineHandler
{
    /// <summary>
    /// Decides whatever we play custom exploration clip and which exactly.
    /// </summary>
    /// <returns>false if we should suppress original music.</returns>
    public static bool PostEvent(string soundName)
    {
        switch (soundName)
        {
            case "Play_musicStinger_IR_Arrive_PortRemnant_SeaCave":
            case "Play_musicStinger_IR_Discover_Boat":
            case "Play_musicStinger_IR_Discover_Train":
            case "Play_musicStinger_IR_Discover_Whale":
            case "Play_musicStinger_IR_Enter_Church":
            case "Play_musicStinger_IR_LoadingScreen":
                Main.Logger.Debug("[AkSoundEngine] Playing short Silence to avoid overlap");
                Main.AudioPlayer.PlayHard(Main.PlaylistManager.ShortSilence, FSituationType.Disabled, 3f);
                break;
        }
        return true;
    }
}